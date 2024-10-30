using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEditor;

public class JsonEncryptorEditor : EditorWindow
{
    private string key = "A1b2C3d4E5f6G7h8"; // 16, 24, 32文字の鍵を指定
    private string selectedFolderPath;
    private string masterDataFolderPath = "Assets/Resources/MasterData"; // 暗号化されたファイルの保存先

    [MenuItem("Tools/Encrypt JSON Files in Folder")]
    public static void ShowWindow()
    {
        GetWindow<JsonEncryptorEditor>("Encrypt JSON Files");
    }

    private void OnGUI()
    {
        GUILayout.Label("AESでフォルダ内のすべてのJSONファイルを暗号化する", EditorStyles.boldLabel);

        if (GUILayout.Button("フォルダを選択"))
        {
            selectedFolderPath = EditorUtility.OpenFolderPanel("Select Folder", Application.dataPath, "");
        }

        if (!string.IsNullOrEmpty(selectedFolderPath))
        {
            GUILayout.Label("選択されたフォルダ: " + selectedFolderPath);

            if (GUILayout.Button("暗号化して保存"))
            {
                EncryptAllJsonFilesInFolder(selectedFolderPath);
            }
        }
    }

    private void EncryptAllJsonFilesInFolder(string folderPath)
    {
        try
        {
            // マスターデータフォルダの作成
            if (!Directory.Exists(masterDataFolderPath))
            {
                Directory.CreateDirectory(masterDataFolderPath);
                AssetDatabase.Refresh(); // Unityエディタに新しいフォルダを認識させる
            }

            // フォルダ内のすべてのjsonファイルを取得
            string[] jsonFiles = Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories);

            if (jsonFiles.Length == 0)
            {
                Debug.LogWarning("指定されたフォルダにJSONファイルが見つかりません。");
                EditorUtility.DisplayDialog("警告", "指定されたフォルダにJSONファイルが見つかりません。", "OK");
                return;
            }

            // 各jsonファイルを暗号化
            foreach (var filePath in jsonFiles)
            {
                // ファイル内容を読み込む
                string jsonContent = File.ReadAllText(filePath);

                // AESで暗号化
                string encryptedContent = Encrypt(jsonContent, key);

                // 暗号化されたファイル名を作成
                string fileName = Path.GetFileNameWithoutExtension(filePath) + ".json";
                string encryptedFilePath = Path.Combine(masterDataFolderPath, fileName);

                // 暗号化された内容を保存
                File.WriteAllText(encryptedFilePath, encryptedContent);

                Debug.Log("ファイルが暗号化されました: " + encryptedFilePath);
            }

            EditorUtility.DisplayDialog("成功", "すべてのJSONファイルが暗号化されました。", "OK");
            AssetDatabase.Refresh(); // 保存されたファイルをエディタに反映
        }
        catch (Exception e)
        {
            Debug.LogError("暗号化中にエラーが発生しました: " + e.Message);
            EditorUtility.DisplayDialog("エラー", "暗号化中にエラーが発生しました: " + e.Message, "OK");
        }
    }

    // AESで暗号化
    public static string Encrypt(string plainText, string key)
    {
        byte[] iv = new byte[16];
        byte[] array;

        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;

            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                    }
                    array = memoryStream.ToArray();
                }
            }
        }

        return Convert.ToBase64String(array);
    }
}
