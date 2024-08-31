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
    [SerializeField] private bool autoUpdateDirection = false; // ターゲットの方向を自動的に設定するかどうか
    [SerializeField] private bool enableTrail = false; // すべての発射物の軌跡を有効にするかどうか
    [SerializeField] private bool spiralMovementEnabled = false; // 螺旋状の動きを有効にするかどうか
    [SerializeField] private float spiralExpansionSpeed = 0.5f; // 螺旋が外側に広がる速度
    [SerializeField] private float delayMultiplier = 0.0f; // 発射間の遅延時間に掛けるランダムな範囲の最大値
    [SerializeField] private float shakeAmplitude = 0.0f; // 振幅的な揺れの強さ

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

    /// <summary>
    /// 初期化処理。発射物のグループオブジェクトを確認し、必要に応じて作成します。
    /// </summary>
    private void Start()
    {
        // AttackObjectGroup が存在しない場合は新規作成
        attackObjectGroup = GameObject.Find("AttackObjectGroup");
        if (attackObjectGroup == null)
        {
            attackObjectGroup = new GameObject("AttackObjectGroup");
        }

        isShooting = false;
    }

    /// <summary>
    /// 毎フレームの更新処理。発射やターゲットの方向を管理します。
    /// </summary>
    private void Update()
    {
        // 発射の有効/無効をチェック
        if (isShootingEnabled && !isShooting)
        {
            isShooting = true;
            StartCoroutine(ShootProjectile());
        }
        else if (!isShootingEnabled && isShooting)
        {
            isShooting = false;
            StopCoroutine(ShootProjectile());
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
            // 遅延時間をランダムに調整
            float adjustedDelay = delayBetweenShots + Random.Range(-delayMultiplier / 2f, delayMultiplier / 2f);

            GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
            projectile.transform.SetParent(attackObjectGroup.transform); // 発射物をグループの子オブジェクトに設定
            projectile.transform.localScale *= sizeMultiplier;

            // 螺旋状の動きが有効な場合、寿命を3倍にする
            float adjustedLifetime = spiralMovementEnabled ? projectileLifetime * 3.0f : projectileLifetime;

            ProjectileBehavior projectileBehavior = projectile.GetComponent<ProjectileBehavior>();

            // 発射物の設定を初期化
            projectileBehavior.Initialize(direction, speed, adjustedLifetime, attackCharThrough, attackObjectThrough, gameObject.tag, followTarget ? targetObject : null, enableTrail, spiralMovementEnabled, spiralExpansionSpeed, shakeAmplitude);

            yield return new WaitForSeconds(adjustedDelay);
        }
    }
}
