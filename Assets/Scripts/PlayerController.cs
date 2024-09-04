using UnityEngine;

/// <summary>
/// プレイヤーの移動とアニメーションを管理するクラス。
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;  // プレイヤーの移動速度
    public Sprite[] walkDownSprites;  // 下方向の移動スプライト
    public Sprite[] walkUpSprites;  // 上方向の移動スプライト
    public Sprite[] walkLeftSprites;  // 左方向の移動スプライト
    public Sprite[] walkRightSprites;  // 右方向の移動スプライト
    public Sprite[] idleDownSprites;  // 下方向の待機スプライト
    public Sprite[] idleUpSprites;  // 上方向の待機スプライト
    public Sprite[] idleLeftSprites;  // 左方向の待機スプライト
    public Sprite[] idleRightSprites;  // 右方向の待機スプライト
    public float animationSpeed = 0.1f;  // アニメーションの速度

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 movement;
    private int currentFrame;
    private float animationTimer;
    private bool isMoving;
    private string currentDirection = "Down";

    private BoxCollider2D playerCollider; // プレイヤーのCollider
    private LayerMask obstacleLayerMask; // 障害物のレイヤーマスク

    /// <summary>
    /// ゲームオブジェクトの初期化時に呼び出されるメソッド。
    /// Rigidbody2DとSpriteRendererのコンポーネントを取得または追加します。
    /// </summary>
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2Dが見つかりません。PlayerオブジェクトにRigidbody2Dを追加してください。");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning("SpriteRendererが見つかりません。PlayerオブジェクトにSpriteRendererを追加します。");
            spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        }

        playerCollider = GetComponent<BoxCollider2D>();

        if (playerCollider == null)
        {
            Debug.LogWarning("BoxCollider2Dが見つかりません。PlayerオブジェクトにBoxCollider2Dを追加します。");
            playerCollider = gameObject.AddComponent<BoxCollider2D>();
        }

        obstacleLayerMask = LayerMask.GetMask("Obstacle"); // 障害物レイヤーを取得
    }

    /// <summary>
    /// ゲーム開始時に呼び出されるメソッド。
    /// 初期状態の待機スプライトを下向きに設定します。
    /// </summary>
    private void Start()
    {
        SetIdleSprites("Down");
    }

    /// <summary>
    /// 毎フレーム呼び出されるメソッド。
    /// プレイヤーの入力を処理し、アニメーションを制御します。
    /// </summary>
    private void Update()
    {
        ProcessInputs();
        AnimateMovement();
    }

    /// <summary>
    /// 一定間隔で呼び出されるメソッド。物理演算に基づいてプレイヤーを移動させます。
    /// </summary>
    private void FixedUpdate()
    {
        MovePlayer();
    }

    /// <summary>
    /// コライダーとの接触を検出するためのメソッド
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("WorldEntrance"))
        {
            // バトルシーンへ移動し、キャラクター配置画面を表示
            GameManager.Instance.LoadScene("InToWorldEntrance");
        }
    }

    /// <summary>
    /// プレイヤーの移動を処理するメソッド。
    /// 移動先に障害物がない場合にのみ移動します。
    /// </summary>
    private void MovePlayer()
    {
        Vector2 targetPosition = rb.position + movement * moveSpeed * Time.fixedDeltaTime;

        // 移動先に障害物がないかをチェック
        if (!Physics2D.OverlapBox(targetPosition, playerCollider.size, 0f, obstacleLayerMask))
        {
            rb.MovePosition(targetPosition);
        }
    }

    /// <summary>
    /// プレイヤーの入力を処理するメソッド。
    /// WASDキーによる移動方向を設定します。
    /// </summary>
    private void ProcessInputs()
    {
        movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        isMoving = movement != Vector2.zero;

        if (isMoving)
        {
            if (movement.x > 0)
            {
                currentDirection = "Right";
            }
            else if (movement.x < 0)
            {
                currentDirection = "Left";
            }
            else if (movement.y > 0)
            {
                currentDirection = "Up";
            }
            else if (movement.y < 0)
            {
                currentDirection = "Down";
            }
        }
    }

    /// <summary>
    /// プレイヤーの移動アニメーションを処理するメソッド。
    /// 移動中のスプライトをアニメーションさせ、待機状態の場合は待機スプライトを設定します。
    /// </summary>
    private void AnimateMovement()
    {
        if (isMoving)
        {
            animationTimer += Time.deltaTime;

            if (animationTimer >= animationSpeed)
            {
                currentFrame = (currentFrame + 1) % Mathf.Max(1, walkDownSprites.Length);
                animationTimer = 0f;

                switch (currentDirection)
                {
                    case "Down":
                        spriteRenderer.sprite = walkDownSprites[Mathf.Min(currentFrame, walkDownSprites.Length - 1)];
                        break;
                    case "Up":
                        spriteRenderer.sprite = walkUpSprites[Mathf.Min(currentFrame, walkUpSprites.Length - 1)];
                        break;
                    case "Left":
                        spriteRenderer.sprite = walkLeftSprites[Mathf.Min(currentFrame, walkLeftSprites.Length - 1)];
                        break;
                    case "Right":
                        spriteRenderer.sprite = walkRightSprites[Mathf.Min(currentFrame, walkRightSprites.Length - 1)];
                        break;
                }
            }
        }
        else
        {
            SetIdleSprites(currentDirection);
        }
    }

    /// <summary>
    /// プレイヤーの待機スプライトを設定するメソッド。
    /// 指定された方向に応じた待機スプライトを設定し、アニメーションさせます。
    /// </summary>
    /// <param name="direction">待機スプライトの方向</param>
    private void SetIdleSprites(string direction)
    {
        animationTimer += Time.deltaTime;

        if (animationTimer >= animationSpeed)
        {
            currentFrame = (currentFrame + 1) % Mathf.Max(1, idleDownSprites.Length);
            animationTimer = 0f;

            switch (direction)
            {
                case "Down":
                    spriteRenderer.sprite = idleDownSprites[Mathf.Min(currentFrame, idleDownSprites.Length - 1)];
                    break;
                case "Up":
                    spriteRenderer.sprite = idleUpSprites[Mathf.Min(currentFrame, idleUpSprites.Length - 1)];
                    break;
                case "Left":
                    spriteRenderer.sprite = idleLeftSprites[Mathf.Min(currentFrame, idleLeftSprites.Length - 1)];
                    break;
                case "Right":
                    spriteRenderer.sprite = idleRightSprites[Mathf.Min(currentFrame, idleRightSprites.Length - 1)];
                    break;
            }
        }
    }
}
