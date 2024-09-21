using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// オブジェクトをドラッグ＆ドロップで移動し、特定の条件で処理を行うクラス。
/// IBeginDragHandler, IDragHandler, IEndDragHandler インターフェースを実装しています。
/// </summary>
public class ItemDandDHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Vector3 offset; // ドラッグ中のオフセット
    private Vector3 originalPosition; // 元の位置を保存する変数
    private BoxCollider2D boxCollider; // 自身のBoxCollider2Dの参照
    private IventryUI iventryUI; // IventryUIの参照
    private InfomationPanelDisplay infomationPanelDisplay; // InfomationPanelDisplayの参照
    [SerializeField] private EqpStats eqpStats; // EqpStatsの参照

    /// <summary>
    /// 初期設定を行うメソッド。BoxCollider2Dを取得します。
    /// </summary>
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>(); // BoxCollider2Dを取得
        iventryUI = FindObjectOfType<IventryUI>(); // IventryUIのインスタンスを取得
        infomationPanelDisplay = FindObjectOfType<InfomationPanelDisplay>(); // InfomationPanelDisplayのインスタンスを取得
    }

    /// <summary>
    /// ドラッグ開始時に呼び出されるメソッド。
    /// 元の位置を保存し、BoxCollider2Dを無効にします。
    /// </summary>
    /// <param name="eventData">ドラッグイベントデータ</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = transform.position; // 元の位置を保存
        offset = transform.position - GetMouseWorldPosition(eventData);
        
        if (boxCollider != null)
        {
            boxCollider.enabled = false; // 自身のBoxCollider2Dを無効にする
        }
    }

    /// <summary>
    /// ドラッグ中に呼び出されるメソッド。
    /// オブジェクトをマウスの位置に追従させます。
    /// </summary>
    /// <param name="eventData">ドラッグイベントデータ</param>
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = GetMouseWorldPosition(eventData) + offset;
    }

    /// <summary>
    /// ドラッグ終了時に呼び出されるメソッド。
    /// マウスの位置に基づいてヒットしたオブジェクトに対して処理を行います。
    /// </summary>
    /// <param name="eventData">ドラッグイベントデータ</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 mouseWorldPosition = GetMouseWorldPosition(eventData); // マウスのワールド座標を取得
        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPosition); // マウス位置でのヒットテスト

        if (hitCollider != null && hitCollider.gameObject != this.gameObject)
        {
            GameObject targetObject = hitCollider.gameObject;
            print("DropTarget: " + targetObject.name);

            if (targetObject.CompareTag("Item"))
            {
                SwapPositionAndParent(targetObject);
            }
            else if (targetObject.name == "Trash")
            {
                OnDestroy();
            }
            else if (targetObject.name == "collider")
            {
                // ドラッグを元の位置に戻す
                transform.position = originalPosition;

                // 自分のオブジェクト名から(Clone)を削除
                string objName = transform.name.Replace("(Clone)", "");
                // 親階層のImageコンポーネントを取得
                Image image = targetObject.transform.parent.GetComponent<Image>();
                if (image == null)
                {
                    Debug.LogError(transform.parent.name + "Parent does not have an Image component.");
                    transform.position = originalPosition;
                    return;
                }
                //targetの親の親の親のゲームオブジェクトの子オブジェクトのリストを取得
                Transform[] targetSkillList = targetObject.transform.parent.parent.parent.GetComponentsInChildren<Transform>();

                // Iventryで装備の入れ替え処理を行う
                iventryUI.changeEquipment(targetSkillList, objName, image, this.gameObject);
            }
            else
            {
                transform.position = originalPosition;
            }
        }
        else
        {
            transform.position = originalPosition;
        }

        if (boxCollider != null)
        {
            boxCollider.enabled = true; // 自身のBoxCollider2Dを再度有効にする
        }
    }

    /// <summary>
    /// マウスがオブジェクトに入ったときに呼び出されるメソッド。
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!infomationPanelDisplay.isClicked)
        {
            // EqpStatsがnullではないとき、EqpStatsの各項目の中でvalueがnull又は0でないものだけを取得して、"変数名: " + value + "\n" という形式でテキストを作成
            string text = "";
            if (eqpStats != null)
            {
                foreach (var field in eqpStats.GetType().GetFields())
                {
                    if (field.GetValue(eqpStats) != null && field.GetValue(eqpStats).ToString() != "0")
                    {
                        text += field.Name + ": " + field.GetValue(eqpStats) + "\n";
                    }
                }
            }

            // ここにマウスオーバー時の処理を追加
            infomationPanelDisplay.SetActiveAndChangeText(text, gameObject);
        }
    }

    /// <summary>
    /// マウスがオブジェクトから出たときに呼び出されるメソッド。
    /// </summary>
    /// <param name="eventData">イベントデータ</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!infomationPanelDisplay.isClicked)
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
        /*
        // クリックされたらオーバーレイの処理を固定する
        if (infomationPanelDisplay.isClicked)
        {
            infomationPanelDisplay.isClicked =false;
        }
        else
        {
            infomationPanelDisplay.isClicked = true;
        }
        */
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
    /// オブジェクトが破棄される際に呼び出されるメソッド。
    /// iventryUIに通知します。
    /// </summary>
    void OnDestroy()
    {
        if (iventryUI != null)
        {
            // アイテムを破棄した際に、アイテムの並び替えを行う
            iventryUI.IventrySorting(gameObject);
        }
        Destroy(gameObject); // オブジェクト破棄
    }
}
