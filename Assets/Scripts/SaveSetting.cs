using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// セーブデータの保存・読み込みを行うクラス
/// </summary>
[Serializable]
public class SaveSetting
{
    // 各項目をプロパティとして定義（昇順）
    public float bgmVolume = 1.0f; // BGM音量
    public float masterVolume = 1.0f; // マスター音量
    public float seVolume = 1.0f; // 効果音音量
    public float systemVolume = 1.0f; // システム音量
    public bool fullScreen = true; // フルスクリーン設定
    public List<string> keyBinding = new List<string>(); // キーバインディング（文字列リスト）
    public int quality = 2; // グラフィッククオリティ (0: Low, 1: Medium, 2: High)
    public int resolutionIndex = 0; // 解像度の選択インデックス
    public bool vsync = true; // VSync設定

    private static string SaveFilePath => Path.Combine(Application.persistentDataPath, "settings.json");

    /// <summary>
    /// 設定を保存する
    /// </summary>
    public void Save()
    {
        string json = JsonUtility.ToJson(this, true);
        File.WriteAllText(SaveFilePath, json);
        Debug.Log("Settings saved: " + SaveFilePath);
    }

    /// <summary>
    /// 設定をロードする
    /// </summary>
    public static SaveSetting Load()
    {
        if (File.Exists(SaveFilePath))
        {
            string json = File.ReadAllText(SaveFilePath);
            return JsonUtility.FromJson<SaveSetting>(json);
        }
        else
        {
            Debug.Log("No settings file found, using default settings.");
            return new SaveSetting();
        }
    }
}
