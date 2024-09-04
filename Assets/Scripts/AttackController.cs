using System.Collections;
using UnityEngine;

/// <summary>
/// 発射物を管理し、発射を制御するクラス
/// </summary>
public class AttackController : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private bool isShootingEnabled = false; // 発射を行うかどうか
    [SerializeField] private bool followTarget = false; // ターゲットを追従するかどうか
    [SerializeField] private bool autoUpdateDirection = true; // ターゲットの方向を自動的に設定するかどうか
    [SerializeField] private bool enableTrail = false; // すべての発射物の軌跡を有効にするかどうか
    [SerializeField] private bool spiralMovementEnabled = false; // 螺旋状の動きを有効にするかどうか
    [SerializeField] private float spiralExpansionSpeed = 0.5f; // 螺旋が外側に広がる速度
    [SerializeField] private float delayMultiplier = 0.0f; // 発射間の遅延時間に掛けるランダムな範囲の最大値
    [SerializeField] private float shakeAmplitude = 0.0f; // 振幅的な揺れの強さ
    [SerializeField] private bool scaleOverTime = false; // 時間経過に応じてスケールする機能を有効にするか
    [SerializeField] private bool shootFourDirections = false; // 上下左右4方向に発射する機能を有効にするか
    [SerializeField] private bool shootFourDiagonalDirections = false; // 斜め4方向に発射する機能を有効にするか
    [SerializeField] private bool shootEightDirections = false; // 8方向に発射する機能を有効にするか
    [SerializeField] private bool useRandomPosition = false; // ランダムな位置から発射する機能を有効にするか
    [SerializeField] private int numberOfShots = 1; // 一度の攻撃で発射する回数

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab; // 発射するプレハブ
    [SerializeField] private Transform targetObject; // 追従するターゲットオブジェクト
    [SerializeField] private Vector2 direction; // 発射方向
    [SerializeField] private float speed = 5f; // 発射速度
    [SerializeField] private float sizeMultiplier = 1f; // サイズ倍率
    [SerializeField] private float delayBetweenShots = 0.3f; // 発射間の遅延時間
    [SerializeField] private float projectileLifetime = 1.0f; // プレハブの寿命時間（デフォルト1.0f）
    [SerializeField] private int attackCharThrough = 1; // 異なるタグのターゲットに対する貫通回数
    [SerializeField] private int attackObjectThrough = 1; // 障害物タグのターゲットに対する貫通回数

    private bool isShooting;
    private GameObject attackObjectGroup; // 発射物をまとめるグループオブジェクト
    private UnitController unitController; // UnitControllerの参照
    private Unit unit; // Unitの参照
    private bool wasShootingEnabled; // 前回の状態を保持する変数
    private Coroutine shootingCoroutine;
    private float lastAttackStartTime; // 最後に攻撃開始が行われた時間
    private const float attackStartCooldown = 0.1f; // 攻撃開始のクールダウン時間

    /// <summary>
    /// 初期化処理。発射物のグループオブジェクトを確認し、必要に応じて作成します。
    /// </summary>
    private void Start()
    {
        // 攻撃物グループの参照を取得
        attackObjectGroup = BattleManager.Instance.attackObjectGroup;

        // UnitControllerの参照を取得
        unitController = GetComponent<UnitController>();
        // Unitの参照を取得
        unit = GetComponent<Unit>();

        isShooting = false;
        wasShootingEnabled = false; // 初期化
        lastAttackStartTime = -attackStartCooldown; // 初期化
    }

    /// <summary>
    /// ターゲットオブジェクトを設定する
    /// </summary>
    /// <param name="target">新しいターゲットオブジェクト</param>
    public void SetTargetObject(Transform target)
    {
        targetObject = target;
    }

    /// <summary>
    /// 毎フレームの更新処理。発射やターゲットの方向を管理します。
    /// </summary>
    private void Update()
    {
        // 攻撃が有効になる条件をチェック
        if (targetObject != null && unitController != null)
        {
            // EnableAttackStanceがONの場合は、AttackStance中でないと攻撃しない
            if (unitController.enableAttackStance)
            {
                isShootingEnabled = unitController.InAttackStance;
            }
            else
            {
                // EnableAttackStanceがOFFの場合は、攻撃範囲内にいる時攻撃する
                isShootingEnabled = unitController.InAttackStance || Vector2.Distance(transform.position, targetObject.position) <= unitController.approachRange;
            }
        }
        else
        {
            isShootingEnabled = false;
        }

        // 発射の有効/無効をチェック
        if (isShootingEnabled != wasShootingEnabled) // 状態が変わったときだけ処理する
        {
            if (isShootingEnabled && shootingCoroutine == null && Time.time - lastAttackStartTime >= attackStartCooldown)
            {
                Debug.Log("攻撃開始");
                isShooting = true;
                shootingCoroutine = StartCoroutine(ShootProjectile());
                lastAttackStartTime = Time.time; // 最後に攻撃開始が行われた時間を更新
            }
            else if (!isShootingEnabled && shootingCoroutine != null)
            {
                Debug.Log("攻撃停止");
                isShooting = false;
                StopCoroutine(shootingCoroutine);
                shootingCoroutine = null;
            }

            wasShootingEnabled = isShootingEnabled; // 前回の状態を更新
        }

        // ターゲット追従の方向設定を更新
        if (autoUpdateDirection && targetObject != null)
        {
            Vector2 targetDirection = (targetObject.position - transform.position).normalized;
            direction = targetDirection;
        }
    }

    /// <summary>
    /// プレハブを指定の方向に飛ばし、指定の条件で消滅させるコルーチン。
    /// </summary>
    /// <returns>コルーチン</returns>
    private IEnumerator ShootProjectile()
    {
        while (isShooting)
        {
            float adjustedDelay = delayBetweenShots + Random.Range(-delayMultiplier / 2f, delayMultiplier / 2f);

            if (useRandomPosition)
            {
                StartCoroutine(ShootRandomPositions());
            }
            else if (shootEightDirections)
            {
                ShootInMultipleDirections(new Vector2[] {
                    Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                    new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
                    new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
                });
            }
            else if (shootFourDirections)
            {
                ShootInMultipleDirections(new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right });
            }
            else if (shootFourDiagonalDirections)
            {
                ShootInMultipleDirections(new Vector2[] {
                    new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
                    new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
                });
            }
            else
            {
                ShootProjectileInDirection(direction);
            }

            yield return new WaitForSeconds(adjustedDelay);
        }
    }

    /// <summary>
    /// 指定された回数、ランダムな位置から発射するコルーチン
    /// </summary>
    /// <returns>コルーチン</returns>
    private IEnumerator ShootRandomPositions()
    {
        for (int i = 0; i < numberOfShots; i++)
        {
            Vector2 randomPosition = GetRandomPositionWithinRange();
            ShootProjectileInDirectionFromPosition(randomPosition, direction);
            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 指定された範囲内のランダムな位置を取得する
    /// </summary>
    /// <returns>ランダムな位置</returns>
    private Vector2 GetRandomPositionWithinRange()
    {
        if (unitController == null) return transform.position;

        float range = unitController.approachRange; // 小文字のapproachRangeに変更
        Vector2 randomOffset = Random.insideUnitCircle * range;
        return (Vector2)transform.position + randomOffset;
    }

    /// <summary>
    /// 指定した複数の方向に向けて発射する
    /// </summary>
    /// <param name="directions">発射する方向の配列</param>
    private void ShootInMultipleDirections(Vector2[] directions)
    {
        foreach (var dir in directions)
        {
            ShootProjectileInDirection(dir);
        }
    }

    /// <summary>
    /// 単一の方向に向けて発射する
    /// </summary>
    /// <param name="dir">発射する方向</param>
    private void ShootProjectileInDirection(Vector2 dir)
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        if (projectile == null) return; // projectileがnullでないことを確認

        projectile.transform.SetParent(attackObjectGroup.transform);
        projectile.transform.localScale *= sizeMultiplier;

        float adjustedLifetime = spiralMovementEnabled ? projectileLifetime * 3.0f : projectileLifetime;

        ProjectileBehavior projectileBehavior = projectile.GetComponent<ProjectileBehavior>();
        if (projectileBehavior != null)
        {
            projectileBehavior.Initialize(dir, speed, adjustedLifetime, attackCharThrough, attackObjectThrough, gameObject.tag, followTarget ? targetObject : null, enableTrail, spiralMovementEnabled, spiralExpansionSpeed, shakeAmplitude, scaleOverTime);

            // Unit型のインスタンスをSetShooterUnitメソッドに渡す
            if (unit != null)
            {
                projectileBehavior.SetShooterUnit(unit);
            }
        }
    }

    /// <summary>
    /// ランダムな位置から単一の方向に向けて発射する
    /// </summary>
    /// <param name="position">発射する位置</param>
    /// <param name="dir">発射する方向</param>
    private void ShootProjectileInDirectionFromPosition(Vector2 position, Vector2 dir)
    {
        GameObject projectile = Instantiate(projectilePrefab, position, Quaternion.identity);
        if (projectile == null) return; // projectileがnullでないことを確認
        
        projectile.transform.SetParent(attackObjectGroup.transform);
        projectile.transform.localScale *= sizeMultiplier;

        float adjustedLifetime = spiralMovementEnabled ? projectileLifetime * 3.0f : projectileLifetime;

        ProjectileBehavior projectileBehavior = projectile.GetComponent<ProjectileBehavior>();
        projectileBehavior.Initialize(dir, speed, adjustedLifetime, attackCharThrough, attackObjectThrough, gameObject.tag, followTarget ? targetObject : null, enableTrail, spiralMovementEnabled, spiralExpansionSpeed, shakeAmplitude, scaleOverTime);
    }
}
