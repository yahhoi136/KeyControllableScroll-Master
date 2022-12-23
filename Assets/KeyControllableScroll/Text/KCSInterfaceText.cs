
namespace KCSScriptGenerator
{
    public class KCSInterfaceText
    {
        public static string GenerateText(string kcsName, string dataSourceArgument)
        {
            // @"～～"とすることで、複数行を書ける。「"」は「""」として書く
            string interfaceCode = @"
namespace UnityEngine.UI
{
    public interface I" + kcsName + @"DataSource
    {
        /// <summary>
        /// 表示されるセルの内容を設定。１つずつ表示されるたびに呼ばれる。
        /// </summary>
        /// <param name=""index"">表示されるセルのコンテンツ番号</param>
        void SetCellData(int index" + dataSourceArgument + @");
    }


    public interface I" + kcsName + @"FocusKeyInput
    {
        /// <summary>
        /// 長押しで連続入力が可能なキーを設定する。フォーカスをキー操作する際には実装必要。
        /// </summary>
        /// <returns></returns>
        (bool UpKey, bool DownKey, bool LeftKey, bool RightKey) GetFocusKeys();
    }


    public interface I" + kcsName + @"ScrollKeyInput
    {
        /// <summary>
        /// 長押しで連続入力が可能なスクロールキーを設定する。スクロールをキーで操作する際には実装必要。
        /// </summary>
        /// <returns></returns>
        (bool ScrollUpOrLeftKey, bool ScrollDownOrRightKey) GetScrollKeys();
    }


    public interface I" + kcsName + @"FocusKeyMoved
    {
        /// <summary>
        /// Focusカーソルが移動した際の追加処理。カーソル移動SEとか
        /// </summary>
        void WhenFocusMoved();
    }


    public interface I" + kcsName + @"ScrollKeyMoved
    {
        /// <summary>
        /// Scrollkeyで移動した際の追加処理。カーソル移動SEとか
        /// </summary>
        void WhenScrollMoved();
    }
}
";

            return interfaceCode;
        }
    }
}
