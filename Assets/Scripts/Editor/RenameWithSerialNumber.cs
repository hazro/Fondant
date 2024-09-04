using UnityEditor;
using UnityEngine;

public class RenameWithSerialNumber : EditorWindow
{
    private string baseName = "Object";
    private int serialNumber = 0;

    [MenuItem("Tools/Rename With Serial Number")]
    private static void RenameSelectedObjects()
    {
        RenameWithSerialNumber window = GetWindow<RenameWithSerialNumber>();
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Base Name:");
        baseName = EditorGUILayout.TextField(baseName);

        GUILayout.Label("Serial Number:");
        serialNumber = EditorGUILayout.IntField(serialNumber);

        if (GUILayout.Button("Rename"))
        {
            foreach (GameObject selectedObject in Selection.gameObjects)
            {
                Undo.RecordObject(selectedObject, "Rename Object");
                selectedObject.name = baseName + serialNumber.ToString("00");
                serialNumber++;
            }
        }
    }
    
    /// <summary>
    /// Renames the selected objects with a base name and a serial number.
    /// </summary>
    [MenuItem("Tools/Rename With Serial Number", true)]
    private static bool ValidateRenameSelectedObjects()
    {
        return Selection.gameObjects.Length > 0;
    }
}