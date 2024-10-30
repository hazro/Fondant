using System.Collections;
using UnityEngine;

/// <summary>
/// 発射物を管理し、発射を制御するクラス
/// </summary>
public class AttackController : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private bool isShootingEnabled = false; // 発射を行うかどうか
    [SerializeField] private bool enableChargeAttack = false; // 貯め撃ちを行うかどうか
    [SerializeField] private bool powerAttack = false; // 強撃の有効/無効を設定
    [SerializeField] private bool enableContinuousAttack = false; //連続攻撃の有効/無効を設定
    [SerializeField] private bool moveBackAfterAttack = false; // 攻撃後に後ろに下がるかどうか
    [SerializeField] private float moveBackDistance = 0.3f; // 後ろに下がる距離
    [SerializeField] private bool targetLowHpFirst = false; // 低HPのターゲットを優先するかどうか
    [SerializeField] private bool changeTargetRandomly = false; // 攻撃ごとにターゲットをランダムに変更するかどうか
    [SerializeField] private bool followTarget = false; // ターゲットを追従するかどうか
    [SerializeField] private bool autoUpdateDirection = true; // ターゲットの方向を自動的に設定するかどうか
    [SerializeField] private bool directHit = false; // ターゲットを直撃するかどうか
    [SerializeField] private bool enableTrail = false; // すべての発射物の軌跡を有効にするかどうか
    [SerializeField] private bool spiralMovementEnabled = false; // 螺旋状の動きを有効にするかどうか
    [SerializeField] private float spiralExpansionSpeed = 0.5f; // 螺旋が外側に広がる速度
    [SerializeField] private float delayRandomRange = 0.0f; // 発射間の遅延時間に掛けるランダムな範囲の最大値
    [SerializeField] private float shakeAmplitude = 0.0f; // 振幅的な揺れの強さ
    [SerializeField] private bool scaleOverTime = false; // 時間経過に応じてスケールする機能を有効にするか
    [SerializeField] private bool shootThreeDirections = false; // ターゲット前方3方向に同時に発射するかどうか
    [SerializeField] private bool shootFourDirections = false; // 上下左右4方向に発射する機能を有効にするか
    [SerializeField] private bool shootFourDiagonalDirections = false; // 斜め4方向に発射する機能を有効にするか
    [SerializeField] private bool shootEightDirections = false; // 8方向に発射する機能を有効にするか
    [SerializeField] private bool useRandomPosition = false; // ランダムな位置から発射する機能を有効にするか
    [SerializeField] private int numberOfShots = 1; // 一度の攻撃で発射する回数


    public GameObject weaponPrefab; // 武器のPrefab

    [Header("Parameter Multiplication")]
    [SerializeField] private float attackMultiplier = 1.0f; // 攻撃力の乗算値
    [SerializeField] private float delayMultiplier = 1.0f; // 発射間の遅延時間の乗算値
    [SerializeField] private float aefSpeedMultiplier = 1.0f; // 発射物速度の乗算値
    [SerializeField] private float maxDistanceMultiplier = 1.0f; // 最大距離の乗算値
    [SerializeField] private float projectileLifetime = 1.0f; // プレハブの寿命時間への乗算値（デフォルト1.0f）
    [SerializeField] private float weaponScaleMultiplier = 1.0f; // 武器のスケールの乗算値
    [SerializeField] private float knockbackMultiplier = 1.0f; // ノックバックの乗算値
    [SerializeField] private int attackCharThroughAdd = 0; // 異なるタグのターゲットに対する貫通回数を加算
    [SerializeField] private int attackObjectThroughAdd = 0; // 障害物タグのターゲットに対する貫通回数を加算

    [Header("Projectile Settings")]
    public GameObject projectilePrefab; // 発射するプレハブ
    [SerializeField] private Transform targetObject; // 追従するターゲットオブジェクト
    [SerializeField] private Vector2 direction; // 発射方向
    [SerializeField] private float speed = 2f; // 発射速度
    [SerializeField] private float sizeMultiplier = 1f; // サイズ倍率
    [SerializeField] private float delayBetweenShots = 2.0f; // 発射間の遅延時間
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
    private Vector2 firingPosition; // 発射位置

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
        // ターゲットが低HPのものを優先する場合はUnitControllerのフラグを変更する
        if (targetLowHpFirst && unitController != null)
        {
            unitController.targetLowHpFirst = true;
        }else if (!targetLowHpFirst && unitController != null)
        {
            unitController.targetLowHpFirst = false;
        }
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
                int weapomAmplitude = 0; // 武器のふり幅の範囲
                if(weaponPrefab != null)
                {
                    // weaponPrefab.nameの上3桁までの数値の違い(111,112,113,114,115,116,117)によって武器のふり幅の範囲をそれぞれ設定
                    if (weaponPrefab.name.Substring(0, 3) == "111") weapomAmplitude = 60; //大剣
                    else if (weaponPrefab.name.Substring(0, 3) == "112") weapomAmplitude = 45; //ダガー、槍
                    else if (weaponPrefab.name.Substring(0, 3) == "113") weapomAmplitude = 60; //斧
                    else if (weaponPrefab.name.Substring(0, 3) == "114") weapomAmplitude = 5; //長杖
                    else if (weaponPrefab.name.Substring(0, 3) == "115") weapomAmplitude = 15; //短杖、本
                    else if (weaponPrefab.name.Substring(0, 3) == "116") weapomAmplitude = 0; //弓
                    else if (weaponPrefab.name.Substring(0, 3) == "117") weapomAmplitude = 15; //銃、特殊武器
                }
                
                shootingCoroutine = StartCoroutine(ShootProjectile(weapomAmplitude));
                lastAttackStartTime = Time.time; // 最後に攻撃開始が行われた時間を更新
            }
            else if (!isShootingEnabled && shootingCoroutine != null)
            {
                Debug.Log("攻撃停止");
                if (weaponPrefab != null)
                {
                    weaponPrefab.transform.rotation = Quaternion.Euler(0, 0, 0); // 武器の角度を元に戻す
                    weaponPrefab.SetActive(false); // weaponPrefabを非アクティブにする
                }
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

    // コールーチンで-範囲～範囲の範囲で武器を振り(RotationZ)、終わったら非アクティブにして元の角度に戻す
    private IEnumerator ShakeWeapon(float amplitude)
    {
        float moveWeapon = 0.1f; // 武器を前に突き出す距離
        // もしweaponPrefaのScaleXが0より低い場合は、amplitudeを反転させる
        if (weaponPrefab.transform.localScale.x < 0)
        {
            amplitude *= -1;
            moveWeapon *= -1;
        }

        weaponPrefab.SetActive(true); // weaponPrefabをアクティブにする

        float time = 0;
        float duration = 0.2f;

        while (time < duration)
        {
            time += Time.deltaTime;

            // Mathf.Lerpで amplitude から -amplitude に0.2秒間で変化させる
            float zRotation = Mathf.Lerp(amplitude, -amplitude, Mathf.PingPong(time * 2 / duration, 1));
            // Mathf.Lerpで moveWeapon から -moveWeapon に0.2秒間で変化させる
            float move = Mathf.Lerp(moveWeapon, -moveWeapon, Mathf.PingPong(time * 2 / duration, 1));

            // Z軸回転を設定
            weaponPrefab.transform.rotation = Quaternion.Euler(0, 0, zRotation);
            // 武器を前に突き出す
            weaponPrefab.transform.position = transform.position + new Vector3(move, 0, 0);

            yield return null;
        }

        weaponPrefab.SetActive(false); // weaponPrefabを非アクティブにする
        // 回転をリセット
        weaponPrefab.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    /// <summary>
    /// プレハブを指定の方向に飛ばし、指定の条件で消滅させるコルーチン。
    /// </summary>
    /// <returns>コルーチン</returns>
    private IEnumerator ShootProjectile(int weapomAmplitude)
    {
        float maxDelay = 10.0f;  // 最大ディレイ
        float minDelay = 0.1f;   // 最小ディレイ
        float offset = 3.0f;     // オフセット値
        while (isShooting)
        {
            // フラグごとの乗算値を設定
            SetMultiplier();

            // コールーチンで-範囲～範囲の範囲で武器を振る(RotationZ)
            if (weaponPrefab != null) StartCoroutine(ShakeWeapon(weapomAmplitude)); // 武器を振るコルーチン
            // ディレイ時間を乗算
            delayBetweenShots = Mathf.Max(delayBetweenShots * delayMultiplier * (float)numberOfShots, 0);
            // 数値が増えるほどディレイ時間が短くなるようにし最大値と最小値の間に収まるように調整
            float delayShots = Mathf.Max(maxDelay / (delayBetweenShots + offset), minDelay); //(例)delayBetweenShotsが1より5の方がdelayが短くなる
            // ディレイ時間にランダムな範囲を追加
            float adjustedDelay = delayShots + Random.Range(-delayRandomRange / 2f, delayRandomRange / 2f);

            /////////////////////////////////////////////////////////////////////////
            //// 1, 発射開始位置を決める /////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////
            
            firingPosition = transform.position; // 発射位置を自分の位置に設定

            // ランダムな位置から発射する場合
            if (useRandomPosition)
            {
                firingPosition = GetRandomPositionWithinRange();
            }

            // ターゲットを直撃する場合
            if (directHit)
            {
                firingPosition = GetDirectHitPosition();
            }

            /////////////////////////////////////////////////////////////////////////
            //// 2, 発射物の動きを決める /////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////

            /////////////////////////////////////////////////////////////////////////
            //// 3, 攻撃方向を決める(どれか一つ) //////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////
            
            // ターゲット前方3方向に同時に発射する場合
            if (shootThreeDirections)
            {
                // ターゲットへの方向を０度として、左右に30度ずつずらした方向に発射する
                Vector2 targetDirection = ((Vector2)targetObject.position - firingPosition).normalized;
                Vector2 leftDirection = Quaternion.Euler(0, 0, 30) * targetDirection;
                Vector2 rightDirection = Quaternion.Euler(0, 0, -30) * targetDirection;
                ShootInMultipleDirections(new Vector2[] { targetDirection, leftDirection, rightDirection });
            }
            // 8方向に発射する場合
            else if (shootEightDirections)
            {
                ShootInMultipleDirections(new Vector2[] {
                    Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                    new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
                    new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
                });
            }
            // 上下左右4方向に発射する場合
            else if (shootFourDirections)
            {
                ShootInMultipleDirections(new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right });
            }
            // 斜め4方向に発射する場合
            else if (shootFourDiagonalDirections)
            {
                ShootInMultipleDirections(new Vector2[] {
                    new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
                    new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
                });
            }
            // それ以外の場合は、指定された方向に発射する
            else
            {
                ShootProjectileInDirection(direction);
            }

            /////////////////////////////////////////////////////////////////////////
            /// 4, 攻撃後の処理を行う ////////////////////////////////////////////////
            /// /////////////////////////////////////////////////////////////////////
            
            // 攻撃後にターゲットをランダムに変更する
            if (changeTargetRandomly && unitController != null)
            {
                unitController.SetClosestTarget(true);
            }

            // 攻撃後に後ろに下がる
            if (moveBackAfterAttack && unitController != null)
            {
                unitController.MoveBackDistance = moveBackDistance;
                unitController.MoveBackFlag = true;
            }

            // ディレイ時間を待つ
            yield return new WaitForSeconds(adjustedDelay);
        }
    }

    /// <summary>
    /// ターゲットオブジェクトに直撃する位置を取得します。
    /// </summary>
    /// <returns></returns>
    private Vector2 GetDirectHitPosition()
    {
        if (targetObject != null)
        {
            Vector2 targetPosition = targetObject.position;
            Vector2 unitPosition = transform.position;
            Vector2 directionToUnit = (unitPosition - targetPosition).normalized;
            // ターゲットの位置に向かって1f手前の位置を返す
            return targetPosition + directionToUnit * 1f;
        }
        return transform.position;
    }

    /// <summary>
    /// 指定された範囲内のランダムな位置を取得する
    /// </summary>
    /// <returns>ランダムな位置</returns>
    private Vector2 GetRandomPositionWithinRange()
    {
        if (unitController == null) return transform.position;

        float range = unitController.approachRange;
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
        //発射位置(transformX)に0.1fオフセットをかける weaponPrefab.transform.localScale.x < 0の場合は-0.1fオフセットをかける
        float offset = 0;
        if (weaponPrefab != null)
        {
            // 武器の向きによってオフセットを変更
            offset = weaponPrefab.transform.localScale.x < 0 ? -0.1f : 0.1f;
        }
        GameObject projectile = Instantiate(projectilePrefab, (firingPosition + new Vector2(offset,0)), Quaternion.identity);
        if (projectile == null) return; // projectileがnullでないことを確認

        projectile.transform.SetParent(attackObjectGroup.transform);
        // サイズを変更
        projectile.transform.localScale *= sizeMultiplier;

        ProjectileBehavior projectileBehavior = projectile.GetComponent<ProjectileBehavior>();
        if (projectileBehavior != null)
        {
            // ProjectileBehaviorの初期化
            ProjectileInitialize(projectileBehavior,dir);

            // Unit型のインスタンスをSetShooterUnitメソッドに渡す
            if (unit != null)
            {
                projectileBehavior.SetShooterUnit(unit);
            }
        }
    }

    //フラグごとの乗算値を設定するメソッド
    public void SetMultiplier()
    {
        // 乗算値を初期化する
        attackMultiplier = 1.0f;
        delayMultiplier = 1.0f;
        aefSpeedMultiplier = 1.0f;
        maxDistanceMultiplier = 1.0f;
        weaponScaleMultiplier = 1.0f;
        knockbackMultiplier = 1.0f;
        attackCharThrough = unit.attackUnitThrough;
        attackObjectThrough = unit.attackObjectThrough;

        if(powerAttack) // 強撃が有効な場合は、攻撃力を2倍にする
        {
            aefSpeedMultiplier *= 2.0f;
            maxDistanceMultiplier *= 2.0f;
            attackCharThrough = 99;
            attackObjectThrough = 99;
            delayMultiplier *= 0.33f;
        }
        if(enableContinuousAttack) // 連続攻撃が有効な場合は、攻撃力を0.5倍、ディレイを3倍にする
        {
            attackMultiplier *= 0.5f;
            delayMultiplier *= 3.0f;
        }
        if(enableChargeAttack) // 貯め撃ちが有効な場合は、攻撃力を1.5倍、ディレイを2倍、発射物速度を1.5倍にする
        {
            attackMultiplier *= 1.5f;
            delayMultiplier *= 2.0f;
            aefSpeedMultiplier *= 1.5f;
        }
        if (moveBackAfterAttack) // 攻撃後に後ろに下がる場合は、攻撃力を1.25倍にする
        {
            attackMultiplier *= 1.25f;
        }
        if(changeTargetRandomly && directHit) // ターゲットをランダムに変更し直撃する場合は、攻撃力を0.33倍にする
        {
            attackMultiplier *= 0.33f;
        }
        if(directHit) // ターゲットを直撃する場合は、攻撃力を0.33倍にする
        {
            attackMultiplier *= 0.33f;
        }
        if(enableTrail) // 軌跡を有効にする場合は、攻撃力を0.5倍にする
        {
            attackMultiplier *= 0.5f;
        }
        if(followTarget) // ターゲットを追従する場合は、攻撃力を0.5倍にする
        {
            attackMultiplier *= 0.5f;
        }
        if(scaleOverTime)
        {
            attackMultiplier *= 0.75f;
        }
        if(shootThreeDirections)
        {
            attackMultiplier *= 0.5f;
        }
        if(shootFourDirections)
        {
            attackMultiplier *= 0.5f;
        }
        if(shootFourDiagonalDirections)
        {
            attackMultiplier *= 0.5f;
        }
        if(shootEightDirections)
        {
            attackMultiplier *= 0.33f;
        }
    }

    // ProjectileBehaviorの初期化処理
    private void ProjectileInitialize(ProjectileBehavior projectileBehavior, Vector2 dir)
    {
        // 螺旋運動が有効な場合は、経過時間の寿命を3倍にする
        float adjustedLifetime = spiralMovementEnabled ? projectileLifetime * 3.0f : projectileLifetime;

        // ProjectileBehaviorの初期化
        projectileBehavior.Initialize(
            dir,
            // 速度に乗算をかける
            speed*aefSpeedMultiplier,
            adjustedLifetime,
            // 貫通回数に加算をかける
            attackCharThrough + attackCharThroughAdd,
            // 障害物貫通回数に加算をかける
            attackObjectThrough + attackObjectThroughAdd,
            gameObject.tag,
            attackMultiplier,
            maxDistanceMultiplier,
            weaponScaleMultiplier,
            knockbackMultiplier,
            // 以下、オプションパラメータ
            followTarget ? targetObject : null,
            enableTrail,
            spiralMovementEnabled,
            spiralExpansionSpeed,
            shakeAmplitude,
            scaleOverTime
            );
    }
}

