using UnityEngine;
using UnityEngine.UI;

namespace Demo
{
    public class VerticalKCSSample : MonoBehaviour, ISampleCellVerticalKCSDataSource, ISampleCellVerticalKCSScrollKeyInput, ISampleCellVerticalKCSFocusKeyInput, ISampleCellVerticalKCSScrollKeyMoved, ISampleCellVerticalKCSFocusKeyMoved
    {
        [SerializeField] private SampleCellVerticalKCS kcs;
        
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
            var scrollUpKey = Input.GetKey(KeyCode.W);
            var scrollDownKey = Input.GetKey(KeyCode.S);

            return (ScrollUpOrLeftKey: scrollUpKey, ScrollDownOrRightKey: scrollDownKey);
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


        public void WhenScrollMoved()
        {
            // SE流すとか
            // print("scrollMoved");
        }


        public void WhenFocusMoved()
        {
            // SE流すとか
            // print("focusMoved");
        }


    }
}

