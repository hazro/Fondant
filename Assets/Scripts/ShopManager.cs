using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System.Text.RegularExpressions;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private StatusLog statusLog; // ステータスログ
    [SerializeField] private GameObject shopItemGroup; // ショップアイテムグループ
    [SerializeField] private List<string> shopItemList = new List<string>(); // ショップアイテムリスト
    [SerializeField] private List<int> stockItem = new List<int>(); // カートに入れたアイテムリスト
    [SerializeField] private int totalPrice; // 合計金額
    [SerializeField] private TextMeshProUGUI totalPriceText; // 合計金額表示用テキスト
    [SerializeField] private TextMeshProUGUI resetText; // リセットボタンテキスト
    [SerializeField] private GameObject infoPanel; // 情報パネル
    [SerializeField] private GameObject cancelButton; // キャンセルボタン
    [SerializeField] private TextMeshProUGUI infoText; // 情報表示用テキスト
    [SerializeField] private TextMeshProUGUI saleText; // セール情報表示用テキスト
    private GameManager gameManager; // ゲームマネージャ
    private WorldManager worldManager; // ワールドマネージャ
    private IventryUI iventryUI; // IventryUIの参照
    private bool isMethodOKExecuted = false;
    private bool isMethodCancelExecuted = false;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        worldManager = WorldManager.Instance;
        // 現在のワールドが0の場合、1に設定
        if (worldManager.currentWorld == 0)
        {
            worldManager.currentWorld = 1;
        }
        // 現在のルームイベントが0の場合、1に設定
        if (worldManager.currentRoomEvent == 0)
        {
            worldManager.currentRoomEvent = 1;
        }
        if (gameManager != null)
        {
            if(gameManager.roomOptions.salePercentage != 0)
            {
                saleText.text = gameManager.roomOptions.salePercentage + "% ON SALE ! ";
                saleText.gameObject.SetActive(true);
            }
            else
            {
                saleText.gameObject.SetActive(false);
            }
            gameManager.UpdateGoldAndExpUI(); // ゴールドと経験値UIを更新
            // ワールド番号とステージ番号を更新
            gameManager.UpdateWorldStageUI();
            // IventryItemを取得
            iventryUI = gameManager.GetComponent<IventryUI>();
        }
        statusLog.shopResetCount = 3; // ショップリセット回数を3に設定
        resetText.text = "shuffle (" + statusLog.shopResetCount + "times)"; // リセットボタンテキストを設定
        // infoPanelを非表示にする
        infoPanel.SetActive(false);
        // ストックアイテムリストを初期化
        stockItem.Clear();
        // totalPriceを0に設定
        totalPrice = 0;
        // totalPriceTextを空に設定
        totalPriceText.text = "Total: " + totalPrice;
        // ショップアイテムリストを更新
        UpdateShopItemList();
        // ショップにアイテムをならべる
        SetShopItem();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// カートにアイテムを追加するメソッド
    /// </summary>
    public void addStockList(GameObject clickedObject)
    {
        // Click SEを再生
        AkSoundEngine.PostEvent("ST_Click", gameObject);

        // クリックされたオブジェクトの名前が"Image"を含む場合
        if(clickedObject.name.Contains("Image")){
            // 親オブジェクトの名前を取得
            GameObject parentObject = clickedObject.transform.parent.gameObject;
            // parentObjectがshopItemGroupの子オブジェクトである場合何番目の子オブジェクトかを取得
            int index = parentObject.transform.GetSiblingIndex();
            // shopItemList[index]が空でない場合
            if (shopItemList[index] != "0")
            {
                // Itemを選択
                if(parentObject.GetComponent<Image>().color != new Color(0.0f, 0.5f, 0.0f, 0.6f))
                {
                    // parentObjectのImageコンポーネントのColorを緑色に変更
                    parentObject.GetComponent<Image>().color = new Color(0.0f, 0.5f, 0.0f, 0.6f);
                    // shopItemList[index]を"_"で分割し、itemIDとitemPriceに代入
                    string itemID = shopItemList[index].Split('_')[0];
                    string itemPrice = shopItemList[index].Split('_')[1];
                    // itemPriceをint型に変換し、totalPriceに加算
                    totalPrice += int.Parse(itemPrice) * (100 - gameManager.roomOptions.salePercentage) / 100;
                    // totalPriceTextにtotalPriceを表示
                    totalPriceText.text = "Total: " + totalPrice;
                    // stockItemリストにitemIDを追加
                    stockItem.Add(int.Parse(itemID));
                }
                // Itemの選択を解除
                else
                {
                    // parentObjectのImageコンポーネントのColorを茶色に変更
                    parentObject.GetComponent<Image>().color = new Color(0.5f, 0.2f, 0.0f, 0.6f);
                    // shopItemList[index]を"_"で分割し、itemIDとitemPriceに代入
                    string itemID = shopItemList[index].Split('_')[0];
                    string itemPrice = shopItemList[index].Split('_')[1];
                    // itemPriceをint型に変換し、totalPriceを減算
                    totalPrice -= int.Parse(itemPrice) * (100 - gameManager.roomOptions.salePercentage) / 100;;
                    // totalPriceTextにtotalPriceを表示
                    totalPriceText.text = "Total: " + totalPrice;
                    // stockItemリストからitemIDを削除
                    stockItem.Remove(int.Parse(itemID));
                }
                // 価格と所持金によってtotalPriceTextの色を変更する
                ChangeTotalPriceTextColor();
            }
        }
    }

    /// <summary>
    /// 価格と所持金によってtotalPriceTextの色を変更するメソッド
    /// </summary>
    public void ChangeTotalPriceTextColor()
    {
        // totalPriceがcurrentGoldより大きい場合 textを赤色に変更
        if (totalPrice > statusLog.currentGold)
        {
            totalPriceText.color = new Color(0.7f, 0.0f, 0.0f, 1.0f);
        }
        else
        {
            totalPriceText.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
    }
    

    /// <summary>
    /// Exitボタンを押したときの処理
    /// </summary>
    public void OnClickExitButton()
    {
        // Click SEを再生
        AkSoundEngine.PostEvent("ST_Click", gameObject);

        // StatusAdjustmentSceneに遷移
        gameManager.LoadScene("StatusAdjustmentScene");
    }

    /// <summary>
    /// リセットボタンを押したときの処理
    /// </summary>
    public void OnClickResetButton()
    {
        // ショップリセット回数が0より大きい場合
        if (statusLog.shopResetCount > 0)
        {
            // Click SEを再生
            AkSoundEngine.PostEvent("ST_Click", gameObject);

            // ストックアイテムリストを初期化
            stockItem.Clear();
            // totalPriceを0に設定
            totalPrice = 0;
            // totalPriceTextを空に設定
            totalPriceText.text = "Total: " + totalPrice;
            // ショップリセット回数を1減らす
            statusLog.shopResetCount--;
            // ショップリセット回数を表示
            resetText.text = "Shuffle (" + statusLog.shopResetCount + "times)"; // リセットボタンテキストを設定
            // ショップアイテムリストを更新
            UpdateShopItemList();
            // ショップにアイテムをならべる
            SetShopItem();
            // 価格と所持金によってtotalPriceTextの色を変更する
            ChangeTotalPriceTextColor();
        }
        else
        {
            // Error SEを再生
            AkSoundEngine.PostEvent("ST_Error", gameObject);

            // infoTextに"ショップリセット回数がありません"を表示
            string text = "No store reset times";
            UpdateInfoPanel(text);
        }
    }

    /// <summary>
    /// 購入ボタンを押したときの処理
    /// </summary>
    public void OnClickPurchaseButton()
    {
        // totalPriceが0より大きい場合
        if (totalPrice > 0)
        {
            // 現在のゴールドがtotalPriceより大きい場合
            if (statusLog.currentGold >= totalPrice)
            {
                // 購入SEを再生
                AkSoundEngine.PostEvent("ST_Buy", gameObject);

                // 現在のゴールドからtotalPriceを引く
                statusLog.currentGold -= totalPrice;
                // ゴールドと経験値UIを更新
                gameManager.UpdateGoldAndExpUI();
                // totalPriceを0に設定
                totalPrice = 0;
                // totalPriceTextを空に設定
                totalPriceText.text = "Total: " + totalPrice;
                // ストックアイテムをIventoryに追加
                if(iventryUI != null)
                {
                    foreach (int itemID in stockItem)
                    {
                        iventryUI.AddItem(itemID);
                    }
                }
                // ストックアイテムリストを初期化
                stockItem.Clear();
                //// 購入したアイテムをショップアイテムリストから削除
                // ShopItemGroupの子要素分繰り返す
                int count = 0;
                foreach (Transform n in shopItemGroup.transform)
                {
                    // Imageコンポーネントの色が緑色の場合
                    if (n.GetComponent<Image>().color == new Color(0.0f, 0.5f, 0.0f, 0.6f))
                    {
                        // shopItemList[count]を"0"に設定
                        shopItemList[count] = "0";
                    }
                    count++;
                }
                // ショップにアイテムをならべる
                SetShopItem();
            }
            else
            {
                // Error SEを再生
                AkSoundEngine.PostEvent("ST_Error", gameObject);

                // infoTextに"ゴールドが足りません"を表示
                string text = "Not enough gold";
                UpdateInfoPanel(text);
            }
        }
        else
        {
            // infoTextに"アイテムが選択されていません"を表示
            // Error SEを再生
            AkSoundEngine.PostEvent("ST_Error", gameObject);

            string text = "No item selected";
            UpdateInfoPanel(text);
        }
    }

    /// <summary>
    /// infoPanel表示の更新
    /// </summary>
    public void UpdateInfoPanel(string text, bool enableCancelButton = false)
    {
        if (enableCancelButton)
        {
            // cancelButtonを表示
            cancelButton.SetActive(true);
        }
        else
        {
            // cancelButtonを非表示
            cancelButton.SetActive(false);
        }
        // infoTextにtextを表示
        infoText.text = text;
        // infoPanelを表示
        infoPanel.SetActive(true);
    }

    /// <summary>
    /// infoPanelのOKボタンを押したときの処理
    /// </summary>
    public void OnClickOKButton()
    {
        // infoPanelを非表示
        infoPanel.SetActive(false);
        // メソッドOKが実行されたことを示すフラグを立てる
        isMethodOKExecuted = true;
    }

    /// <summary>
    /// infoPanelのキャンセルボタンを押したときの処理
    /// </summary>
    public void OnClickCancelButton()
    {
        // infoPanelを非表示
        infoPanel.SetActive(false);
        // メソッドCancelが実行されたことを示すフラグを立てる
        isMethodCancelExecuted = true;
    }

    /// <summary>
    /// ショップアイテムリストを更新するメソッド
    /// </summary>
    public void UpdateShopItemList()
    {
        // ショップアイテムリストを初期化
        shopItemList.Clear();
        // ショップアイテムのリスト数を35に設定
        for (int i = 0; i < 35; i++)
        {
            shopItemList.Add("0");
        }

        // 現在のワールド番号を取得
        int currentWorld = worldManager.currentWorld;

        // 武器のリストを取得
        List<ItemData.WpnListData> wpnList = gameManager.itemData.wpnList;
        // 装備のリストを取得
        List<ItemData.EqpListData> eqpList = gameManager.itemData.eqpList;
        // ルーンのリストを取得
        List<ItemData.RuneListData> runeList = gameManager.itemData.runeList;

        // wpnList.worldが0ではなく、wpnList.worldがcurrentWorld以下でIDが0以外でIDの百の位が9以外のものを取得
        List<ItemData.WpnListData> wpnListByWorld = wpnList
            .Where(wpn => wpn.world != 0 && wpn.world <= currentWorld && wpn.ID != 0 && (wpn.ID / 100)%10 != 9)
            .ToList();
        // eqpList.worldが0ではなく、eqpList.worldがcurrentWorld以下でIDが0以外のものを取得
        List<ItemData.EqpListData> eqpListByWorld = eqpList
            .Where(eqp => eqp.world != 0 && eqp.world <= currentWorld && eqp.ID != 0 && (eqp.ID / 100) % 10 != 9)
            .ToList();
        // runeList.worldが0ではなく、runeList.worldがcurrentWorld以下で、runeSpawnSettings.IDの数値が411で始まるものを取得
        List<ItemData.RuneListData> redRuneList = runeList
            .Where(rune => rune.world != 0 && rune.world <= currentWorld && rune.ID >= 411000 && rune.ID < 412000 && (rune.ID / 100) % 10 != 9)
            .ToList();
        // runeList.worldが0ではなく、runeList.worldがcurrentWorld以下で、runeSpawnSettings.IDの数値が412で始まるものを取得
        List<ItemData.RuneListData> orgRuneList = runeList
            .Where(rune => rune.world != 0 && rune.world <= currentWorld && rune.ID >= 412000 && rune.ID < 413000 && (rune.ID / 100) % 10 != 9)
            .ToList();
        // runeList.worldが0ではなく、runeList.worldがcurrentWorld以下で、runeSpawnSettings.IDの数値が413で始まるものを取得
        List<ItemData.RuneListData> grnRuneList = runeList
            .Where(rune => rune.world != 0 && rune.world <= currentWorld && rune.ID >= 413000 && rune.ID < 414000 && (rune.ID / 100) % 10 != 9)
            .ToList();
        // runeList.worldが0ではなく、runeList.worldがcurrentWorld以下で、runeSpawnSettings.IDの数値が414で始まるものを取得
        List<ItemData.RuneListData> palRuneList = runeList
            .Where(rune => rune.world != 0 && rune.world <= currentWorld && rune.ID >= 414000 && rune.ID < 415000 && (rune.ID / 100) % 10 != 9)
            .ToList();
        // runeList.worldが0ではなく、runeList.worldがcurrentWorld以下で、runeSpawnSettings.IDの数値が414で始まるものを取得
        List<ItemData.RuneListData> bluRuneList = runeList
            .Where(rune => rune.world !=0 && rune.world <= currentWorld && rune.ID >= 415000 && rune.ID < 416000 && (rune.ID / 100) % 10 != 9)
            .ToList();

        // lineGridの子要素を全て取得
        int count = 0;
        foreach (Transform n in shopItemGroup.transform)
        {
            //nのオブジェクト名に"wpn"が含まれる場合
            if (n.name.Contains("wpn"))
            {
                if (wpnListByWorld.Count != 0)
                {
                    // wpnListByWorldの要素数からランダムな正数値を取得
                    int randomIndex = Random.Range(0, wpnListByWorld.Count);
                    shopItemList[count] = wpnListByWorld[randomIndex].ID + "_" + wpnListByWorld[randomIndex].price;
                }
            }
            else if (n.name.Contains("eqp"))
            {
                if (eqpListByWorld.Count != 0)
                {
                    // eqpListByWorldの要素数からランダムな正数値を取得
                    int randomIndex = Random.Range(0, eqpListByWorld.Count);
                    shopItemList[count] = eqpListByWorld[randomIndex].ID + "_" + eqpListByWorld[randomIndex].price;
                }
            }
            // nのオブジェクト名に"rne" + 01~02が含まれる場合
            else if (Regex.IsMatch(n.name, @"rne(0[1-2])"))
            {
                if (redRuneList.Count != 0)
                {
                    // redRuneListの要素数からランダムな正数値を取得
                    int randomIndex = Random.Range(0, redRuneList.Count);
                    shopItemList[count] = redRuneList[randomIndex].ID + "_" + redRuneList[randomIndex].price;
                }
            }
            // nのオブジェクト名に"rne" + 03~05が含まれる場合
            else if (Regex.IsMatch(n.name, @"rne(0[3-5])"))
            {
                if (orgRuneList.Count != 0)
                {
                    // orgRuneListの要素数からランダムな正数値を取得
                    int randomIndex = Random.Range(0, orgRuneList.Count);
                    shopItemList[count] = orgRuneList[randomIndex].ID + "_" + orgRuneList[randomIndex].price;
                }
            }
            // nのオブジェクト名に"rne" + 06~09が含まれる場合
            else if (Regex.IsMatch(n.name, @"rne(0[6-9])"))
            {
                if (grnRuneList.Count != 0)
                {
                    // grnRuneListの要素数からランダムな正数値を取得
                    int randomIndex = Random.Range(0, grnRuneList.Count);
                    shopItemList[count] = grnRuneList[randomIndex].ID + "_" + grnRuneList[randomIndex].price;
                }
            }
            // nのオブジェクト名に"rne10"が含まれる場合
            else if (n.name.Contains("rne10"))
            {
                if (palRuneList.Count != 0)
                {
                    // palRuneListの要素数からランダムな正数値を取得
                    int randomIndex = Random.Range(0, palRuneList.Count);
                    shopItemList[count] = palRuneList[randomIndex].ID + "_" + palRuneList[randomIndex].price;
                }
            }
            // nのオブジェクト名に"rne" + 11~15が含まれる場合
            else if (Regex.IsMatch(n.name, @"rne(1[1-5])"))
            {
                if (bluRuneList.Count != 0)
                {
                    // bluRuneListの要素数からランダムな正数値を取得
                    int randomIndex = Random.Range(0, bluRuneList.Count);
                    shopItemList[count] = bluRuneList[randomIndex].ID + "_" + bluRuneList[randomIndex].price;
                }
            }
            count++;
        }
    }

    /// <summary>
    /// ショップにアイテムをならべるメソッド
    /// </summary>
    public void SetShopItem()
    {
        // shopItemGroupの子要素分繰り返す
        int count = 0;
        foreach (Transform n in shopItemGroup.transform)
        {
            // shopItemList[i]が空でない場合
            if (shopItemList[count] != "0")
            {
                // shopItemList[i]を"_"で分割し、itemIDとitemPriceに代入
                string itemID = shopItemList[count].Split('_')[0];
                string itemPrice = shopItemList[count].Split('_')[1];

                GameObject itemPrefab = null;
                // iが0~9の場合は武器のprefabを取得
                if (count < 10) itemPrefab = Resources.Load<GameObject>("Prefabs/Weapons/" + itemID);
                // iが10~19の場合は装備のprefabを取得
                else if (count < 20) itemPrefab = Resources.Load<GameObject>("Prefabs/Equipment/" + itemID);
                // iが20~24の場合はルーンのprefabを取得
                else itemPrefab = Resources.Load<GameObject>("Prefabs/Runes/" + itemID);

                // nの子オブジェクトを全て取得
                foreach (Transform child in n.transform)
                {
                    n.GetComponent<Image>().color = new Color(0.5f, 0.2f, 0.0f, 0.6f);
                    // Imageコンポーネントがある場合
                    if (child.GetComponent<Image>() != null)
                    {
                        child.GetComponent<Image>().enabled = true;
                        // nの孫要素のImageコンポーネントのspriteにweaponPrefabのspriteを代入
                        child.GetComponent<Image>().sprite = itemPrefab.GetComponent<SpriteRenderer>().sprite;
                    }
                    // TextMeshProUGUIコンポーネントがある場合
                    if (child.GetComponent<TextMeshProUGUI>() != null)
                    {
                        // nの孫要素のTextMeshProUGUIコンポーネントのtextにpriceを代入
                        child.GetComponent<TextMeshProUGUI>().text = itemPrice;
                    }
                }
            }
            else
            {
                n.GetComponent<Image>().color = new Color(0.33f, 0.33f, 0.33f, 0.6f);
                // nの子オブジェクトを全て取得
                foreach (Transform child in n.transform)
                {
                    // Imageコンポーネントがある場合
                    if (child.GetComponent<Image>() != null)
                    {
                        // nの孫要素のImageコンポーネントを非表示にする
                        child.GetComponent<Image>().enabled = false;
                    }
                    // TextMeshProUGUIコンポーネントがある場合
                    if (child.GetComponent<TextMeshProUGUI>() != null)
                    {
                        // nの孫要素のTextMeshProUGUIコンポーネントのtextに"SOLD OUT"を代入
                        child.GetComponent<TextMeshProUGUI>().text = "SOLD OUT";
                    }
                }
            }
            count++;
        }
    }

    /// <summary>
    /// Sellボタンを押したときの処理
    /// 選択したものをすべて売る処理
    /// </summary>
    public void OnClickSellButton()
    {
        SellItem(null,true);
    }

    /// <summary>
    /// Itemを半額で売るメソッド
    /// </summary>
    public void SellItem(GameObject sellObject, bool isSelected = false)
    {
        int itemPrice = 0;
        List<int> itemIDList = new List<int>();
        List<GameObject> sellObjectList = new List<GameObject>();
        if (isSelected)
        {
            // 選択したアイテムの合計金額を取得
            if(iventryUI == null) iventryUI = gameManager.GetComponent<IventryUI>();
            if (iventryUI != null)
            {
                // IventryPanelの選択中の孫オブジェクトを全て取得
                foreach (Transform child in iventryUI.IventryPanel.transform)
                {
                    foreach (Transform grandChild in child)
                    {
                        if (grandChild.GetComponent<ItemDandDHandler>().isSelected)
                        {
                            // IventryのItemに限定されるので、オブジェクト名=ItemID
                            itemPrice += GetPrice(int.Parse(grandChild.name));
                            itemIDList.Add(int.Parse(grandChild.name));
                            sellObjectList.Add(grandChild.gameObject);
                        }
                    }
                }
            }
            string text = "Are you sure you want to sell \n\n<b> Selected Items </b>\n\n for <color=yellow><b>total: " + itemPrice + "</b></color> gold?";
            UpdateInfoPanel(text,true); // キャンセルボタンも表示する
            // OK、Cancelフラグの初期化
            isMethodOKExecuted = false;
            isMethodCancelExecuted = false;
        }
        else
        {
            string itemID = "0";
            // オブジェクトにSpriteRendererがアタッチされている場合
            if (sellObject.GetComponent<SpriteRenderer>() != null)
            {
                // オブジェクトの名前からIDを取得
                itemID = sellObject.name;
            }
            // オブジェクトにImageがアタッチされている場合
            else if (sellObject.GetComponent<Image>() != null)
            {
                // オブジェクトのspriteの名前から1つめの_と2つめの_の間のIDを取得
                itemID = sellObject.GetComponent<Image>().sprite.name.Split('_')[1];
            }
            // 半額の値段を取得
            itemPrice = GetPrice(int.Parse(itemID));
            // OK、Cancelフラグの初期化
            isMethodOKExecuted = false;
            isMethodCancelExecuted = false;

            itemIDList.Add(int.Parse(itemID));
            sellObjectList.Add(sellObject);
            
        }
        // OnClickOKButtonかOnClickCancelButtonが押されるまで待ってOKなら売る
        StartCoroutine(SubmitSell(sellObjectList, itemIDList, itemPrice));
    }

    /// <summary>
    /// itemIDを引数として、itemNameと金額を出力するメソッド
    /// </summary>
    public int GetPrice(int itemID)
    {
        // gameManagerのitemDataのEqpListDataクラス(eqpList)、WpnListDataクラス(wpnList)、RuneListDataクラス(runeList)からitemIDと一致するデータを取得
        ItemData.EqpListData eqpListData = gameManager.itemData.eqpList.Find(eqp => eqp.ID == itemID);
        ItemData.WpnListData wpnListData = gameManager.itemData.wpnList.Find(wpn => wpn.ID == itemID);
        ItemData.RuneListData runeListData = gameManager.itemData.runeList.Find(rune => rune.ID == itemID);
        // itemIDがeqpListDataのIDと一致する場合
        string itemName = "";
        int itemPrice = 0;
        if (eqpListData != null)
        {
            itemName = eqpListData.name;
            itemPrice = eqpListData.price;
        }
        // itemIDがwpnListDataのIDと一致する場合
        else if (wpnListData != null)
        {
            itemName = wpnListData.name;
            itemPrice = wpnListData.price;
        }
        // itemIDがruneListDataのIDと一致する場合
        else if (runeListData != null)
        {
            itemName = runeListData.name;
            itemPrice = runeListData.price;
        }
        // infoTextに"Are you sure you want to sell \n\n" + itemName(ボールド) + "\n\n for " + itemPrice / 2(ボールド黄色) + " gold?"を表示
        string text = "Are you sure you want to sell \n\n<b>" + itemName + "</b>\n\n for <color=yellow><b>" + itemPrice / 2 + "</b></color> gold?";
        UpdateInfoPanel(text,true); // キャンセルボタンも表示する
        return itemPrice / 2;
    }

    public IEnumerator SubmitSell(List<GameObject> sellObjectList, List<int> itemIDList, int Price)
    {
        // メソッドBまたはメソッドCが実行されるまで待機
        yield return new WaitUntil(() => isMethodOKExecuted || isMethodCancelExecuted);

        // OKボタンが押された場合、アイテムを売る
        if (isMethodOKExecuted)
        {
            // sellObjectListの要素数分繰り返す
            for (int i = 0; i < sellObjectList.Count; i++)
            {
                // sellObjectの親オブジェクトがiventryPanelの何番目の子オブジェクトかを取得
                int index = sellObjectList[i].transform.parent.GetSiblingIndex();
                // iventryItem[index]がitemIDと一致する場合、iventryItem[index]を0に設定
                if (iventryUI != null)
                {
                    // 7桁を6桁に変換して比較
                    if (iventryUI.IventryItem[index] / 10 == itemIDList[i])
                    {
                        iventryUI.IventryItem[index] = 0;
                    }
                }
            }
            // IventryPanelを更新
            iventryUI.UpdateIventryPanel();
            //goldをPrice分増やす
            statusLog.currentGold += Price;
            
            // buySEを再生
            AkSoundEngine.PostEvent("ST_Buy", gameObject);

            // ゴールドと経験値UIを更新
            gameManager.UpdateGoldAndExpUI();
            // 価格と所持金によってtotalPriceTextの色を変更する
            ChangeTotalPriceTextColor();
        }

        // フラグのリセット
        isMethodOKExecuted = false;
        isMethodCancelExecuted = false;
    }
    
}
