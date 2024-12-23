using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using MyGame.Managers;
using System.Collections;

/// <summary>
/// オブジェクトをドラッグ＆ドロップで移動し、特定の条件で処理を行うクラス。
/// IBeginDragHandler, IDragHandler, IEndDragHandler インターフェースを実装しています。
/// </summary>
public class ItemDandDHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [SerializeField] private bool isShopItem = false; // ショップアイテムかどうかのフラグ
    [SerializeField] private bool isCharaName = false; // キャラクター名かどうかのフラグ 
    [SerializeField] private bool isEmptyPanel = false; // 空のパネルかどうかのフラグ
    public bool isFixItem = false; // アイテムを固定するかどうかのフラグ (武器固有ルーンの場合はUnitクラスでtrueにする)
    public bool isSelected = false; // 選択されているかどうかのフラグ
    private Vector3 offset; // ドラッグ中のオフセット
    private Vector3 originalPosition; // 元の位置を保存する変数
    private BoxCollider2D boxCollider; // 自身のBoxCollider2Dの参照
    private IventryUI iventryUI; // IventryUIの参照
    public InfomationPanelDisplay infomationPanelDisplay; // InfomationPanelDisplayの参照
    public int runeLevel = 1; // ルーンのレベルを格納する変数
    public GameObject wpnAef; // 武器のエフェクトを登録するための変数
    private GameManager gameManager; // GameManagerの参照
    private ShopManager shopManager; // ShopManagerの参照
    private BlackSmithManager blackSmithManager; // BlackSmithManagerの参照
    private bool isDraggable = true; // ドラッグ可能かどうかのフラグ

    /// <summary>
    /// 初期設定を行うメソッド。BoxCollider2Dを取得します。
    /// </summary>
    void Start()
    {
        gameManager = GameManager.Instance; // GameManagerのインスタンスを取得
        boxCollider = GetComponent<BoxCollider2D>(); // BoxCollider2Dを取得
        iventryUI = FindObjectOfType<IventryUI>(); // IventryUIのインスタンスを取得
        //infomationPanelDisplay = FindObjectOfType<InfomationPanelDisplay>(); // InfomationPanelDisplayのインスタンスを取得
        if (gameManager != null) infomationPanelDisplay = gameManager.GetComponent<InfomationPanelDisplay>(); // InfomationPanelDisplayのインスタンスを取得
        shopManager = FindObjectOfType<ShopManager>(); // ShopManagerのインスタンスを取得
        //blackSmithManager = FindObjectOfType<BlackSmithManager>(); // BlackSmithManagerのインスタンスを取得
        // this.gameObjectのマテリアルのLVにruneLevelを代入
        if (this.gameObject.GetComponent<Image>() != null)
        {
            this.gameObject.GetComponent<Image>().material.SetFloat("_Lv", runeLevel);
        }
        else if (this.gameObject.GetComponent<SpriteRenderer>() != null)
        {
            this.gameObject.GetComponent<SpriteRenderer>().material.SetFloat("_Lv", runeLevel);
        }
    }

    /// <summary>
    /// ドラッグ開始時に呼び出されるメソッド。
    /// 元の位置を保存し、BoxCollider2Dを無効にします。
    /// </summary>
    /// <param name="eventData">ドラッグイベントデータ</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isFixItem)
        {
            isDraggable = false;
            return; // アイテムを固定する場合、処理を終了
        }

        if (blackSmithManager == null)
        {
            blackSmithManager = FindObjectOfType<BlackSmithManager>(); // BlackSmithManagerのインスタンスを取得
        }
        if (blackSmithManager != null && blackSmithManager.isSetItem) return; // 強化アイテムがセットされている場合、処理を終了

        string spriteID = "000000"; // SpriteIDを格納する変数
        // ショップアイテムの場合、ドラッグを無効にする
        if (isShopItem || isCharaName)
        {
            isDraggable = false;
            return;
        }
        // オブジェクトにImageコンポーネントがアタッチされている場合、そのSprite名を取得し、1つめの_と2つめの_の間の文字列の末尾が"000"の場合、ドラッグを無効にする
        else if (GetComponent<Image>() != null)
        {
            spriteID = CheckSpriteIDAndSetDraggable(GetComponent<Image>().sprite.name);
        }
        else if (GetComponent<SpriteRenderer>() != null)
        {
            spriteID = CheckSpriteIDAndSetDraggable(GetComponent<SpriteRenderer>().sprite.name);
        }
        else
        {
            isDraggable = true;
        }

        // 空のパネルの場合、ドラッグを無効にする
        if (isEmptyPanel)
        {
            isDraggable = false;
            return;
        }

        originalPosition = transform.position; // 元の位置を保存
        offset = transform.position - GetMouseWorldPosition(eventData);

        // 自分以下のオブジェクトのBoxCollider2Dが有効であったら無効にする
        if (boxCollider != null)
        {
            boxCollider.enabled = false; // 自身のBoxCollider2Dを無効にする
        }
        foreach (Transform child in transform)
        {
            BoxCollider2D childBoxCollider = child.GetComponent<BoxCollider2D>();
            if (childBoxCollider != null)
            {
                childBoxCollider.enabled = false; // 子オブジェクトのBoxCollider2Dを無効にする
            }
        }

        // 装備可能なアイテムの場合、SkillListの枠の色を黄色くする
        if (iventryUI != null)
        {
            iventryUI.ChangeSkillListFrameColor(spriteID);
        }

        // ドラッグ開始SEを再生
        AkSoundEngine.PostEvent("ST_Grip", gameObject);
    }

    /// <summary>
    /// ドラッグ中に呼び出されるメソッド。
    /// オブジェクトをマウスの位置に追従させます。
    /// </summary>
    /// <param name="eventData">ドラッグイベントデータ</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggable) return; // ドラッグ不可の場合、処理を終了
        if (blackSmithManager != null && blackSmithManager.isSetItem) return; // 強化アイテムがセットされている場合、処理を終了
        transform.position = GetMouseWorldPosition(eventData) + offset;
    }

    /// <summary>
    /// ドラッグ終了時に呼び出されるメソッド。
    /// マウスの位置に基づいてヒットしたオブジェクトに対して処理を行います。
    /// </summary>
    /// <param name="eventData">ドラッグイベントデータ</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggable) return; // ドラッグ不可の場合、処理を終了
        if (blackSmithManager != null && blackSmithManager.isSetItem) return; // 強化アイテムがセットされている場合、処理を終了

        Vector3 mouseWorldPosition = GetMouseWorldPosition(eventData); // マウスのワールド座標を取得
        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPosition); // マウス位置でのヒットテスト

        if (hitCollider != null && hitCollider.gameObject != this.gameObject)
        {
            GameObject targetObject = hitCollider.gameObject;
            print("DropTarget: " + targetObject.name);
            if (shopManager == null)
            {
                shopManager = FindObjectOfType<ShopManager>(); // ShopManagerのインスタンスを取得
            }

            // イベントリ内同士のアイテムの入れ替え処理
            if (targetObject.CompareTag("Item") && this.gameObject.CompareTag("Item"))
            {
                SwapPositionAndParent(targetObject);
                // Fit SEを再生
                AkSoundEngine.PostEvent("ST_Fit", gameObject);
            }
            // ゴミ箱にドロップした場合、アイテムを破棄 
            else if (targetObject.name == "Trash" && this.gameObject.CompareTag("Item"))
            {
                if (iventryUI != null)
                {
                    // ゴミ箱SEを再生
                    AkSoundEngine.PostEvent("ST_Trash", gameObject);
                    iventryUI.TrashItem(this.gameObject);
                }
            }
            // ドラッグ先がShopのmainGridの場合、アイテムを半額で売る
            else if (targetObject.name == "SellPosition" && shopManager != null)
            {
                shopManager.SellItem(gameObject);
                transform.position = originalPosition;
            }
            // ドラッグ先がBlackSmithのArrowRightである場合アイテム強化Panelにアイテムをセットする
            else if (targetObject.name == "ArrowRight" && blackSmithManager != null)
            {
                // ドラッグを元の位置に戻す
                transform.position = originalPosition;

                // 自身のspriteRendererまたはImageのsprite名を取得
                string spriteName = "";
                if (GetComponent<SpriteRenderer>() != null)
                {
                    spriteName = GetComponent<SpriteRenderer>().sprite.name;
                }
                else if (GetComponent<Image>() != null)
                {
                    spriteName = GetComponent<Image>().sprite.name;
                }
                // spriteNameがnullでない場合、_で分割して2番目の要素を取得し、setItemIDに代入
                if (spriteName != "")
                {
                    int itemID = int.Parse(spriteName.Split('_')[1]);
                    // 400000代であればルーンと判定し、それ以外はretuen
                    if (itemID / 100000 != 4) return;

                    blackSmithManager.setItemID = itemID;
                    blackSmithManager.setItem = this.gameObject;
                    // 自身にitemDandDHandlerがアタッチされている場合、runeLevelをblackSmithManager.setItemLvに代入
                    if (GetComponent<ItemDandDHandler>() != null)
                    {
                        blackSmithManager.setItemLv = GetComponent<ItemDandDHandler>().runeLevel;
                    }
                    blackSmithManager.isSetItem = true;
                    blackSmithManager.SetItem();
                }
                // Fit SEを再生
                AkSoundEngine.PostEvent("ST_Fit", gameObject);

            }
            //　どちらもItemタグでない場合は何もせずに元の位置に戻す
            else if (!targetObject.CompareTag("Item") && !this.gameObject.CompareTag("Item"))
            {
                transform.position = originalPosition;
            }
            // 装備スロットとの入れ替え処理
            else if (targetObject.name == "collider" || targetObject.CompareTag("Item"))
            {
                // ドラッグを元の位置に戻す
                transform.position = originalPosition;

                string skillItemID = "";
                string iventryItemID = "";
                string socketName = "";
                int skillItemLv = 0;
                int iventryItemLv = 0;
                Transform skillListTransform = null;

                void SetItemIDs(GameObject source, GameObject target, out string sourceID, out string targetID, out int sourceLv, out int targetLv)
                {
                    sourceID = source.GetComponent<SpriteRenderer>().sprite.name;
                    sourceID = sourceID.Substring(sourceID.IndexOf("_") + 1, sourceID.LastIndexOf("_") - sourceID.IndexOf("_") - 1);
                    sourceLv = source.GetComponent<ItemDandDHandler>().runeLevel;

                    targetID = target.GetComponent<Image>().sprite.name;
                    targetID = targetID.Substring(targetID.IndexOf("_") + 1, targetID.LastIndexOf("_") - targetID.IndexOf("_") - 1);
                    targetLv = target.GetComponent<ItemDandDHandler>().runeLevel;
                }

                GameObject skillObject = null;
                if (targetObject.CompareTag("Item"))
                {
                    Debug.Log("Iventryにドロップしました(iventryがターゲット)");
                    socketName = this.name;
                    SetItemIDs(targetObject, this.gameObject, out skillItemID, out iventryItemID, out skillItemLv, out iventryItemLv);
                    skillListTransform = this.gameObject.transform;
                    skillObject = this.gameObject;
                }
                else
                {
                    if(targetObject.transform.parent.gameObject.GetComponent<ItemDandDHandler>().isFixItem)
                    {
                        Debug.Log("武器固定ルーンのため装備変更不可");
                        iventryUI.UpdateIventryPanel();
                        // エラーSEを再生
                        AkSoundEngine.PostEvent("ST_Error", gameObject);
                        return;
                    }
                    Debug.Log("Skillスロットにドロップしました(skillがターゲット)");
                    socketName = targetObject.transform.parent.name;
                    SetItemIDs(this.gameObject, targetObject.transform.parent.gameObject, out skillItemID, out iventryItemID, out skillItemLv, out iventryItemLv);
                    skillListTransform = targetObject.transform.parent;
                    skillObject = targetObject;
                }
                // skillListTransformの親の子に名前が"waku"のものがあったらImageコンポーネントを持つものを取得
                foreach (Transform child in skillListTransform.parent)
                {
                    if (child.name == "waku")
                    {
                        // Imageコンポーネントを取得
                        if (child.GetComponent<Image>())
                        {
                            if (child.GetComponent<Image>().color != new Color(1, 1, 0, 1))
                            {
                                Debug.Log(" 装備不可のスキルスロットにドロップまたは入れ替えようとしました");
                                iventryUI.UpdateIventryPanel();
                                return;
                            }
                        }
                    }
                }

                // Sprite名もImage名も取得できない場合、エラーログを出力して終了
                if (skillItemID == "" || iventryItemID == "" || socketName == "")
                {
                    Debug.Log("itemID or imageName is null.");
                    return;
                }

                // 該当SkillPnaelのtxNameオブジェクトを取得
                GameObject targetCharNameObj = skillListTransform.parent.parent.GetComponentsInChildren<Transform>()[1].GetComponentsInChildren<Transform>()[2].gameObject;

                // Iventryで装備の入れ替え処理を行う
                iventryUI.changeEquipment(targetCharNameObj, iventryItemID, skillItemID, socketName, this.gameObject, targetObject, skillItemLv, iventryItemLv);
            }
            // それ以外の場合は何もせずに元の位置に戻す
            else
            {
                transform.position = originalPosition;
                Debug.Log("hitしましたがTargetが入れ替え対象ではない為何もせず元に戻しました");
            }
        }
        else
        {
            transform.position = originalPosition;
        }

        // 自分以下のオブジェクトのBoxCollider2Dが無効であったら有効にする
        if (boxCollider != null)
        {
            boxCollider.enabled = true;
        }
        foreach (Transform child in transform)
        {
            BoxCollider2D childBoxCollider = child.GetComponent<BoxCollider2D>();
            if (childBoxCollider != null)
            {
                childBoxCollider.enabled = true;
            }
        }

        // SkillListの枠の色をもとに戻す
        if (iventryUI != null)
        {
            iventryUI.ChangeSkillListFrameColor("000000");
        }
    }

    /// <summary>
    /// マウスがオブジェクトに入ったときに呼び出されるメソッド。
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (infomationPanelDisplay == null) return; // InfomationPanelDisplayがnullの場合、処理を終了

        if (!infomationPanelDisplay.isClicked)
        {
            // EqpStatsがnullではないとき、EqpStatsの各項目の中でvalueがnull又は0でないものだけを取得して、"変数名: " + value + "\n" という形式でテキストを作成
            string text = "";
            string objectID = "0";
            // オブジェクトにspriteRendererがアタッチされている場合、オブジェクト名を取得
            if (gameObject.GetComponent<SpriteRenderer>() != null)
            {
                objectID = gameObject.name;
            }
            // オブジェクトにImageがアタッチされている場合、spriteの名前を取得し、2つめの_と3つめの_の間の文字列をobjectIDに代入
            else if (gameObject.GetComponent<Image>() != null)
            {
                // spriteがセットされている場合、その名前を取得
                if (gameObject.GetComponent<Image>().sprite != null)
                {
                    objectID = gameObject.GetComponent<Image>().sprite.name;
                    // objectIDに_が2つ以上含まれているか確認
                    if (objectID.IndexOf("_") != objectID.LastIndexOf("_"))
                    {
                        objectID = objectID.Substring(objectID.IndexOf("_") + 1, objectID.LastIndexOf("_") - objectID.IndexOf("_") - 1);
                    }
                }
            }

            // オブジェクト名の頭が1の場合は武器の情報を表示
            if (objectID.Substring(0, 1) == "1")
            {
                // 武器の情報を取得
                ItemData.WpnListData wpnListData = gameManager.itemData.wpnList.Find(x => x.ID == int.Parse(objectID));
                if (wpnListData != null)
                {
                    foreach (var field in wpnListData.GetType().GetFields())
                    {
                        // 変数がID以外で、値が0でない場合のみ表示
                        if (field.GetValue(wpnListData) != null && field.GetValue(wpnListData).ToString() != "0" && field.Name != "ID")
                        {
                            text += field.Name + ": " + field.GetValue(wpnListData) + "\n";
                        }
                    }
                }
            }
            //　オブジェクト名の頭が2の場合は装備の情報を表示
            else if (objectID.Substring(0, 1) == "2")
            {
                // 装備の情報を取得
                ItemData.EqpListData eqpListData = gameManager.itemData.eqpList.Find(x => x.ID == int.Parse(objectID));
                if (eqpListData != null)
                {
                    foreach (var field in eqpListData.GetType().GetFields())
                    {
                        if (field.GetValue(eqpListData) != null && field.GetValue(eqpListData).ToString() != "0" && field.Name != "ID")
                        {
                            text += field.Name + ": " + field.GetValue(eqpListData) + "\n";
                        }
                    }
                }
            }
            // オブジェクト名の頭が4の場合はルーンの情報を表示
            else if (objectID.Substring(0, 1) == "4")
            {
                // ルーンの情報を取得
                ItemData.RuneListData runeListData = gameManager.itemData.runeList.Find(x => x.ID == int.Parse(objectID));
                if (runeListData != null)
                {
                    int lineCount = 0;
                    foreach (var field in runeListData.GetType().GetFields())
                    {
                        // 値が0でない場合のみ表示 (IDは除く)
                        if (field.GetValue(runeListData) != null && field.GetValue(runeListData).ToString() != "0" && field.Name != "ID")
                        {
                            // 2行目はRune Levelを表示
                            if (lineCount == 1)
                            {
                                text += "runeLevel: " + runeLevel + "\n";
                                lineCount++;
                            }
                            // 変数名の末尾に"Lv１桁の整数"がつく場合"整数"の値がruneLevelと一致する場合のみ表示
                            if (field.Name.Substring(field.Name.Length - 3) == "Lv" + runeLevel)
                            {
                                // field.Nameの文字列から"Lv" + runeLevelを削除して表示
                                text += field.Name.Substring(0, field.Name.Length - 3) + ": " + field.GetValue(runeListData) + "\n";
                                lineCount++;
                            }
                            // field.Name.Length -1~-3がLvでない場合、そのまま表示
                            else if (field.Name.Substring(field.Name.Length - 3, 2) != "Lv")
                            {
                                text += field.Name + ": " + field.GetValue(runeListData) + "\n";
                                lineCount++;
                            }
                        }
                    }
                }
            }
            // オブジェクトが"teName"の場合、objectIDに"teName"を代入
            else if (gameObject.name == "txName")
            {
                // 親の親の親の名前の末尾2桁をint型に変換して取得
                int unitID = int.Parse(gameObject.transform.parent.parent.parent.name.Substring(gameObject.transform.parent.parent.parent.name.Length - 2));
                // gameManager.ItemData.jonListのIDとunit.jobが一致するものを取得し、abilityをtextに代入
                ItemData.JobListData jobListData = gameManager.itemData.jobList.Find(x => x.ID == unitID);
                if (jobListData != null)
                {
                    text = jobListData.ability;
                }
            }
            else
            {
                // その他の場合はinfomationPanelDisplayを非表示にする
                infomationPanelDisplay.SetInactive();
                text = "";
                return;
            }
            // ここにマウスオーバー時の処理を追加
            infomationPanelDisplay.SetActiveAndChangeText(text);
        }
    }

    /// <summary>
    /// マウスがオブジェクトから出たときに呼び出されるメソッド。
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (infomationPanelDisplay == null) return; // InfomationPanelDisplayがnullの場合、処理を終了

        if (!infomationPanelDisplay.isClicked) //
        {
            // ここにマウスオーバー解除時の処理を追加
            infomationPanelDisplay.SetInactive();
        }
    }

    /// <summary>
    /// オブジェクトがクリックされたときに呼び出されるメソッド。
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        // ショップアイテムの場合、カートに追加する
        if (isShopItem && shopManager != null)
        {
            shopManager.addStockList(gameObject);
        }
        else
        {
            ItemSelect();
        }

    }

    /// <summary>
    /// 位置と親オブジェクトを入れ替えるメソッド。
    /// </summary>
    /// <param name="targetObject">ターゲットオブジェクト</param>
    private void SwapPositionAndParent(GameObject targetObject)
    {
        Transform originalParentTemp = transform.parent;
        Vector3 targetPositionTemp = targetObject.transform.position;
        Transform targetParentTemp = targetObject.transform.parent;

        transform.SetParent(targetParentTemp, true);
        transform.localPosition = new Vector3(0, 0, 0);

        targetObject.transform.SetParent(originalParentTemp, true);
        targetObject.transform.localPosition = new Vector3(0, 0, 0);

        // ターゲットと自分自身がiventryItem配列の何番目にあるかを取得
        int originalIndex = originalParentTemp.GetSiblingIndex();
        int targetIndex = targetParentTemp.GetSiblingIndex();

        // iventryItem配列も入れ替える
        if (iventryUI != null)
        {
            iventryUI.SwapItem(originalIndex, targetIndex);
        }
    }

    /// <summary>
    /// マウスのワールド座標を取得するメソッド。
    /// </summary>
    /// <param name="eventData">ドラッグイベントデータ</param>
    /// <returns>マウスのワールド座標</returns>
    private Vector3 GetMouseWorldPosition(PointerEventData eventData)
    {
        Vector3 mousePosition = eventData.position;
        mousePosition.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(mousePosition);
    }

    /// <summary>
    /// SpriteIDをチェックし、ドラッグ可能かどうかを設定するメソッド。spriteNameを返す。
    /// </summary>
    private string CheckSpriteIDAndSetDraggable(string spriteName)
    {
        string spriteID = "000000";
        if (spriteName != null)
        {
            spriteID = spriteName.Substring(spriteName.IndexOf("_") + 1, spriteName.LastIndexOf("_") - spriteName.IndexOf("_") - 1);
            // spriteIDの末尾が"000"の場合、ドラッグを無効にする
            if (spriteID.Substring(spriteID.Length - 3) == "000")
            {
                isDraggable = false;
            }
        }
        return spriteID;
    }

    /// <summary>
    /// 選択されているかどうかのフラグを切り替えるメソッド。
    /// </summary>
    public void ItemSelect(bool forcedCancel = false)
    {
        if (isSelected == true || forcedCancel == true)
        {
            if (GetComponent<SpriteRenderer>() != null)
            {
                //選択解除して元の枠色に戻す
                isSelected = false;
                GetComponent<SpriteRenderer>().material.SetColor("_BaseColor", Color.white);
            }
        }
        else
        {
            if (GetComponent<SpriteRenderer>() != null)
            {
                // ShaderのBaseColorを変更すると枠が出現するようにしてある
                isSelected = true;
                GetComponent<SpriteRenderer>().material.SetColor("_BaseColor", new Color(0.0f, 0.15f, 0.0f, 1));
            }
        }
    }
}
