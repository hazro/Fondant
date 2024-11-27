using System.Collections;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using static ItemData;
using UnityEngine.UI;

/// <summary>
/// 発射物を管理し、発射を制御するクラス
/// </summary>
public class AttackController : MonoBehaviour
{
    [Header("Options")]
    [SerializeField] private bool isDebugInfo = false; // デバッグ情報を表示するかどうか
    [SerializeField] private bool isShootingEnabled = false; // 発射を行うかどうか
    [SerializeField] private bool enableEqpSkill = true; // 装備によるskillの発動を有効にするかどうか
    [SerializeField] private bool autoUpdateDirection = true; // ターゲットの方向を自動的に設定するかどうか
    [SerializeField] private bool damageZero = false; // ダメージを0にする

    [Header("direction Skill (select one)")]
    [SerializeField] private bool shoot2Directions = false; // ターゲット前方2方向に同時に発射
    [SerializeField] private bool shoot3Directions = false; // ターゲット前方3方向に同時に発射
    [SerializeField] private bool shoot4Directions = false; // 上下左右4方向に発射
    [SerializeField] private bool shoot4DiagonalDirections = false; // 斜め4方向に発射
    [SerializeField] private bool shoot8Directions = false; // 8方向に発射

    [Header("Attak Skill (select eny)")]
    [SerializeField] private bool targetYourGroup = false; // 自分のグループをターゲット
    [SerializeField] private bool randomPosition = false; // ランダムな位置から発射
    [SerializeField] [Tooltip("ターゲットを追従")] private bool followTarget = false; // ターゲットを追従
    [SerializeField] private bool laser = false; // 発射物に軌跡を付ける
    [SerializeField] private bool spiral = false; // 螺旋状
    [SerializeField] private float spiralExpansionSpeed = 0.5f; // 螺旋が外側に広がる速度
    [SerializeField] private bool delayRandom = false; // 発射間の遅延時間にランダムな範囲を追加 
    [SerializeField] private bool wave = false; // 武器の振幅の範囲を設定
    private float shakeAmplitude = 0.3f; // 振幅的な揺れの強さ
    [SerializeField] private bool backStep = false; // 攻撃後に後ろに下がる
    private float moveBackDistance = 0.3f; // 後ろに下がる距離
    [SerializeField] private bool targetLowHp = false; // 低HPのターゲットを優先
    [SerializeField] private bool changeTarget = false; // 攻撃ごとにターゲットをランダムに変更
    [SerializeField] private bool randomDirectHit = false; // 直撃するターゲットをランダムに変更
    [SerializeField] private bool directHit = false; // ターゲットを直撃
    [SerializeField] private bool scaleUpBlow = false; // 時間経過に応じてスケール
    [SerializeField] private bool chainAttack = false; // 貫通したら近い別の対手をターゲットにする
    [SerializeField] private bool spread = false; // 発射物がぶつかったら2つに拡散する

    [Header("Projectile Settings")]
    public GameObject weaponPrefab; // 武器のPrefab
    public GameObject projectilePrefab; // 発射するプレハブ
    [SerializeField] private Transform targetObject; // 追従するターゲットオブジェクト
    [SerializeField] private Vector2 direction; // 発射方向

    [SerializeField] private float lastAttackStopTime = 0; // 最後に攻撃を停止した時刻
    [SerializeField] private float adjustedDelay; // 攻撃再開までのディレイ時間
    private Coroutine shootingCoroutine; // 攻撃を繰り返すコルーチン
    private bool isShooting; // 攻撃中かどうかのフラグ
    private GameObject attackObjectGroup; // 発射物をまとめるグループオブジェクト
    private GameManager gameManager; // GameManagerの参照
    private IventryUI iventryUI; // IventryUIの参照
    private UnitController unitController; // UnitControllerの参照
    private Unit unit; // Unitの参照
    [HideInInspector] public bool wasShootingEnabled; // 前回の状態を保持する変数
    private float lastAttackStartTime; // 最後に攻撃開始が行われた時間
    private const float attackStartCooldown = 0.1f; // 攻撃開始のクールダウン時間
    private Vector2 firingPosition; // 発射位置
    private BattleManager battleManager; // BattleManagerの参照

    // Delay待ちの視覚表示関連
    private float attackElapsedTime; // 経過時間を記録


    /// <summary>
    /// 初期化処理。発射物のグループオブジェクトを確認し、必要に応じて作成します。
    /// </summary>
    private void Start()
    {
        // BattleManagerの参照を取得
        BattleManager battleManager = BattleManager.Instance;
        if (battleManager != null)
        {
            // 発射物をまとめるグループオブジェクトを取得
            attackObjectGroup = battleManager.attackObjectGroup;
        }

        // GameManagerの参照を取得
        gameManager = GameManager.Instance;
        // IventryUIの参照を取得
        if (gameManager != null){
            iventryUI = gameManager.GetComponent<IventryUI>();
        }
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
        if (unit != null)
        {
            if (unit.ironWall) {
                isShootingEnabled = false;
                return; // 鉄壁の場合は攻撃しない
            }
        }
        if (unitController != null)
        {
            // 武器によってメンバーをターゲット指定していなければ、自分のグループをターゲットにするかどうかを設定
            if (!unitController.targetSameTagWpn)
            {
                unitController.targetSameTag = targetYourGroup;
            }
            // ターゲットが低HPのものを優先する場合はUnitControllerのフラグを変更する
            unitController.targetLowHpFirst = targetLowHp;
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
                // ターゲットが攻撃範囲に入ったらtrueにする (通常攻撃)
                isShootingEnabled = Vector2.Distance(transform.position, targetObject.position) <= unitController.approachRange;
            }
        }
        else
        {
            isShootingEnabled = false;
        }

        // 発射の有効/無効をチェック（変更時のみコルーチンを開始・停止）
        if (isShootingEnabled != wasShootingEnabled)
        {
            // 攻撃が有効に切り替わった場合、前回の停止時間からクールダウン時間を経過しているかを確認
            if (isShootingEnabled && shootingCoroutine == null && Time.time - lastAttackStopTime >= adjustedDelay)
            {
                StartShooting();
            }
            // 攻撃が無効に切り替わった場合、攻撃中であれば停止
            else if (!isShootingEnabled && shootingCoroutine != null)
            {
                StopShooting();
            }
            // クールダウン時間を経過していない場合は、再度攻撃を開始しないでwasShootingEnabledも更新せずに終了
            else
            {
                return;
            }
            /*
            else if (backStep && Vector2.Distance(transform.position, targetObject.position) <= unitController.approachRange)
            {
                // backStepで後退後、再び攻撃範囲内に戻ったら攻撃を再開
                StartShooting();
            }
            */

            wasShootingEnabled = isShootingEnabled; // 前回の状態を更新(状態が変化したときだけコールーチンの切り替えを行うため)
        }

    }

    /// <summary>
    /// 攻撃を開始するメソッド
    /// </summary>
    private void StartShooting()
    {
        if(isDebugInfo) Debug.Log("攻撃開始: " + gameObject.name);
        isShooting = true;
        int weaponAmplitude = 0; // 武器のふり幅の範囲

        if(weaponPrefab != null)
        {
            // weaponPrefab.nameの上3桁までの数値の違い(111,112,113,114,115,116,117)によって武器のふり幅の範囲をそれぞれ設定
            if (weaponPrefab.name.Substring(0, 3) == "111") weaponAmplitude = 60; //大剣
            else if (weaponPrefab.name.Substring(0, 3) == "112") weaponAmplitude = 45; //ダガー、槍
            else if (weaponPrefab.name.Substring(0, 3) == "113") weaponAmplitude = 60; //斧
            else if (weaponPrefab.name.Substring(0, 3) == "114") weaponAmplitude = 5; //長杖
            else if (weaponPrefab.name.Substring(0, 3) == "115") weaponAmplitude = 15; //短杖、本
            else if (weaponPrefab.name.Substring(0, 3) == "116") weaponAmplitude = 0; //弓
            else if (weaponPrefab.name.Substring(0, 3) == "117") weaponAmplitude = 15; //銃、特殊武器
        }

        shootingCoroutine = StartCoroutine(ShootProjectile(weaponAmplitude));
        lastAttackStartTime = Time.time; // 最後に攻撃開始が行われた時間を更新
    }

    /// <summary>
    /// 攻撃を停止するメソッド
    /// </summary>
    private void StopShooting()
    {
        if(isDebugInfo) Debug.Log("攻撃停止: " + gameObject.name);
        if (weaponPrefab != null)
        {
            weaponPrefab.transform.rotation = Quaternion.Euler(0, 0, 0); // 武器の角度を元に戻す
            weaponPrefab.SetActive(false); // weaponPrefabを非アクティブにする
        }
        if (gameObject.tag == "Ally")
        {
            // クールダウン表示をリセット
            Image attackCooldownImage = gameManager.IventryUI.wpnDelayImage[unit.ID -1];
            attackCooldownImage.fillAmount = 0f; // リセット
        }

        isShooting = false;
        StopCoroutine(shootingCoroutine);
        shootingCoroutine = null;
        lastAttackStopTime = Time.time; // 攻撃停止時刻を更新
        wasShootingEnabled = true; // 攻撃再開をトリガーするためにフラグを更新
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
        float shotInterval = 0.03f * unit.attackSpeed; // 発射間隔

        while (isShooting)
        {
            // 装備によるskillの発動が有効の場合は、装備しているルーンのスキルをすべて取得し有効無効を判定
            if (enableEqpSkill)
            {
                // UnitのmainSocketとsubSocketに装備しているルーンのIDをすべて取得し配列に格納
                List<int> eqpRuneIDs = new List<int>();
                eqpRuneIDs.Add(unit.mainSocket);
                for (int i = 0; i < unit.subSocket.Length; i++)
                {
                    eqpRuneIDs.Add(unit.subSocket[i]);
                }
                // IDに該当するルーンのnameを取得
                List<string> eqpRuneNames = new List<string>();
                for (int i = 0; i < eqpRuneIDs.Count; i++)
                {
                    eqpRuneNames.Add(gameManager.itemData.runeList.Find(x => x.ID == eqpRuneIDs[i]).name);
                }
                // skill関連のboolメソッドのメソッド名がeqpRuneNamesに含まれるかどうかで有効無効を切り替える
                shoot2Directions = eqpRuneNames.Contains(nameof(shoot2Directions));
                shoot3Directions = eqpRuneNames.Contains(nameof(shoot3Directions));
                shoot4Directions = eqpRuneNames.Contains(nameof(shoot4Directions));
                shoot4DiagonalDirections = eqpRuneNames.Contains(nameof(shoot4DiagonalDirections));
                shoot8Directions = eqpRuneNames.Contains(nameof(shoot8Directions));
                targetYourGroup = eqpRuneNames.Contains(nameof(targetYourGroup));
                randomPosition = eqpRuneNames.Contains(nameof(randomPosition));
                followTarget = eqpRuneNames.Contains(nameof(followTarget));
                laser = eqpRuneNames.Contains(nameof(laser));
                spiral = eqpRuneNames.Contains(nameof(spiral));
                delayRandom = eqpRuneNames.Contains(nameof(delayRandom));
                wave = eqpRuneNames.Contains(nameof(wave));
                backStep = eqpRuneNames.Contains(nameof(backStep));
                targetLowHp = eqpRuneNames.Contains(nameof(targetLowHp));
                changeTarget = eqpRuneNames.Contains(nameof(changeTarget));
                randomDirectHit = eqpRuneNames.Contains(nameof(randomDirectHit));
                directHit = eqpRuneNames.Contains(nameof(directHit));
                scaleUpBlow = eqpRuneNames.Contains(nameof(scaleUpBlow));
                chainAttack = eqpRuneNames.Contains(nameof(chainAttack));
                spread = eqpRuneNames.Contains(nameof(spread));
                // 以下同様に設定予定
            }

            // コールーチンで-範囲～範囲の範囲で武器を振る(RotationZ)
            if (weaponPrefab != null) StartCoroutine(ShakeWeapon(weapomAmplitude)); // 武器を振るコルーチン
            // ディレイ時間を乗算
            float delayBetweenShots = Mathf.Max(unit.attackDelay, 0);
            // 数値が増えるほどディレイ時間が短くなるようにし最大値と最小値の間に収まるように調整
            float delayShots = Mathf.Max(maxDelay / (delayBetweenShots + offset), minDelay); //(例)delayBetweenShotsが1より5の方がdelayが短くなる
            // ディレイ時間にランダムな範囲を追加
            float delayRandomRange = delayRandom ? 3.0f : 0.0f; // delayRandomの場合は3.0f、それ以外は0.0f
            adjustedDelay = delayShots + Random.Range(-delayRandomRange / 2f, delayRandomRange / 2f); // ディレイを設定攻撃再開までのディレイ時間にも使用

            // プレイヤーの場合はクールダウン表示をリセット
            if (gameObject.tag == "Ally")
            {
                Image attackCooldownImage = gameManager.IventryUI.wpnDelayImage[unit.ID -1];

                // クールダウン表示用の初期化
                if (attackCooldownImage != null)
                {
                    attackElapsedTime = 0f;
                    attackCooldownImage.fillAmount = 0f; // リセット
                }

                // 待機時間の経過をFillAmountで表示
                while (attackElapsedTime < adjustedDelay)
                {
                    attackElapsedTime += Time.deltaTime;

                    // FillAmountを更新
                    if (attackCooldownImage != null)
                    {
                        attackCooldownImage.fillAmount = Mathf.Clamp01(attackElapsedTime / adjustedDelay);
                    }

                    yield return null; // 次のフレームまで待機
                }
            }
            else
            {
                yield return new WaitForSeconds(adjustedDelay); // ディレイ時間を待機
            }

            /////////////////////////////////////////////////////////////////////////
            //// 1, 発射開始位置を決める /////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////
            
            firingPosition = transform.position; // 発射位置を自分の位置に設定

            // ランダムな位置から発射する場合
            if (randomPosition)
            {
                firingPosition = GetRandomPositionWithinRange();
            }

            // ターゲットを直撃する場合
            if (directHit || randomDirectHit)
            {
                firingPosition = GetDirectHitPosition();
            }

            /////////////////////////////////////////////////////////////////////////
            //// 2, 発射物の動きを決める /////////////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////
            
            UpdateDirection(firingPosition); // 攻撃が進む方向を更新

            /////////////////////////////////////////////////////////////////////////
            //// 3, 攻撃方向を決める(どれか一つ) //////////////////////////////////////
            /////////////////////////////////////////////////////////////////////////
            
            // ターゲット前方2方向に同時に発射する場合
            if (shoot2Directions)
            {
                // ターゲットへの方向を０度として、左右に15度ずつずらした方向に発射する
                Vector2 targetDirection = ((Vector2)targetObject.position - firingPosition).normalized;
                Vector2 leftDirection = Quaternion.Euler(0, 0, 15) * targetDirection;
                Vector2 rightDirection = Quaternion.Euler(0, 0, -15) * targetDirection;
                yield return StartCoroutine(ShootWithDelay(new Vector2[] { leftDirection, rightDirection }, shotInterval));
            }
            
            // ターゲット前方3方向に同時に発射する場合
            else if (shoot3Directions)
            {
                // ターゲットへの方向を０度として、左右に30度ずつずらした方向に発射する
                Vector2 targetDirection = ((Vector2)targetObject.position - firingPosition).normalized;
                Vector2 leftDirection = Quaternion.Euler(0, 0, 30) * targetDirection;
                Vector2 rightDirection = Quaternion.Euler(0, 0, -30) * targetDirection;
                yield return StartCoroutine(ShootWithDelay(new Vector2[] { targetDirection, leftDirection, rightDirection }, shotInterval));
            }
            // 8方向に発射する場合
            else if (shoot8Directions)
            {
                yield return StartCoroutine(ShootWithDelay(new Vector2[] {
                    Vector2.up, Vector2.down, Vector2.left, Vector2.right,
                    new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
                    new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
                }, shotInterval));
            }
            // 上下左右4方向に発射する場合
            else if (shoot4Directions)
            {
                yield return StartCoroutine(ShootWithDelay(new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right }, shotInterval));
            }
            // 斜め4方向に発射する場合
            else if (shoot4DiagonalDirections)
            {
                yield return StartCoroutine(ShootWithDelay(new Vector2[] {
                    new Vector2(1, 1).normalized, new Vector2(1, -1).normalized,
                    new Vector2(-1, 1).normalized, new Vector2(-1, -1).normalized
                }, shotInterval));
            }
            // それ以外の場合は、指定された方向に発射する
            else
            {
                ShootProjectileInDirection(direction);
            }

            /////////////////////////////////////////////////////////////////////////
            /// 4, 攻撃後の処理を行う ////////////////////////////////////////////////
            /// /////////////////////////////////////////////////////////////////////
            if (unitController != null)
            {
                // 攻撃後に後ろに下がる
                unitController.MoveBackFlag = backStep;
                unitController.MoveBackDistance = moveBackDistance;
            }
            
            // 攻撃後にターゲットをランダムに変更する
            if ((changeTarget || randomDirectHit) && unitController != null)
            {
                unitController.isRandomTarget = true;
                unitController.SetClosestTarget();
            }
            else
            {
                unitController.isRandomTarget = false;
            }

            // ディレイ時間を待つ
            yield return new WaitForSeconds(shotInterval); // ショット間隔を待機
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
            // ターゲットの位置に向かって0.5f手前の位置を返す
            return targetPosition + directionToUnit * 0.5f;
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
    /// interval秒ごとに発射する
    /// </summary>
    /// <param name="directions">発射する方向の配列</param>
    private IEnumerator ShootWithDelay(Vector2[] directions, float interval)
    {
        foreach (var dir in directions)
        {
            ShootProjectileInDirection(dir);
            yield return new WaitForSeconds(interval);
        }
    }

    /// <summary>
    /// 単一の方向に向けて発射する
    /// </summary>
    /// <param name="dir">発射する方向</param>
    public void ShootProjectileInDirection(Vector2 dir, Vector2 changePosition = default , bool throughEnforce = false, float damageMult = 1.0f, GameObject skipObject = null)
    {
        if (changePosition != default)
        {
            firingPosition = changePosition;
        }
        //発射位置(transformX)に0.1fオフセットをかける weaponPrefab.transform.localScale.x < 0の場合は-0.1fオフセットをかける
        float offset = 0;
        if (weaponPrefab != null)
        {
            // 武器の向きによってオフセットを変更
            offset = weaponPrefab.transform.localScale.x < 0 ? -0.1f : 0.1f;
        }
        GameObject projectile = Instantiate(projectilePrefab, (firingPosition + new Vector2(offset,0)), Quaternion.identity);
        if (projectile == null) return; // projectileがnullでないことを確認

        if (attackObjectGroup == null) attackObjectGroup = BattleManager.Instance.attackObjectGroup;
        
        projectile.transform.SetParent(attackObjectGroup.transform);
        // サイズを変更
        projectile.transform.localScale *= unit.attackSize;

        ProjectileBehavior projectileBehavior = projectile.GetComponent<ProjectileBehavior>();
        if (projectileBehavior != null)
        {
            // ProjectileBehaviorの初期化
            ProjectileInitialize(projectileBehavior,dir, throughEnforce, damageMult, skipObject);

            // Unit型のインスタンスをSetShooterUnitメソッドに渡す
            if (unit != null)
            {
                projectileBehavior.SetShooterUnit(unit);
            }
        }
    }

    // ProjectileBehaviorの初期化処理
    private void ProjectileInitialize(ProjectileBehavior projectileBehavior, Vector2 dir, bool throughEnforce , float damageMult, GameObject skipObject)
    {
        // 螺旋運動が有効な場合は、経過時間の寿命を3倍にする
        float adjustedLifetime = spiral ? unit.attackLifetime * 3.0f : unit.attackLifetime;
        // 武器の振幅の範囲が無効な場合は、振幅を0にする
        float aefShakeAmplitude = shakeAmplitude;
        if(!wave)
        {
            aefShakeAmplitude = 0;
        }
        else
        {
            aefShakeAmplitude = shakeAmplitude * unit.attackSpeed;
        }
         
        float pysicalAttackPower = unit.physicalAttackPower * damageMult;
        float magicAttackAttackPower = unit.magicalAttackPower * damageMult;
        if(damageZero) 
        {
            pysicalAttackPower = 0;
            magicAttackAttackPower = 0;
        }

        // ProjectileBehaviorの初期化
        projectileBehavior.Initialize(
            unit.ID,
            dir,
            skipObject,
            adjustedLifetime,
            gameObject.tag,
            pysicalAttackPower,
            magicAttackAttackPower,
            unit.attackSpeed,
            unit.attackUnitThrough,
            unit.attackObjectThrough,
            unit.attackDistance,
            unit.attackSize,
            // 以下、オプションパラメータ
            followTarget ? targetObject : null,
            laser,
            spiral,
            spiralExpansionSpeed,
            aefShakeAmplitude,
            scaleUpBlow,
            chainAttack,
            spread,
            throughEnforce
            );
    }

    /// <summary>
    /// 攻撃を停止するメソッド
    /// </summary>
    public void StopAttack()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
        isShooting = false; // 攻撃中フラグをOFFにする
        wasShootingEnabled = true; // 前回の状態をtrueにしておくことで再び攻撃が開始するのを防ぐ。battleStartでoffにすること！
        direction = Vector2.zero; // 攻撃方向をリセット
        if (weaponPrefab != null)
        {
            weaponPrefab.SetActive(false); // 武器を非アクティブにする
        }
    }

    /// <summary>
    /// 発射位置を引数として進行方向を更新する
    /// </summary>
    private void UpdateDirection(Vector2 firingPosition)
    {
        // ターゲット追従の方向設定を更新
        if (autoUpdateDirection && targetObject != null)
        {
            Vector2 targetDirection = ((Vector2)targetObject.position - firingPosition).normalized;
            direction = targetDirection;
        }
    }
}

