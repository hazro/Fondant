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
    [SerializeField] private Transform IventryPanel;

    private const int MaxItems = 48; // アイテムの最大数
    public int[] IventryItem = new int[MaxItems]; // を格納する配列

    // スキルパネルオブジェクトを格納するリスト
    public List<GameObject> IventrySkillList1 = new List<GameObject>();
    public List<GameObject> IventrySkillList2 = new List<GameObject>();
    public List<GameObject> IventrySkillList3 = new List<GameObject>();
    public List<GameObject> IventrySkillList4 = new List<GameObject>();
    public List<GameObject> IventrySkillList5 = new List<GameObject>();
    
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

    // ドロップしたアイテムをIventryItemに追加する
    public void AddItem(int itemID)
    {
        for (int i = 0; i < IventryItem.Length; i++)
        {
            if (IventryItem[i] == 0)
            {
                // 入手直後はruneLevel1の為itemIDを10倍して+1(runeLevel1)する
                IventryItem[i] = itemID*10+1;
                // itemID6桁の100000の位が1なら武器、2なら防具、4ならルーン
                if (itemID / 100000 == 1)
                {
                    GameObject itemSprite = Resources.Load<GameObject>("Prefabs/Weapons/" + itemID);
                    // IventryItemObject[i]の子としてIventryItemObject[i]の位置にitemSpriteを生成
                    itemSprite = Instantiate(itemSprite, children[i].position, Quaternion.identity);
                    //名前から(Clone)を削除
                    itemSprite.name = itemSprite.name.Replace("(Clone)", "");
                    itemSprite.transform.SetParent(children[i]);
                }
                else if (itemID / 100000 == 2)
                {
                    GameObject itemSprite = Resources.Load<GameObject>("Prefabs/Equipment/" + itemID);
                    // IventryItemObject[i]の子としてIventryItemObject[i]の位置にitemSpriteを生成
                    itemSprite = Instantiate(itemSprite, children[i].position, Quaternion.identity);
                    //名前から(Clone)を削除
                    itemSprite.name = itemSprite.name.Replace("(Clone)", "");
                    itemSprite.transform.SetParent(children[i]);
                }
                else if (itemID / 100000 == 4)
                {
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
        int unitNum = int.Parse(unitObject.name[unitObject.name.Length - 1].ToString());

        // IventrySkillList1~5の番号がunitNumのものを取得
        List<GameObject> IventrySkillList = null;
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
        if(unit.mainSocket != 0) mainSoketId = unit.mainSocket.ToString("D6");
        GameObject mainSockettImage = Resources.Load<GameObject>("Prefabs/Runes/" + mainSoketId);
        string subSocketId = "412000";
        if (unit.subSocket[0] != 0) subSocketId = unit.subSocket[0].ToString("D6");
        GameObject socket01Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[1] != 0) subSocketId = unit.subSocket[1].ToString("D6");
        GameObject socket02Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[2] != 0) subSocketId = unit.subSocket[2].ToString("D6");
        GameObject socket03Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[3] != 0) subSocketId = unit.subSocket[3].ToString("D6");
        GameObject socket04Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[4] != 0) subSocketId = unit.subSocket[4].ToString("D6");
        GameObject socket05Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[5] != 0) subSocketId = unit.subSocket[5].ToString("D6");
        GameObject socket06Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[6] != 0) subSocketId = unit.subSocket[6].ToString("D6");
        GameObject socket07Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[7] != 0) subSocketId = unit.subSocket[7].ToString("D6");
        GameObject socket08Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[8] != 0) subSocketId = unit.subSocket[8].ToString("D6");
        GameObject socket09Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[9] != 0) subSocketId = unit.subSocket[9].ToString("D6");
        GameObject socket10Image = Resources.Load<GameObject>("Prefabs/Runes/" + subSocketId);
        subSocketId = "412000";
        if (unit.subSocket[10] != 0) subSocketId = unit.subSocket[10].ToString("D6");
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
        //sprite = AtkImage.GetComponent<SpriteRenderer>().sprite;
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

        for(int i = 6; i < IventrySkillList.Count; i++)
        {
            // IventrySkillList[i]の子オブジェクトのItemDandDHandlerコンポーネントを取得
            ItemDandDHandler item = IventrySkillList[i].GetComponentInChildren<ItemDandDHandler>();
            float runeLevel = item.runeLevel;
            Image imageComponent = IventrySkillList[i].GetComponent<Image>();
            // Materialをinstance化してitemのruneLevelを設定
            Material materialInstance = new Material(imageComponent.material);
            imageComponent.material = materialInstance;
            materialInstance.SetFloat("_Lv", runeLevel);
        }

        //socketCountの取得
        int socketCount = unit.socketCount;
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
    public void changeEquipment(Transform[] targetSkillList, string iventryItemID, string skillItemID ,string socketName, GameObject originalObj, GameObject targetObj, int skillItemLv, int iventryItemLv)
    {
        // unit名を取得  
        string unitName = targetSkillList[0].gameObject.GetComponentInChildren<TextMeshProUGUI>().text;
        // unit名の0番目が1ならPlayer01、2ならPlayer02、3ならPlayer03、4ならPlayer04、5ならPlayer05
        unitName = "P" + unitName[1];

        // GameManagerがアクティブでない場合は処理を終了
        if (gameManager != null && gameManager.livingUnits != null)
        {
            foreach (GameObject unitObj in gameManager.livingUnits)
            {
                Unit unit = unitObj.GetComponent<Unit>();
                if (unit.unitName == unitName)
                {
                    if(iventryItemID[0] == '1' && skillItemID[0] == '1')
                    {
                        print("武器を入れ替えます");
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
                        unit.mainSocket = int.Parse(skillItemID);
                        
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
                        unit.subSocket[socketId] = int.Parse(skillItemID);
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
                    MoveAndUpdate(originalObj, targetObj, iventryItemID, unitObj, iventryItemLv);
                    
                    // unitのステータスを更新
                    unit.updateStatus();
                    // ステータス画面UIの表示を更新
                    if (statusAdjustmentManager != null)
                    {
                        statusAdjustmentManager.UpdateUI();
                    }
                }
            }
        }
        else
        {
            Debug.LogError("gameManager.playerUnits is null");
        }
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
                if (unit != null && unit.condition != 1)
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
}
