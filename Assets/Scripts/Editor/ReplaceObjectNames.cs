using UnityEditor;
using UnityEngine;

public class ReplaceObjectNames : EditorWindow
{
    private string searchText = "";
    private string replaceText = "";

    [MenuItem("Tools/Replace Object Names")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceObjectNames>("Replace Object Names");
    }

    private void OnGUI()
    {
        GUILayout.Label("Replace Object Names", EditorStyles.boldLabel);

        searchText = EditorGUILayout.TextField("Search Text", searchText);
        replaceText = EditorGUILayout.TextField("Replace Text", replaceText);

        if (GUILayout.Button("Replace Names"))
        {
            ReplaceNames();
        }
    }

    private void ReplaceNames()
    {
        if (string.IsNullOrEmpty(searchText))
        {
            Debug.LogWarning("Search text is empty. Please enter a value.");
            return;
        }

        var selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("No objects selected. Please select objects in the Hierarchy.");
            return;
        }

        Undo.RecordObjects(selectedObjects, "Replace Object Names");

        foreach (var obj in selectedObjects)
        {
            if (obj.name.Contains(searchText))
            {
                obj.name = obj.name.Replace(searchText, replaceText);
            }
        }

        Debug.Log("Replacement completed.");
    }
}
