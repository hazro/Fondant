using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// イベントリーUI
/// </summary>
public class IventryUI : MonoBehaviour
{
    [SerializeField] private Button openIventryButton;
    [SerializeField] private Transform Iventry;
    [SerializeField] private Transform IventryImage;
    [SerializeField] private Transform IventryPanel;

    private const int MaxItems = 24; // アイテムの最大数
    public int[] IventryItem = new int[MaxItems]; // を格納する配列

    // スキルパネルオブジェクトを格納するリスト
    public List<GameObject> IventrySkillList = new List<GameObject>();
    
    // IventryPanelの子オブジェクトをすべて取得
    private List<Transform> children = new List<Transform>();

    // プレイヤーユニットのリスト
    [HideInInspector] public List<GameObject> playerUnits = new List<GameObject>();

    private void Start()
    {
        // IventryPanelの子オブジェクトをchildrenに格納
        UpdateIventryPanelPosition();
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
                IventryItem[i] = itemID;
                // itemID6桁の100000の位が1なら武器、2なら防具
                if (itemID / 100000 == 1)
                {
                    GameObject itemSprite = Resources.Load<GameObject>("Prefabs/Weapons/" + itemID);
                    // IventryItemObject[i]の子としてIventryItemObject[i]の位置にitemSpriteを生成
                    itemSprite = Instantiate(itemSprite, children[i].position, Quaternion.identity);
                    itemSprite.transform.SetParent(children[i]);
                }
                else if (itemID / 100000 == 2)
                {
                    GameObject itemSprite = Resources.Load<GameObject>("Prefabs/Equipment/" + itemID);
                    // IventryItemObject[i]の子としてIventryItemObject[i]の位置にitemSpriteを生成
                    itemSprite = Instantiate(itemSprite, children[i].position, Quaternion.identity);
                    itemSprite.transform.SetParent(children[i]);
                }
                break;
            }
        }
    }

    // Skillリストを選択したunitのものに変更する
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

        // IventrySkillListの孫オブジェクトをすべて削除
        foreach (GameObject child in IventrySkillList)
        {
            foreach (Transform grandChild in child.transform)
            {
                // 孫オブジェクトの名前がtxNameやwakuでなければ削除
                if (grandChild.name != "txName" && grandChild.name != "waku" && grandChild.name != "collider")
                {
                    Destroy(grandChild.gameObject);
                }  
            }
        }

        GameObject wpnImage = Resources.Load<GameObject>("Prefabs/Weapons/" + unit.currentWeapons.ToString("D6"));
        GameObject AtkImage = Resources.Load<GameObject>("Prefabs/AttackEffects/" + unit.currentAttackEffects.ToString("D6"));
        GameObject shieldImage = Resources.Load<GameObject>("Prefabs/Equipment/" + unit.currentShields.ToString("D6"));
        GameObject armorImage = Resources.Load<GameObject>("Prefabs/Equipment/" + unit.currentArmor.ToString("D6"));
        GameObject accsseImage = Resources.Load<GameObject>("Prefabs/Equipment/" + unit.currentAccessories.ToString("D6"));

        // IventrySkillList[0]の子オブジェクトのTextMeshProコンポーネントを取得
        TextMeshProUGUI unitNameText = IventrySkillList[0].GetComponentInChildren<TextMeshProUGUI>();
        unitNameText.text = unit.unitName;

        // IventrySkillList[1]の子オブジェクトのImageコンポーネントを取得し、スプライトを設定
        Sprite sprite = wpnImage.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[1].GetComponent<Image>().sprite = sprite;
        sprite = AtkImage.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[2].GetComponent<Image>().sprite = sprite;
        sprite = shieldImage.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[3].GetComponent<Image>().sprite = sprite;
        sprite = armorImage.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[4].GetComponent<Image>().sprite = sprite;
        sprite = accsseImage.GetComponent<SpriteRenderer>().sprite;
        IventrySkillList[5].GetComponent<Image>().sprite = sprite;
    }
    
    // アイテムを削除するメソッドをコールーチンで呼び出す
    public void IventrySorting(GameObject destroyObject)
    {
        // GameManagerがアクティブであることを確認
        if (!GameManager.Instance.gameObject.activeInHierarchy)
        {
            return;
        }
        StartCoroutine(IventrySortingCoroutine(destroyObject));
    }

    // Iventryをソートする(空きを詰める)メソッド
    IEnumerator IventrySortingCoroutine(GameObject destroyObject)
    {
        // オブジェクトが存在する間はループを続ける
        while (destroyObject != null)
        {
            yield return null; // 毎フレームチェック
        }
        // オブジェクトが存在しなくなったら処理を続ける

        // IventryPanelの子オブジェクトをすべて取得
        UpdateIventryPanelPosition();
        // childrenを順番に取り出し、子オブジェクトの数が0ならば子オブジェクトがあるchildrenまで順番に確認しその子オブジェクトを取得してオブジェクト数が0だったchildrenの子にする。をchildrenの数繰り返す
        for (int i = 0; i < children.Count; i++)
        {
            if (children[i].childCount == 0)
            {
                for (int j = i + 1; j < children.Count; j++)
                {
                    if (children[j].childCount != 0)
                    {
                        Transform child = children[j].GetChild(0);
                        child.SetParent(children[i]);
                        child.localPosition = Vector3.zero;
                        break;
                    }
                }
            }
        }

    }

    // 装備を変更するメソッド(ItemDandDHandlerから呼び出される)
    public void changeEquipment(string objName, Image image, GameObject obj)
    {
        // unit名を取得  
        string unitName = IventrySkillList[0].GetComponentInChildren<TextMeshProUGUI>().text;
        foreach (GameObject unitObj in playerUnits)
        {
            Unit unit = unitObj.GetComponent<Unit>();
            if (unit.unitName == unitName)
            {
                if(objName[0] == '1' && image.name == "charWpn")
                {
                    print(image.name + "武器を入れ替えます" + objName);
                    unit.currentWeapons = int.Parse(objName);
                    // イベントリから移動したobjを削除
                    Destroy(obj);
                    // イベントリをソート
                    StartCoroutine(IventrySortingCoroutine(obj));
                    // もともと持っていた武器があればイベントリに追加
                    if (unit.currentWeapons != 0)
                    {
                        AddItem(unit.currentWeapons);
                    }
                    // スキルパネルの更新
                    UpdateUnitSkillUI(unitObj);
                }
                // 名前の0番目が2で1番目が1で2番目が1ならばシールド
                if(objName[0] == '2' && objName[1] == '1' && objName[2] == '1' && image.name == "charShild")
                {
                    print(image.name + " 盾を入れ替えます " + objName);
                    unit.currentShields = int.Parse(objName);
                    // イベントリから移動したobjを削除
                    Destroy(obj);
                    // イベントリをソート
                    StartCoroutine(IventrySortingCoroutine(obj));
                    // もともと持っていた盾があればイベントリに追加
                    if (unit.currentShields != 0)
                    {
                        AddItem(unit.currentShields);
                    }
                    // スキルパネルの更新
                    UpdateUnitSkillUI(unitObj);
                }
                // 名前の0番目が2で1番目が1で2番目が2~4のいずれかならば防具
                else if (objName[0] == '2' && objName[1] == '1' && (objName[2] >= '2' && objName[2] <= '4') && image.name == "charArmor")
                {
                    print(image.name + "防具を入れ替えます" + objName);
                    unit.currentArmor = int.Parse(objName);
                    // イベントリから移動したobjを削除
                    Destroy(obj);
                    // イベントリをソート
                    StartCoroutine(IventrySortingCoroutine(obj));
                    // もともと持っていた防具があればイベントリに追加
                    if (unit.currentArmor != 0)
                    {
                        AddItem(unit.currentArmor);
                    }
                    // スキルパネルの更新
                    UpdateUnitSkillUI(unitObj);
                }
                // 名前の0番目が2で1番目が1で2番目が5ならばアクセサリ
                else if(objName[0] == '2' && objName[1] == '1' && objName[2] == '5' &&  image.name == "charAccsse")
                {
                    print(image.name + "アクセサリを入れ替えます" + objName);
                    unit.currentAccessories = int.Parse(objName);
                    // イベントリから移動したobjを削除
                    Destroy(obj);
                    // イベントリをソート
                    StartCoroutine(IventrySortingCoroutine(obj));
                    // もともと持っていたアクセサリがあればイベントリに追加
                    if (unit.currentAccessories != 0)
                    {
                        AddItem(unit.currentAccessories);
                    }
                    // スキルパネルの更新
                    UpdateUnitSkillUI(unitObj);
                }
            }
        }
    }

}
