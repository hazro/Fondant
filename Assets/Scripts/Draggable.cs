using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// プレイヤーのオブジェクトをドラッグ可能にするためのスクリプト。
/// </summary>
public class Draggable : MonoBehaviour
{
    private Vector3 startPos;
    private bool isDragging = false;
    private SpriteRenderer spriteRenderer;
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

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
        if (currentSceneName != "BattleSetupScene")
        {
            return;
        }
        // ドラッグ開始処理
        startPos = transform.position;
        isDragging = true;
        originalSortingOrder = spriteRenderer.sortingOrder;

        SetSpriteTransparency(0.5f); // 透明度を半分に設定
    }

    private void OnMouseDrag()
    {
        if (isDragging)
        {
            // マウスのワールド座標を取得し、追従する位置を計算
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            // ドラッグ可能範囲内に制限
            mousePos.x = Mathf.Clamp(mousePos.x, minDragBounds.x, maxDragBounds.x);
            mousePos.y = Mathf.Clamp(mousePos.y, minDragBounds.y, maxDragBounds.y);

            // 追従する位置にイーズインアウトで移動
            transform.position = Vector3.Lerp(transform.position, mousePos, Time.deltaTime * followSpeed);
        }
    }

    private void OnMouseUp()
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
                    Draggable otherPlayer = null;

                    foreach (Collider2D zoneCollider in dropZoneColliders)
                    {
                        if (zoneCollider != collider)
                        {
                            otherPlayer = zoneCollider.GetComponent<Draggable>();
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
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
