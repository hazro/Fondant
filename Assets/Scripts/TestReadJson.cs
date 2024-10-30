using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestReadJson : MonoBehaviour
{
    private JsonDecryptor jsonDecryptor;
    public ItemData itemData; // デシリアライズしたデータを格納するクラス

    void Start()
    {
        // JsonDecryptorクラスを使用する
        jsonDecryptor = new JsonDecryptor();
        string filename = "items"; // ファイル名
        itemData = GetItemData(filename);

        if (itemData != null)
        {
            // 読み込んだデータを確認（デバッグ用）
            Debug.Log(itemData.jobList[0].name + "\n\n\n"); // デシリアライズしたデータを表示
            /*
            if (itemData != null && itemData.jobList != null)
            {
                foreach (var job in itemData.jobList)
                {
                    Debug.Log($"Job ID: {job.ID}, Name: {job.name}");
                }
            }
            */
            //job.IDが30のjob.nameを取得
            Debug.Log(itemData.jobList.Find(x => x.ID == 30).name);
        }

    }

    // itemDataを取得するメソッド
    public ItemData GetItemData(string filename)
    {
        string encryptedJsonFilePath = "Assets/Resources/MasterData/" + filename + ".json";

        // 暗号化されたJSONファイルを復号化
        string decryptedJson = jsonDecryptor.ReadAndDecryptJson(encryptedJsonFilePath);

        if (!string.IsNullOrEmpty(decryptedJson))
        {
            // 復号化されたJSONをクラスにデシリアライズ
            ItemData data = JsonUtility.FromJson<ItemData>(decryptedJson);
            return data;
        }
        else
        {
            Debug.LogError("復号化に失敗しました");
        }
        return null;
    }
}
