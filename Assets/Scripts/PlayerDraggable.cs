using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// プレイヤーのオブジェクトをドラッグ可能にするためのスクリプト。
/// </summary>
public class PlayerDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPos;
    private bool isDragging = false;
    [SerializeField] private SpriteRenderer unitSprite;
    public float followSpeed = 10f; // 追従の速度
    public Vector2 minDragBounds;
    public Vector2 maxDragBounds;
    private int originalSortingOrder;
    private string currentSceneName;

    private void Start()
    {
        isDragging = false; // ドラッグ中の初期化
        // 現在のシーン名を取得
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentSceneName != "BattleSetupScene")
        {
            return;
        }
        // ドラッグ開始処理
        startPos = transform.position;
        isDragging = true;
        originalSortingOrder = unitSprite.sortingOrder;

        SetSpriteTransparency(0.5f); // 透明度を半分に設定
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            // マウスのワールド座標を取得し、追従する位置を計算
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(eventData.position);
            mousePos.z = 0;

            // ドラッグ可能範囲内に制限
            mousePos.x = Mathf.Clamp(mousePos.x, minDragBounds.x, maxDragBounds.x);
            mousePos.y = Mathf.Clamp(mousePos.y, minDragBounds.y, maxDragBounds.y);

            // 追従する位置にイーズインアウトで移動
            transform.position = Vector3.Lerp(transform.position, mousePos, Time.deltaTime * followSpeed);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            // ドラッグ終了処理
            isDragging = false;
            SetSpriteTransparency(1f); // 透明度を元に戻す

            // 吸着処理
            Collider2D[] colliders = Physics2D.OverlapPointAll(transform.position);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("DropZone"))
                {
                    // ターゲットのDropZone内の他のPlayerをチェック
                    Collider2D[] dropZoneColliders = Physics2D.OverlapBoxAll(collider.bounds.center, collider.bounds.size, 0f);
                    PlayerDraggable otherPlayer = null;

                    foreach (Collider2D zoneCollider in dropZoneColliders)
                    {
                        if (zoneCollider != collider)
                        {
                            otherPlayer = zoneCollider.GetComponent<PlayerDraggable>();
                            if (otherPlayer != null && otherPlayer != this)
                            {
                                // 他のPlayerが存在する場合、位置を入れ替える
                                Vector3 otherPlayerStartPos = otherPlayer.startPos;
                                otherPlayer.transform.position = startPos;
                                otherPlayer.startPos = otherPlayerStartPos;
                                break;
                            }
                        }
                    }

                    // 新しいドロップゾーンにプレイヤーを移動
                    transform.position = collider.transform.position + new Vector3(0,0,-0.1f);
                    return;
                }
            }

            // ドロップゾーンが見つからない場合、元の位置に戻す
            transform.position = startPos;
        }
    }

    /// <summary>
    /// スプライトの透明度を設定するメソッド
    /// </summary>
    /// <param name="alpha">設定する透明度（0-1の範囲）</param>
    private void SetSpriteTransparency(float alpha)
    {
        Color color = unitSprite.color;
        color.a = alpha;
        unitSprite.color = color;
    }
}
