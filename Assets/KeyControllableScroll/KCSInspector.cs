using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

[CustomEditor(typeof(KeyControllableScrollBase), true)]
public class KCSInspector : Editor
{
    private int jumpNo;

    public override void OnInspectorGUI()
    {
        // 変更を検知
        EditorGUI.BeginChangeCheck();

        KeyControllableScrollBase KCS = (KeyControllableScrollBase)target;

        base.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        
        // ランタイム中編集不可
        GUI.enabled = !Application.isPlaying;

        EditorGUILayout.LabelField("Cell Recycle Param", EditorStyles.boldLabel);
        KCS.IsSmoothMode = EditorGUILayout.Toggle("Smooth Mode", KCS.IsSmoothMode);
        KCS.CellPrefab = EditorGUILayout.ObjectField("  Cell Prefab", KCS.CellPrefab, typeof(GameObject), true) as GameObject;
        if (KCS.IsHorizontal)
        {
            KCS.ViewportWidth = EditorGUILayout.FloatField("  Viewport Width", KCS.ViewportWidth);
        }
        if (!KCS.IsHorizontal)
        {
            KCS.ViewportHeight = EditorGUILayout.FloatField("  Viewport Height", KCS.ViewportHeight);
        }
        KCS.AllContentsNum = EditorGUILayout.IntField("  All Contents Num", KCS.AllContentsNum);
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        EditorGUILayout.LabelField("Grid Layout Group Param", EditorStyles.boldLabel);
        if (KCS.isFolding1 = EditorGUILayout.Foldout(KCS.isFolding1, "  Padding"))
        {
            if (KCS.IsHorizontal)
            {
                KCS.PaddingLeft = EditorGUILayout.FloatField("        Left   (Limited)", KCS.PaddingLeft);
                EditorGUILayout.LabelField("        Right   (Fixed)", $" {KCS.PaddingRight}");
                KCS.PaddingTop = EditorGUILayout.FloatField("        Top", KCS.PaddingTop);
                KCS.PaddingBottom = EditorGUILayout.FloatField("        Bottom", KCS.PaddingBottom);
            }
            if (!KCS.IsHorizontal)
            {
                KCS.PaddingLeft = EditorGUILayout.FloatField("        Left", KCS.PaddingLeft);
                KCS.PaddingRight = EditorGUILayout.FloatField("        Right", KCS.PaddingRight);
                KCS.PaddingTop = EditorGUILayout.FloatField("        Top   (Limited)", KCS.PaddingTop);
                EditorGUILayout.LabelField("        Bottom   (Fixed)", $" {KCS.PaddingBottom}");
            }
        }
        KCS.CellSize = EditorGUILayout.Vector2Field("  Cell Size", KCS.CellSize);
        KCS.Spacing = EditorGUILayout.Vector2Field("  Spacing", KCS.Spacing);
        EditorGUILayout.LabelField("  Start Coner", "Upper Left   (Fixed)");
        if (KCS.IsHorizontal) EditorGUILayout.LabelField("  Start Axis", "Vertical   (Fixed)");
        if (!KCS.IsHorizontal) EditorGUILayout.LabelField("  Start Axis", "Horizontal   (Fixed)");
        EditorGUILayout.LabelField("  Child Alignment", "Upper Left   (Fixed)");
        if (KCS.IsHorizontal) EditorGUILayout.LabelField("  Constraint", "Fixed Row Count   (Fixed)");
        if (!KCS.IsHorizontal) EditorGUILayout.LabelField("  Constraint", "Fixed Column Count   (Fixed)");
        KCS.ConstraintCount = EditorGUILayout.IntField("        Constraint Count", KCS.ConstraintCount);
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        // いつでも編集可
        GUI.enabled = true;

        // トグルを太字に
        EditorStyles.label.fontStyle = FontStyle.Bold;
        KCS.IsScrollKeyControllable = EditorGUILayout.Toggle("Scroll KeyControl", KCS.IsScrollKeyControllable);
        EditorStyles.label.fontStyle = FontStyle.Normal;
        if (KCS.IsScrollKeyControllable)
        {
            KCS.ScrollKeySpeed = EditorGUILayout.Slider("  Scroll Speed", KCS.ScrollKeySpeed, 0.1f, 1f);
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        EditorStyles.label.fontStyle = FontStyle.Bold;
        KCS.IsFocusKeyControllable = EditorGUILayout.Toggle("Focus KeyControl", KCS.IsFocusKeyControllable);
        EditorStyles.label.fontStyle = FontStyle.Normal;
        if (KCS.IsFocusKeyControllable)
        {
            GUI.enabled = !Application.isPlaying;
            if (KCS.IsHorizontal)
            {
                // maxFiexedRow決定
                int maxDisplayRomNumComp = Mathf.FloorToInt(KCS.ViewportWidth / (KCS.CellSize.x + KCS.Spacing.x)); // セルが完全に見える最大行数
                int maxFixedRow = maxDisplayRomNumComp % 2 == 0 ? (maxDisplayRomNumComp - 2) / 2 : (maxDisplayRomNumComp - 1) / 2;
                if (maxFixedRow == 0) GUI.enabled = false;
                KCS.LeftFocusFixedRow = EditorGUILayout.IntSlider("  Left Focus Fixed Row", KCS.LeftFocusFixedRow, 0, -maxFixedRow);
                KCS.RightFocusFixedRow = EditorGUILayout.IntSlider("  Right Focus Fixed Row", KCS.RightFocusFixedRow, 0, maxFixedRow);
            }
            if (!KCS.IsHorizontal)
            {
                // maxFiexedRow決定
                int maxDisplayRomNumComp = Mathf.FloorToInt(KCS.ViewportHeight / (KCS.CellSize.y + KCS.Spacing.y)); // セルが完全に見える最大行数
                int maxFixedRow = maxDisplayRomNumComp % 2 == 0 ? (maxDisplayRomNumComp - 2) / 2 : (maxDisplayRomNumComp - 1) / 2;
                if (maxFixedRow == 0) GUI.enabled = false;
                KCS.UpFocusFixedRow = EditorGUILayout.IntSlider("  Up Focus Fixed Row", KCS.UpFocusFixedRow, 0, -maxFixedRow);
                KCS.DownFocusFixedRow = EditorGUILayout.IntSlider("  Down Focus Fixed Row", KCS.DownFocusFixedRow, 0, maxFixedRow);
            }
            GUI.enabled = true;
            
            KCS.ScrollChaseSpeed = EditorGUILayout.Slider("  Scroll Chase Speed", KCS.ScrollChaseSpeed, 0.1f, 1f);
            KCS.IsHoldRepeatable = EditorGUILayout.Toggle("  Key Hold Repeat", KCS.IsHoldRepeatable);
            if (KCS.IsHoldRepeatable)
            {
                KCS.RepeatInterval = EditorGUILayout.Slider("          Repeat Interval", KCS.RepeatInterval, 0.05f, 1f);
                KCS.RepeatStartDelay = EditorGUILayout.Slider("          Repeat Start Delay", KCS.RepeatStartDelay, 0, 1);
                KCS.IsDelayDecreasing = EditorGUILayout.Toggle("          Delay Decreasing", KCS.IsDelayDecreasing);
                if (KCS.IsDelayDecreasing) KCS.DelayDecreasingTime = EditorGUILayout.IntSlider("          Decreasing Time", KCS.DelayDecreasingTime, 1, 5);
            }
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();


        EditorGUILayout.LabelField("Public Methods & Fields", EditorStyles.boldLabel);
        
        // ランタイムのみ
        GUI.enabled = Application.isPlaying;
        KCS.IsScrollKeySuspended = EditorGUILayout.Toggle("Scroll Key Suspended", KCS.IsScrollKeySuspended);
        KCS.IsFocusKeySuspended = EditorGUILayout.Toggle("Focus Key Suspended", KCS.IsFocusKeySuspended);

        GUI.enabled = false;
        EditorGUILayout.IntField("Focused Cell No", KCS.FocusedCellNo);

        // ランタイムのみ
        GUI.enabled = Application.isPlaying;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Jump To "))
        {
            KCS.JumpTo(jumpNo);
        }
        jumpNo = EditorGUILayout.IntField("", jumpNo);
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Scroll Init"))
        {
            KCS.Init();
        }
        EditorGUILayout.Space();  


        // 変更を検知終了。値の変更を保存可能に(*の出現)
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Change Property");
            EditorUtility.SetDirty(target);
        }
    }
}
