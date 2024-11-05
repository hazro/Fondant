using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MyGame.Managers
{
public class BlackSmithManager : MonoBehaviour
{
    [SerializeField] private GameObject OriginalRuneImage;
    [SerializeField] private GameObject LvUpRuneImage;

    [Header("Rune Lv Text")]
    [SerializeField] private TextMeshProUGUI OriginalLvText;
    [SerializeField] private TextMeshProUGUI LvUpLvText;

    [Header("Rune Lv Display")]
    [SerializeField] private Image Lv1DisplayWaku;
    [SerializeField] private Image Lv2DisplayWaku;
    [SerializeField] private Image Lv3DisplayWaku;
    [SerializeField] private TextMeshProUGUI Lv1DisplayText;
    [SerializeField] private TextMeshProUGUI Lv2DisplayText;
    [SerializeField] private TextMeshProUGUI Lv3DisplayText;
    
    [Header("Button")]
    [SerializeField] private TextMeshProUGUI PriceText;

    [Header("InfoPanel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject CancelButton;
    [SerializeField] private TextMeshProUGUI infoText;

    [Header("SetItem")]
    public bool isSetItem; // 強化するアイテムがセットされているかどうかを判定する変数
    public GameObject setItem; // 強化するアイテムを格納する変数
    public int setItemID; // 強化するアイテムのIDを格納する変数
    public int setItemLv; // 強化するアイテムのレベルを格納する変数
    private int price; // 強化するアイテムの価格を格納する変数
    private ItemData.RuneListData runeListData; // 強化するアイテムのデータを格納する変数
    private GameManager gameManager; // GameManagerの参照
    private IventryUI iventryUI; // IventryUIの参照
    private Material materialInstance; // Materialのインスタンス
    private Image lvUpImageComponent; // LvUpRuneImageのImageコンポーネント

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance; // GameManagerのインスタンスを取得
        if (gameManager != null)
        {
            // IventryUIのインスタンスを取得
            iventryUI = gameManager.GetComponent<IventryUI>();
        }
        // マテリアルをコピーしてインスタンスを生成
        lvUpImageComponent = LvUpRuneImage.GetComponent<Image>();
        materialInstance = new Material(lvUpImageComponent.material);
        OnResetButtonClicked();
    }

    /// <summary>
    /// 強化するアイテムをセットする関数です。
    /// </summary>
    public void SetItem()
    {
        if(!isSetItem) return;
        // setItemIDに該当するPrefabリソースをロードする
        GameObject setItemPrefab = Resources.Load<GameObject>("Prefabs/Runes/" + setItemID);
        // OriginalRuneImageとLvUpRuneImageをアクティブにしてsetItemPrefabにspriteRendererがあったらspriteをセットする
        OriginalRuneImage.SetActive(true);
        LvUpRuneImage.SetActive(true);
        Image originalImageComponent = OriginalRuneImage.GetComponent<Image>();
        if(setItemPrefab.GetComponent<SpriteRenderer>() != null)
        {
            originalImageComponent.sprite = setItemPrefab.GetComponent<SpriteRenderer>().sprite;
            lvUpImageComponent.sprite = setItemPrefab.GetComponent<SpriteRenderer>().sprite;
        }
        else
        {
            originalImageComponent.sprite = setItemPrefab.GetComponent<Image>().sprite;
            lvUpImageComponent.sprite = setItemPrefab.GetComponent<Image>().sprite;
        }
        // gameManagerのRuneDataからsetItemIDに該当するRuneDataを取得する
        //List<ItemData.RuneListData> runeData = gameManager.itemData.runeList[setItemID];
        runeListData = gameManager.itemData.runeList.Find(x => x.ID == setItemID);

        OriginalLvText.text = "Lv." + setItemLv;
        OriginalRuneImage.GetComponent<ItemDandDHandler>().runeLevel = setItemLv;
        if(runeListData.maxLevel == setItemLv)
        {
            LvUpLvText.text = "Lv." + setItemLv + " (Max)";
            OriginalLvText.text = "Lv." + setItemLv + " (Max)";
            LvUpRuneImage.GetComponent<ItemDandDHandler>().runeLevel = setItemLv;
        }
        else if(runeListData.maxLevel == setItemLv+1)
        {
            LvUpLvText.text = "Lv." + (setItemLv + 1) + " (Max)";
            LvUpRuneImage.GetComponent<ItemDandDHandler>().runeLevel = setItemLv + 1;
        }
        else
        {
            LvUpLvText.text = "Lv." + (setItemLv + 1);
            LvUpRuneImage.GetComponent<ItemDandDHandler>().runeLevel = setItemLv + 1;
        }
        int price = runeListData.price * 2 * (setItemLv);
        PriceText.text = "Price: " + price;

        // MaterialインスタンスにitemのruneLevelを設定
        materialInstance.SetFloat("_Lv", LvUpRuneImage.GetComponent<ItemDandDHandler>().runeLevel);
        lvUpImageComponent.material = materialInstance;

        // 手持ちのお金が足りない場合はtextを赤くする
        if(gameManager.statusLog.currentGold < price)
        {
            PriceText.color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
        else
        {
            PriceText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
    }

    /// <summary>
    /// レベルアップボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnLvUpButtonClicked()
    {
        infoPanel.SetActive(true);
        if(LvUpLvText.text.Contains("Max"))
        {
            infoText.text = "This rune is the maximum level.";
        }
        else
        {
            infoText.text = "Do you want to enhance this rune?\n" + PriceText.text;
            CancelButton.SetActive(true);
        }
    }

    /// <summary>
    /// リセットボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnResetButtonClicked()
    {
        infoPanel.SetActive(false);
        CancelButton.SetActive(false);
        OriginalRuneImage.SetActive(false);
        LvUpRuneImage.SetActive(false);
        isSetItem = false; // 強化するアイテムがセットされているかどうかを判定する変数ItemDandDHandlerのアイテム移動にも影響する
        setItem = null;
        setItemID = 0;
        setItemLv = 0;
        OriginalLvText.text = "";
        LvUpLvText.text = "";
        Lv1DisplayWaku.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        Lv2DisplayWaku.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        Lv3DisplayWaku.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
        Lv1DisplayText.text = "";
        Lv2DisplayText.text = "";
        Lv3DisplayText.text = "";
        PriceText.text = "Price: 0";
        PriceText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }

    /// <summary>
    /// Exitボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnExitButtonClicked()
    {
        OnResetButtonClicked();
        // StatusAdjustmentSceneに遷移
        gameManager.LoadScene("StatusAdjustmentScene");
    }

    /// <summary>
    /// OKボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnOKButtonClicked()
    {
        // 手持ちのお金が足りていたら強化処理を行う
        if(gameManager.statusLog.currentGold > price)
        {
            gameManager.statusLog.currentGold -= price;
            gameManager.UpdateGoldAndExpUI();
            // 元のアイテムのruneLevelを上げる
            setItem.GetComponent<ItemDandDHandler>().runeLevel++;
            Material originalMat;
            if(setItem.GetComponent<SpriteRenderer>() != null)
            {
                originalMat = setItem.GetComponent<SpriteRenderer>().material;
            }
            else
            {
                originalMat = setItem.GetComponent<Image>().material;
            }
            // Materialをinstance化してitemのruneLevelを設定
            Material setMaterialInstance = new Material(originalMat);
            originalMat = setMaterialInstance;
            originalMat.SetFloat("_Lv", setItem.GetComponent<ItemDandDHandler>().runeLevel);
            if(iventryUI != null)
            {
                // イベントリの更新
                iventryUI.UpdateIventryPanel();
                // 全キャラのスキルパネルを更新
                if(gameManager.livingUnits != null)
                {
                    foreach(GameObject unit in gameManager.livingUnits)
                    {

                        iventryUI.UpdateUnitSkillUI(unit);
                    }
                }
            }
            OnResetButtonClicked();
        }
        infoPanel.SetActive(false);
    }

    /// <summary>
    /// Cancelボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnCancelButtonClicked()
    {
        infoPanel.SetActive(false);
    }
}
}