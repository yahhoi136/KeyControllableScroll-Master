using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    public class KCSCreateMenu
    {
        [MenuItem("GameObject/UI/KCS/Horizontal KCS Framework", false, 2031)]
        static public void AddHorizontalKeCS(MenuCommand menuCommand)
        {
            GameObject go = CreateHorizontalKCSObjcet();
            PlaceUI(go, menuCommand);
            
        }
    
        [MenuItem("GameObject/UI/KCS/Vertical KCS Framework", false, 2031)]
        static public void AddVerticalKCS(MenuCommand menuCommand)
        {
            GameObject go = CreateVerticalKCSObject();
            PlaceUI(go, menuCommand);
        }


#region CreateObject

        public static GameObject CreateHorizontalKCSObjcet()
        {
            // オブジェクトセット
            GameObject root = CreateRootUI("HorizontalKCS", new Vector2(300, 300));
            GameObject viewport = CreateChildUI("Viewport", root);
            GameObject cellList = CreateChildUI("CellList", viewport);
            GameObject scrollbarHorizontal = CreateChildUI("Scrollbar Horizontal", root);
            GameObject slidingArea = CreateChildUI("Sliding Area", scrollbarHorizontal);
            GameObject handle = CreateChildUI("Handle", slidingArea);

            // RTセット
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = new Vector2(0, 0);
            viewportRT.anchorMax = new Vector2(1, 1);
            viewportRT.pivot = new Vector2(0, 1);
            viewportRT.offsetMin = new Vector2(0, 0);
            viewportRT.offsetMax = new Vector2(0, 0);

            RectTransform cellListRT = cellList.GetComponent<RectTransform>();
            cellListRT.anchorMin = new Vector2(0, 0);
            cellListRT.anchorMax = new Vector2(0, 1);
            cellListRT.pivot = new Vector2(0, 1);
            cellListRT.offsetMin = new Vector2(0, 0);
            cellListRT.offsetMax = new Vector2(0, 0);
            cellListRT.sizeDelta = new Vector2(0, 0);

            RectTransform scrollbarHorizontalRT = scrollbarHorizontal.GetComponent<RectTransform>();
            scrollbarHorizontalRT.anchorMin = new Vector2(0, 0);
            scrollbarHorizontalRT.anchorMax = new Vector2(1, 0);
            scrollbarHorizontalRT.pivot = new Vector2(0, 0);
            scrollbarHorizontalRT.offsetMin = new Vector2(5, 0);
            scrollbarHorizontalRT.sizeDelta = new Vector2(-10, 20);
            
            RectTransform slidingAreaRT = slidingArea.GetComponent<RectTransform>();
            slidingAreaRT.anchorMin = new Vector2(0, 0);
            slidingAreaRT.anchorMax = new Vector2(1, 1);
            slidingAreaRT.pivot = new Vector2(0.5f, 0.5f);
            slidingAreaRT.offsetMin = new Vector2(10, 10);
            slidingAreaRT.offsetMax = new Vector2(-10, -10);
            
            RectTransform handleRT = handle.GetComponent<RectTransform>();
            handleRT.anchorMin = new Vector2(0, 0);
            handleRT.anchorMax = new Vector2(1, 1);
            handleRT.pivot = new Vector2(0.5f, 0.5f);
            handleRT.offsetMin = new Vector2(-10, -10);
            handleRT.offsetMax = new Vector2(10, 10);

            // コンポーネントセット(子から親に)
            handle.AddComponent<CanvasRenderer>();
            var handleImg = handle.AddComponent<Image>();
            handleImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            handleImg.raycastTarget = false;
            handleImg.type = Image.Type.Sliced;

            scrollbarHorizontal.AddComponent<CanvasRenderer>();
            var scrollbarHorizontalImg = scrollbarHorizontal.AddComponent<Image>();
            scrollbarHorizontalImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            scrollbarHorizontalImg.color = new Color32(110, 120, 120, 255);
            scrollbarHorizontalImg.raycastTarget = false;
            scrollbarHorizontalImg.type = Image.Type.Sliced;
            var scrollbarHorizontalScrollbar = scrollbarHorizontal.AddComponent<Scrollbar>();
            scrollbarHorizontalScrollbar.targetGraphic = handleImg;
            var navi = new Navigation();
            navi.mode = Navigation.Mode.None;
            scrollbarHorizontalScrollbar.navigation = navi;
            scrollbarHorizontalScrollbar.handleRect = handleRT;
            scrollbarHorizontalScrollbar.direction = Scrollbar.Direction.LeftToRight;
            scrollbarHorizontalScrollbar.value = 1;
            scrollbarHorizontalScrollbar.size = 1;

            viewport.AddComponent<CanvasRenderer>();
            var viewportImg = viewport.AddComponent<Image>();
            viewportImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            viewportImg.color = new Color32(255, 255, 255, 65);
            viewportImg.raycastTarget = false;
            viewportImg.type = Image.Type.Sliced;
            viewport.AddComponent<RectMask2D>();

            root.AddComponent<CanvasRenderer>();
            var rootScrollRect = root.AddComponent<ScrollRect>();
            rootScrollRect.content = cellListRT;
            rootScrollRect.vertical = false;
            rootScrollRect.movementType = ScrollRect.MovementType.Clamped;
            rootScrollRect.viewport = viewportRT;
            rootScrollRect.horizontalScrollbar = scrollbarHorizontalScrollbar;
            rootScrollRect.horizontalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            var rootCG = root.AddComponent<CanvasGroup>();
            rootCG.blocksRaycasts = false;

            return root;
        }

        public static GameObject CreateVerticalKCSObject()
        {
            // オブジェクトセット
            GameObject root = CreateRootUI("VerticalKCS", new Vector2(300, 300));
            GameObject viewport = CreateChildUI("Viewport", root);
            GameObject cellList = CreateChildUI("CellList", viewport);
            GameObject scrollbarVertical = CreateChildUI("Scrollbar Vertical", root);
            GameObject slidingArea = CreateChildUI("Sliding Area", scrollbarVertical);
            GameObject handle = CreateChildUI("Handle", slidingArea);

            // RTセット
            RectTransform viewportRT = viewport.GetComponent<RectTransform>();
            viewportRT.anchorMin = new Vector2(0, 0);
            viewportRT.anchorMax = new Vector2(1, 1);
            viewportRT.pivot = new Vector2(0, 1);
            viewportRT.offsetMin = new Vector2(0, 0);
            viewportRT.offsetMax = new Vector2(0, 0);

            RectTransform cellListRT = cellList.GetComponent<RectTransform>();
            cellListRT.anchorMin = new Vector2(0, 1);
            cellListRT.anchorMax = new Vector2(1, 1);
            cellListRT.pivot = new Vector2(0, 1);
            cellListRT.offsetMin = new Vector2(0, 0);
            cellListRT.offsetMax = new Vector2(0, 0);
            cellListRT.sizeDelta = new Vector2(0, 0);

            RectTransform scrollbarVerticalRT = scrollbarVertical.GetComponent<RectTransform>();
            scrollbarVerticalRT.anchorMin = new Vector2(1, 0);
            scrollbarVerticalRT.anchorMax = new Vector2(1, 1);
            scrollbarVerticalRT.pivot = new Vector2(1, 1);
            scrollbarVerticalRT.offsetMax = new Vector2(0, -5);
            scrollbarVerticalRT.sizeDelta = new Vector2(20, -10);
            
            RectTransform slidingAreaRT = slidingArea.GetComponent<RectTransform>();
            slidingAreaRT.anchorMin = new Vector2(0, 0);
            slidingAreaRT.anchorMax = new Vector2(1, 1);
            slidingAreaRT.pivot = new Vector2(0.5f, 0.5f);
            slidingAreaRT.offsetMin = new Vector2(10, 10);
            slidingAreaRT.offsetMax = new Vector2(-10, -10);
            
            RectTransform handleRT = handle.GetComponent<RectTransform>();
            handleRT.anchorMin = new Vector2(0, 0);
            handleRT.anchorMax = new Vector2(1, 1);
            handleRT.pivot = new Vector2(0.5f, 0.5f);
            handleRT.offsetMin = new Vector2(-10, -10);
            handleRT.offsetMax = new Vector2(10, 10);

            // コンポーネントセット(子から親に)
            handle.AddComponent<CanvasRenderer>();
            var handleImg = handle.AddComponent<Image>();
            handleImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            handleImg.raycastTarget = false;
            handleImg.type = Image.Type.Sliced;

            scrollbarVertical.AddComponent<CanvasRenderer>();
            var scrollbarVerticalImg = scrollbarVertical.AddComponent<Image>();
            scrollbarVerticalImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            scrollbarVerticalImg.color = new Color32(110, 120, 120, 255);
            scrollbarVerticalImg.raycastTarget = false;
            scrollbarVerticalImg.type = Image.Type.Sliced;
            var scrollbarVerticalScrollbar = scrollbarVertical.AddComponent<Scrollbar>();
            scrollbarVerticalScrollbar.targetGraphic = handleImg;
            var navi = new Navigation();
            navi.mode = Navigation.Mode.None;
            scrollbarVerticalScrollbar.navigation = navi;
            scrollbarVerticalScrollbar.handleRect = handleRT;
            scrollbarVerticalScrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbarVerticalScrollbar.value = 1;
            scrollbarVerticalScrollbar.size = 1;

            viewport.AddComponent<CanvasRenderer>();
            var viewportImg = viewport.AddComponent<Image>();
            viewportImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            viewportImg.color = new Color32(255, 255, 255, 65);
            viewportImg.raycastTarget = false;
            viewportImg.type = Image.Type.Sliced;
            viewport.AddComponent<RectMask2D>();

            root.AddComponent<CanvasRenderer>();
            var rootScrollRect = root.AddComponent<ScrollRect>();
            rootScrollRect.content = cellListRT;
            rootScrollRect.horizontal = false;
            rootScrollRect.movementType = ScrollRect.MovementType.Clamped;
            rootScrollRect.viewport = viewportRT;
            rootScrollRect.verticalScrollbar = scrollbarVerticalScrollbar;
            rootScrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
            var rootCG = root.AddComponent<CanvasGroup>();
            rootCG.blocksRaycasts = false;

            return root;
        }

        private static GameObject CreateRootUI(string name, Vector2 size)
        {
            GameObject child = new GameObject(name);
            RectTransform rectTransform = child.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size;
            return child;
        }

        static GameObject CreateChildUI(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.AddComponent<RectTransform>();
            if (parent is not null)
            {
                go.transform.SetParent(parent.transform, false); 
                SetLayerRecursively(go, parent.layer); // 自分以下の階層の全てのオブジェクトに親と同じレイヤーを設定
            }
            return go;
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++) // 再帰的にしてより下位のものを全て網羅。
                SetLayerRecursively(t.GetChild(i).gameObject, layer);
        }

#endregion


#region PlaceUI()
        private static void PlaceUI(GameObject element, MenuCommand menuCommand)
        {
            // menuCommand.contextでメニューコマンドのターゲット(選択しているオブジェクトとか)を返す。
            // ここでは、右クリでCreateした時に右クリ選択していたHierarchy上のオブジェクトを返す(左クリ選択ではNULLになる)
            GameObject parent = menuCommand.context as GameObject;
            // 未選択か、自身かそれ以上の階層にCanvasがない時
            if (parent is null || parent.GetComponentInParent<Canvas>() is null) // このGetComponentInParent()は自身以上の階層なら何階上でもOK
            {
                // Canvasを持つ別のActiveなオブジェクトを親に、それもない場合Canvasを作って親に
                Canvas canvas = Object.FindObjectOfType(typeof(Canvas)) as Canvas;
                parent = canvas is not null && canvas.gameObject.activeInHierarchy ? canvas.gameObject : CreateNewCanvas();
            }

            // ユニークネームに
            string uniqueName = GameObjectUtility.GetUniqueNameForSibling(parent.transform, element.name);
            element.name = uniqueName;
            // シーン上でオブジェクトを追加時、親を変更時に保存可能に(*マークの出現)
            Undo.RegisterCreatedObjectUndo(element, "Create " + element.name);
            Undo.SetTransformParent(element.transform, parent.transform, "Parent " + element.name);
            // 子を親と同じレイヤーと位置に
            GameObjectUtility.SetParentAndAlign(element, parent);
            // 作ったオブジェクトを現在選択中に
            Selection.activeGameObject = element; 
        }

        static public GameObject CreateNewCanvas()
        {
            var root = new GameObject("Canvas");
            root.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = root.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            root.AddComponent<CanvasScaler>();
            root.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(root, "Create " + root.name);

            return root;
        }
#endregion


    }
}