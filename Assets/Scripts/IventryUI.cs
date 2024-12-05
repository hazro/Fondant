using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using MyGame.Managers;

/// <summary>
/// イベントリーUI
/// </summary>
public class IventryUI : MonoBehaviour
{
    [SerializeField] private Button openIventryButton;
    [SerializeField] private Transform Iventry;
    [SerializeField] private Transform IventryImage;
    public Transform IventryPanel;

    private const int MaxItems = 48; // アイテムの最大数
    public int[] IventryItem = new int[MaxItems]; // を格納する配列

    // スキルパネルオブジェクトを格納するリスト
    public List<GameObject> IventrySkillList1 = new List<GameObject>();
    public List<GameObject> IventrySkillList2 = new List<GameObject>();
    public List<GameObject> IventrySkillList3 = new List<GameObject>();
    public List<GameObject> IventrySkillList4 = new List<GameObject>();
    public List<GameObject> IventrySkillList5 = new List<GameObject>();

    public List<Image> wpnDelayImage = new List<Image>();

    // informationPanel
    public GameObject infoPanel;
    public TextMeshProUGUI infoPanelText;
    public Button OKButton;
    public Button cancelButton;
    
    // IventryPanelの子オブジェクトをすべて取得
    private List<Transform> children = new List<Transform>();
    private GameManager gameManager; // GameManagerの参照
    private StatusAdjustmentManager statusAdjustmentManager; // StatusAdjustmentManagerの参照
    private BlackSmithManager blackSmishManager; // BlackSmishManagerの参照
    
    private void OnEnable()
    {
        // シーンがロードされるたびにOnSceneLoadedメソッドを呼び出す
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // イベントの登録を解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // GameManagerのインスタンスを取得
        gameManager = GameManager.Instance;
        // IventryPanelの子オブジェクトをchildrenに格納
        UpdateIventryPanelPosition();
    }

    // シーンがロードされるたびに呼び出されるメソッド
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーンがロードされるたびにStatusAdjustmentManagerを探す
        statusAdjustmentManager = FindObjectOfType<StatusAdjustmentManager>();
        blackSmishManager = FindObjectOfType<BlackSmithManager>();
    }

    // 選択をすべて解除するメソッド
    public void DeSelectedButton()
    {
        // iventryPanelの孫オブジェクトをすべて取得
        foreach (Transform child in IventryPanel)
        {
            foreach (Transform grandChild in child)
            {
                // grandChildのItemDandDHandlerコンポーネントを取得し、ItemSelect(true)を実行
                ItemDandDHandler item = grandChild.GetComponent<ItemDandDHandler>();
                item.ItemSelect(true);
            }
        }
    }


    // ボタンの有効無効切り替え
    public void SetButtonEnabled(bool enabled)
    {
        Iventry.gameObject.SetActive(enabled);
        openIventryButton.gameObject.SetActive(enabled);;
    }

    //ボタンを押したらイベントを開く
    public void OpenIventry()
    {
        //Iventryが非表示なら表示、表示なら非表示にする
        Iventry.gameObject.SetActive(!Iventry.gameObject.activeSelf);
    }

    // イベントパネルのTransformを取得(更新)
    public void UpdateIventryPanelPosition()
    {
        // childrenリストを初期化
        children.Clear();
        // IventryPanelの子オブジェクトをchildrenに格納
        foreach (Transform child in IventryPanel.transform)
        {
            children.Add(child);
        }
    }

    // ドロップしたアイテムをIventryItemに追加する runeLevelは1で初期化するが、itemIDが7桁ならばruneLevelを取得し、itemIDを6桁にする
    public void AddItem(int itemID)
    {
        int runeLevel = 1;
        // もしitemIDが7桁ならばruneLevelを取得し、itemIDを6桁にする
        if (itemID >= 1000000)
        {
            runeLevel = itemID % 10;
            itemID /= 10;
        }
        //Debug.Log("AddItemメソッド内、イベントリに入る itemID = " + itemID + " runeLevel = " + runeLevel);
        for (int i = 0; i < IventryItem.Length; i++)
        {
            if (IventryItem[i] == 0)
            {
                // 入手直後はruneLevel1の為itemIDを10倍して+1(runeLevel1)する
                IventryItem[i] = itemID*10+runeLevel;
                // itemID6桁の100000の位が1なら武器、2なら防具、4ならルーン
                if (itemID / 100000 == 1)
                {
                    // children[i]の子オブジェクトをすべて削除
                    foreach (Transform child in children[i])
                    {
                        Destroy(child.gameObject);
                    }
                    GameObject itemSprite = Resources.Load<GameObject>("Prefabs/Weapons/" + itemID);
                    // IventryItemObject[i]の子としてIventryItemObject[i]の位置にitemSpriteを生成
                    itemSprite = Instantiate(itemSprite, children[i].position, Quaternion.identity);
                    //名前から(Clone)を削除
                    itemSprite.name = itemSprite.name.Replace("(Clone)", "");
                    itemSprite.transform.SetParent(children[i]);
                }
                else if (itemID / 100000 == 2)
                {
                    // children[i]の子オブジェクトをすべて削除
                    foreach (Transform child in children[i])
                    {
                        Destroy(child.gameObject);
                    }
                    GameObject itemSprite = Resources.Load<GameObject>("Prefabs/Equipment/" + itemID);
                    // IventryItemObject[i]の子としてIventryItemObject[i]の位置にitemSpriteを生成
                    itemSprite = Instantiate(itemSprite, children[i].position, Quaternion.identity);
                    //名前から(Clone)を削除
                    itemSprite.name = itemSprite.name.Replace("(Clone)", "");
                    itemSprite.transform.SetParent(children[i]);
                }
                else if (itemID / 100000 == 4)
                {
                    // children[i]の子オブジェクトをすべて削除
                    foreach (Transform child in children[i])
                    {
                        Destroy(child.gameObject);
                    }
                    GameObject itemSprite = Resources.Load<GameObject>("Prefabs/Runes/" + itemID);
                    // IventryItemObject[i]の子としてIventryItemObject[i]の位置にitemSpriteを生成
                    itemSprite = Instantiate(itemSprite, children[i].position, Quaternion.identity);
                    //名前から(Clone)を削除
                    itemSprite.name = itemSprite.name.Replace("(Clone)", "");
                    itemSprite.transform.SetParent(children[i]);
                }
                break;
            }
        }
        // イベントリをソート
        //StartCoroutine(IventrySortingCoroutine(null));
    }

    // 指定UnitのSkillリストを更新し、キャラの装備を変更する
    public void UpdateUnitSkillUI(GameObject unitObject)
    {
        // unitObjectがnullならエラーログを出力して終了
        if (unitObject == null)
        {
            Debug.LogError("unitObject is null");
            return;
        }

        // unitObjectからUnitコンポーネントを取得
        Unit unit = unitObject.GetComponent<Unit>();
        if (unit == null)
        {
            Debug.LogError("Unit component not found on unitObject");
            return;
        }

        // unit番号の取得
        // unitObjectの名前に(Clone)がついている場合は削除する
        if (unitObject.name.Contains("(Clone)"))
        {
            unitObject.name = unitObject.name.Replace("(Clone)", "");
        }

        // 該当のIventrySkillListを取得
        List<List<GameObject>> IventrySkillLists = new List<List<GameObject>>{IventrySkillList1,IventrySkillList2,IventrySkillList3,IventrySkillList4,IventrySkillList5};
        List<GameObject> IventrySkillList = IventrySkillLists[unit.ID-1];

        // IventrySkillListの孫オブジェクトをすべて削除
        foreach (GameObject child in IventrySkillList)
        {
            foreach (Transform grandChild in child.transform)
            {
                if(grandChild != null)
                {
                    // 孫オブジェクトの名前がtxNameやwakuでなければ削除
                    if (grandChild.name != "txName" && grandChild.name != "waku" && grandChild.name != "collider")
                    {
                        Destroy(grandChild.gameObject);
                    } 
                }
            }
        }

        GameObject wpnImage = Resources.Load<GameObject>("Prefabs/Weapons/" + unit.currentWeapons.ToString("D6"));
        GameObject shieldImage = Resources.Load<GameObject>("Prefabs/Equipment/" + unit.currentShields.ToString("D6"));
        GameObject armorImage = Resources.Load<GameObject>("Prefabs/Equipment/" + unit.currentArmor.ToString("D6"));
        GameObject accsseImage = Resources.Load<GameObject>("Prefabs/Equipment/" + unit.currentAccessories.ToString("D6"));
        
        string mainSoketId = "411000";
        if(unit.mainSocket != 0) mainSoketId = (unit.mainSocket / 10).ToString("D6");
        GameObject mainSockettImage = Resources.Load<GameObject>("Prefabs/Runes/" + mainSoketId);
        string subSocketId = "412000";
        if (unit.subSocket[0] != 0) subSocketId = (unit.subSocket[0] / 10).ToString("D6");
        GameObject socket01Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[1] != 0) subSocketId = (unit.subSocket[1] / 10).ToString("D6");
        GameObject socket02Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[2] != 0) subSocketId = (unit.subSocket[2] / 10).ToString("D6");
        GameObject socket03Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[3] != 0) subSocketId = (unit.subSocket[3] / 10).ToString("D6");
        GameObject socket04Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[4] != 0) subSocketId = (unit.subSocket[4] / 10).ToString("D6");
        GameObject socket05Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[5] != 0) subSocketId = (unit.subSocket[5] / 10).ToString("D6");
        GameObject socket06Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[6] != 0) subSocketId = (unit.subSocket[6] / 10).ToString("D6");
        GameObject socket07Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[7] != 0) subSocketId = (unit.subSocket[7] / 10).ToString("D6");
        GameObject socket08Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[8] != 0) subSocketId = (unit.subSocket[8] / 10).ToString("D6");
        GameObject socket09Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[9] != 0) subSocketId = (unit.subSocket[9] / 10).ToString("D6");
        GameObject socket10Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[10] != 0) subSocketId = (unit.subSocket[10] / 10).ToString("D6");
        GameObject socket11Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);

        // IventrySkillList[0]の子オブジェクトのTextMeshProコンポーネントを取得
        TextMeshProUGUI unitNameText = IventrySkillList[0].GetComponentInChildren<TextMeshProUGUI>();
        // 現在のフォントサイズを取得
        float originalFontSize = unitNameText.fontSize;
        // 75%のサイズを計算
        float reducedFontSize = originalFontSize * 0.75f;

        // unitNameTextにunitの名前とレベルを設定 (レベルは小さく表示)
        unitNameText.text = unit.unitName + "\n" + "<size={" + reducedFontSize + "}>" + "Lv." + unit.currentLevel + "</size>";

        // IventrySkillList[1]の子オブジェクトのImageコンポーネントを取得し、スプライトを設定
        Sprite sprite = wpnImage.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[1].GetComponent<Image>().sprite = sprite;
        // 武器のdelayImageにもスプライトを設定
        wpnDelayImage[unitObject.GetComponent<Unit>().ID-1].sprite = sprite;

        // 顔グラ設定
        IventrySkillList[2].GetComponent<Image>().sprite = unit.unitSprite.sprite;
        sprite = shieldImage.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[3].GetComponent<Image>().sprite = sprite;
        sprite = armorImage.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[4].GetComponent<Image>().sprite = sprite;
        sprite = accsseImage.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[5].GetComponent<Image>().sprite = sprite;
        sprite = mainSockettImage.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[6].GetComponent<Image>().sprite = sprite;
        sprite = socket01Image.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[7].GetComponent<Image>().sprite = sprite;
        sprite = socket02Image.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[8].GetComponent<Image>().sprite = sprite;
        sprite = socket03Image.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[9].GetComponent<Image>().sprite = sprite;
        sprite = socket04Image.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[10].GetComponent<Image>().sprite = sprite;
        sprite = socket05Image.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[11].GetComponent<Image>().sprite = sprite; 
        sprite = socket06Image.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[12].GetComponent<Image>().sprite = sprite;
        sprite = socket07Image.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[13].GetComponent<Image>().sprite = sprite;
        sprite = socket08Image.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[14].GetComponent<Image>().sprite = sprite;
        sprite = socket09Image.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[15].GetComponent<Image>().sprite = sprite;
        sprite = socket10Image.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[16].GetComponent<Image>().sprite = sprite;
        sprite = socket11Image.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[17].GetComponent<Image>().sprite = sprite;

        // 武器のステータスを取得
        ItemData.WpnListData wpnListData = gameManager.itemData.wpnList.Find(x => x.ID == unit.currentWeapons);
        if (wpnListData == null)
        {
            Debug.LogError("unit name: " + unitObject.name + " currentWeapons: " + unit.currentWeapons + " wpnListData is not assigned.");
            return;
        }

        // ルーンレベルをunitからスキルパネルとマテリアルに設定
        for(int i = 6; i < IventrySkillList.Count; i++)
        {
            // unitからルーンレベルを取得　iが6ならmainSocket、7~17ならsubSocket
            int runeLevel = i == 6 ? unit.mainSocket % 10 : unit.subSocket[i-7] % 10;
            ItemDandDHandler item = IventrySkillList[i].GetComponentInChildren<ItemDandDHandler>();
            // 固定ルーンの場合はItemのドラッグを無効にする
            item.isFixItem = false;
            if (wpnListData.fixMainRune != 0 && i == 6) item.isFixItem = true;
            if (wpnListData.fixSubRune1 != 0 && i == 7) item.isFixItem = true;
            if (wpnListData.fixSubRune2 != 0 && i == 8) item.isFixItem = true;
            if (wpnListData.fixSubRune3 != 0 && i == 9) item.isFixItem = true;
            
            item.runeLevel = runeLevel;
            Image imageComponent = IventrySkillList[i].GetComponent<Image>();
            // Materialをinstance化してitemのruneLevelを設定
            Material materialInstance = new Material(imageComponent.material);
            imageComponent.material = materialInstance;
            materialInstance.SetFloat("_Lv", runeLevel);
        }

        //socketCountの取得
        int socketCount = wpnListData.socketCount;
        //IventrySkillList[6 ~ 6+socketCount]の親オブジェクトを表示し、IventrySkillList[6+socketCount+1]以降の親オブジェクトを非表示にする
        for (int i = 6; i < IventrySkillList.Count; i++)
        {
            if (i < 6 + socketCount)
            {
                IventrySkillList[i].transform.parent.gameObject.SetActive(true);
            }
            else
            {
                IventrySkillList[i].transform.parent.gameObject.SetActive(false);
            }
        }
    }
    
    // ソートボタンを押したらアイテムをソートし、アイテムを入れ替える 
    public void IventrySortingButton()
    {
        // 強化武器がセットされている場合はソートできない
        if (blackSmishManager != null && blackSmishManager.isSetItem)
        {
            return;
        }
        // ClickSoundを再生
        AkSoundEngine.PostEvent("ST_Click", gameObject);

        // IventryItemをソート
        IventrySorting();
        // IventryPanelの画像をすべて再設定
        UpdateIventryPanel();
    }

    // iventryItemをソートするメソッド
    public void IventrySorting()
    {
        // IventryItemの要素を昇順に並べ替える
        Array.Sort(IventryItem);
        // IventryItemの要素を順番に取り出し、0で有れば0以外の要素が出るまで繰り返し、0以外の要素が出たらSwapItemメソッドで要素を入れ替える
        for (int i = 0; i < IventryItem.Length; i++)
        {
            if (IventryItem[i] == 0)
            {
                for (int j = i + 1; j < IventryItem.Length; j++)
                {
                    if (IventryItem[j] != 0)
                    {
                        SwapItem(i, j);
                        break;
                    }
                }
            }
        } 
    }

    // 装備を変更するメソッド(ItemDandDHandlerから呼び出される)
    public void changeEquipment(GameObject targetCharNameObj, string iventryItemID, string skillItemID ,string socketName, GameObject originalObj, GameObject targetObj, int skillItemLv, int iventryItemLv)
    {
        // GameManagerがアクティブでない場合は処理を終了
        if (gameManager == null)
        {
            Debug.LogError("gameManager is null");
            return;
        }

        // IventrySkillListいくつに該当するかを取得
        List<GameObject> iventrySkillList = new List<GameObject>();
        Unit unit = null;
        // 事前にリストを用意
        List<List<GameObject>> IventrySkillLists = new List<List<GameObject>>{IventrySkillList1,IventrySkillList2,IventrySkillList3,IventrySkillList4,IventrySkillList5};
        // 該当unitと該当skillListを取得
        for(int i = 0; i < IventrySkillLists.Count; i++)
        {
            if (IventrySkillLists[i][0] == targetCharNameObj)
            {
                iventrySkillList = IventrySkillLists[i];
                unit = gameManager.livingUnits[i].GetComponent<Unit>();
                break;
            }
        }
        Debug.Log(" unitName = " + unit.unitName);
        Debug.Log("iventryItemID = " + iventryItemID + " skillItemID = " + skillItemID);
        Debug.Log("originalObj = " + originalObj.name);
        Debug.Log("skillItemLv = " + skillItemLv + " iventryItemLv = " + iventryItemLv);

        if(iventryItemID[0] == '1' && skillItemID[0] == '1')
        {
            bool isShildTakeOff = false; // 両手持ちの為に盾を外すか
            bool isFixSubRuneTakeOff = false; // 固定ルーンによるソケット差分の固定サブルーンを外すか
            int removeSubRuneCount = 0; // 外れるサブルーンの数
            bool isFixMainRuneTakeOff = false; // 固定ルーンによるソケット差分の固定メインルーンを外すか
            int removeMainRuneCount = 0; // 外れるメインルーンの数
            bool isSubRuneTakeOff = false; // ソケット差分のサブルーンを外すか
            List<int> diffRuneList = new List<int>();
            int removeItemCount = 0; // 外れる装備の総数
            int eqpWpnMainCount = 0; // 装備中の武器の固定メインルーンの数
            int eqpWpnSubCount = 0; // 装備中の武器の固定サブルーンの数

            print("武器を入れ替えます");
            // skillItemID[2]が1又は3又は4又は6ならば両手持ち武器なので、盾を装備していたら外してイベントリに追加する
            if (skillItemID[2] == '1' || skillItemID[2] == '3' || skillItemID[2] == '4' || skillItemID[2] == '6')
            {   
                // 盾を装備している場合
                if (unit.currentShields != 0)
                {
                    removeItemCount++;
                    // イベントリに空きがあれば盾をイベントリに追加フラグを立てる
                    isShildTakeOff = true;
                }
            }
            // 固定のルーンを持つ武器の場合
            ItemData.WpnListData wpnListData = gameManager.itemData.wpnList.Find(x => x.ID == int.Parse(skillItemID));
            ItemData.WpnListData originalWpnListData = gameManager.itemData.wpnList.Find(x => x.ID == int.Parse(iventryItemID));
            if (wpnListData != null)
            {
                int mainSocketCount = 0; // 変更する武器の固定メインルーンの数
                int subSocketCount = 0; // 変更する武器の固定サブルーンの数
                int eqpMainCount = 0; // 装備中のメインルーンの
                int eqpSubCount = 0; // 装備中のサブルーン[1~3]スロットにセットされている数

                // 固定のルーンの数を取得
                if (wpnListData.fixMainRune != 0) mainSocketCount++;
                if (originalWpnListData.fixMainRune != 0) eqpWpnMainCount++;
                for (int i = 0; i < 3; i++)
                {
                    switch (i)
                    {
                        case 0:
                            if (wpnListData.fixSubRune1 != 0) subSocketCount++; // 変更する武器の固定サブルーンの数を加算
                            if (originalWpnListData.fixSubRune1 != 0) eqpWpnSubCount++; // 装備中の武器の固定サブルーンの数を加算
                            break;
                        case 1:
                            if (wpnListData.fixSubRune2 != 0) subSocketCount++;
                            if (originalWpnListData.fixSubRune2 != 0) eqpWpnSubCount++;
                            break;
                        case 2:
                            if (wpnListData.fixSubRune3 != 0) subSocketCount++;
                            if (originalWpnListData.fixSubRune3 != 0) eqpWpnSubCount++;
                            break;
                    }
                }

                // 装備中のルーンの数を取得
                if (unit.mainSocket != 0) eqpMainCount++;
                for (int i = 0; i < 3; i++)
                {
                    if (unit.subSocket[i] != 0) eqpSubCount++;
                }
                
                // 外れるルーンの数を計算
                removeMainRuneCount = (mainSocketCount + eqpMainCount >= 2) ? 1 : 0; // 固定メインルーンの数によって外れるルーンを計算
                removeSubRuneCount =  (subSocketCount + eqpSubCount >= 2) ? Math.Min(subSocketCount, eqpSubCount) : 0; // 固定サブルーンによって外れるルーンの数を計算

                removeMainRuneCount += eqpWpnMainCount; // 外れる装備中の固定メインルーンの数
                removeSubRuneCount +=  eqpWpnSubCount; //  外れる装備中の固定サブルーンの数

                removeItemCount += removeMainRuneCount + removeSubRuneCount;
                if (removeSubRuneCount > 0) isFixSubRuneTakeOff = true;
                if (removeMainRuneCount > 0) isFixMainRuneTakeOff = true;
                
                Debug.Log("wpnName: " + wpnListData.name);
                Debug.Log("mainSocketCount(変更する武器の固定メインルーンの数) = " + mainSocketCount + " subSocketCount(変更する武器の固定サブルーンの数) = " + subSocketCount +
                 "\n eqpWpnMainCount(装備中の武器の固定メインルーンの数) = " + eqpWpnMainCount + " eqpWpnSubCount(装備中の武器の固定サブルーンの数) = " + eqpWpnSubCount +
                 "\n 装備中の武器が固定ルーンだったら消す。 固定ルーンじゃなかったら消してイベントリに移す" +
                 "\n eqpMainCount(装備中のメインルーンの数) = " + eqpMainCount + " eqpSubCount(装備中のサブルーン[1~3]スロットにセットされている数) = " + eqpSubCount +
                 "\n removeMainRuneCount(外す固定メインルーンの数) = " + removeMainRuneCount + " removeSubRuneCount(外す固定サブルーンの数) = " + removeSubRuneCount +
                 "\n " + (isFixSubRuneTakeOff ? "固定サブルーンを外します" : "固定サブルーンは外しません") + 
                 (isFixMainRuneTakeOff ? " 固定メインルーンを外します" : " 固定メインルーンは外しません"));
            }

            // 元の武器のソケット数を取得 gameManager.itemData.wpnListのIDがunit.currentWeaponsのものからsocketCountを取得
            int originalSocketCount = gameManager.itemData.wpnList.Find(x => x.ID == unit.currentWeapons).socketCount;
            // 新しい武器のソケット数を取得 gameManager.itemData.wpnListのIDがint.Parse(skillItemID)のものからsocketCountを取得
            int newSocketCount = gameManager.itemData.wpnList.Find(x => x.ID == int.Parse(skillItemID)).socketCount;

            List<GameObject> IventrySkillList = null;
            // unit.IDから該当するiventrySkillListを取得
            switch (unit.ID)
            {
                case 1:
                    IventrySkillList = IventrySkillList1;
                    break;
                case 2:
                    IventrySkillList = IventrySkillList2;
                    break;
                case 3:
                    IventrySkillList = IventrySkillList3;
                    break;
                case 4:
                    IventrySkillList = IventrySkillList4;
                    break;
                case 5:
                    IventrySkillList = IventrySkillList5;
                    break;
                default:
                    Debug.LogError("unitID is invalid");
                    return;
            }

            // 武器の元ソケットの方が多かった場合、外すソケット数を計算
            if (originalSocketCount > newSocketCount)
            {
                // 差分のソケットに入っているサブルーンを取得(idが0なら空)
                for (int i = newSocketCount - 1; i < originalSocketCount - 1; i++)
                {
                    if(unit.subSocket[i] != 0)
                    {
                        diffRuneList.Add(unit.subSocket[i]);
                        //Debug.Log("このルーンを外します subSoketからそのまま出力した場合" + unit.subSocket[i] + "diffListの最後に追加されたID" + diffRuneList[diffRuneList.Count-1] + "\n\n");
                    }
                }
                if (diffRuneList.Count > 0) isSubRuneTakeOff = true;
                removeItemCount += diffRuneList.Count;
            }

            // イベントリに空きがあるか確認し、空きがなければエラーログを出力して終了
            if (CountEmptySlots() < removeItemCount)
            {
                // エラーSEを再生
                AkSoundEngine.PostEvent("ST_Error", gameObject);

                infoPanelText.text = "The change of weapon is <color=red><b> cancelled </b></color> \nNot enough iventry space. \n (Need to have <color=yellow>" + removeItemCount + " slots</color> available)";
                infoPanel.SetActive(true);
                OKButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(false);
                // OKボタンを押したらinfoPanel.SetActive(false)にする;
                OKButton.onClick.AddListener(() => infoPanel.SetActive(false));
                return;
            }

            // シールドを装備から外してイベントリに追加する
            if (isShildTakeOff)
            {
                AddItem(unit.currentShields);
                unit.currentShields = 0;
            }
            // サブルーンの差分ソケット分を外してイベントリに追加する
            if (isSubRuneTakeOff)
            {
                foreach (int runeID in diffRuneList)
                {
                    //Debug.Log("AddItemメソッドでイベントリに入れるruneID = " + runeID + "これで違うルーンになればAddItemメソッドがおかしい");
                    AddItem(runeID);
                }
                for (int i = newSocketCount - 1; i < originalSocketCount - 1; i++)
                {
                    unit.subSocket[i] = 0;
                    IventrySkillList[7 + i].gameObject.GetComponent<ItemDandDHandler>().runeLevel = 0;
                }
            }
            // 固定ルーンのメインルーンを外してイベントリに追加する
            if (isFixMainRuneTakeOff)
            {
                // 装備中の武器が固定メインルーンでない場合は外す(イベントリに移動)　そうでない場合は移動せず削除
                if (eqpWpnMainCount == 0)
                {
                    AddItem(unit.mainSocket);
                }
                unit.mainSocket = 0;
                IventrySkillList[6].gameObject.GetComponent<ItemDandDHandler>().runeLevel = 0;
                
                Debug.Log("固定メインルーンを外しました" + (eqpWpnMainCount == 0 ? "元装備が固定メインルーンでないのでイベントリに移動" : "元装備が固定メインルーンなので削除"));
            }
            // 固定ルーンのサブルーンを外してイベントリに追加する
            ////// ＊＊＊＊＊＊　固定ルーンの装備はUnit.updateStatus()で処理されるため、ここでは処理しない（理由初期装備や、敵にも適用したいため）　＊＊＊＊＊＊ ///////
            if (isFixSubRuneTakeOff)
            {
                for(int i = 0; i < removeSubRuneCount; i++)
                {
                    // 装備中の武器が固定サブルーンでない場合は外す(イベントリに移動) そうでない場合は移動せず削除
                    if(eqpWpnSubCount-1 < i)
                    {
                        AddItem(unit.subSocket[i]);
                    }
                    unit.subSocket[i] = 0;
                    IventrySkillList[7 + i].gameObject.GetComponent<ItemDandDHandler>().runeLevel = 0;
                    Debug.Log(i+1 + "/" + removeSubRuneCount + "つ目の固定サブルーンを外しました" + (eqpWpnSubCount-1 < i ? "元装備が固定サブルーンでないのでイベントリに移動" : "元装備が固定サブルーンなので削除"));
                }
            }
            
            // アイテムIDをunitのcurrentWeaponsに設定
            unit.currentWeapons = int.Parse(skillItemID);
        }
        // 名前の0番目が2で1番目が1で2番目が1ならばシールド
        else if(iventryItemID[0] == '2' && iventryItemID[1] == '1' && iventryItemID[2] == '1' && skillItemID[0] == '2' && skillItemID[1] == '1' && skillItemID[2] == '1')
        {
            print(" 盾を入れ替えます ");
            unit.currentShields = int.Parse(skillItemID);
        }
        // 名前の0番目が2で1番目が1で2番目が2~4のいずれかならば防具
        else if (iventryItemID[0] == '2' && iventryItemID[1] == '1' && (iventryItemID[2] >= '2' && iventryItemID[2] <= '4') && skillItemID[0] == '2' && skillItemID[1] == '1' && (skillItemID[2] >= '2' && skillItemID[2] <= '4'))
        {
            print("防具を入れ替えます");
            unit.currentArmor = int.Parse(skillItemID);
        }
        // 名前の0番目が2で1番目が1で2番目が5ならばアクセサリ
        else if(iventryItemID[0] == '2' && iventryItemID[1] == '1' && iventryItemID[2] == '5' && skillItemID[0] == '2' && skillItemID[1] == '1' && skillItemID[2] == '5')
        {
            print("アクセサリを入れ替えます");
            unit.currentAccessories = int.Parse(skillItemID);
        }
        // 名前の0番目が4で1番目が1で2番目が1ならばメインルーン
        else if(iventryItemID[0] == '4' && iventryItemID[1] == '1' && iventryItemID[2] == '1' && skillItemID[0] == '4' && skillItemID[1] == '1' && skillItemID[2] == '1')
        {
            print("メインルーンを入れ替えます");
            unit.mainSocket = int.Parse(skillItemID) * 10 + skillItemLv;
            
        }
        // 名前の0番目が4かつ1番目が1で2番目が1でなければサブルーン、image.nameがsubSocket0~11ならばサブルーン
        else if(iventryItemID[0] == '4'
            && iventryItemID[1] == '1'
            && iventryItemID[2] != '1'
            && skillItemID[0] == '4'
            && skillItemID[1] == '1'
            && skillItemID[2] != '1')
        {
            print("サブルーンを入れ替えます");
            print ("socketName = " + socketName);
            // どのサブルーンを入れ替えるかを取得
            int socketId = int.Parse(socketName.Substring(socketName.Length - 2)) - 1;
            unit.subSocket[socketId] = int.Parse(skillItemID) * 10 + skillItemLv;
        }
        // SkillからIventryに移動した場合、移動元のアイテムIDが0(空)なら装備を外す
        else if(skillItemID[0] == '0')
        {
            print("スキルスロットの装備を外します");
            // originalObjのオブジェクト名を変数に代入
            if(originalObj.name == "charWpn")
            {
                // エラーSEを再生
                AkSoundEngine.PostEvent("ST_Error", gameObject);
                
                //unit.currentWeapons = 0;
                // 武器は外せない
                Debug.Log("武器は交換のみで外せません");
                // originalObjを移動前の位置に戻す
                //originalObj.transform.position = targetObj.transform.position;
                // 武器を振るわせる
                ShakeObject(originalObj);
                return;
            }
            else if(originalObj.name == "charShild")
            {
                unit.currentShields = 0;
            }
            else if(originalObj.name == "charArmor")
            {
                unit.currentArmor = 0;
            }
            else if(originalObj.name == "charAccsse")
            {
                unit.currentAccessories = 0;
            }
            else if(originalObj.name == "mainSocket")
            {
                unit.mainSocket = 0;
            }
            //socketという文字列が含まれている場合はサブルーン
            else if(originalObj.name.Contains("socket"))
            {
                // どのサブルーンを外すかを取得
                int socketId = int.Parse(socketName.Substring(socketName.Length - 2)) - 1;
                unit.subSocket[socketId] = 0;
            }
        }
        // IventryからSkillに移動した場合、ターゲットのskillPanelが空の場合は装備を移す
        else if(iventryItemID[0] == '0')
        {
            Debug.Log("スキルスロットに装備します");
            // originalObjのオブジェクト名を変数に代入
            if(targetObj.transform.parent.name == "charWpn")
            {
                unit.currentWeapons = int.Parse(skillItemID);
            }
            else if(targetObj.transform.parent.name == "charShild")
            {
                unit.currentShields = int.Parse(skillItemID);
            }
            else if(targetObj.transform.parent.name == "charArmor")
            {
                unit.currentArmor = int.Parse(skillItemID);
            }
            else if(targetObj.transform.parent.name == "charAccsse")
            {
                unit.currentAccessories = int.Parse(skillItemID);
            }
        }
        else
        {
            Debug.Log("アイテムIDが不正です");
            return;
        }
        // IventryからSkillに移動した場合、ターゲットのskillPanelに対してIventryのルーンレベルを移す
        if(targetObj.transform.parent.GetComponent<ItemDandDHandler>())
        {
            ItemDandDHandler item = targetObj.transform.parent.GetComponent<ItemDandDHandler>();
            item.runeLevel = skillItemLv;
        }
        // SkillからIventryに移動して交換した場合、移動元のSkillに対してIventryのルーンレベルを移す
        else
        {
            ItemDandDHandler item = originalObj.GetComponent<ItemDandDHandler>();
            item.runeLevel = skillItemLv;

        }
        
        // オブジェクトを移動し、イベントリとスキルパネルを更新する
        MoveAndUpdate(originalObj, targetObj, iventryItemID, unit.gameObject, iventryItemLv);

        // 装備変更SEを再生
        AkSoundEngine.PostEvent("ST_ChangeEqp", gameObject);
        
        // unitのステータスを更新
        unit.updateStatus();

        // ステータス画面UIの表示を更新
        if (statusAdjustmentManager != null)
        {
            statusAdjustmentManager.UpdateUI();
        }
    }

    /// <summary>
    /// イベントリの空きスロットの数をカウントするメソッド
    /// </summary>
    /// <returns>空きスロットの数</returns>
    private int CountEmptySlots()
    {
        int emptySlotCount = 0;
        foreach (int item in IventryItem)
        {
            if (item == 0)
            {
                emptySlotCount++;
            }
        }
        return emptySlotCount;
    }

    /// <summary>
    /// 指定のオブジェクトを0.5秒の間に振るわせて元に戻すコルーチン
    /// </summary>
    private void ShakeObject(GameObject obj)
    {
        // 指定のオブジェクトを0.5秒の間に振るわせる
        StartCoroutine(ShakeObjectCoroutine(obj));
    }
    /// <summary>
    /// ShakeObjectCoroutineのコルーチン
    /// </summary>
    private IEnumerator ShakeObjectCoroutine(GameObject obj)
    {
        // 振るわせる時間
        float shakeTime = 0.25f;
        // 振るわせる時間の経過時間
        float elapsedTime = 0f;
        // 振るわせる振幅
        float shakeAmount = 0.1f;

        // 元の位置を保存
        Vector3 originalPosition = obj.transform.position;

        // 振るわせる時間の間繰り返す
        while (elapsedTime < shakeTime)
        {
            // 振幅を計算
            float x = UnityEngine.Random.Range(-1f, 1f) * shakeAmount;
            float y = UnityEngine.Random.Range(-1f, 1f) * shakeAmount;
            
            // 元の位置に対して振幅を設定
            obj.transform.position = originalPosition + new Vector3(x, y, 0);

            // 経過時間を加算
            elapsedTime += Time.deltaTime;
            
            // 1フレーム待つ
            yield return null;
        }

        // 元の位置に戻す
        obj.transform.position = originalPosition;
    }

    /// <summary>
    /// オブジェクトを移動し、イベントリとスキルパネルを更新する関数
    /// </summary>
    /// <param name="originalObj">元のオブジェクト</param>
    /// <param name="targetObj">ターゲットオブジェクト</param>
    /// <param name="iventryItemID">イベントリアイテムID</param>
    /// <param name="unitObj">ユニットオブジェクト</param>
    public void MoveAndUpdate(GameObject originalObj, GameObject targetObj, string iventryItemID, GameObject unitObj, int iventryItemLv)
    {
        // Imageコンポーネントがなければ originalObjを設定、それ以外はtargetObjを設定
        GameObject obj = originalObj.GetComponent<Image>() == null ? originalObj : targetObj;

        if (obj != null)
        {
            // 移動先のオブジェクトを消す
            Destroy(obj);
            // 移動先のオブジェクトの親がiventryPanelの何番目の子かを取得 (objの親の名前の下から2文字を取得し、intに変換し-1する)
            int index = int.Parse(obj.transform.parent.name.Substring(obj.transform.parent.name.Length - 2)) - 1;
            // SkikkListのメインルーンやサブルーンが空の場合、IDを0にする
            if(iventryItemID== "411000" || iventryItemID == "412000") iventryItemID = "0";
            // IventryItemのindex番目にアイテムIDを設定
            SetItem(index, int.Parse(iventryItemID), iventryItemLv);
        }

        // イベントリを入れ替える
        UpdateIventryPanel();
        // スキルパネルの更新
        UpdateUnitSkillUI(unitObj);
    }

    // イベントリのアイテムをすべて削除するメソッド(townに戻った時などの初期化に)
    public void ClearItem()
    {
        // IventryItemのすべての要素を0にする
        for (int i = 0; i < IventryItem.Length; i++)
        {
            IventryItem[i] = 0;
        }
        // IventryPanelの孫オブジェクトをすべて削除
        foreach (Transform child in IventryPanel)
        {
            foreach (Transform grandChild in child)
            {
                Destroy(grandChild.gameObject);
            }
        }
        UpdateIventryPanel();
    }

    // ivenrtyItemの指定番号と指定番号の要素内容を入れ替えるメソッド
    public void SwapItem(int index1, int index2)
    {
        int temp = IventryItem[index1];
        IventryItem[index1] = IventryItem[index2];
        IventryItem[index2] = temp;
    }

    //  ivenrtyItemの指定番号に指定の要素を代入するメソッド
    public void SetItem(int index, int itemID, int itemLv)
    {
        // itemIDが0ならIventryItem[index]に0を代入(空きのskillPanelにアイテムを移動した場合)
        if(itemID == 0)
        {
            IventryItem[index] = 0;
            return;
        }
        // itemIDが0でなければIventryItem[index]にitemIDとitemLvを代入
        IventryItem[index] = (itemID * 10) + itemLv;
    }

    // iventryPanelの孫オブジェクトをすべて削除し、IventryItemのオブジェクトに対応するアイテムを生成するメソッド
    public void UpdateIventryPanel()
    {
        // IventryPanelの孫オブジェクトをすべて削除
        foreach (Transform child in IventryPanel)
        {
            foreach (Transform grandChild in child)
            {
                Destroy(grandChild.gameObject);
            }
        }
        
        // IventryItemの要素を順番に取り出し、0でなければアイテムを生成
        for (int i = 0; i < IventryItem.Length; i++)
        {
            if (IventryItem[i] != 0)
            {
                // IventryItem[i]の1の位(1レベル情報)を削除
                int  itemID = IventryItem[i] / 10;
                int itemLv = IventryItem[i] % 10;

                // itemID6桁の100000の位が1なら武器、2なら防具、4ならルーン
                if (itemID / 100000 == 1)
                {
                    GameObject itemSprite = Resources.Load<GameObject>("Prefabs/Weapons/" + itemID);
                    // IventryItemObject[i]の子としてIventryItemObject[i]の位置にitemSpriteを生成
                    itemSprite = Instantiate(itemSprite, children[i].position, Quaternion.identity);
                    //名前から(Clone)を削除
                    itemSprite.name = itemSprite.name.Replace("(Clone)", "");
                    itemSprite.transform.SetParent(children[i]);
                    // itemLvを設定
                    itemSprite.GetComponent<ItemDandDHandler>().runeLevel = itemLv;
                }
                else if (itemID / 100000 == 2)
                {
                    GameObject itemSprite = Resources.Load<GameObject>("Prefabs/Equipment/" + itemID);
                    // IventryItemObject[i]の子としてIventryItemObject[i]の位置にitemSpriteを生成
                    itemSprite = Instantiate(itemSprite, children[i].position, Quaternion.identity);
                    //名前から(Clone)を削除
                    itemSprite.name = itemSprite.name.Replace("(Clone)", "");
                    itemSprite.transform.SetParent(children[i]);
                    // itemLvを設定
                    itemSprite.GetComponent<ItemDandDHandler>().runeLevel = itemLv;
                }
                else if (itemID / 100000 == 4)
                {
                    GameObject itemSprite = Resources.Load<GameObject>("Prefabs/Runes/" + itemID);
                    // IventryItemObject[i]の子としてIventryItemObject[i]の位置にitemSpriteを生成
                    itemSprite = Instantiate(itemSprite, children[i].position, Quaternion.identity);
                    //名前から(Clone)を削除
                    itemSprite.name = itemSprite.name.Replace("(Clone)", "");
                    itemSprite.transform.SetParent(children[i]);
                    // itemLvを設定
                    itemSprite.GetComponent<ItemDandDHandler>().runeLevel = itemLv;
                }
            }
            // 0の場合は空のパネルを生成
            else
            {
                int emptyPanelID = 0;
                GameObject emptyPanel = Resources.Load<GameObject>("Prefabs/" + emptyPanelID.ToString("D6"));
                // IventryItemObject[i]の子としてIventryItemObject[i]の位置にemptyPanelを生成
                emptyPanel = Instantiate(emptyPanel, children[i].position, Quaternion.identity);
                //名前から(Clone)を削除
                emptyPanel.name = emptyPanel.name.Replace("(Clone)", "");
                emptyPanel.transform.SetParent(children[i]);
                // itemLvを設定
                //emptyPanel.GetComponent<ItemDandDHandler>().runeLevel = 0;
            }
        }
    }

    /// <summary>
    /// 全UnitのスキルリストがspriteIDを装備可能な場合、スキルリストの枠の色を黄色くする
    /// </summary>
    public void ChangeSkillListFrameColor(string spriteID)
    {
        if(gameManager != null)
        {
            // IventrySkillList1~5の各リストの要素を取得
            foreach (GameObject unitObj in gameManager.livingUnits)
            {
                Unit unit = unitObj.GetComponent<Unit>();
                // 死亡中でない場合
                if (unit != null && !unit.condition[0])
                {
                    // joblistからunit.jonに該当するIDのjobDataを取得
                    ItemData.JobListData jobListData = gameManager.itemData.jobList.Find(x => x.ID == unit.job);
                    // IventrySkillList1~5の番号がunitNumのものを取得
                    List<GameObject> IventrySkillList = null;
                    // unitObj.nameの下2文字を取得し、intに変換
                    int unitNum = int.Parse(unitObj.name.Substring(unitObj.name.Length - 2));
                    switch (unitNum)
                    {
                        case 1:
                            IventrySkillList = IventrySkillList1;
                            break;
                        case 2:
                            IventrySkillList = IventrySkillList2;
                            break;
                        case 3:
                            IventrySkillList = IventrySkillList3;
                            break;
                        case 4:
                            IventrySkillList = IventrySkillList4;
                            break;
                        case 5:
                            IventrySkillList = IventrySkillList5;
                            break;
                        default:
                            Debug.LogError("unitNum is invalid");
                            return;
                    }

                    // IventrySkillListのオブジェクトをすべて取得
                    foreach (GameObject obj in IventrySkillList)
                    {
                        // objの名前がtxNameやcharFaceであればスキップ
                        if (obj.name == "txName" || obj.name == "charFace") continue;

                        Image image = null;
                        // objと同じ改装にあるその他のオブジェクトを取得
                        foreach (Transform child in obj.transform.parent)
                        {
                            // childの名前が"waku"の場合Imageコンポーネントを取得
                            if (child.name == "waku")
                            {
                                image = child.GetComponent<Image>();
                                // 枠の色を初期化
                                image.color = new Color(1, 1, 1, 1);
                            }
                        }
                        bool isChangeColor = false;
                        string equipable = "000000";
                        // spriteID6桁の111000の位が111ならば剣、112ならば短剣、113ならば斧、114ならば杖、115ならばワンド、116ならば弓、117ならば特殊武器、211ならば盾、212ならば重鎧、213ならば軽鎧、214ならばローブ、215ならばアクセサリ、411ならばメインルーン、412～415ならばサブルーン
                        bool isInRange = spriteID.StartsWith("412") || spriteID.StartsWith("413") || spriteID.StartsWith("414") || spriteID.StartsWith("415");
                        if (obj.name.Contains("charWpn") && spriteID[0].ToString() == "1")
                        {
                            // ItemDada.wpnListのequipableを取得(装備できる職業のIDが入っている)
                            equipable = gameManager.itemData.wpnList.Find(x => x.ID == int.Parse(spriteID)).equipable.ToString("D6");

                            if(jobListData.equeLargeSwordAxe == 1 && ( spriteID.Substring(1, 2) == "11" || spriteID.Substring(1, 2) == "13"))
                            {
                                isChangeColor = true;
                            }
                            else if (jobListData.equeSwordDagger == 1 && spriteID.Substring(1, 2) == "12")
                            {
                                isChangeColor = true;
                            }
                            else if (jobListData.equeStickStaffBook == 1 && ( spriteID.Substring(1, 2) == "14" || spriteID.Substring(1, 2) == "15"))
                            {
                                isChangeColor = true;
                            }
                            else if (jobListData.arrow == 1 && spriteID.Substring(1, 2) == "16")
                            {
                                isChangeColor = true;
                            }
                            else if (jobListData.spWeapon == 1 && spriteID.Substring(1, 2) == "17")
                            {
                                isChangeColor = true;
                            }
                        }
                        else if (obj.name.Contains("charShild") && spriteID.Substring(0, 3) == "211")
                        {
                            // ItemDada.eqpListのequipableを取得(装備できる職業のIDが入っている)
                            equipable = gameManager.itemData.eqpList.Find(x => x.ID == int.Parse(spriteID)).equipable.ToString("D6");
                            //　両手武器を装備している場合はタンク以外盾を装備できない
                            bool twoHand = false;
                            if (unit.currentWeapons.ToString("D6").Substring(1, 2) == "11" || unit.currentWeapons.ToString("D6").Substring(1, 2) == "13" || unit.currentWeapons.ToString("D6").Substring(1, 2) == "14" || unit.currentWeapons.ToString("D6").Substring(1, 2) == "16")
                            {
                                if (unit.job != 4)
                                {
                                    twoHand = true;
                                }
                            }
                            if(jobListData.equeShield == 1 && !twoHand)
                            {
                                isChangeColor = true;
                            }
                        }
                        else if (obj.name.Contains("charArmor") && spriteID.Substring(0, 2) == "21")
                        {
                            // ItemDada.eqpListのequipableを取得(装備できる職業のIDが入っている)
                            equipable = gameManager.itemData.eqpList.Find(x => x.ID == int.Parse(spriteID)).equipable.ToString("D6");

                            if(jobListData.equeHeavyArmor == 1 && spriteID.Substring(1, 2) == "12")
                            {
                                isChangeColor = true;
                            }
                            else if (jobListData.equeLightArmor == 1 && spriteID.Substring(1, 2) == "13")
                            {
                                isChangeColor = true;
                            }
                            else if (jobListData.equeRobe == 1 && spriteID.Substring(1, 2) == "14")
                            {
                                isChangeColor = true;
                            }
                        }
                        else if (obj.name.Contains("charAccsse") && spriteID.Substring(0, 3) == "215")
                        {
                            // ItemDada.eqpListのequipableを取得(装備できる職業のIDが入っている)
                            equipable = gameManager.itemData.eqpList.Find(x => x.ID == int.Parse(spriteID)).equipable.ToString("D6");
                            
                            if(jobListData.equeAccessories == 1)
                            {
                                isChangeColor = true;
                            }
                        }
                        else if (obj.name.Contains("mainSocket") && spriteID.Substring(0, 3) == "411")
                        {
                            isChangeColor = true;
                        }
                        else if (obj.name.Contains("socket") && isInRange)
                        {
                            isChangeColor = true;
                        }

                        // unit.jobの値が0の場合equipable[0]が0ならばfalse、unit.jobの値が1の場合equipable[1]が0ならばfalse、unit.jobの値が2の場合equipable[2]が0ならばfalse、unit.jobの値が3の場合equipable[3]が0ならばfalse、unit.jobの値が4の場合equipable[4]が0ならばfalse
                        if(equipable != "000000")
                        {
                            if (unit.job == 0 && equipable[0] == '0')
                            {
                                isChangeColor = false;
                            }
                            else if (unit.job == 1 && equipable[1] == '0')
                            {
                                isChangeColor = false;
                            }
                            else if (unit.job == 2 && equipable[2] == '0')
                            {
                                isChangeColor = false;
                            }
                            else if (unit.job == 3 && equipable[3] == '0')
                            {
                                isChangeColor = false;
                            }
                            else if (unit.job == 4 && equipable[4] == '0')
                            {
                                isChangeColor = false;
                            }
                        }

                        if (isChangeColor && image != null)
                        {
                            // 枠の色を黄色に変更
                            image.color = new Color(1, 1, 0, 1);
                        }
                    }
                }
            }
        }
    }

    // ゴミ箱をクリックした際に呼び出されるメソッド
    public void TrashButton()
    {
        infoPanelText.text = "Delete all selected items ?";
        infoPanel.SetActive(true);
        OKButton.gameObject.SetActive(true);
        cancelButton.gameObject.SetActive(true);
        // OKボタンを押した際に呼び出されるメソッドを設定
        OKButton.onClick.AddListener(TrashAllItem);
        // キャンセルボタンを押した際に呼び出されるメソッドを設定
        cancelButton.onClick.AddListener(Cancel);
    }
    // ゴミ箱でOKボタンを押した際に呼び出されるメソッド
    public void TrashAllItem()
    {
        // ゴミ捨て音を再生
        AkSoundEngine.PostEvent("ST_Trash", gameObject);

        // ゴミ箱の選択状態の孫オブジェクトをすべて削除
        foreach (Transform child in IventryPanel.transform)
        {
            foreach (Transform grandChild in child)
            {
                if (grandChild.GetComponent<ItemDandDHandler>().isSelected)
                {
                    TrashItem(grandChild.gameObject);
                }
            }
        }
        infoPanel.SetActive(false);
        OKButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
    }
    // キャンセルボタンを押した際に呼び出されるメソッド
    public void Cancel()
    {
        infoPanel.SetActive(false);
        OKButton.gameObject.SetActive(false);
        cancelButton.gameObject.SetActive(false);
    }
    /// <summary>
    /// オブジェクトをゴミ箱に入れて破棄される際に呼び出されるメソッド。
    /// </summary>
    public void TrashItem(GameObject obj)
    {
        // アイテムを破棄した際に、0(空)を入れる
        int index = obj.transform.parent.GetSiblingIndex();
        SetItem(index, 0, 0);
        Destroy(obj); // オブジェクト破棄
    }
}
