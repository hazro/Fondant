using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/// <summary>
/// プレイヤーのオブジェクトをドラッグ可能にするためのスクリプト。
/// ドロップゾーンにプレイヤーを移動させ、他のプレイヤーと位置を入れ替える機能を提供します。
/// </summary>
public class PlayerDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPos;
    private bool isDragging = false;
    [SerializeField] private SpriteRenderer unitSprite;
    //public float followSpeed = 10f; // 追従の速度
    public Vector2 minDragBounds;
    public Vector2 maxDragBounds;
    private int originalSortingOrder;
    private string currentSceneName;
    private Camera mainCamera;  // Camera.main をキャッシュ

    /// <summary>
    /// 初期設定処理。現在のシーン名を取得し、カメラをキャッシュします。
    /// </summary>
    private void Start()
    {
        isDragging = false;
        currentSceneName = SceneManager.GetActiveScene().name;
        mainCamera = Camera.main;  // Camera.main のキャッシュ
    }

    /// <summary>
    /// SetActive(true) で有効化されたときに呼び出される
    /// </summary>
    private void OnEnable()
    {
        // シーンがロードされるたびにOnSceneLoadedメソッドを呼び出す
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// SetActive(false) で無効化されたときに呼び出される
    /// </summary>
    private void OnDisable()
    {
        // イベントの登録を解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // シーンがロードされるたびにシーン名を更新
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    /// <summary>
    /// ドラッグが開始されたときに呼び出されるメソッド。プレイヤーの透明度を半分にし、ドラッグの準備をします。
    /// </summary>
    /// <param name="eventData">ドラッグイベントのデータ</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (currentSceneName != "BattleSetupScene") return;
        
        startPos = transform.position;
        isDragging = true;
        originalSortingOrder = unitSprite.sortingOrder;

        SetSpriteTransparency(0.5f); // 透明度を半分に設定
    }

    /// <summary>
    /// ドラッグ中に呼び出されるメソッド。プレイヤーがマウス位置に追従します。
    /// </summary>
    /// <param name="eventData">ドラッグイベントのデータ</param>
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            // キャッシュしたカメラを使用
            Vector3 mousePos = mainCamera.ScreenToWorldPoint(eventData.position);
            mousePos.z = 0;

            // 範囲内に制限
            mousePos.x = Mathf.Clamp(mousePos.x, minDragBounds.x, maxDragBounds.x);
            mousePos.y = Mathf.Clamp(mousePos.y, minDragBounds.y, maxDragBounds.y);

            // 追従速度を最適化してプレイヤーの位置をマウスに追従
            transform.position = mousePos;
        }
    }

    /// <summary>
    /// ドラッグが終了したときに呼び出されるメソッド。ドロップゾーン内にプレイヤーを配置し、他のプレイヤーとの位置を交換します。
    /// </summary>
    /// <param name="eventData">ドラッグイベントのデータ</param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

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
                transform.position = collider.transform.position + new Vector3(0, 0, -0.1f);
                return;
            }
        }

        // ドロップゾーンが見つからない場合、元の位置に戻す
        transform.position = startPos;
    }

    /// <summary>
    /// スプライトの透明度を設定するメソッド。
    /// </summary>
    /// <param name="alpha">設定する透明度（0-1の範囲）</param>
    private void SetSpriteTransparency(float alpha)
    {
        Color color = unitSprite.color;
        color.a = alpha;
        unitSprite.color = color;
    }
}
