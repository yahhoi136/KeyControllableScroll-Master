using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class HorizontalKCSSample : MonoBehaviour, ISampleCellHorizontalKCSDataSource, ISampleCellHorizontalKCSScrollKeyInput, ISampleCellHorizontalKCSFocusKeyInput, ISampleCellHorizontalKCSScrollKeyMoved, ISampleCellHorizontalKCSFocusKeyMoved
    {
        [SerializeField] private SampleCellHorizontalKCS kcs;

        // インターフェースのセット、Start() より前に。
        private void Awake()
        {
            kcs.DataSource = this;
            kcs.ScrollKeyInput = this;
            kcs.ScrollKeyMoved = this;
            kcs.FocusKeyInput = this;
            kcs.FocusKeyMoved = this;
        }

        // IDataSource の実装
        public void SetCellData(int index, Image image, Button button, Image image_0, Text text_1)
        {
            text_1.text = $"{index}";
        }

        // IScrollKeyInput の実装
        public (bool ScrollUpOrLeftKey, bool ScrollDownOrRightKey) GetScrollKeys()
        {
            var scrollLeftKey = Input.GetKey(KeyCode.A);
            var scrollRightKey = Input.GetKey(KeyCode.D);

            return (ScrollUpOrLeftKey: scrollLeftKey, ScrollDownOrRightKey: scrollRightKey);
        }

        // IFocusKeyInput の実装
        public (bool UpKey, bool DownKey, bool LeftKey, bool RightKey) GetFocusKeys()
        {
            var upKey = Input.GetKey(KeyCode.UpArrow);
            var downKey = Input.GetKey(KeyCode.DownArrow);
            var leftKey = Input.GetKey(KeyCode.LeftArrow);
            var rightKey = Input.GetKey(KeyCode.RightArrow);

            return (UpKey: upKey, DownKey: downKey, LeftKey: leftKey, RightKey: rightKey);
        }

        // IScrollMoved の実装
        public void WhenScrollMoved()
        {
            // SE流すとか
            // print("scrollMoved");
        }

        // IFocusMoved の実装
        public void WhenFocusMoved()
        {
            // SE流すとか
            // print("focusMoved");
        }

    }
}