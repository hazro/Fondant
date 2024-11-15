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
    private GameManager gameManager;

    /// <summary>
    /// 初期設定処理。現在のシーン名を取得し、カメラをキャッシュします。
    /// </summary>
    private void Start()
    {
        // gameManagerのインスタンスを取得
        gameManager = GameManager.Instance;
        isDragging = false;
        currentSceneName = SceneManager.GetActiveScene().name;
        mainCamera = Camera.main;  // Camera.main のキャッシュ
        // シーンの種類によってドラッグ範囲を設定
        SetDragBounds();
        // 一番近い敵の方向を向く
        LookAtNearestTarget();
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
        currentSceneName = SceneManager.GetActiveScene().name;
        // シーンがロードされるたびにドラッグ範囲を設定
        SetDragBounds();
        // 一番近い敵の方向を向く
        LookAtNearestTarget();
    }

    /// <summary>
    /// ターゲットが存在する場合、最も近い距離にいる敵の方を向く
    /// </summary>
    private void Update()
    {
        // ドラッグ中だけ処理
        if (isDragging)
        {
            // 一番近い敵の方向を向く
            LookAtNearestTarget();
        }

    }

    /// <summary>
    /// 一番近い敵の方向を向くメソッド
    /// </summary>
    public void LookAtNearestTarget()
    {
        if (currentSceneName != "BattleSetupAScene" && currentSceneName != "BattleSetupBScene") return;

        if(gameManager==null) gameManager = GameManager.Instance;
        if(gameManager != null)
        {
            GameObject TargetGroup = null;
            // 自身のtagがAllyの場合
            if (gameObject.CompareTag("Ally"))
            {
                TargetGroup = gameManager.enemyGroup;
            }
            else if (gameObject.CompareTag("Enemy"))
            {
                TargetGroup = gameManager.livingUnits[0].transform.parent.gameObject;
            }
            // TargetGroupが存在しない、またはターゲットがいない場合は処理をスキップ
            if (TargetGroup == null || TargetGroup.transform.childCount == 0) return;

            // 最も近い距離にいる敵の方を向く
            Transform nearestTarget = TargetGroup.transform.GetChild(0);
            float minDistance = Vector2.Distance(transform.position, nearestTarget.position);
            for (int i = 1; i < TargetGroup.transform.childCount; i++)
            {
                Transform target = TargetGroup.transform.GetChild(i);
                float distance = Vector2.Distance(transform.position, target.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestTarget = target;
                }
            }
            // unitSpriteの向きを変更
            if (nearestTarget.position.x < transform.position.x)
            {
                unitSprite.flipX = true;
            }
            else
            {
                unitSprite.flipX = false;
            }

        }
    }

    /// <summary>
    /// バトルシーンの種類によってドラッグ範囲を変更する
    /// </summary>
    private void SetDragBounds()
    {
        if (currentSceneName == "BattleSetupAScene")
        {
            minDragBounds = new Vector2(-4.6f, -2.4f);
            maxDragBounds = new Vector2(-2.25f, 2.4f);
        }
        else if (currentSceneName == "BattleSetupBScene")
        {
            minDragBounds = new Vector2(-1.65f, -2.0f);
            maxDragBounds = new Vector2(1.65f, 2.0f);
        }
    }

    /// <summary>
    /// ドラッグが開始されたときに呼び出されるメソッド。プレイヤーの透明度を半分にし、ドラッグの準備をします。
    /// </summary>
    /// <param name="eventData">ドラッグイベントのデータ</param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // バトルセットアップシーン以外ではドラッグを無効化
        if (currentSceneName != "BattleSetupAScene" && currentSceneName !="BattleSetupBScene") return;
        // tagがEnemyの場合はドラッグを無効化
        if (gameObject.CompareTag("Enemy")) return;
        
        startPos = transform.position;
        isDragging = true;
        originalSortingOrder = unitSprite.sortingOrder;

        // つかんだ時のSEを再生
        AkSoundEngine.PostEvent("ST_Grip", gameObject);

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
                // colliderが親オブジェクトの何番目の子かを取得
                int siblingIndex = collider.transform.GetSiblingIndex() + 1;
                // UnitのpositionIDを更新
                GetComponent<Unit>().positionID = siblingIndex;
                // UnitがFitした時のSEを再生
                AkSoundEngine.PostEvent("ST_Fit", gameObject);
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
        unitSprite.material.SetColor("_Color", new Color(1, 1, 1, alpha));
    }
}
