using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System;

/// <summary>
/// ・同名ファイルの確認、他のOS(Mac)は要確認かも
/// ・もっとトグルや折り畳みをすぐ左に置いて見やすくわかりやすくしたい。でも拡張カスタムまじで時間かかりそうだから、先にほかの部分実装したほうがいい気がする、、、。
/// ・CellPrefabごとにKCSを作る＆CellPrefabの中身が変わるならKCSを作り直す必要があることを言及。
/// ・fullChildCountが分かりにくいなら、各々で引数名を変更してください。ただしInterfaceと実装の両方で引数名を変える必要があります。
/// ・作成時にエラーが出ることがあるが、原因はDataSouceの引数にUsingが必要なTypeを使っているためです。その時は、各エラーを生じるスクリプトに行って必要なUsingを追加してください。
/// ・アセンブリ分けは、もうKCSのフォルダでひとくくりにする。そんで、あとで中身をinternalにできるところはして安全にしたい。
/// ・一応注意書きとかを加えよう。・同じ名前のKCSを作ると上書きされます。・不必要なKCSを削除したいときはKeyControllableScroll内に作られた、「KCS-"あなたの設定した名前"」、「KCSInspector-"あなたの設定した名前"」、「KCSCreateMenu？？-"あなたの設定した名前」を削除してください。
/// </summary>

namespace KCSScriptGenerator
{
    public class KCSScriptGeneratorWindow : EditorWindow
    {
        // ウィンドウを表示
        [MenuItem("Window/UI Toolkit/KCS Script Generator", false, 0)]
        static void Open()
        {
            var window = GetWindow<KCSScriptGeneratorWindow>();
            window.titleContent = new GUIContent("KCS Script Generator");
        }


        // 設定用変数
        private Vector2 EditorScroll = new Vector2(0, 0);
        public enum KCSType
        {
            Horizontal,
            Vertical,
        }
        KCSType kcsType;
        private GameObject cellPrefab;
        private List<GameObject> gameObjects;
        private List<Component[]> componentsList;
        private List<List<bool>> togglesList;
        private List<bool> foldings;
        private List<(int layer, string layerChildCount, string fullChildCount, GameObject gameObject)> prefabObjects;
        private string KCSName = "SampleKCS";
        private List<int[]> layerNums;
        private int nowLayer;
        private string layerChildCount;
        private string fullChildCount;


        // ウィンドウの設定
        // 基本OnGUI()はカーソル移動毎にめっちゃUpDateされる。BeginChangeCheck()とEndChangeCheck()を宣言している時には、その間の処理だけパラメ変化時にUpDateされる
        // なんかEditor拡張のOnGUI()系あるあるらしいけど、メソッドをはさみまくるとバグる。特にreturn; が機能しなくなることが多いから、挙動変なら直接書く。
        void OnGUI()
        {
            // ランタイムは使用不可
            GUI.enabled = !Application.isPlaying;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.PrefixLabel("KCS Type:");
            EditorGUI.BeginChangeCheck();
            kcsType = (KCSType)EditorGUILayout.EnumPopup(kcsType);
            if (EditorGUI.EndChangeCheck())
            {
                if (cellPrefab == null) return;
                KCSName = cellPrefab.name + kcsType + "KCS";
                KCSName = char.ToUpper(KCSName[0]) + KCSName.Substring(1); // 先頭大文字に
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PrefixLabel("Cell Prefab:");
            cellPrefab = EditorGUILayout.ObjectField(cellPrefab, typeof(GameObject), true) as GameObject;

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            // EditorScroll = EditorGUILayout.BeginScrollView(EditorScroll); // スクロール可能、、なはずなのにバグる。
            if (cellPrefab == null) return;

            // GO毎にコンポーネント, トグル, 折り畳みのリストを作成
            if (EditorGUI.EndChangeCheck())
            {
                gameObjects = new();
                SetGameObjects(cellPrefab);

                componentsList = new();
                togglesList = new();
                foldings = new();

                for (int i = 0; i < gameObjects.Count; i++)
                {
                    // コンポーネント
                    componentsList.Add(gameObjects[i].GetComponents<Component>());
                    // トグル
                    var toggles = new List<bool>();
                    foreach (var component in componentsList[i])
                    {
                        toggles.Add(false);
                    }
                    togglesList.Add(toggles);
                    // 折り畳み
                    foldings.Add(false);
                }

                layerNums = new();
                nowLayer = 0;
                layerChildCount = "";
                fullChildCount = "";
                KCSName = cellPrefab.name + kcsType + "KCS";
                KCSName = char.ToUpper(KCSName[0]) + KCSName.Substring(1); // 先頭大文字に
                prefabObjects = new();
                ListTargetComponents(cellPrefab); // なんか中でreturn入れないと動作が変になるから、変数の最後に実行
            }

            // UIの作成
            EditorGUILayout.LabelField("DataSource Target Components:");
            if (prefabObjects is null) return;
            for (int i = 0; i < prefabObjects.Count; i++)
            {
                string space = "";
                for (int sp = 0; sp < prefabObjects[i].layer; sp++) space += "    ";

                if (foldings[i] = EditorGUILayout.Foldout(foldings[i], i == 0 ? "    " + prefabObjects[i].gameObject.name : space + " └ " + prefabObjects[i].layerChildCount + ". " + prefabObjects[i].gameObject.name))
                {
                    for (int ii = 0; ii < togglesList[i].Count; ii++)
                    {
                        togglesList[i][ii] = EditorGUILayout.ToggleLeft(space + "        - " + componentsList[i][ii].GetType().Name, togglesList[i][ii]);
                    }
                }
            }


            // EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();
            GUILayout.Box(GUIContent.none, GUILayout.Height(2), GUILayout.ExpandWidth(true));
            GUILayout.Box(GUIContent.none, GUILayout.Height(2), GUILayout.ExpandWidth(true));
            EditorGUILayout.PrefixLabel("KCS Name:", EditorStyles.boldLabel);
            KCSName = EditorGUILayout.TextField(KCSName);
            KCSName = char.ToUpper(KCSName[0]) + KCSName.Substring(1); // 先頭大文字に

            // このスクリプトがあるフォルダ名を取得
            var mono = MonoScript.FromScriptableObject(this);
            var thisPath = AssetDatabase.GetAssetPath(mono);
            string thisFolderPath = thisPath.Substring(0, thisPath.IndexOf("/KCSScriptGeneratorWindow.cs"));

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate KCS Script at Default Path"))
            {
                // スクリプトの名前にできるもの → 先頭が数字以外で、使われているのが半角のA~Z, a~z, 0~9, _, のみ
                if (!Regex.IsMatch(KCSName, "^([A-Z]|_)([A-Z]|[0-9]|_)*$", RegexOptions.IgnoreCase)) // メソッド化すると謎にバグる
                {
                    EditorUtility.DisplayDialog("Invalid Script Name", $"'{KCSName}' contains invalid type name.\n Use only a~z, A~Z, _, 0~9.", "OK");
                    return;
                }

                // ユニークネームか検証。ファイル保存先を指定する方は、Windowsエクスプローラーが確認してくるので無し
                if (!IsClassNameUnique())
                {
                    if (!EditorUtility.DisplayDialog("Warning: Same Name KCS Script Already Exists.", $"A KCS Script Already Exists with the name '{KCSName}'.\n\nDo you want to override the script?", "OK", "Cancel")) return;
                }

                string newFolderPath = thisFolderPath + "/NewKCSs/"+ KCSName;
                Directory.CreateDirectory(newFolderPath);
                string path = newFolderPath + "/" + KCSName + ".cs";
                string interfacePath = newFolderPath + "/" + KCSName + "Interface.cs";
                GenerateKCS(path, interfacePath);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate KCS Script at Select Path"))
            {
                // スクリプトの名前にできるもの → 先頭が数字以外で、使われているのが半角のA~Z, a~z, 0~9, _ のみ
                if (!Regex.IsMatch(KCSName, "^([A-Z]|_)([A-Z]|[0-9]|_)*$", RegexOptions.IgnoreCase))
                {
                    EditorUtility.DisplayDialog("Invalid Script Name", $"\"{KCSName}\" contains invalid type name.\n Use only a~z, A~Z, _, 0~9.", "OK");
                    return;
                }

                string newFolderPath = thisFolderPath + "/" + KCSName;
                Directory.CreateDirectory(newFolderPath);
                string path = EditorUtility.SaveFilePanel("Save New Script", newFolderPath, KCSName + ".cs", ".cs");
                if (!string.IsNullOrEmpty(path))
                {
                    string interfacePath = path.Substring(0, path.IndexOf(".cs")) + "Interface.cs";
                    GenerateKCS(path, interfacePath);
                }
            }
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }


        /// <summary>
        /// 引数以下の階層にある全てのGOを取得
        /// </summary>
        private void SetGameObjects(GameObject go)
        {
            gameObjects.Add(go);
            Transform t = go.transform;
            for (int i = 0; i < t.childCount; i++)
            {
                SetGameObjects(t.GetChild(i).gameObject);
            }
        }


        /// <summary>
        /// 引数のGameObject以下の全てのGoとその階層構造を取得
        /// </summary>
        private void ListTargetComponents(GameObject go)
        {
            Transform t = go.transform;
            prefabObjects.Add((nowLayer, layerChildCount, fullChildCount, go));

            if (t.childCount != 0)
            {
                layerNums.Add(new int[2] { 1, t.childCount });
                nowLayer += 1;
            }
            else
            {
                if (layerNums.Count == 0) return;
                if (layerNums[^1][0] != layerNums[^1][1])
                {
                    layerNums[^1][0] += 1;
                }
                else
                {
                    while (layerNums[^1][0] == layerNums[^1][1])
                    {
                        if (layerNums.Count == 1) return;
                        layerNums.Remove(layerNums[^1]);
                        nowLayer -= 1;
                    }
                    layerNums[^1][0] += 1;
                }
            }

            layerChildCount = $"{layerNums[^1][0] - 1}";
            fullChildCount = "";
            foreach (var e in layerNums) { fullChildCount += ("_" + $"{e[0] - 1}"); }

            for (int i = 1; i <= t.childCount; i++)
            {
                ListTargetComponents(t.GetChild(i - 1).gameObject);
            }
        }


        /// <summary>
        /// ユニークネームか検証。ファイル保存先を指定する方は、Windowsエクスプローラーが確認してくるので無し、他のOSは要確認かも
        /// </summary>
        private bool IsClassNameUnique()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) // 現在読み込まれている全Assemblyを取得
            {
                foreach (var type in assembly.GetTypes()) // foreach assembly.GetTypes() .Name でそのAssembly内の全てのクラスとかインタ名
                {
                    if (type.Name == KCSName) return false;
                }
            }
            return true;
        }


        /// <summary>
        /// KCSとKCSインターフェースを作成
        /// </summary>
        private void GenerateKCS(string path, string interfacePath)
        {
            // フォーカスを外す
            EditorGUIUtility.keyboardControl = 0;
            EditorGUIUtility.editingTextField = false;

            /* 引数部分、こんな形に
             RectTransform rectTransform, Button button_0_2, Text text_2 */
            string dataSourceArgument = "";
            for (int i = 0; i < prefabObjects.Count; i++) // 全ての下位オブジェクト毎に
            {
                for (int ii = 0; ii < componentsList[i].Length; ii++) // そのアタッチされた全てのコンポーネントに対して
                {
                    if (togglesList[i][ii]) // トグルがチェックされてれば
                    {
                        string aug1 = $"{componentsList[i][ii].GetType().Name}";
                        string aug2 = char.ToLower(aug1[0]) + aug1.Substring(1) + prefabObjects[i].fullChildCount; // 先頭を小文字にしてChildCount追加
                        dataSourceArgument += ", " + aug1 + " " + aug2;
                    }
                }
            }
            
            /* 初期化時のコンポーネント取得部分、こんな形にする。
            var rectTransform = rectT.GetComponent<RectTransform>();
            var button_0_2 = rectT.GetChild(0).GetChild(2).GetComponent<Button>();
            var text_2 = rectT.GetChild(2).GetComponent<Text>();
            recycleArray[i] = (go, rectT, rectTransform, button_0_2, text_2);      */
            string instantiateText = "";
            string recycleSListAdd = "\n" + "                " + "recycleArray[i] = (go, rectT";
            for (int i = 0; i < prefabObjects.Count; i++)
            {
                for (int ii = 0; ii < componentsList[i].Length; ii++)
                {
                    if (togglesList[i][ii])
                    {
                        string componentName = $"{componentsList[i][ii].GetType().Name}";
                        string varName = char.ToLower(componentName[0]) + componentName.Substring(1) + prefabObjects[i].fullChildCount;
                        string getChildText = "";

                        for (string s = prefabObjects[i].fullChildCount; s != "";)
                        {
                            s = s.Substring(1);
                            getChildText += $"GetChild({s[0]}).";
                            s = s.Substring(1);
                        }

                        instantiateText += "\n" + "                " + "var " + varName + " = rectT." + getChildText + $"GetComponent<{componentName}>();";
                        recycleSListAdd += ", " + varName;
                    }
                }
            }
            recycleSListAdd += ");";
            instantiateText += recycleSListAdd;

            /* 再描写時のセル内容実装、こんな形にする。
            DataSource.SetData(drawCellNo, recycleArray[i].rectTransform, recycleArray[i].button_0_2, recycleArray[i].text_2); */
            string dataSourceSetData = "DataSource.SetCellData(drawCellNo";
            for (int i = 0; i < prefabObjects.Count; i++)
            {
                for (int ii = 0; ii < componentsList[i].Length; ii++)
                {
                    if (togglesList[i][ii])
                    {
                        string componentName = $"{componentsList[i][ii].GetType().Name}";
                        string aug = char.ToLower(componentName[0]) + componentName.Substring(1) + prefabObjects[i].fullChildCount;
                        dataSourceSetData += ", recycleArray[i]." + aug;
                    }
                }
            }
            dataSourceSetData += ");";

            // 書き出し
            if (kcsType == KCSScriptGeneratorWindow.KCSType.Horizontal)
            {
                File.WriteAllText(path, KCSHorizontalText.GenerateText(KCSName, dataSourceArgument, instantiateText, dataSourceSetData), Encoding.UTF8);
            }

            if (kcsType == KCSScriptGeneratorWindow.KCSType.Vertical)
            {
                File.WriteAllText(path, KCSVerticalText.GenerateText(KCSName, dataSourceArgument, instantiateText, dataSourceSetData), Encoding.UTF8);
            }

            File.WriteAllText(interfacePath, KCSInterfaceText.GenerateText(KCSName, dataSourceArgument), Encoding.UTF8);
            AssetDatabase.Refresh();
        }
    }
}