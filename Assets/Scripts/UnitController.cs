using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ユニットをランダムな目標地点に向かって移動させ、目標地点に到達したら新しい目標地点を設定します。
/// 追従移動、テレポート機能、および逃走機能を追加しました。
/// </summary>
public class UnitController : MonoBehaviour
{
    // Unitのスプライトを取得
    [SerializeField] private SpriteRenderer unitSprite;
    private Animator animator; // Animatorコンポーネントの参照

    // 各種設定を保持するフィールド
    [Header("Debug表示設定")]
    public bool showGizmos = true; // Gizmosの表示を制御するチェックボックス（デフォルトでオフ）
    public bool showDebugInfo = false; // デバッグ情報の表示を制御するチェックボックス（デフォルトでオフ）

    [Header("テレポート機能設定")]
    public bool enableTeleport = false; // テレポート機能の有効化（デフォルトでオフ）
    public float teleportInterval = 10.0f; // テレポートの実行間隔（秒）
    public float teleportDistance = 2.0f; // テレポートで移動する最大距離

    [Header("逃走機能設定")]
    public bool enableEscape = false; // 逃走機能の有効化（デフォルトでオフ）
    public float escapeSpeedMultiplier = 1.5f; // 逃走時の速度倍率
    public float escapeDistanceMultiplier = 1.5f; // 逃走距離の倍率

    [Header("ターゲット設定")]
    public bool targetSameTag = false; // 自分と同じタグを持つオブジェクトをターゲットにするかどうか
    [HideInInspector] public bool targetLowHpFirst = false; // 低HPのターゲットを優先するかどうか

    [Header("通常移動設定")]
    public float movementSpeed = 1.0f; // 移動速度
    public float followRange = 7.0f; // 追従範囲
    public float approachRange = 0.3f; // 近づきすぎたら止まる範囲
    [Range(0, 100)] public float followWeight = 99f; // 追従移動の反映ウェイト（%）

    [Header("攻撃モード設定")]
    public bool enableAttackStance = false; // 攻撃モードの有効化
    public float attackStanceDuration = 10.0f; // 攻撃モードでその場にとどまる時間（秒）
    public float attackDelay = 5.0f; // 攻撃後のディレイ時間（秒）

    // インターナルステートの追跡
    private float lastTargetUpdateTime; // 最後にターゲットを更新した時間
    public float targetUpdateInterval = 0.5f; // ターゲット再評価の間隔
    private float lastTeleportTime; // 最後にテレポートした時間を記録
    private bool isEscaping = false; // 逃走中かどうかのフラグ

    public bool MoveBackFlag = false; // ユニットが後退するかどうかのフラグ
    public float MoveBackDistance = 1.0f; // 後退する距離

    private Camera mainCamera;
    private Transform targetTransform; // ターゲットのTransform
    private RandomWalkerBezier2D randomWalker; // RandomWalkerBezier2Dの参照
    private Vector2 startPosition; // 移動開始位置
    private float startTime; // 移動開始時間
    private Vector2 previousPosition; // 前回の位置を記憶
    private Vector2 previousDirection; // 前回のスプライト方向を記憶
    //private bool isMoving; // 移動中かどうかのフラグ
    private int currentSpriteIndex; // 現在のスプライトインデックス
    private float lastSpriteChangeTime; // 最後にスプライトを変更した時間

    private bool inAttackStance = false; // その場でとどまる攻撃モード中かどうか
    public bool InAttackStance => inAttackStance; // 攻撃モード中かどうかを取得するプロパティ
    private float attackStanceStartTime; // その場でとどまる攻撃モードの開始時間
    private bool inAttackDelay = false; // その場でとどまる攻撃ディレイ中かどうか
    private float attackDelayStartTime; // その場でとどまる撃ディレイの開始時間
    private Vector2 escapePosition; //　逃走先の記録

    private AttackController attackController; // AttackControllerの参照

    Unit unit; // ユニットの参照

    /// <summary>
    /// 初期設定を行います。
    /// </summary>
    void Start()
    {
        unit = GetComponent<Unit>(); // Unitの参照を取得

        // Animatorコンポーネントを取得
        animator = GetComponent<Animator>();

        // ランダムなオフセットを設定 (0から1の範囲で)
        float randomOffset = UnityEngine.Random.Range(0f, animator.GetCurrentAnimatorStateInfo(0).length);
        animator.Play(unit.job.ToString("D2") + "_idle", 0, randomOffset); // Idleアニメーションを再生

        // RandomWalkerBezier2Dコンポーネントがアタッチされていない場合はアタッチする
        if (GetComponent<RandomWalkerBezier2D>() == null)
        {
            gameObject.AddComponent<RandomWalkerBezier2D>();
        }
        
        randomWalker = GetComponent<RandomWalkerBezier2D>(); // RandomWalkerBezier2Dの参照を取得

        if (randomWalker == null)
        {
            Debug.LogError("RandomWalkerBezier2D component is missing on this GameObject.");
        }

        // unitSpriteが設定されているか確認
        if (unitSprite == null)
        {
            Debug.LogError("unitSpriteが設定されていません。");
        }

        // 最初の目標地点を設定
        startPosition = transform.position;
        startTime = Time.time;

        SetClosestTarget();
        lastTargetUpdateTime = Time.time;
        lastTeleportTime = Time.time; // テレポートの初期時間を設定

        previousPosition = transform.position;
        previousDirection = Vector2.zero; // 初期化
        //isMoving = false; // 初期状態では移動していない

        // AttackControllerの参照を取得
        attackController = GetComponent<AttackController>();
    }

    /// <summary>
    /// フレームごとの更新を行います。
    /// </summary>
    void Update()
    {
        Vector2 currentPosition = transform.position;
        Vector2 newPosition;

        // 攻撃モードが有効の場合の処理
        if (enableAttackStance)
        {
            if (inAttackStance || inAttackDelay)
            {
                HandleAttackStance(currentPosition);
                if (showDebugInfo) DisplayDebugInfo();
                if (!inAttackDelay)
                {
                    return; // 攻撃モード中は通常の移動処理を行わない
                }
            }
        }

        if (Time.time - lastTargetUpdateTime > targetUpdateInterval)
        {
            SetClosestTarget();
            lastTargetUpdateTime = Time.time;
        }

        if (randomWalker != null)
        {
            Vector2 randomMovePosition = randomWalker.GetBezierPosition(movementSpeed, currentPosition);
            Vector2 followMovePosition = CalculateFollowMove(currentPosition);

            float followRatio = followWeight / 100f;
            float randomRatio = 1f - followRatio;

            newPosition = (randomMovePosition * randomRatio) + (followMovePosition * followRatio);

            // ターゲットの方向を向いた状態で一定距離ターゲットの反対方向に移動する
            if (MoveBackFlag)
            {
                Vector2 directionToTarget = ((Vector2)targetTransform.position - currentPosition).normalized;
                newPosition = currentPosition - directionToTarget * MoveBackDistance;
                MoveBackFlag = false;
            }

            newPosition = KeepWithinCameraBounds(newPosition);

            transform.position = newPosition;

            // 移動中かどうかを判断し、アニメーションを切り替える
            if (currentPosition != (Vector2)transform.position)
            {
                //isMoving = true;
                string stateName = unit.job.ToString("D2") + "_walk";
                if(PlayAnimatorStateIfExists(stateName))
                {
                    animator.Play(stateName); // 動いている場合はWalkステートをセット
                }
            }
            else
            {
                //isMoving = false;
                string stateName = unit.job.ToString("D2") + "_idle";
                if(PlayAnimatorStateIfExists(stateName))
                {
                    animator.Play(stateName); // 動いていない場合はIdleステートをセット
                }
            }

                // ターゲットがapproachRangeの範囲内にいるときは、逃走中以外で常にターゲット方向を向く
                if (targetTransform != null && Vector2.Distance(currentPosition, targetTransform.position) <= approachRange && !isEscaping)
                {
                    Vector2 directionToTarget = ((Vector2)targetTransform.position - currentPosition).normalized;
                    UpdateSpriteBasedOnDirection(directionToTarget, unitSprite);
                }
                else
                {
                    Vector2 averageDirection = CalculateAverageDirection(previousPosition, currentPosition, newPosition);
                    UpdateSpriteBasedOnDirection(averageDirection, unitSprite);
                }

            previousPosition = currentPosition;
        }
        else
        {
            Debug.LogError("RandomWalkerBezier2D is missing on this GameObject."); // RandomWalkerBezier2Dがアタッチされていない場合はエラーを表示
        }
    }

    /// <summary>
    /// 指定されたステートがAnimatorに存在する場合に再生します。
    /// </summary>
    /// <param name="stateName">再生するステートの名前</param>
    private bool PlayAnimatorStateIfExists(string stateName)
    {
        // state名をハッシュ値に変換
        int stateID = Animator.StringToHash(stateName);

        // 指定したレイヤーの中にstateIDのステートが存在するか確認
        return animator.HasState(0, stateID);
    }

    /// <summary>
    /// 攻撃範囲内に別タグのユニットがいる場合に逃走するためのチェックを行います。
    /// </summary>
    private void CheckAndPerformEscape(Vector2 currentPosition)
    {
        string oppositeTag = gameObject.CompareTag("Enemy") ? "Ally" : "Enemy";
        float closestDistance = Mathf.Infinity;
        Transform closestThreat = null;
        float threatApproachRange = 0.0f;

        foreach (GameObject potentialThreat in GameObject.FindGameObjectsWithTag(oppositeTag))
        {
            float distance = Vector2.Distance(transform.position, potentialThreat.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestThreat = potentialThreat.transform;
                UnitController threatController = potentialThreat.GetComponent<UnitController>();
                if (threatController != null)
                {
                    threatApproachRange = threatController.approachRange;
                }
            }
        }

        if (closestThreat != null && closestDistance <= threatApproachRange)
        {
            Vector2 directionAway = (currentPosition - (Vector2)closestThreat.position).normalized;
            float escapeDistance = threatApproachRange * escapeDistanceMultiplier;
            escapePosition = currentPosition + directionAway * escapeDistance;

            if (showDebugInfo)
            {
                Debug.Log($"逃走先計算: 新しい位置 {escapePosition}, 逃走元: {closestThreat.name}");
            }
            isEscaping = true;
        }
    }

    /// <summary>
    /// 実際に逃走する処理を行います。
    /// </summary>
    private void PerformEscape(Vector2 currentPosition)
    {
        Vector2 newEscapePosition = Vector2.MoveTowards(currentPosition, escapePosition, movementSpeed * escapeSpeedMultiplier * Time.deltaTime);

        newEscapePosition = KeepWithinCameraBounds(newEscapePosition);

        transform.position = newEscapePosition;

        Vector2 escapeDirection = (newEscapePosition - currentPosition).normalized;
        UpdateSpriteBasedOnDirection(escapeDirection, unitSprite);

        if (Vector2.Distance(transform.position, escapePosition) < 0.1f)
        {
            randomWalker.SetNewTargetPosition();
            isEscaping = false;
        }
    }

    /// <summary>
    /// テレポート機能を実行します。
    /// </summary>
    private void PerformTeleport(Vector2 currentPosition)
    {
        if (inAttackStance) return;

        if (targetTransform == null) return;

        Vector2 targetPosition = targetTransform.position;
        Vector2 directionToTarget = (targetPosition - currentPosition).normalized;
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

        float teleportDistanceLimited = Mathf.Min(teleportDistance, distanceToTarget - approachRange);

        if (teleportDistanceLimited <= 0)
        {
            if (showDebugInfo)
            {
                Debug.Log("テレポートが必要ないか、ターゲットに近すぎるため、テレポートを実行しません。");
            }
            return;
        }

        Vector2 teleportPosition = currentPosition + directionToTarget * teleportDistanceLimited;

        if (teleportPosition != (Vector2)transform.position)
        {
            transform.position = teleportPosition;
            if (showDebugInfo)
            {
                Debug.Log($"テレポート実行: 新しい位置：{teleportPosition} = 自分の位置：{transform.position}");
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log("テレポート位置が現在位置と同じため、移動を実行しません。");
            }
        }
    }

    /// <summary>
    /// 攻撃モードを処理します。
    /// </summary>
    private void HandleAttackStance(Vector2 currentPosition)
    {
        if (targetTransform != null)
        {
            if (inAttackStance)
            {
                if (Time.time - attackStanceStartTime < attackStanceDuration)
                {
                    return;
                }
                else
                {
                    inAttackStance = false;
                    inAttackDelay = true;
                    attackDelayStartTime = Time.time;
                    return;
                }
            }

            if (inAttackDelay)
            {
                if (Time.time - attackDelayStartTime < attackDelay)
                {
                    return;
                }
                else
                {
                    inAttackDelay = false;
                }
            }

            float distanceToTarget = Vector2.Distance(currentPosition, targetTransform.position);
            if (distanceToTarget <= approachRange)
            {
                inAttackStance = true;
                attackStanceStartTime = Time.time;

                Vector2 directionToTarget = (targetTransform.position - (Vector3)currentPosition).normalized;
                previousDirection = directionToTarget;
                UpdateSpriteBasedOnDirection(directionToTarget, unitSprite);
            }
        }
    }

    /// <summary>
    /// 追従移動を計算します。
    /// </summary>
    private Vector2 CalculateFollowMove(Vector2 currentPosition)
    {
        if (targetTransform == null)
        {
            return currentPosition;
        }

        Vector2 targetPosition = targetTransform.position;
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

        if (distanceToTarget > followRange)
        {
            if (showDebugInfo)
            {
                Debug.Log($"ターゲットが追従範囲外です: 距離 = {distanceToTarget}");
            }
            return currentPosition;
        }

        HandleAttackStance(currentPosition);

        if (distanceToTarget <= approachRange)
        {
            if (showDebugInfo)
            {
                Debug.Log($"ターゲットに近づきすぎています: {distanceToTarget}");
            }
            return currentPosition;
        }

        return Vector2.MoveTowards(currentPosition, targetPosition, movementSpeed * Time.deltaTime);
    }

    /// <summary>
    /// ターゲットとの距離を評価し、最も近いターゲットを設定します。
    /// </summary>
    public void SetClosestTarget(bool randomSelection = false)
    {
        string targetTag = targetSameTag ? gameObject.tag : (gameObject.CompareTag("Enemy") ? "Ally" : "Enemy");
        float closestDistance = Mathf.Infinity;
        Transform selectedTarget = null;

        // 低HPターゲット優先の処理
        if (targetLowHpFirst)
        {
            // HPの低い順にターゲットを選択
            List<Unit> potentialTargets = new List<Unit>();

            foreach (GameObject potentialTarget in GameObject.FindGameObjectsWithTag(targetTag))
            {
                if (potentialTarget == gameObject) continue;

                Unit targetUnit = potentialTarget.GetComponent<Unit>();
                if (targetUnit != null && Vector2.Distance(transform.position, potentialTarget.transform.position) <= approachRange)
                {
                    potentialTargets.Add(targetUnit); // HPの低い順にリストに追加
                }
            }

            if (potentialTargets.Count > 0)
            {
                // HPの低い順にソート
                potentialTargets.Sort((x, y) => x.Hp.CompareTo(y.Hp));
                selectedTarget = potentialTargets[0].transform; // HPが最も低いユニットを選択
            }
        }

        // randomSelectionがtrueの場合はApproachRange内のターゲットをランダムに選ぶ
        if (randomSelection && selectedTarget == null)
        {
            List<Transform> potentialTargets = new List<Transform>();

            foreach (GameObject potentialTarget in GameObject.FindGameObjectsWithTag(targetTag))
            {
                if (potentialTarget == gameObject) continue;

                float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
                if (distance <= approachRange)
                {
                    potentialTargets.Add(potentialTarget.transform);
                }
            }

            // ターゲットがいる場合、ランダムに1つ選ぶ
            if (potentialTargets.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, potentialTargets.Count);
                selectedTarget = potentialTargets[randomIndex];
            }
        }

        // randomSelectionがtrueでターゲットがいなかった場合、またはrandomSelectionがfalseの場合、最も近いターゲットを選ぶ
        if (selectedTarget == null)
        {
            foreach (GameObject potentialTarget in GameObject.FindGameObjectsWithTag(targetTag))
            {
                if (potentialTarget == gameObject) continue;

                float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    selectedTarget = potentialTarget.transform;
                }
            }
        }

        targetTransform = selectedTarget;

        if (attackController != null && targetTransform != null)
        {
            attackController.SetTargetObject(targetTransform);
        }

        if (showDebugInfo)
        {
            if (targetTransform != null)
            {
                Debug.Log($"ターゲット設定: {targetTransform.name}");
            }
            else
            {
                Debug.Log("ターゲットが見つかりませんでした。");
            }
        }
    }

    /// <summary>
    /// ユニットをカメラの範囲内に留め、Obstacleレイヤーが設定されたオブジェクトの範囲に進入不可にするメソッドです。
    /// </summary>
    private Vector2 KeepWithinCameraBounds(Vector2 position)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        Vector3 minScreenBounds = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.transform.position.z));
        Vector3 maxScreenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.transform.position.z));

        float clampedX = Mathf.Clamp(position.x, minScreenBounds.x, maxScreenBounds.x);
        float clampedY = Mathf.Clamp(position.y, minScreenBounds.y, maxScreenBounds.y);

        Vector2 clampedPosition = new Vector2(clampedX, clampedY);

        // **障害物チェック（最優先）**
        float checkRadius = 0.1f; // 進入不可のチェックに使用する半径
        LayerMask obstacleLayerMask = LayerMask.GetMask("Obstacle");

        if (Physics2D.OverlapCircle(clampedPosition, checkRadius, obstacleLayerMask))
        {
            // 障害物がある場合はその位置に進入を防止
            if (showDebugInfo)
            {
                Debug.Log("Obstacle detected, cannot move to position: " + clampedPosition);
            }
            return (Vector2)transform.position; // 障害物がある場合は現在の位置を維持
        }

        // **EnemyやAllyタグを持つオブジェクトの回避（次に優先）**
        float avoidRadius = 0.1f; // 回避する範囲
        string[] tagsToAvoid = { "Enemy", "Ally" };

        foreach (string tag in tagsToAvoid)
        {
            Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(clampedPosition, avoidRadius);
            foreach (Collider2D col in nearbyObjects)
            {
                // 自分自身を含まないように除外
                if (col.gameObject != this.gameObject && col.CompareTag(tag))
                {
                    // タグを持つオブジェクトを回避する処理
                    Vector2 directionAway = (clampedPosition - (Vector2)col.bounds.center).normalized;
                    clampedPosition += directionAway * avoidRadius;

                    if (showDebugInfo)
                    {
                        Debug.Log($"{tag}タグのオブジェクトを回避: {col.name}, 回避先: {clampedPosition}");
                    }
                }
            }
        }

        return clampedPosition;
    }



    /// <summary>
    /// 移動方向に基づいてスプライトを更新します。
    /// </summary>
    private void UpdateSpriteBasedOnDirection(Vector2 direction, SpriteRenderer unitSprite)
    {
        // スプライトの方向をスムージングするために補間
        float smoothingFactor = 0.1f; // スムージングの度合いを調整する係数
        Vector2 smoothedDirection = Vector2.Lerp(previousDirection, direction, smoothingFactor).normalized;
        previousDirection = smoothedDirection; // 次のフレームのために現在の方向を記憶

        // 左向きかどうかを判定してFlipXを設定
        unitSprite.flipX = smoothedDirection.x < 0;
        // 左向きかどうかを判定して武器のオブジェクトのScaleXを元の値に*-1して反転させる
        if(GetComponent<AttackController>().weaponPrefab!=null)
        {
            Transform weaponTransform = GetComponent<AttackController>().weaponPrefab.transform;

            if (smoothedDirection.x < 0)
            {
                weaponTransform.localScale = new Vector3(-0.33f, 0.33f, 0.33f);
            }
            else
            {
                weaponTransform.localScale = new Vector3(0.33f, 0.33f, 0.33f);
            }
        }


    }

    /// <summary>
    /// 以前の位置、今の位置、移動先の位置のベクトルの平均値を計算します。
    /// </summary>
    private Vector2 CalculateAverageDirection(Vector2 previousPosition, Vector2 currentPosition, Vector2 targetPosition)
    {
        // 以前の位置、現在の位置、移動先の位置を平均して、スプライトの向きを滑らかにします
        Vector2 direction = (currentPosition - previousPosition + targetPosition - currentPosition).normalized;
        return direction;
    }

    /// <summary>
    /// Gizmosを使用して追従範囲と近づく範囲を視覚化します。
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showGizmos) return; // Gizmosの表示が無効なら早期リターン

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, followRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, approachRange);

        if (targetTransform != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, targetTransform.position);
        }
    }

    /// <summary>
    /// デバッグ情報を表示するメソッド
    /// </summary>
    private void DisplayDebugInfo()
    {
        if (inAttackStance)
        {
            float elapsedStanceTime = Time.time - attackStanceStartTime;
            Debug.Log($"Attack Stance Active: {elapsedStanceTime:F2} / {attackStanceDuration} seconds");
        }

        if (inAttackDelay)
        {
            float elapsedDelayTime = Time.time - attackDelayStartTime;
            Debug.Log($"Attack Delay Active: {elapsedDelayTime:F2} / {attackDelay} seconds");
        }
    }
}
