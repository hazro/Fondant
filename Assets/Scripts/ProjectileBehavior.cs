using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 発射物の挙動を制御するクラス
/// </summary>
public class ProjectileBehavior : MonoBehaviour
{
    private AttackController attackController; // 攻撃コントローラーの参照
    [HideInInspector] public int unitID; // 発射元のユニットID
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>(); // ヒットしたオブジェクトを追跡するためのセット
    private Dictionary<GameObject, float> damageTimers = new Dictionary<GameObject, float>(); // trailColliderが接触中のオブジェクトと最後にダメージを与えた時間を管理
    private Vector2 moveDirection;
    private float moveSpeed;
    private int remainingCharThrough;
    private int remainingObjectThrough;
    private bool throughOff = false; // 通過を強制無効にするかどうか
    private string shooterTag; // 発射元のタグ
    private Transform followTarget;
    private TrailRenderer trailRenderer; // 軌跡を描くためのTrail Renderer
    [SerializeField] private List<GameObject> trailColliders = new List<GameObject>();
    private Vector3 lastColliderPosition; // 最後にtrailコライダーを生成した位置
    private float colliderSpacing = 0.33f; // trailコライダーを生成する間隔
    private SpriteRenderer[] spriteRenderers; // PrefabのすべてのSpriteRenderer
    private float shakeAmplitude; // 振幅の強さ
    private bool scaleOverTime; // スケーリングを時間経過に応じて行うか
    private float followStrength = 0f; // 追従の強さ
    private float followIncreaseDuration = 1.0f; // 追従力が最大になるまでの時間
    private float timeSinceLaunch; // 発射後の経過時間
    private float damageInterval = 0.5f; // ダメージを与える間隔（秒）
    private GameObject skipObject; // 衝突判定をスキップするオブジェクト

    [Header("Attributes")]
    [SerializeField] public List<Attribute> attributes = new List<Attribute>(); // Attributesをpublicに変更
    private float pysicalPower = 1.0f; // 物理攻撃力
    private float magicalPower = 1.0f; // 魔法攻撃力

    [Header("Trail Settings")]
    [SerializeField] private GameObject projectileImage; // 発射物の画像
    private float trailTime = 1.0f; // 軌跡が残る時間（秒）
    [SerializeField] private float trailWidth = 0.1f; // 軌跡の幅
    [SerializeField] private Color trailStartColor = Color.white; // 軌跡の開始色
    [SerializeField] private Color trailEndColor = new Color(1, 1, 1, 0); // 軌跡の終了色
    private bool isWaitingForTrailToDisappear = false; // トレイルの消失待機フラグ

    // 消滅条件
    [Header("Destruction Conditions")]
    [SerializeField] private bool useDistance = false; // 到達距離で消滅するかどうか、falseの場合はlifetimeで消滅
    private float maxDistance = 3.0f; // 到達距離(物理攻撃)
    private float lifetime = 1.0f; // 消滅までの時間(魔法攻撃)
    private Vector3 startPosition; // 発生位置

    // スキルの効果
    [Header("Skill Effects")]
    [HideInInspector] public bool chainAttackEnabled = false; // チェイン攻撃の有効化
    [HideInInspector] public bool spreadAttackEnabled = false; // スプレッド攻撃の有効化

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
        // 初期位置を設定
        lastColliderPosition = transform.position;
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
        GameObject skipObj,
        float adjustedLifetime, 
        string shooterTag, 
        float pysicalAttackPower,
        float magicalAttackPower,
        float attackSpeed,
        int attackUnitThrough,
        int attackObjectThrough,
        float attackDistance,
        float attackSize,
        // オプションパラメータ
        Transform target = null, 
        bool enableTrail = false, 
        bool enableSpiralMovement = false, 
        float spiralExpansionSpeed = 1f, 
        float shakeAmplitude = 0f, 
        bool scaleOverTime = false,
        bool chainAttack = false,
        bool spreadAttack = false,
        bool throughEnforce = false,
        List<Attribute> attributes = null, 
        StatusAilment? statusAilment = null, 
        StatusEffect? statusEffect = null
        )
    {
        this.unitID = unitID;
        this.moveDirection = direction.normalized;
        this.skipObject = skipObj;
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
        this.transform.localScale *= attackSize;
        this.spreadAttackEnabled = spreadAttack;
        this.throughOff = throughEnforce;

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

        // 軌跡に沿ったコライダーを生成・更新
        GenerateTrailColliders();

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
            float scaleIncrease = 1 + moveSpeed * Time.deltaTime * 0.3f;
            transform.localScale *= scaleIncrease;
        }
    }

    /// <summary>
    /// TrailRendererに沿って等間隔でコライダーを生成する
    /// </summary>
    private void GenerateTrailColliders()
    {
        if (trailRenderer == null) return;

        Vector3 currentTrailPosition = transform.position;

        float distance = Vector3.Distance(lastColliderPosition, currentTrailPosition);

        if (distance >= colliderSpacing)
        {
            GameObject colliderObject = new GameObject("TrailCollider");
            colliderObject.transform.position = currentTrailPosition;
            colliderObject.transform.parent = trailRenderer.transform.parent;

            // コライダーの設定
            BoxCollider2D collider = colliderObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            // コライダーのサイズを設定
            collider.size = new Vector2(distance, 0.1f);

            // Rigidbody2Dを追加して設定
            Rigidbody2D rb = colliderObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic; // 物理演算に影響しない
            rb.gravityScale = 0; // 重力の影響を受けない

            // TrailColliderBehaviorを追加し、ProjectileBehaviorへの参照を渡す
            TrailColliderBehavior trailBehavior = colliderObject.AddComponent<TrailColliderBehavior>();
            trailBehavior.Initialize(this);

            // 発射物のすべてのコライダーを取得してIgnoreCollisionを適用
            Collider2D[] projectileColliders = GetComponents<Collider2D>();
            foreach (var projectileCollider in projectileColliders)
            {
                Physics2D.IgnoreCollision(collider, projectileCollider);
            }

            trailColliders.Add(colliderObject);
            lastColliderPosition = currentTrailPosition;
        }
    }

    /// <summary>
    /// 発射元のユニットを設定する
    /// </summary>
    /// <param name="unit"></param>
    public void SetShooterUnit(Unit unit)
    {
        shooterUnit = unit;
        attackController = unit.GetComponent<AttackController>();
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
            // トレイルが有効なら発射物を非アクティブ化し、待機モードに移行
            if (trailRenderer != null && trailRenderer.time > 0)
            {
                StartCoroutine(WaitForTrailToDisappear());
            }
            else
            {
                // トレイルがない場合は即座に消滅
                Destroy(gameObject);
            }
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
            // トレイルが有効なら発射物を非アクティブ化し、待機モードに移行
            if (trailRenderer != null && trailRenderer.time > 0)
            {
                StartCoroutine(WaitForTrailToDisappear());
            }
            else
            {
                // トレイルがない場合は即座に消滅
                Destroy(gameObject);
            }
    }

    /// <summary>
    /// トレイルコライダーが接触を検知した際に呼び出される
    /// TriggerStay2D処理：接触したオブジェクトにダメージを間隔をあけて与え続ける
    /// </summary>
    /// <param name="collision">接触したオブジェクト</param>
    public void OnTrailColliderTriggerStay(TrailColliderBehavior trailCollider, Collider2D collision)
    {
        // 接触対象が有効でない場合はスキップ
        if (collision == null || shooterUnit == null) return;
        // 発射元のtargetSameTagを取得
        bool targetSameTag = shooterUnit.GetComponent<UnitController>().targetSameTag;

        if ((collision.CompareTag("Ally") || collision.CompareTag("Enemy")) && 
            ((targetSameTag && collision.tag == shooterTag) || (!targetSameTag && collision.tag != shooterTag)))
        {
            GameObject target = collision.gameObject;

            // 最初の接触またはダメージ間隔を超えた場合にダメージを与える
            if (!damageTimers.ContainsKey(target) || Time.time - damageTimers[target] >= damageInterval)
            {
                Unit targetUnit = target.gameObject.GetComponent<Unit>();
                ApplyDamage(targetUnit , 0.25f); // 通常のダメージの1/4を与える
                damageTimers[target] = Time.time; // 最終ダメージ時間を記録
            }
        }
    }

    /// <summary>
    /// TriggerExit2D処理：接触が終了したオブジェクトをタイマーから削除
    /// </summary>
    /// <param name="collision">接触が終了したオブジェクト</param>
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (damageTimers.ContainsKey(collision.gameObject))
        {
            damageTimers.Remove(collision.gameObject);
        }
    }

    /// <summary>
    /// 発射物が他のオブジェクトに衝突したときの処理
    /// </summary>
    /// <param name="collision">衝突したオブジェクトの情報</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // トレイルの消失待機中なら処理を無視
        if (isWaitingForTrailToDisappear)
        {
            return;
        }

        // TrailColliderとの衝突を無視する
        if (collision.gameObject.name.StartsWith("TrailCollider"))
        {
            return;
        }
        
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
                // トレイルが有効なら発射物を非アクティブ化し、待機モードに移行
                if (trailRenderer != null && trailRenderer.time > 0)
                {
                    StartCoroutine(WaitForTrailToDisappear());
                }
                else
                {
                    // トレイルがない場合は即座に消滅
                    Destroy(gameObject);
                }
            }
        }

        /* 攻撃は範囲外に出るのを許可するためコメントアウト
        // 衝突したオブジェクトがArenaLimitタグを持つ場合は消滅する(そこまでしか行けない)
        else if (collision.CompareTag("ArenaLimit"))
        {
            Destroy(gameObject);
        }
        */
        
        // 発射元のtargetSameTagがfalseであれば発射元のタグと異なる場合、trueであれば発射元のタグと同じ場合の処理
        // collision
        else if ((collision.CompareTag("Ally") || collision.CompareTag("Enemy")) && ((targetSameTag && collision.tag == shooterTag) || (!targetSameTag && collision.tag != shooterTag)) && skipObject != collision.gameObject)
        {
            // 自分をターゲットにしていないのに自分に当たった場合は無視する
            if (collision.gameObject == shooterUnit.gameObject)
            {
                return;
            }
            // ダメージを与える
            Unit targetUnit = collision.gameObject.GetComponent<Unit>();
            ApplyDamage(targetUnit);
            // 通過回数が0以下になったら消滅する throughOffがtrueの場合は強制消滅
            if (remainingCharThrough <= 0)
            {
                // トレイルが有効なら発射物を非アクティブ化し、待機モードに移行
                if (trailRenderer != null && trailRenderer.time > 0)
                {
                    StartCoroutine(WaitForTrailToDisappear());
                }
                else
                {
                    // トレイルがない場合は即座に消滅
                    Destroy(gameObject);
                }
            }
            // 衝突後消滅しない場合は現在のターゲット以外の一番近い位置にいるターゲットに変更する
            else
            {
                // 拡散攻撃が有効の場合は進んでいた方向の-30度と+30度の2方向それぞれにもう1発づつ発射する
                if (spreadAttackEnabled  && !throughOff)
                {
                    if (attackController == null)
                    {
                        attackController = shooterUnit.GetComponent<AttackController>();
                    }
                    // 0.1秒起きに拡散発射を行う。引数はスキップするオブジェクト
                    StartCoroutine(WaitForNextSpreadAttack(collision.gameObject));
                }
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

            // 通過回数を減らす
            remainingCharThrough--;
        }
    }

    /// <summary>
    /// 拡散発射を行う
    /// </summary>
    private IEnumerator WaitForNextSpreadAttack(GameObject skipObj)
    {
        // 拡散数
        int spreadCount = 1 + shooterUnit.spreadCount;
        float damageMulti = shooterUnit.spreadDamage;
        for (int i = 0; i < spreadCount; i++)
        {
            // 拡散角度
            float spreadAngle = (30 * (i + 1)) - ((spreadCount+1) * 30)/2;
            // 拡散角度の方向
            Vector2 spreadDirection = Quaternion.Euler(0, 0, spreadAngle) * moveDirection;
            // 発射、現在の位置から発射するためにtransform.positionを渡し、貫通を強制無効にする
            attackController.ShootProjectileInDirection(spreadDirection, transform.position, true, damageMulti , skipObj);
            // 0.1秒待機して次の発射を行う
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// トレイルが完全に消えるまで待機し、発射物を削除する
    /// </summary>
    private IEnumerator WaitForTrailToDisappear()
    {
        isWaitingForTrailToDisappear = true;

        // 発射物の見た目用オブジェクトを非表示にする
        if (spriteRenderers != null)
        {
            if (projectileImage.activeSelf)
            {
                projectileImage.SetActive(false);
            }
        }

        // 発射物をその場に固定
        moveSpeed = 0;
        followTarget = null;

        // 発射物の接触処理を停止
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // コライダーを削除
        while (trailColliders.Count > 0)
        {
            GameObject trailCollider = trailColliders[0]; // 最初のコライダーを取得
            trailColliders.RemoveAt(0); // リストから削除
            if (trailCollider != null)
            {
                Destroy(trailCollider); // コライダーを削除
            }

            // 次の削除まで少し待機
            yield return new WaitForSeconds(0.3f);
        }

        // トレイルが完全に消えるまで待機
        yield return new WaitForSeconds(trailRenderer.time);

        // 親オブジェクトを削除
        Destroy(gameObject);
    }

    /// <summary>
    /// 指定のUnitにダメージを与える
    /// </summary>
    private void ApplyDamage(Unit target , float damageMultiplier = 1.0f)
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
                    float magicDamage = magicalPower - (target.magicalDefensePower * 0.1f);
                    magicDamage *= damageMultiplier;
                    totalDamage += magicDamage;
                    break;

                case Attribute.Physical:
                    float physicalDamage = pysicalPower - (target.physicalDefensePower * 0.1f);
                    physicalDamage *= damageMultiplier;
                    totalDamage += physicalDamage;
                    break;

                case Attribute.Healing:
                    float healing = magicalPower;
                    healing *= damageMultiplier;
                    totalHealing += healing;
                    break;
            }
        }

        if (totalDamage > 0)
        {
            target.TakeDamage(totalDamage, shooterUnit);
        }

        if (totalHealing > 0)
        {
            target.Heal(totalHealing, shooterUnit);
        }
    }
}
