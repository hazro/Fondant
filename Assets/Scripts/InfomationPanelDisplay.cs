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
    public void SetActiveAndChangeText(string text, GameObject selectObject)
    {
        infomationPanel.SetActive(true); // アクティブにする
        infoText.text = text; // テキストを変更

        // infoTextのtextを黒字にして初めの行がnameから始まる場合、白字&太字にして改行を増やし、その後は通常のフォントに戻して黒字にし、最後の行の前に改行を入れる
        if (infoText.text.StartsWith("eqpName"))
        {
            //"eqpName: "の文字列を削除
            infoText.text = infoText.text.Substring(8);
            //最初の行を白字&太字にして改行を増やす
            infoText.text = "<color=white><b>" + infoText.text.Replace("\n", "\n\n") + "</b></color>\n";
            infoText.text = infoText.text.Replace("\n", "</color>\n<color=black>");
            infoText.text = infoText.text.Substring(0, infoText.text.Length - 13) + "\n";
        }
        else
        {
            infoText.text = "<color=black>" + infoText.text + "</color>";
        }
        
    }

    // 非アクティブにするメソッド
    public void SetInactive()
    {
        infomationPanel.SetActive(false); // 非アクティブにする
    }
}
