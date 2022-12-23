
namespace UnityEngine.UI
{
    public interface ISampleCellHorizontalKCSDataSource
    {
        /// <summary>
        /// 表示されるセルの内容を設定。１つずつ表示されるたびに呼ばれる。
        /// </summary>
        /// <param name="index">表示されるセルのコンテンツ番号</param>
        void SetCellData(int index, Image image, Button button, Image image_0, Text text_1);
    }


    public interface ISampleCellHorizontalKCSFocusKeyInput
    {
        /// <summary>
        /// 長押しで連続入力が可能なキーを設定する。フォーカスをキー操作する際には実装必要。
        /// </summary>
        /// <returns></returns>
        (bool UpKey, bool DownKey, bool LeftKey, bool RightKey) GetFocusKeys();
    }


    public interface ISampleCellHorizontalKCSScrollKeyInput
    {
        /// <summary>
        /// 長押しで連続入力が可能なスクロールキーを設定する。スクロールをキーで操作する際には実装必要。
        /// </summary>
        /// <returns></returns>
        (bool ScrollUpOrLeftKey, bool ScrollDownOrRightKey) GetScrollKeys();
    }


    public interface ISampleCellHorizontalKCSFocusKeyMoved
    {
        /// <summary>
        /// Focusカーソルが移動した際の追加処理。カーソル移動SEとか
        /// </summary>
        void WhenFocusMoved();
    }


    public interface ISampleCellHorizontalKCSScrollKeyMoved
    {
        /// <summary>
        /// Scrollkeyで移動した際の追加処理。カーソル移動SEとか
        /// </summary>
        void WhenScrollMoved();
    }
}
