
namespace UnityEngine.UI
{
    public class KeyControllableScrollBase : MonoBehaviour
    {
    
    #region 変数定義
    // 外部(エディタ拡張)からもアクセスするのでプロパティ
    // Cell Recycle
        /// <summary>
        /// オンにするとセルの数を上下一行ずつ追加して、キーを高速連打した時にフォーカスの移動速度が遅くなるのを改善できる。ただし上下一行のセル分だけ再描写処理が重くなる。
        /// </summary>
        [SerializeField, HideInInspector] protected bool _isSmoothMode = true;
        [SerializeField, HideInInspector] protected GameObject _cellPrefab;
        [SerializeField, HideInInspector] protected float _viewportWidth; // Horizontalだけ
        [SerializeField, HideInInspector] protected float _viewportHeight; // Verticalだけ
        [SerializeField, HideInInspector] protected int _allContentsNum;
        public bool IsSmoothMode { get { return _isSmoothMode; } set { _isSmoothMode = value; } }
        public GameObject CellPrefab { get { return _cellPrefab; } set { _cellPrefab = value; } }
        public float ViewportWidth { get { return _viewportWidth; } set { _viewportWidth = value; } }
        public float ViewportHeight { get { return _viewportHeight; } set { _viewportHeight = value; } }
        public int AllContentsNum { get { return _allContentsNum; } set { _allContentsNum = value; } }


    // GridLayoutGroup param       
        [SerializeField, HideInInspector] protected float _paddingLeft;
        [SerializeField, HideInInspector] protected float _paddingRight;
        [SerializeField, HideInInspector] protected float _paddingTop;
        [SerializeField, HideInInspector] protected float _paddingBottom;
        [SerializeField, HideInInspector] protected Vector2 _cellSize = new Vector2 (100, 100);
        [SerializeField, HideInInspector] protected Vector2 _spacing = new Vector2 (10, 10);
        /// <summary>
        /// 列数
        /// </summary>
        [SerializeField, HideInInspector] protected int _constraintCount = 1;
        public float PaddingLeft { get { return _paddingLeft; } set { _paddingLeft = value; } }
        public float PaddingRight { get { return _paddingRight; } set { _paddingRight = value; } }
        public float PaddingTop { get { return _paddingTop; } set { _paddingTop = value; } }
        public float PaddingBottom { get { return _paddingBottom; } set { _paddingBottom = value; } }
        public Vector2 CellSize { get { return _cellSize; } set { _cellSize = value; } }
        public Vector2 Spacing { get { return _spacing; } set { _spacing = value; } }
        public int ConstraintCount { get { return _constraintCount; } set { _constraintCount = value; } }


    // Scroll Key Control
        /// <summary>
        /// キーボードorコントローラーでスクロール操作が出来るか否か
        /// </summary>
        [SerializeField, HideInInspector] protected bool _isScrollKeyControllable;
        /// <summary>
        /// スクロールの速度(フレームとScrollViewの大きさに非依存)
        /// </summary>
        [SerializeField, HideInInspector] protected float _scrollKeySpeed = 0.5f;
        public bool IsScrollKeyControllable { get { return _isScrollKeyControllable; } set { _isScrollKeyControllable = value; } }
        public float ScrollKeySpeed { get { return _scrollKeySpeed; } set { _scrollKeySpeed = value; } }


    // Focus Key Control
        /// <summary>
        /// キーボードorコントローラーでフォーカス操作が出来るか否か
        /// </summary>
        [SerializeField, HideInInspector] protected bool _isFocusKeyControllable;
        /// <summary>
        /// Scrollがフォーカスを追従する速度、1.06 - 0.99 * scrollChaseSpeed で追従する時間。本来は時間(秒)で設定してたけどscrollKeySpeedと合わせた
        /// </summary>
        [SerializeField, HideInInspector] protected float _scrollChaseSpeed = 0.5f;

        /// <summary>
        /// スクロール時にViewport中心から何行左にフォーカスを固定するか
        /// </summary>
        [SerializeField, HideInInspector] protected int _leftFocusFixedRow; // Horizontalだけ
        /// <summary>
        /// スクロール時にViewport中心から何行右にフォーカスを固定するか
        /// </summary>
        [SerializeField, HideInInspector] protected int _rightFocusFixedRow; // Horizontalだけ
        /// <summary>
        /// スクロール時にViewport中心から何行上にフォーカスを固定するか
        /// </summary>
        [SerializeField, HideInInspector] protected int _upFocusFixedRow; // Verticalだけ
        /// <summary>
        /// スクロール時にViewport中心から何行下にフォーカスを固定するか
        /// </summary>
        [SerializeField, HideInInspector] protected int _downFocusFixedRow; // Verticalだけ
        public bool IsFocusKeyControllable { get { return _isFocusKeyControllable; } set { _isFocusKeyControllable = value; } }
        public float ScrollChaseSpeed { get { return _scrollChaseSpeed; } set { _scrollChaseSpeed = value; } }
        public int LeftFocusFixedRow { get { return _leftFocusFixedRow; } set { _leftFocusFixedRow = value; } }
        public int RightFocusFixedRow { get { return _rightFocusFixedRow; } set { _rightFocusFixedRow = value; } }
        public int UpFocusFixedRow { get { return _upFocusFixedRow; } set { _upFocusFixedRow = value; } }
        public int DownFocusFixedRow { get { return _downFocusFixedRow; } set { _downFocusFixedRow = value; } }


    // Focus Key Repeat
        /// <summary>
        /// 長押しが有効か
        /// </summary>
        [SerializeField, HideInInspector] protected bool _isHoldRepeatable;
        /// <summary>
        /// リピートのインターバル秒。0にするとスクロール止まらないので0より大きく。
        /// </summary>
        [SerializeField, HideInInspector] protected float _repeatInterval = 0.2f;
        /// <summary>
        /// 長押し時にリピートが始まるまでの時間
        /// </summary>
        [SerializeField, HideInInspector] protected float _repeatStartDelay = 0.5f;
        /// <summary>
        /// 長押しリピートの遅延が徐々に早まる
        /// </summary>
        [SerializeField, HideInInspector] protected bool _isDelayDecreasing;
        /// <summary>
        /// 長押しリピートの遅延が完全になくなるまでの時間
        /// </summary>
        [SerializeField, HideInInspector] protected int _delayDecreasingTime = 3;
        public bool IsHoldRepeatable { get { return _isHoldRepeatable; } set { _isHoldRepeatable = value; } }
        public float RepeatInterval { get { return _repeatInterval; } set { _repeatInterval = value; } }
        public float RepeatStartDelay { get { return _repeatStartDelay; } set { _repeatStartDelay = value; } }
        public bool IsDelayDecreasing { get { return _isDelayDecreasing; } set { _isDelayDecreasing = value; } }
        public int DelayDecreasingTime { get { return _delayDecreasingTime; } set { _delayDecreasingTime = value; } }


    // Public Methods & Fields
        [SerializeField, HideInInspector] protected bool _isScrollKeySuspended;
        [SerializeField, HideInInspector] protected bool _isFocusKeySuspended;
        [SerializeField, HideInInspector] protected int _focusedCellNo;
        public bool IsScrollKeySuspended { get { return _isScrollKeySuspended; } set { _isScrollKeySuspended = value; } }
        public bool IsFocusKeySuspended { get { return _isFocusKeySuspended; } set { _isFocusKeySuspended = value; } }
        public int FocusedCellNo { get { return _focusedCellNo; } protected set { _focusedCellNo = value;} }


        // KCSが縦か横か判別
        virtual public bool IsHorizontal => true;
        // Inspectorで折り畳み状況を保持用
        [HideInInspector] public bool isFolding1, isFolding2;

    #endregion


        virtual public void Init()
        {
        }

        virtual public void JumpTo(int jumpNo)
        {
        }

    }
}