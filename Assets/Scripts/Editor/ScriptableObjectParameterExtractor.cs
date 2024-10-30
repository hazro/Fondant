using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class ScriptableObjectParameterExtractor : EditorWindow
{
    private string folderPath = "Assets/YourFolder"; // ScriptableObjectがあるフォルダパスを指定
    private string variableName = ""; // 取得する変数名を入力
    private Vector2 scrollPos;

    [MenuItem("Tools/ScriptableObject Parameter Extractor")]
    static void OpenWindow()
    {
        ScriptableObjectParameterExtractor window = GetWindow<ScriptableObjectParameterExtractor>("SO Parameter Extractor");
        window.Show();
    }

    void OnGUI()
    {
        GUILayout.Label("ScriptableObject パラメータ抽出ツール", EditorStyles.boldLabel);

        // フォルダパスの入力フィールド
        folderPath = EditorGUILayout.TextField("フォルダパス", folderPath);

        // 変数名の入力フィールド
        variableName = EditorGUILayout.TextField("取得する変数名", variableName);

        if (GUILayout.Button("パラメータを抽出してクリップボードにコピー"))
        {
            ExtractParametersAndCopyToClipboard();
        }
    }

    void ExtractParametersAndCopyToClipboard()
    {
        // 指定フォルダ内のすべてのScriptableObjectを取得
        string[] assetGuids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { folderPath });
        List<ScriptableObject> scriptableObjects = new List<ScriptableObject>();

        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject obj = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
            if (obj != null)
            {
                scriptableObjects.Add(obj);
            }
        }

        // ファイル名でソート
        scriptableObjects.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        // 結果を格納する文字列
        string result = "";

        // 各ScriptableObjectの変数値を取得
        foreach (var so in scriptableObjects)
        {
            // 反射を使って指定変数の値を取得
            var soType = so.GetType();
            var field = soType.GetField(variableName);
            if (field != null)
            {
                var value = field.GetValue(so);
                result += value.ToString() + "\n";
            }
            else
            {
                result += so.name + ": 指定された変数が見つかりません\n";
            }
        }

        // クリップボードにコピー
        EditorGUIUtility.systemCopyBuffer = result;

        // 完了メッセージを表示
        Debug.Log("パラメータをクリップボードにコピーしました:\n" + result);
    }
}
