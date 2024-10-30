using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfomationPanelDisplay : MonoBehaviour
{
    public GameObject infomationPanel; // 情報パネル
    [SerializeField] private TMPro.TextMeshProUGUI infoText; // 情報テキスト
    public bool isClicked { get; set; } // クリック状態を管理するプロパティ

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // アクティブにしてテキストを変更するメソッド
    public void SetActiveAndChangeText(string text)
    {
        // infomationPanelがnullでないことを確認
        if (infomationPanel == null)
        {
            Debug.LogError("InfomationPanel is null.");
            return;
        }
        
        infomationPanel.SetActive(true); // パネルをアクティブにする
        infoText.text = text; // テキストを設定

        // テキストを行ごとに分割して処理する
        string[] lines = infoText.text.Split('\n'); // 改行で各行を分割
        List<string> processedLines = new List<string>();
        string abilityLine = null; // abilityで始まる行を保存するための変数

        for (int i = 0; i < lines.Length; i++)
        {
            // 最初の行がnameで始まる場合、白字＆太字にする
            if (lines[i].StartsWith("name"))
            {
                // 最初の「:」までを削除して内容を取得
                lines[i] = lines[i].Substring(lines[i].IndexOf(":") + 1);
                lines[i] = "<size=+2><color=white><b>" + lines[i] + "</b></color></size>\n";
                processedLines.Add(lines[i]);
            }
            // abilityで始まる行は後で黒字にして最後に移動
            else if (lines[i].StartsWith("ability"))
            {
                abilityLine = "<color=black>" + lines[i] + "</color>\n"; // 黒字にして保存
            }
            //worldで始まる行は飛ばす
            else if (lines[i].StartsWith("world"))
            {
                continue;
            }
            else
            {
                // 他の行は通常の黒字に設定
                processedLines.Add("<color=black>" + lines[i] + "</color>");
            }
        }

        // abilityで始まる行が存在した場合、テキストの最後に移動
        if (abilityLine != null)
        {
            processedLines.Add(abilityLine);
        }

        // 行を結合してinfoText.textに設定
        infoText.text = string.Join("\n", processedLines);
    }

    // 非アクティブにするメソッド
    public void SetInactive()
    {
        infomationPanel.SetActive(false); // 非アクティブにする
    }
}
