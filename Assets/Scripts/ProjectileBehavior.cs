using System.Collections;
using UnityEngine;

/// <summary>
/// 発射物の挙動を制御するクラス
/// </summary>
public class ProjectileBehavior : MonoBehaviour
{
    private Vector2 moveDirection;
    private float moveSpeed;
    private float lifetime;
    private int remainingCharThrough;
    private int remainingObjectThrough;
    private string shooterTag; // 発射元のタグ
    private Transform followTarget;
    private bool isFadingOut = false; // アルファフェードアウトフラグ
    private TrailRenderer trailRenderer; // 軌跡を描くためのTrail Renderer
    private SpriteRenderer[] spriteRenderers; // PrefabのすべてのSpriteRenderer
    private float shakeAmplitude; // 振幅の強さ
    private bool scaleOverTime; // スケーリングを時間経過に応じて行うか
    private float followStrength = 0f; // 追従の強さ
    private float followIncreaseDuration = 1.0f; // 追従力が最大になるまでの時間
    private float timeSinceLaunch; // 発射後の経過時間

    [Header("Trail Settings")]
    [SerializeField] private float trailTime = 0.5f; // 軌跡が残る時間（秒）
    [SerializeField] private float trailWidth = 0.1f; // 軌跡の幅
    [SerializeField] private Color trailStartColor = Color.white; // 軌跡の開始色
    [SerializeField] private Color trailEndColor = new Color(1, 1, 1, 0); // 軌跡の終了色

    private bool spiralMovementEnabled = false; // 螺旋状の動きを有効にするかどうか
    private float spiralExpansionSpeed = 1f; // 螺旋の外側に広がる速度
    private float spiralAngle = 0f; // 螺旋状の角度
    private float shakeOffset = 0f; // 揺れのオフセット

    private void Start()
    {
        // Prefab以下のすべてのSpriteRendererを取得
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// 発射物の初期化
    /// </summary>
    public void Initialize(Vector2 direction, float speed, float lifetime, int charThrough, int objectThrough, string shooterTag, Transform target = null, bool enableTrail = false, bool enableSpiralMovement = false, float spiralExpansionSpeed = 1f, float shakeAmplitude = 0f, bool scaleOverTime = false)
    {
        this.moveDirection = direction.normalized;
        this.moveSpeed = speed;
        this.lifetime = lifetime;
        this.remainingCharThrough = charThrough;
        this.remainingObjectThrough = objectThrough;
        this.shooterTag = shooterTag;
        this.followTarget = target;
        this.spiralMovementEnabled = enableSpiralMovement; 
        this.spiralExpansionSpeed = spiralExpansionSpeed; 
        this.shakeAmplitude = shakeAmplitude; 
        this.scaleOverTime = scaleOverTime;

        // 軌跡の設定
        if (enableTrail)
        {
            trailRenderer = gameObject.AddComponent<TrailRenderer>();
            trailRenderer.time = trailTime;
            trailRenderer.startWidth = trailWidth;
            trailRenderer.endWidth = trailWidth;
            trailRenderer.material = new Material(Shader.Find("Sprites/Default"));
            trailRenderer.startColor = trailStartColor;
            trailRenderer.endColor = trailEndColor;

            SetSpriteRenderersAlpha(0f);
        }
        else
        {
            SetSpriteRenderersAlpha(1f);
        }

        timeSinceLaunch = 0f; // 発射時に経過時間をリセット
        StartCoroutine(DestroyAfterLifetime());
    }

    /// <summary>
    /// 毎フレームの更新処理。発射物の移動とスケーリングを制御します。
    /// </summary>
    private void Update()
    {
        timeSinceLaunch += Time.deltaTime;

        // 追従力を徐々に増加させる
        if (followTarget != null)
        {
            followStrength = Mathf.Clamp01(timeSinceLaunch / followIncreaseDuration);
        }

        // サインカーブの揺れを左右方向に加える
        shakeOffset = Mathf.Sin(Time.time * moveSpeed) * shakeAmplitude;
        Vector2 perpendicularDirection = new Vector2(-moveDirection.y, moveDirection.x); // 進行方向に直交するベクトル
        Vector2 shakeVector = perpendicularDirection * shakeOffset; // 直交方向の揺れを計算

        if (spiralMovementEnabled)
        {
            spiralAngle += moveSpeed * Time.deltaTime;
            float radius = spiralAngle * spiralExpansionSpeed;
            Vector2 spiralOffset = new Vector2(Mathf.Cos(spiralAngle), Mathf.Sin(spiralAngle)) * radius;
            transform.position = (Vector2)transform.position + (spiralOffset + shakeVector) * Time.deltaTime;

            float angle = Mathf.Atan2(spiralOffset.y, spiralOffset.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        else if (followTarget != null)
        {
            // ターゲットに向かう方向を徐々に強化
            moveDirection = Vector2.Lerp(moveDirection, (followTarget.position - transform.position).normalized, followStrength);
            transform.Translate((moveDirection * moveSpeed + shakeVector) * Time.deltaTime, Space.World);
            
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        else
        {
            transform.Translate((moveDirection * moveSpeed + shakeVector) * Time.deltaTime, Space.World);
            
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }

        if (scaleOverTime)
        {
            float scaleIncrease = 1 + moveSpeed * Time.deltaTime * 0.1f;
            transform.localScale *= scaleIncrease;
        }
    }

    private void SetSpriteRenderersAlpha(float alpha)
    {
        if (spriteRenderers != null)
        {
            foreach (var sr in spriteRenderers)
            {
                Color color = sr.color;
                color.a = alpha;
                sr.color = color;
            }
        }
    }

    /// <summary>
    /// 発射物が指定時間経過後に消滅する
    /// </summary>
    /// <returns>コルーチン</returns>
    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    /// <summary>
    /// 発射物が他のオブジェクトに衝突したときの処理
    /// </summary>
    /// <param name="collision">衝突したオブジェクトの情報</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 衝突したオブジェクトが障害物タグを持つ場合の処理
        if (collision.CompareTag("Obstacle"))
        {
            remainingObjectThrough--;
            if (remainingObjectThrough <= 0)
            {
                StartCoroutine(FadeOutAndDestroy());
            }
        }
        // 衝突したオブジェクトのタグが発射元のタグと異なる場合の処理
        else if ((collision.CompareTag("Ally") || collision.CompareTag("Enemy")) && collision.tag != shooterTag)
        {
            remainingCharThrough--;
            if (remainingCharThrough <= 0)
            {
                StartCoroutine(FadeOutAndDestroy());
            }
        }
    }

    /// <summary>
    /// 発射物をフェードアウトさせてから消滅させるコルーチン
    /// </summary>
    /// <returns>コルーチン</returns>
    private IEnumerator FadeOutAndDestroy()
    {
        isFadingOut = true;
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        float fadeDuration = 0.3f; // フェードアウトにかける時間
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

            foreach (SpriteRenderer sprite in sprites)
            {
                Color color = sprite.color;
                color.a = alpha;
                sprite.color = color;
            }

            yield return null;
        }

        // フェードアウト完了後、オブジェクトを消滅させる
        Destroy(gameObject);
    }
}
