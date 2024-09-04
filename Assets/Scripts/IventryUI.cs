using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    
    // IventryPanelの子オブジェクトをすべて取得
    private List<Transform> children = new List<Transform>();

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


}
