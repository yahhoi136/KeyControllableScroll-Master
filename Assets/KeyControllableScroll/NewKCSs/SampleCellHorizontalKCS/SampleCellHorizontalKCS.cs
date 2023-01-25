
using System;
using UnityEditor.UI;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/KCS/SampleCellHorizontalKCS", 100)]
    public class SampleCellHorizontalKCS : KeyControllableScrollBase
    {

    #region 変数定義

        // Cell Recycle
        public ISampleCellHorizontalKCSDataSource DataSource;
        private ScrollRect scrollRect;
        private RectTransform cellListRT;
        private (GameObject go, RectTransform rectT, Image image, Button button, Image image_0, Text text_1)[] recycleArray;
        private int allRowNum; // 全行数
        /// <summary>
        /// 一画面におけるセルの最大表示可能行数(一部でも見えてたらカウント)＋上下２行ずつ
        /// </summary>
        private int maxDisplayRowNum;
        /// <summary>
        /// 一画面におけるセルの最大表示可能数(一部でも見えてたらカウント)＋上下２行分のセル
        /// </summary>
        private int maxDisplayCellNum;
        /// <summary>
        /// 一画面におけるセルの最大表示可能行数(セル＋１個分スペースの全てが見えてたらカウント)
        /// </summary>
        private int maxDisplayRomNumComp;
        private int firstNormalizedRowNo;
        /// <summary>
        /// 常にViewportの中心を0行目とした行数
        /// </summary>
        private int nowNormalizedRowNo;
        /// <summary>
        /// スクロール時にViewportの中心から最大上下何行までフォーカスを固定できるか
        /// </summary>
        private int maxFixedRow;
        private int firstCenterRowNo = 0;
        private int nowCenterRowNo; // 今中心にいる行数
        private int preCenterRowNo;

        // KeyActvate・Repeat
        private bool isScrollLeftKeyActive = false;
        private bool isScrollRightKeyActive = false;
        private bool isLeftKeyActive = false;
        private bool isRightKeyActive = false;
        private bool isUpKeyActive = false;
        private bool isDownKeyActive = false;
        private float intervalTime = 0f;
        private bool isDelayOn = true;
        private float delayDecreaseTime = 0f;

        // ScrollKeyControl
        public ISampleCellHorizontalKCSScrollKeyMoved ScrollKeyMoved;
        public ISampleCellHorizontalKCSScrollKeyInput ScrollKeyInput;
        private bool isFocusChasingScroll;

        // FocusKeyControl
        public ISampleCellHorizontalKCSFocusKeyMoved FocusKeyMoved;
        public ISampleCellHorizontalKCSFocusKeyInput FocusKeyInput;
        private bool isScrollChasingFocus;        
        private int preFocusedCellNo;
        private float elapsedTime = 0f;
        private float oldLocalPosX;
        private float newLocalPosX;

    #endregion

        override public bool IsHorizontal => true;
        
        private void Start()
        {
            cellListRT = transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
            Init();
            scrollRect = GetComponent<ScrollRect>();
            scrollRect.onValueChanged.AddListener(RecycleCell);
        }


    #region 初期化

        /// <summary>
        /// LightLoopScroll全体を再描写する
        /// </summary>
        override public void Init()
        {
#if UNITY_EDITOR
            if (_cellPrefab == null || _viewportWidth <= 0 || _allContentsNum <= 0)
            {
                throw new InvalidOperationException("インスペクターで Cell Recycle Param を埋めてください");
            }
            if (DataSource == null)
            {
                throw new InvalidOperationException("DataSourceにインターフェースをセットしてください");
            }
            if (_cellSize.x + _spacing.x > _viewportWidth)
            {
                throw new InvalidOperationException("セルとスペースのサイズが大きすぎます、cellSize.x + spacing.x をviewportWidth 以下にしてください");
            }
#endif
            // スクロール戻す
            var newRectLocalPosition = cellListRT.localPosition;
            newRectLocalPosition.x = 0;
            cellListRT.localPosition = newRectLocalPosition;
            
            // パラメーターリセット
            if (recycleArray is not null)
                foreach (var t in recycleArray)
                    Destroy(t.go);
            
            nowCenterRowNo = 0;
            preCenterRowNo = 0;
            isScrollChasingFocus = false;
            _focusedCellNo = 0;
            elapsedTime = 0f;
            isScrollLeftKeyActive = false;
            isScrollRightKeyActive = false;
            isLeftKeyActive = false;
            isRightKeyActive = false;
            isUpKeyActive = false;
            isDownKeyActive = false;
            intervalTime = 0f;
            isDelayOn = true;
            delayDecreaseTime = 0f;

            // 生成する行数 = 一部でもセルが見えている行数＋1,2行
            maxDisplayRowNum = Mathf.CeilToInt(_viewportWidth / (_cellSize.x + _spacing.x));
            if (_isSmoothMode) maxDisplayRowNum = maxDisplayRowNum % 2 == 0 ? maxDisplayRowNum + 5 : maxDisplayRowNum + 4;
            else maxDisplayRowNum = maxDisplayRowNum % 2 == 0 ? maxDisplayRowNum + 3 : maxDisplayRowNum + 2;

            // maxFiexedRow決定
            maxDisplayRomNumComp = Mathf.FloorToInt(_viewportWidth / (_cellSize.x + _spacing.x)); // セルが完全に見える最大行数
            maxFixedRow = maxDisplayRomNumComp % 2 == 0 ? (maxDisplayRomNumComp - 2) / 2 : (maxDisplayRomNumComp - 1) / 2;
            _leftFocusFixedRow = Mathf.Clamp(_leftFocusFixedRow, -maxFixedRow, 0);
            _rightFocusFixedRow = Mathf.Clamp(_rightFocusFixedRow, 0, maxFixedRow);

            // スクロール初めのスペース制限
            var maxPaddingLeft = _viewportWidth / 2 - _cellSize.x / 2 + (_cellSize.x + _spacing.x) * _leftFocusFixedRow;
            _paddingLeft = Mathf.Clamp(_paddingLeft, 0, maxPaddingLeft);

            // スクロール最後のスペース制限
            var minPaddingRight = (_viewportWidth - _cellSize.x) * 1 / 2 - (_cellSize.x + _spacing.x) * _rightFocusFixedRow; // スクロール追従のための最低値
            var maxPaddingRight = minPaddingRight; // 今はまだ未実装
            _paddingRight = Mathf.Clamp(_paddingRight, minPaddingRight, maxPaddingRight);

            // セルの数に応じてCellListの大きさ決定
            var newDelta = cellListRT.sizeDelta;
            allRowNum = Mathf.CeilToInt((float)_allContentsNum / _constraintCount);
            newDelta.x = _paddingLeft - _spacing.x + (_cellSize.x + _spacing.x) * allRowNum + _paddingRight;
            cellListRT.sizeDelta = newDelta;

            // スタート時nowCenterRowNo
            if (_viewportWidth / 2 - _paddingLeft >= _cellSize.x + _spacing.x / 2)
            {
                firstCenterRowNo = Mathf.FloorToInt(((_viewportWidth / 2 - _paddingLeft) - (_cellSize.x + _spacing.x / 2)) / (_cellSize.x + _spacing.x)) + 1;
            }
            nowCenterRowNo = firstCenterRowNo;

            // スタート時normalizedRowNo
            firstNormalizedRowNo = 0 - firstCenterRowNo;
            nowNormalizedRowNo = firstNormalizedRowNo;

            // セルを最大表示可能数作ってindexと変数コンポーネント割り振る。
            // セル表示時に設定したいコンポーネントを予めキャッシュしたい時は、ここに宣言してrecycleDict, tempDict, SetDataを修正する。
            maxDisplayCellNum = maxDisplayRowNum * _constraintCount;
            recycleArray = new (GameObject go, RectTransform rectT, Image image, Button button, Image image_0, Text text_1)[maxDisplayCellNum];
            var cellDelta = new Vector2(_cellSize.x, _cellSize.y);
            for (int i = 0; i < maxDisplayCellNum; ++i)
            {
                var go = Instantiate(_cellPrefab, cellListRT);
                var rectT = go.GetComponent<RectTransform>();
                rectT.sizeDelta = cellDelta;
                
                var image = rectT.GetComponent<Image>();
                var button = rectT.GetComponent<Button>();
                var image_0 = rectT.GetChild(0).GetComponent<Image>();
                var text_1 = rectT.GetChild(1).GetComponent<Text>();
                recycleArray[i] = (go, rectT, image, button, image_0, text_1);
            }

            RedrawAll();
            Invoke(nameof(FocusFirstCell), 0.05f); // EventSystem.SetSelectedあるあるのずれ、なぜか一瞬ずらさないとフォーカスが外れる。
        }

        // 最初のセルにフォーカスする
        private void FocusFirstCell()
        {
            int targetIndex = _focusedCellNo - nowCenterRowNo * _constraintCount + (maxDisplayRowNum / 2) * _constraintCount;
            EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
        }

    #endregion


    #region 指定のセルまでフォーカス移動

        /// <summary>
        /// 指定したセルにフォーカスを移動させる
        /// </summary>
        override public async void JumpTo(int jumpNo)
        {
            // スクロールの移動中にやるとバグる。
            isScrollChasingFocus = false;

            if (jumpNo < 0) jumpNo = 0;
            if (jumpNo >= _allContentsNum) jumpNo = _allContentsNum - 1;
            if (jumpNo == _focusedCellNo) return;

            // NowNormalizedRowNo指定
            if (jumpNo / _constraintCount < firstCenterRowNo) // int ÷ int は端数切捨て、行番号で判別
            {
                nowNormalizedRowNo = jumpNo / _constraintCount - firstCenterRowNo;
            }
            else if (jumpNo / _constraintCount > (_allContentsNum - 1) / _constraintCount - _rightFocusFixedRow)
            {
                nowNormalizedRowNo = jumpNo / _constraintCount - ((_allContentsNum - 1) / _constraintCount - _rightFocusFixedRow);
            }
            else nowNormalizedRowNo = 0;

            // 移動先のセルまでRectTransform移動
            var newLocalPositionX = 0f;
            if (jumpNo < _focusedCellNo) // Leftスクロール
            {
                newLocalPositionX = - (_paddingLeft + (_cellSize.x + _spacing.x) * (jumpNo / _constraintCount) - _viewportWidth / 2 + _cellSize.x / 2);
            }
            if (jumpNo > _focusedCellNo) // Rightスクロール
            {
                newLocalPositionX = - (_paddingLeft + (_cellSize.x + _spacing.x) * (jumpNo / _constraintCount) - _viewportWidth / 2 + _cellSize.x / 2);
            }
            if (newLocalPositionX > 0) newLocalPositionX = 0;
            var newRectLocalPosition = cellListRT.localPosition;
            newRectLocalPosition.x = newLocalPositionX;
            cellListRT.localPosition = newRectLocalPosition;
            
            // フォーカス指定
            _focusedCellNo = jumpNo;
            await Task.Delay(50); // SetSelectedあるあるのバグ、少し遅らせないとNullになる。
            int targetIndex = _focusedCellNo - nowCenterRowNo * _constraintCount + (maxDisplayRowNum / 2) * _constraintCount;
            EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
        }

    #endregion


        private bool onceCautioned;
        private void Update()
        {
#if UNITY_EDITOR
            if (onceCautioned) return;
            if (_isScrollKeyControllable && ScrollKeyInput == null && !onceCautioned)
            {
                onceCautioned = true;
                throw new InvalidOperationException("ScrollKeyInputにインターフェースをセットするか、ScrollKeyを無効にしてください");
            }
            if (_isFocusKeyControllable && FocusKeyInput == null && !onceCautioned)
            {
                onceCautioned = true;
                throw new InvalidOperationException("FocusKeyInputにインターフェースをセットするか、FocusKeyを無効にしてください");
            }
#endif

            if (_isScrollKeyControllable && !_isScrollKeySuspended)
            {
                ScrollKeyActivate();
                ChaseScroll();
            }

            if (_isFocusKeyControllable && !_isFocusKeySuspended)
            {
                FocusKeyActivate();
                FocusKeyUINavi();
                ChaseFocus();
            }
        }


    #region セル再利用

        private void RecycleCell(Vector2 pos)
        {
            // 今中心にいる行は何行目か。これの変化でセルの再利用を起動。
            nowCenterRowNo = Mathf.FloorToInt(((-cellListRT.localPosition.x + _viewportWidth / 2) - (_paddingLeft - _spacing.x / 2)) / (_cellSize.x + _spacing.x));

            // スクロールで中心の行が変化
            if (nowCenterRowNo != preCenterRowNo)
            {
                RedrawAll();
            }

            preCenterRowNo = nowCenterRowNo;
        }

        /// <summary>
        /// セルを全部再描写する。
        /// </summary>
        private void RedrawAll()
        {
            // 表示するセル番号
            int drawCellNo = (nowCenterRowNo - (maxDisplayRowNum - 1) / 2) * _constraintCount;

            for (int i = 0; i < maxDisplayCellNum; ++i)
            {
                // 順に配置
                var rectTLocalPosition = recycleArray[i].rectT.localPosition;
                rectTLocalPosition.x = _paddingLeft + (_cellSize.x + _spacing.x) * (drawCellNo / _constraintCount);
                rectTLocalPosition.y = -(_paddingTop + (_cellSize.y + _spacing.y) * (drawCellNo % _constraintCount));
                recycleArray[i].rectT.localPosition = rectTLocalPosition;

                // コンテンツ表示範囲外は非表示。
                recycleArray[i].go.SetActive(0 <= drawCellNo && drawCellNo < _allContentsNum);

                // 表示内容を変更
                if (0 <= drawCellNo && drawCellNo < _allContentsNum)
                {
                    DataSource.SetCellData(drawCellNo, recycleArray[i].image, recycleArray[i].button, recycleArray[i].image_0, recycleArray[i].text_1);
                }

                drawCellNo += 1;
            }
        }

    #endregion


    #region キー判定

        /// <summary>
        /// スクロールキー判定
        /// </summary>
        private void ScrollKeyActivate()
        {
            if (ScrollKeyInput.GetScrollKeys().ScrollUpOrLeftKey)
            {
                isScrollLeftKeyActive = true;
                isScrollRightKeyActive = false;
                return;
            }

            if (ScrollKeyInput.GetScrollKeys().ScrollDownOrRightKey)
            {
                isScrollLeftKeyActive = false;
                isScrollRightKeyActive = true;
                return;
            }

            isScrollLeftKeyActive = false;
            isScrollRightKeyActive = false;
        }

        /// <summary>
        /// フォーカスキー判定
        /// </summary>
        private void FocusKeyActivate()
        {
            // スクロールキー操作中はフォーカスキー無効
            if (isScrollLeftKeyActive || isScrollRightKeyActive) return;

            if (intervalTime > 0)
            {
                intervalTime -= Time.deltaTime;
                isLeftKeyActive = false;
                isRightKeyActive = false;
                isUpKeyActive = false;
                isDownKeyActive = false;
                if (isDelayOn)
                {
                    isDelayOn = false;
                }
            }

            if (delayDecreaseTime < _repeatStartDelay * _delayDecreasingTime)

            {
                delayDecreaseTime += Time.deltaTime;
            }

            if (_isHoldRepeatable)
            {
                WaitInterval(_repeatInterval);
            }
            else
            {
                WaitInterval(1000);
            }
        }

        /// <summary>
        /// インターバル秒だけ長押しを無効
        /// </summary>
        private void WaitInterval(float interval)
        {
            if (FocusKeyInput.GetFocusKeys().LeftKey)
            {
                if (intervalTime <= 0) // インターバル終わってるとき
                {
                    isLeftKeyActive = true;
                    intervalTime = interval;
                    if (isDelayOn)
                    {
                        intervalTime += _repeatStartDelay;
                    }

                    if (_isDelayDecreasing)
                    {
                        intervalTime = LinerDecrease(delayDecreaseTime, _repeatStartDelay * _delayDecreasingTime, interval + _repeatStartDelay, interval);
                    }
                }
            }
            else if (FocusKeyInput.GetFocusKeys().RightKey)
            {
                if (intervalTime <= 0)
                {
                    isRightKeyActive = true;
                    intervalTime = interval;
                    if (isDelayOn)
                    {
                        intervalTime += _repeatStartDelay;
                    }

                    if (_isDelayDecreasing)
                    {
                        intervalTime = LinerDecrease(delayDecreaseTime, _repeatStartDelay * _delayDecreasingTime, interval + _repeatStartDelay, interval);
                    }
                }
            }
            else if (FocusKeyInput.GetFocusKeys().UpKey)
            {
                if (intervalTime <= 0)
                {
                    isUpKeyActive = true;
                    intervalTime = interval;
                    if (isDelayOn)
                    {
                        intervalTime += _repeatStartDelay;
                    }

                    if (_isDelayDecreasing)
                    {
                        intervalTime = LinerDecrease(delayDecreaseTime, _repeatStartDelay * _delayDecreasingTime, interval + _repeatStartDelay, interval);
                    }
                }
            }
            else if (FocusKeyInput.GetFocusKeys().DownKey)
            {
                if (intervalTime <= 0)
                {
                    isDownKeyActive = true;
                    intervalTime = interval;
                    if (isDelayOn)
                    {
                        intervalTime += _repeatStartDelay;
                    }

                    if (_isDelayDecreasing)
                    {
                        intervalTime = LinerDecrease(delayDecreaseTime, _repeatStartDelay * _delayDecreasingTime, interval + _repeatStartDelay, interval);
                    }
                }
            }
            else // 何も押されていなけば連続単押しも有効
            {
                intervalTime = 0;
                if (_repeatStartDelay == 0)
                {
                    isDelayOn = false;
                    _isDelayDecreasing = false;
                }
                else
                {
                    isDelayOn = true;
                }

                delayDecreaseTime = 0;
            }
        }

    #endregion


    #region フォーカスがスクロールのキー操作移動を追従

        private void ChaseScroll()
        {
            if (isScrollLeftKeyActive)
            {
                // スクロールが最左部まで来たら一番左のセルまでフォーカスを移動
                var nowFocusedConstraint = _focusedCellNo % _constraintCount;
                if (scrollRect.normalizedPosition.x <= 0)
                {
                    var newPos = cellListRT.localPosition;
                    newPos.x = 0;
                    cellListRT.localPosition = newPos;
                    
                    _focusedCellNo = nowFocusedConstraint;
                    nowNormalizedRowNo = firstNormalizedRowNo;
                    int targetIndex = _focusedCellNo - nowCenterRowNo * _constraintCount + (maxDisplayRowNum / 2) * _constraintCount;
                    EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
                    return;
                }

                // フォーカスは再描写時に自動で勝手に左に移動する
                _focusedCellNo = nowCenterRowNo * _constraintCount + nowFocusedConstraint + nowNormalizedRowNo * _constraintCount;
                
                // スクロール移動
                var newRectLocalPosition = cellListRT.localPosition;
                newRectLocalPosition.x += 2500 * Time.deltaTime * _scrollKeySpeed;
                cellListRT.localPosition = newRectLocalPosition;
                ScrollKeyMoved?.WhenScrollMoved();
            }


            if (isScrollRightKeyActive)
            {
                // スクロールが最右部まで来たら一番最後のセルまでフォーカスを移動
                if (scrollRect.normalizedPosition.x >= 1) 
                {
                    _focusedCellNo = _allContentsNum - 1;
                    nowNormalizedRowNo = _rightFocusFixedRow;
                    int targetIndex = _focusedCellNo - nowCenterRowNo * _constraintCount + (maxDisplayRowNum / 2) * _constraintCount;
                    EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
                    return;
                }
                // フォーカス移動
                if (nowNormalizedRowNo < _leftFocusFixedRow) // 再描写が生じないので、フォーカスを手動でFixedRowまで移動(スクロールの一番左最初のみ)
                {
                    _focusedCellNo += _constraintCount;
                    nowNormalizedRowNo += 1;
                    int targetIndex = _focusedCellNo - nowCenterRowNo * _constraintCount + (maxDisplayRowNum / 2) * _constraintCount;
                    EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
                }
                else // フォーカスは再描写時に自動で勝手に右に移動する
                {
                    var nowFocusedConstraint = _focusedCellNo % _constraintCount;
                    _focusedCellNo = nowCenterRowNo * _constraintCount + nowFocusedConstraint + nowNormalizedRowNo * _constraintCount;
                }

                if (_focusedCellNo >= _allContentsNum) // focusedCellNoの移動がセルの表示より先に行かないように戻す
                {
                    _focusedCellNo = _allContentsNum - 1;
                    int targetIndex = _focusedCellNo - nowCenterRowNo * _constraintCount + (maxDisplayRowNum / 2) * _constraintCount;
                    EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
                }

                // スクロール移動
                var newRectLocalPosition = cellListRT.localPosition;
                newRectLocalPosition.x -= 2500 * Time.deltaTime * _scrollKeySpeed;
                cellListRT.localPosition = newRectLocalPosition;
                ScrollKeyMoved?.WhenScrollMoved();
            }
        }

    #endregion


    #region スクロールがフォーカスのキー操作移動を追従

        #region UINavigation設定

        /// <summary>
        /// 十字キー操作をカウントして、Focusキー操作のUINavigationを疑似的に設定する
        /// </summary>
        private void FocusKeyUINavi()
        {
            preFocusedCellNo = _focusedCellNo;

            if (isLeftKeyActive)
            {
                if (_focusedCellNo >= _constraintCount)
                {
                    _focusedCellNo -= _constraintCount;
                }
            }

            if (isRightKeyActive)
            {
                if (_focusedCellNo < _allContentsNum - _constraintCount)
                {
                    _focusedCellNo += _constraintCount;
                }
            }

            if (isUpKeyActive)
            {
                if (_focusedCellNo % _constraintCount != 0)
                {
                    _focusedCellNo -= 1;
                }
            }

            if (isDownKeyActive)
            {
                if (_focusedCellNo < _allContentsNum - 1 && _focusedCellNo % _constraintCount != _constraintCount - 1)
                {
                    _focusedCellNo += 1;
                }
            }

            // focusedCellNoの移動がセルの表示より先に行かないように戻す
            if (!(nowCenterRowNo * _constraintCount - (maxDisplayCellNum - _constraintCount) / 2 <= _focusedCellNo &&
                  _focusedCellNo < (nowCenterRowNo + 1) * _constraintCount + (maxDisplayCellNum - _constraintCount) / 2))
            {
                _focusedCellNo = preFocusedCellNo;
            }

            if (_focusedCellNo != preFocusedCellNo)
            {
                FocusKeyMoved?.WhenFocusMoved();
            }
        }

        #endregion

        #region フォーカス追従
        private void ChaseFocus()
        {
            if (isLeftKeyActive)
            {
                int targetIndex = _focusedCellNo - nowCenterRowNo * _constraintCount + (maxDisplayRowNum / 2) * _constraintCount;
                if (nowNormalizedRowNo > _leftFocusFixedRow)
                {
                    nowNormalizedRowNo -= 1;
                    EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
                    if (nowNormalizedRowNo != _leftFocusFixedRow) return;
                }

                oldLocalPosX = cellListRT.localPosition.x;
                // フォーカスするセルが、指定したnowNormalizedRowNo行になるようにスクロール量決定
                var focusTargetLocalPos = -recycleArray[targetIndex].rectT.localPosition;
                newLocalPosX = focusTargetLocalPos.x - _cellSize.x * 1 / 2 + (_viewportWidth * 1 / 2 + (_cellSize.x + _spacing.x) * _leftFocusFixedRow);

                // 次のスクロールでnowCenerRowNoが変わらない(再描写が生じない)なら、フォーカスを手動で移動(スクロールの一番左最初のみ)
                if (newLocalPosX > 0)
                {
                    var nextCeterRowNo = Mathf.FloorToInt(((-cellListRT.localPosition.x + oldLocalPosX + _viewportWidth / 2) - (_paddingLeft - _spacing.x / 2)) / (_cellSize.x + _spacing.x));
                    if (nowCenterRowNo == nextCeterRowNo)
                    {
                        EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
                        if (nowNormalizedRowNo > firstNormalizedRowNo) nowNormalizedRowNo -= 1;
                    }
                }
                else { /* フォーカスは再描写時に自動で勝手に上に移動する */}

                isScrollChasingFocus = true;
                elapsedTime = 0f;
            }


            if (isRightKeyActive)
            {
                if (preFocusedCellNo >= _allContentsNum - _constraintCount) return;

                int targetIndex = _focusedCellNo - nowCenterRowNo * _constraintCount + (maxDisplayRowNum / 2) * _constraintCount;
                if (nowNormalizedRowNo < _rightFocusFixedRow)
                {
                    nowNormalizedRowNo += 1;
                    EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
                    if (nowNormalizedRowNo != _rightFocusFixedRow) return;
                }

                oldLocalPosX = cellListRT.localPosition.x;
                // フォーカスするセルが、指定したnowNormalizedRowNo行になるようにスクロール量決定
                var focusTargetLocalPos = -recycleArray[targetIndex].rectT.localPosition;
                newLocalPosX = focusTargetLocalPos.x - _cellSize.x * 1 / 2 + (_viewportWidth * 1 / 2 + (_cellSize.x + _spacing.x) * _rightFocusFixedRow);

                // 次のスクロールでnowCenerRowNoが変わらない(再描写が生じない)なら、フォーカスを手動で移動(スクロールの一番左最初のみ)
                var nextCeterRowNo = Mathf.FloorToInt(((-cellListRT.localPosition.x - (newLocalPosX - oldLocalPosX) + _viewportWidth / 2) - (_paddingLeft - _spacing.x / 2)) / (_cellSize.x + _spacing.x));
                if (nowCenterRowNo == nextCeterRowNo)
                {
                    EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
                    if (nowNormalizedRowNo < _rightFocusFixedRow) nowNormalizedRowNo += 1;
                }
                else { /* フォーカスは再描写時に自動で勝手に下に移動する */}

                isScrollChasingFocus = true;
                elapsedTime = 0f;
            }


            if (isUpKeyActive || isDownKeyActive)
            {
                int targetIndex = _focusedCellNo - nowCenterRowNo * _constraintCount + (maxDisplayRowNum / 2) * _constraintCount;
                EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
            }


            if (isScrollChasingFocus) // 移動中
            {
                elapsedTime += Time.deltaTime;
                // スクロールが終わる前に逆方向のキーを押すとフォーカスがバグるの防止。ただし一瞬だけフォーカスがずれる。
                if (_focusedCellNo / _constraintCount - nowNormalizedRowNo == nowCenterRowNo)
                {
                    int targetIndex = _focusedCellNo - nowCenterRowNo * _constraintCount + (maxDisplayRowNum / 2) * _constraintCount;
                    EventSystem.current.SetSelectedGameObject(recycleArray[targetIndex].go);
                }

                if (elapsedTime > 1.06f - 0.99f * _scrollChaseSpeed)
                {
                    elapsedTime = 0f;
                    isScrollChasingFocus = false;
                }
                else
                {
                    var localPos = cellListRT.localPosition;
                    localPos.x = -CubicOut(elapsedTime, 1.06f - 0.99f * _scrollChaseSpeed, -oldLocalPosX, -newLocalPosX);
                    cellListRT.localPosition = localPos;
                }
            }
        }
        #endregion

    #endregion


    #region Ease計算

        /// <summary>
        /// timeが進行時間、totalTimeが目標の時間、startValueが開始値、endValueが目標値
        /// </summary>
        private float CubicOut(float time, float totalTime, float startValue, float endValue)
        {
            endValue -= startValue;
            time = time / totalTime - 1;
            var result = endValue * (time * time * time + 1) + startValue;
            return result <= 0 ? 0 : result;
        }

        /// <summary>
        /// timeが進行時間、totalTimeが目標の時間、startValueが開始値、endValueが目標値
        /// </summary>
        private float LinerDecrease(float time, float totalTime, float startValue, float endValue)
        {
            return startValue - (startValue - endValue) / totalTime * time;
        }

    #endregion

    }
}
