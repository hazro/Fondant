using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 発射物の挙動を制御するクラス
/// </summary>
public class ProjectileBehavior : MonoBehaviour
{
    [HideInInspector] public int unitID; // 発射元のユニットID
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>(); // ヒットしたオブジェクトを追跡するためのセット
    private Vector2 moveDirection;
    private float moveSpeed;
    private int remainingCharThrough;
    private int remainingObjectThrough;
    private string shooterTag; // 発射元のタグ
    private Transform followTarget;
    private TrailRenderer trailRenderer; // 軌跡を描くためのTrail Renderer
    private SpriteRenderer[] spriteRenderers; // PrefabのすべてのSpriteRenderer
    private float shakeAmplitude; // 振幅の強さ
    private bool scaleOverTime; // スケーリングを時間経過に応じて行うか
    private float followStrength = 0f; // 追従の強さ
    private float followIncreaseDuration = 1.0f; // 追従力が最大になるまでの時間
    private float timeSinceLaunch; // 発射後の経過時間

    [Header("Attributes")]
    [SerializeField] public List<Attribute> attributes = new List<Attribute>(); // Attributesをpublicに変更
    private float pysicalPower = 1.0f; // 物理攻撃力
    private float magicalPower = 1.0f; // 魔法攻撃力

    [Header("Trail Settings")]
    [SerializeField] private float trailTime = 0.5f; // 軌跡が残る時間（秒）
    [SerializeField] private float trailWidth = 0.1f; // 軌跡の幅
    [SerializeField] private Color trailStartColor = Color.white; // 軌跡の開始色
    [SerializeField] private Color trailEndColor = new Color(1, 1, 1, 0); // 軌跡の終了色

    // 消滅条件
    [Header("Destruction Conditions")]
    [SerializeField] private bool useDistance = false; // 到達距離で消滅するかどうか、falseの場合はlifetimeで消滅
    private float maxDistance = 3.0f; // 到達距離(物理攻撃)
    private float lifetime = 1.0f; // 消滅までの時間(魔法攻撃)
    private Vector3 startPosition; // 発生位置

    // スキルの効果
    [Header("Skill Effects")]
    [HideInInspector] public bool chainAttackEnabled = false; // チェイン攻撃の有効化

    // 発射元のユニット
    public Unit shooterUnit { get; private set; }

    private bool spiralMovementEnabled = false; // 螺旋状の動きを有効にするかどうか
    private float spiralExpansionSpeed = 1f; // 螺旋の外側に広がる速度
    private float spiralAngle = 0f; // 螺旋状の角度
    private float shakeOffset = 0f; // 揺れのオフセット

    public enum Attribute
    {
        Physical,
        Magical,
        Technology,
        Nature,
        Healing,
        StatusAilment,
        StatusEffect
    }

    public enum StatusAilment
    {
        Poison,
        Burn,
        Freeze,
        Paralysis,
        Weakness
    }

    public enum StatusEffect
    {
        AttackBoost,
        MagicBoost,
        DefenseBoost,
        MagicDefenseBoost,
        SpeedBoost,
        AutoHeal
    }

    // フィールドを非公開にして、プロパティで管理する
    private StatusAilment currentStatusAilment; // インスペクタには表示しない
    private StatusEffect currentStatusEffect;   // インスペクタには表示しない

    /// <summary>
    /// ステータス異常のプロパティ
    /// </summary>
    public StatusAilment CurrentStatusAilment
    {
        get => currentStatusAilment;
        set => currentStatusAilment = value;
    }

    /// <summary>
    /// ステータス効果のプロパティ
    /// </summary>
    public StatusEffect CurrentStatusEffect
    {
        get => currentStatusEffect;
        set => currentStatusEffect = value;
    }

    private void Start()
    {
        // Prefab以下のすべてのSpriteRendererを取得
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }

    /// <summary>
    /// 発射物の初期化
    /// </summary>
    public void Initialize(
        // 必須パラメータ
        int unitID, // 死亡時にステータスログに記録するためのユニットID
        Vector2 direction, 
        float adjustedLifetime, 
        string shooterTag, 
        float pysicalAttackPower,
        float magicalAttackPower,
        float attackSpeed,
        int attackUnitThrough,
        int attackObjectThrough,
        float attackDistance,
        // オプションパラメータ
        Transform target = null, 
        bool enableTrail = false, 
        bool enableSpiralMovement = false, 
        float spiralExpansionSpeed = 1f, 
        float shakeAmplitude = 0f, 
        bool scaleOverTime = false,
        bool chainAttack = false,
        List<Attribute> attributes = null, 
        StatusAilment? statusAilment = null, 
        StatusEffect? statusEffect = null
        )
    {
        this.unitID = unitID;
        this.moveDirection = direction.normalized;
        this.moveSpeed = attackSpeed;
        this.lifetime = lifetime * adjustedLifetime;
        this.remainingCharThrough = attackUnitThrough;
        this.remainingObjectThrough = attackObjectThrough;
        this.shooterTag = shooterTag;
        this.followTarget = target;
        this.spiralMovementEnabled = enableSpiralMovement;
        this.spiralExpansionSpeed = spiralExpansionSpeed;
        this.shakeAmplitude = shakeAmplitude;
        this.scaleOverTime = scaleOverTime;
        this.pysicalPower = pysicalAttackPower;
        this.magicalPower = magicalAttackPower;
        this.maxDistance = attackDistance;
        this.chainAttackEnabled = chainAttack;

        if (attributes != null)
        {
            this.attributes = attributes;
        }

        if (statusAilment.HasValue)
        {
            this.CurrentStatusAilment = (StatusAilment)statusAilment; // 明示的なキャストを使用
        }

        if (statusEffect.HasValue)
        {
            this.CurrentStatusEffect = (StatusEffect)statusEffect; // 明示的なキャストを使用
        }

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

        // 消滅条件によってコルーチンを選択
        if (useDistance)
        {
            // 到達距離で消滅する場合
            StartCoroutine(CheckDistance());
        }
        else
        {
            // 指定時間経過後に消滅する場合
            StartCoroutine(DestroyAfterLifetime());
        }
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
            // ターゲットに向かう方向を徐々に強化
            moveDirection = Vector2.Lerp(moveDirection, (followTarget.position - transform.position).normalized, followStrength);
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
            // ターゲットに向かって移動
            transform.Translate((moveDirection * moveSpeed + shakeVector) * Time.deltaTime, Space.World);
            
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        else
        {
            // followTargetがない場合は直進
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


    /// <summary>
    /// 発射元のユニットを設定する
    /// </summary>
    /// <param name="unit"></param>
    public void SetShooterUnit(Unit unit)
    {
        shooterUnit = unit;
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
    /// 発射物の開始距離からのトータル移動距離がmaxDistanceに到達したときに消滅する
    /// </summary>
    /// <returns></returns>
    private IEnumerator  CheckDistance()
    {
        startPosition = transform.position;
        while (Vector3.Distance(startPosition, transform.position) < maxDistance)
        {
            yield return null;
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// 発射物が他のオブジェクトに衝突したときの処理
    /// </summary>
    /// <param name="collision">衝突したオブジェクトの情報</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ヒットしたオブジェクトをセットに追加
        hitObjects.Add(collision.gameObject);

        // 発射元が設定されていない場合は処理を行わない
        if (shooterUnit == null)
        {
            return;
        }

        // 衝突したオブジェクトが発射物の場合は無視する
        if (collision.CompareTag("Aef"))
        {
            return;
        }

        // 発射元のtargetSameTagを取得
        bool targetSameTag = shooterUnit.GetComponent<UnitController>().targetSameTag;

        // 衝突したオブジェクトが障害物タグを持つ場合の処理
        if (collision.CompareTag("Obstacle"))
        {
            remainingObjectThrough--;
            if (remainingObjectThrough <= 0)
            {
                StartCoroutine(FadeOutAndDestroy());
            }
        }

        // 衝突したオブジェクトがArenaLimitタグを持つ場合は消滅する(そこまでしか行けない)
        else if (collision.CompareTag("ArenaLimit"))
        {
            Destroy(gameObject);
        }
        
        // 発射元のtargetSameTagがfalseであれば発射元のタグと異なる場合、trueであれば発射元のタグと同じ場合の処理
        // collision
        else if ((collision.CompareTag("Ally") || collision.CompareTag("Enemy")) && (targetSameTag && collision.tag == shooterTag) || (!targetSameTag && collision.tag != shooterTag))
        {
            // 自分をターゲットにしていないのに自分に当たった場合は無視する
            if (collision.gameObject == shooterUnit.gameObject)
            {
                return;
            }
            // ダメージを与える
            Unit targetUnit = collision.gameObject.GetComponent<Unit>();
            if (targetUnit != null)
            {
                float totalDamage = 0;
                float totalHealing = 0;

                foreach (Attribute attribute in attributes)
                {
                    switch (attribute)
                    {
                        case Attribute.Magical:
                        case Attribute.Technology:
                        case Attribute.Nature:
                            float magicDamage = magicalPower - (targetUnit.magicalDefensePower * 0.1f);
                            totalDamage += magicDamage;
                            break;

                        case Attribute.Physical:
                            float physicalDamage = pysicalPower - (targetUnit.physicalDefensePower * 0.1f);
                            totalDamage += physicalDamage;
                            break;

                        case Attribute.Healing:
                            float healing = magicalPower;
                            totalHealing += healing;
                            break;
                    }
                }

                if (totalDamage > 0)
                {
                    targetUnit.TakeDamage(totalDamage, unitID);
                }

                if (totalHealing > 0)
                {
                    targetUnit.Heal(totalHealing, unitID);
                }
            }
            // 通過回数を減らす
            remainingCharThrough--;
            // 通過回数が0以下になったら消滅する
            if (remainingCharThrough <= 0)
            {
                Destroy(gameObject);
                //StartCoroutine(FadeOutAndDestroy());
            }
            // 衝突後消滅しない場合は現在のターゲット以外の一番近い位置にいるターゲットに変更する
            else
            {
                // チェイン攻撃が有効の場合は一番近いターゲットを取得してターゲットを変更する
                if (chainAttackEnabled)
                {
                    // 一番近いターゲットを取得
                    Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 10f);
                    float minDistance = Mathf.Infinity;
                    Transform nearestTarget = null;
                    foreach (Collider2D col in colliders)
                    {
                        // ターゲットが元のターゲットtagと同じ場合のみ処理を行う。元のターゲットと同じオブジェクトの場合や、すでにhitしたオブジェクトは無視する
                        if ((col.CompareTag("Ally") || col.CompareTag("Enemy")) && (targetUnit.tag == col.tag) && (collision.gameObject != col.gameObject) && !hitObjects.Contains(col.gameObject))
                        {
                            float distance = Vector2.Distance(transform.position, col.transform.position);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                nearestTarget = col.transform;
                            }
                        }
                    }
                    // 一番近いターゲットがいる場合はターゲットを変更する
                    if (nearestTarget != null)
                    {
                        // hitした位置からnearestTargetの方向を取得し、moveDirectionを変更する
                        moveDirection = (nearestTarget.position - transform.position).normalized;

                        // followTargetが設定されていない場合はnearestTargetに変更する
                        if(followTarget != null)
                        {
                            followTarget = nearestTarget;
                            timeSinceLaunch = 0f; // リセットして追従効果をすぐに適用
                        }
                    }
                    else
                    {
                        // 一番近いターゲットがいない場合は今のmoveDirection方向に直進する
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 発射物をフェードアウトさせてから消滅させるコルーチン
    /// </summary>
    /// <returns>コルーチン</returns>
    private IEnumerator FadeOutAndDestroy()
    {
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        float fadeDuration = 0.1f; // フェードアウトにかける時間
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
