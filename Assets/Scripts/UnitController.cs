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

    public bool isMoving = true; // 移動を制御するフラグ

    // 各種設定を保持するフィールド
    [Header("Debug表示設定")]
    public bool showGizmos = true; // Gizmosの表示を制御するチェックボックス（デフォルトでオフ）
    public bool showDebugInfo = false; // デバッグ情報の表示を制御するチェックボックス（デフォルトでオフ）

    [Header("テレポート機能設定")]
    public bool enableTeleport = false; // テレポート機能の有効化（デフォルトでオフ）
    public float teleportInterval = 1.0f; // テレポートの実行間隔（秒）
    public float teleportDistance = 1.0f; // テレポートで移動する最大距離

    [Header("逃走機能設定")]
    public bool enableEscape = false; // 逃走機能の有効化（デフォルトでオフ）
    public float escapeSpeedMultiplier = 1.5f; // 逃走時の速度倍率
    public float escapeDistanceMultiplier = 2.0f; // 逃走距離の倍率

    [Header("ターゲット設定")]
    public bool targetSameTag = false; // 自分と同じタグを持つオブジェクトをターゲットにするかどうか
    [HideInInspector] public bool targetSameTagWpn = false; // 武器によるターゲットの同一タグ設定
    [HideInInspector] public bool targetAnomalyFirst = false; // 異常状態のターゲットを優先するかどうか
    [HideInInspector] public bool targetLowHpFirst = false; // 低HPのターゲットを優先するかどうか

    [Header("通常移動設定")]
    private float movementSpeedMulti = 300.0f; // 移動速度の倍率手動で設定
    public float movementSpeed = 1.0f; // 移動速度
    public float followRange = 7.0f; // 追従範囲
    public float approachRange = 0.3f; // 近づきすぎたら止まる範囲
    [Range(0, 100)] public float followWeight = 99f; // 追従移動の反映ウェイト（%）

    // Unit同士の接触回避のための設定
    private float lastAvoidanceCheckTime; // 最後に回避チェックを行った時間
    private float avoidanceCheckInterval = 0.1f; // チェック間隔（秒）
    private bool isAvoiding = false; // 回避中かどうかのフラグ
    private Vector2 avoidanceDirection; // 回避方向を記憶する

    // 障害物回避のための設定
    [Header("障害物回避中情報")]
    [SerializeField] private Vector2 previousAvoidanceDir = Vector2.zero; // 前回の障害物回避角度を保持するフィールド　初期値は未設定（無効値）

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
    [HideInInspector] public bool isRandomTarget = false; // ランダムなターゲットを選択するかどうかのフラグ

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
        if (!isMoving) return; // 移動フラグが無効の場合は早期リターン

        // 逃走機能が有効の場合、逃走チェックを行う
        if (enableEscape && !isEscaping)
        {
            CheckAndPerformEscape(transform.position);
        }
        // 逃走中の場合、逃走処理を行う
        else if (isEscaping)
        {
            PerformEscape(transform.position);
            return;
        }

        // テレポート機能が有効の場合、テレポート処理を実行する
        if (enableTeleport)
        {
            // 一番近い位置にいるターゲットを選択
            SetClosestTarget();
            // ターゲットの方向を向く
            Vector2 targetDirection = (targetTransform.position - transform.position).normalized;
            UpdateSpriteBasedOnDirection(targetDirection, unitSprite);

            if (Time.time - lastTeleportTime >= teleportInterval)
            {
                PerformTeleport(transform.position); // テレポート処理を実行
                lastTeleportTime = Time.time; // テレポートの実行時間を更新
            }
            // テレポート後、またはテレポートが必要ない場合は通常の攻撃ロジックを継続
            else
            {
                // 通常攻撃のためにアタックコントローラーへターゲットを設定
                if (targetTransform != null && Vector2.Distance(transform.position, targetTransform.position) <= approachRange)
                {
                    if (attackController != null)
                    {
                        attackController.SetTargetObject(targetTransform);
                    }
                }
            }
            // 位置の記録を更新
            previousPosition = transform.position;
            return; // テレポート実行中は通常の移動処理をスキップ
        }
        
        Vector2 currentPosition = transform.position;
        Vector2 newPosition;
        movementSpeed = unit.moveSpeed * 0.3f; // ユニットの移動速度を更新早すぎるのでとりあえずx0.3

        // 前の位置からの移動距離を計算
        float distanceMoved = Vector2.Distance(previousPosition, currentPosition);
        
        // 距離に基づいてアニメーション再生スピードを設定
        float walkSpeedMultiplier = Mathf.Clamp(distanceMoved * movementSpeedMulti, 0.5f, 2f); // スピードの最小・最大を制限
        animator.speed = walkSpeedMultiplier;

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

        if (Time.time - lastTargetUpdateTime > targetUpdateInterval && !isRandomTarget)
        {
            SetClosestTarget();
            lastTargetUpdateTime = Time.time;
        }

        // 回避中の場合、回避方向に移動する
        if (isAvoiding)
        {
            newPosition = currentPosition + avoidanceDirection * movementSpeed * Time.deltaTime;
            newPosition = KeepWithinCameraBounds(newPosition);
            transform.position = newPosition;

            // 回避中でも歩行アニメーションを再生
            string stateName = unit.job.ToString("D2") + "_walk";
            if (PlayAnimatorStateIfExists(stateName))
            {
                animator.Play(stateName);
            }

            // 回避方向に応じてスプライトの向きを更新
            UpdateSpriteBasedOnDirection(avoidanceDirection, unitSprite);

            // 一定距離進んだら回避終了
            if (Vector2.Distance(currentPosition, transform.position) < 0.3f)
            {
                isAvoiding = false;
            }
            previousPosition = transform.position;
            return; // 回避中は通常の追従処理をスキップ
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
        // 逃走でも歩行アニメーションを再生
        string stateName = unit.job.ToString("D2") + "_walk";
        if (PlayAnimatorStateIfExists(stateName))
        {
            animator.Play(stateName);
        }
        // アニメーション速度を更新
        animator.speed = movementSpeed * escapeSpeedMultiplier;

        Vector2 newEscapePosition = Vector2.MoveTowards(currentPosition, escapePosition, movementSpeed * escapeSpeedMultiplier * Time.deltaTime);

        newEscapePosition = KeepWithinCameraBounds(newEscapePosition);

        transform.position = newEscapePosition;

        Vector2 escapeDirection = (newEscapePosition - currentPosition).normalized;
        UpdateSpriteBasedOnDirection(escapeDirection, unitSprite);

        if (Vector2.Distance(transform.position, escapePosition) < 0.1f)
        {
            randomWalker.SetNewTargetPosition();
            isEscaping = false;
            // 逃走が終わったら再び攻撃を受けるまで逃げないようにする
            enableEscape = false;
        }
    }

    /// <summary>
    /// テレポート機能を実行します。
    /// </summary>
    private void PerformTeleport(Vector2 currentPosition)
    {
        if (inAttackStance) return;

        if (targetTransform == null) return;

        // 現在のテレポート間隔を計算
        float currentTeleportInterval = teleportInterval;

        // 脅威がapproachRange内にいるかどうかを確認
        string oppositeTag = gameObject.CompareTag("Enemy") ? "Ally" : "Enemy";
        Collider2D[] threatsInRange = Physics2D.OverlapCircleAll(currentPosition, approachRange, LayerMask.GetMask(oppositeTag));
        if (threatsInRange.Length > 0)
        {
            currentTeleportInterval = teleportInterval * 3; // テレポート間隔を3倍にする
            if (showDebugInfo) Debug.Log("脅威がapproachRange内にいるため、テレポート間隔を3倍に設定しました。");
        }

        // テレポートの実行条件
        if (Time.time - lastTeleportTime < currentTeleportInterval)
        {
            return; // テレポート待ち時間中は実行しない
        }

        Vector2 targetPosition = targetTransform.position;
        Vector2 directionToTarget = (targetPosition - currentPosition).normalized;
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);
        float teleportDistanceLimited = Mathf.Min(teleportDistance, distanceToTarget - approachRange);
        Vector2 teleportPosition = new Vector2(); // テレポート先の位置

        // ターゲットがapproachRangeの範囲内にいる場合
        if (teleportDistanceLimited <= 0)
        {
            // targetPositionからapproachRangeの範囲内のランダムな場所にテレポートする
            Vector2 randomTeleportPosition = targetPosition + Random.insideUnitCircle.normalized * approachRange / 2;
            teleportPosition = randomTeleportPosition;
        }
        else
        {
            // テレポート先の位置を計算
            Vector2 calcPosition = currentPosition + directionToTarget * teleportDistanceLimited;

            // 今いる位置と同じでない場合は通常の位置にテレポート
            if (calcPosition != (Vector2)transform.position)
            {
                teleportPosition = calcPosition;
            }
            else
            {
                // targetPositionからapproachRangeの範囲内のランダムな場所にテレポートする
                Vector2 randomTeleportPosition = targetPosition + Random.insideUnitCircle.normalized * approachRange;
                teleportPosition = randomTeleportPosition;
            }
        }

        // テレポート実行
        if (teleportPosition != Vector2.zero)
        {
            // teleportPositionをカメラ範囲内に制限
            teleportPosition = KeepWithinCameraBounds(teleportPosition);
            transform.position = teleportPosition;
            randomWalker.SetNewTargetPosition(); // ランダム移動の新しい目標地点を設定
            if (showDebugInfo)
            {
                Debug.Log($"テレポート実行: 新しい位置：{teleportPosition} = 自分の位置：{transform.position}");
            }
        }

        // targetの方向を向く
        directionToTarget = (targetTransform.position - transform.position).normalized;
        UpdateSpriteBasedOnDirection(directionToTarget, unitSprite);

        // テレポート実行時間を記録
        lastTeleportTime = Time.time;
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
    public void SetClosestTarget()
    {
        string targetTag = targetSameTag ? gameObject.tag : (gameObject.CompareTag("Enemy") ? "Ally" : "Enemy"); // 同じタグのオブジェクトをターゲットにするかどうか
        float closestDistance = Mathf.Infinity;
        Transform selectedTarget = null;

        // 異常状態のターゲット優先の処理(低HPターゲット優先よりも優先度が高い)
        if (targetAnomalyFirst)
        {
            // 異常状態のターゲットを優先して選択
            List<Unit> potentialTargets = new List<Unit>();

            Debug.Log("異常状態のターゲットを優先して選択します。");
            Debug.Log("targetTag: " + targetTag);

            foreach (GameObject potentialTarget in GameObject.FindGameObjectsWithTag(targetTag))
            {
                if (potentialTarget == gameObject) continue; // 自分自身は除外
                Debug.Log("potentialTarget: " + potentialTarget.name);

                Unit targetUnit = potentialTarget.GetComponent<Unit>();

                if (targetUnit != null && Vector2.Distance(transform.position, potentialTarget.transform.position) <= followRange)
                {
                    // targetUnit.condition[1~8]のいずれかがtrueの場合は異常状態と判定
                    if (targetUnit.condition[1] || targetUnit.condition[2] || targetUnit.condition[3] || targetUnit.condition[4] || targetUnit.condition[5] || targetUnit.condition[6] || targetUnit.condition[7] || targetUnit.condition[8])
                    {
                        potentialTargets.Add(targetUnit); // 異常状態のユニットをリストに追加
                    }
                }
            }

            if (potentialTargets.Count > 0)
            {
                // potentialTargetsをGameObjectの配列に変換
                List<GameObject> potentialTargetsGameObjects = new List<GameObject>();
                foreach (Unit unit in potentialTargets)
                {
                    potentialTargetsGameObjects.Add(unit.gameObject);
                }
                // 仲間の場合は最も近いターゲットを選択
                if (targetSameTag)
                {
                    selectedTarget = GetClosestObject(potentialTargetsGameObjects.ToArray());
                }
                // 敵の場合
                else
                {
                    // 距離と狙われやすさの両方を考慮したスコアを計算し、そのスコアが最も低いターゲットを選択
                    selectedTarget = DistanceEasilyTargeted(potentialTargetsGameObjects.ToArray());
                }
                Debug.Log("異常状態のユニットを選択しました。" + selectedTarget.name);
            }
        }

        // 低HPターゲット優先の処理
        if (targetLowHpFirst && selectedTarget == null)
        {
            // HPの低い順にターゲットを選択
            List<Unit> potentialTargets = new List<Unit>();

            Debug.Log("HPの低い順にターゲットを選択します。");
            Debug.Log("targetTag: " + targetTag);

            foreach (GameObject potentialTarget in GameObject.FindGameObjectsWithTag(targetTag))
            {
                if (potentialTarget == gameObject) continue; // 自分自身は除外
                Debug.Log("potentialTarget: " + potentialTarget.name);

                Unit targetUnit = potentialTarget.GetComponent<Unit>();
                if (targetUnit != null && Vector2.Distance(transform.position, potentialTarget.transform.position) <= followRange)
                {
                    potentialTargets.Add(targetUnit); // HPの低い順にリストに追加
                }
            }

            if (potentialTargets.Count > 0)
            {
                // currentHPの低い順にソート
                potentialTargets.Sort((a, b) => (a.currentHp / a.maxHp).CompareTo(b.currentHp / b.maxHp));
                selectedTarget = potentialTargets[0].transform; // HPが最も低いユニットを選択
                Debug.Log("HPの低いユニットを選択しました。" + selectedTarget.name);
            }
        }

        // isRandomTargetがtrueの場合は範囲内のターゲットをランダムに選ぶ
        if (isRandomTarget && selectedTarget == null)
        {
            Debug.Log("ランダムに範囲内のターゲットを変更します。");

            List<Transform> potentialTargets = new List<Transform>();

            // 1. approachRange内のターゲットを検索
            foreach (GameObject potentialTarget in GameObject.FindGameObjectsWithTag(targetTag))
            {
                // 自分自身と現在のターゲットは除外
                if (potentialTarget == gameObject || potentialTarget == targetTransform.gameObject) continue;

                float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
                if (distance <= approachRange)
                {
                    potentialTargets.Add(potentialTarget.transform);
                }
            }

            // 2. approachRange内にターゲットがいる場合
            if (potentialTargets.Count > 0)
            {
                Debug.Log("approachRange内のターゲットを選択します。");
                int randomIndex = UnityEngine.Random.Range(0, potentialTargets.Count);
                selectedTarget = potentialTargets[randomIndex];
            }
            else
            {
                // 3. approachRange内にターゲットがいない場合、followRange内を検索
                foreach (GameObject potentialTarget in GameObject.FindGameObjectsWithTag(targetTag))
                {
                    // 自分自身と現在のターゲットは除外
                    if (potentialTarget == gameObject || potentialTarget == targetTransform.gameObject) continue;

                    float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
                    if (distance <= followRange)
                    {
                        potentialTargets.Add(potentialTarget.transform);
                    }
                }

                // 4. followRange内にターゲットがいる場合
                if (potentialTargets.Count > 0)
                {
                    Debug.Log("approachRange内にターゲットがいないため、followRange内のターゲットを選択します。");
                    int randomIndex = UnityEngine.Random.Range(0, potentialTargets.Count);
                    selectedTarget = potentialTargets[randomIndex];
                }
            }
        }

        // それ以外の場合、最も近いターゲットを選ぶ
        if (selectedTarget == null)
        {
            // 仲間の場合は最も近いターゲットを選択
            if (targetSameTag)
            {
                selectedTarget = GetClosestObject(GameObject.FindGameObjectsWithTag(targetTag));
            }
            // 敵の場合
            else
            {
                // 距離と狙われやすさの両方を考慮したスコアを計算し、そのスコアが最も低いターゲットを選択
                selectedTarget = DistanceEasilyTargeted(GameObject.FindGameObjectsWithTag(targetTag));
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
    /// 距離と狙われやすさの両方を考慮したスコアを計算し、そのスコアが最も低いターゲットを返す
    /// easilyTargetedが高いほど狙われやすい
    /// </summary>
    private Transform DistanceEasilyTargeted(GameObject[] potentialTargets)
    {
        Transform selectedTarget = null;
         float closestScore = Mathf.Infinity;
        foreach (GameObject potentialTarget in potentialTargets)
        {
            if (potentialTarget == gameObject) continue;

            Unit targetUnit = potentialTarget.GetComponent<Unit>();
            if (targetUnit != null)
            {
                float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
                float easilyTargetedFactor = targetUnit.easilyTargeted / 50.0f; // 平均値50を基準にしてスコアを調整
                float score = distance * (1.0f + easilyTargetedFactor); // 距離と狙われやすさを組み合わせたスコアを計算

                if (score < closestScore)
                {
                    closestScore = score;
                    selectedTarget = potentialTarget.transform;
                }
            }
        }
        return selectedTarget;
    }

    /// <summary>
    /// 配列を引数として受け取り、自身との距離が最も近いオブジェクトを返します。
    /// </summary>
    private Transform GetClosestObject(GameObject[] potentialTargets)
    {
        Transform selectedTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (GameObject obj in potentialTargets)
        {
            if (obj == gameObject) continue; // 自分自身は除外

            float distance = Vector2.Distance(transform.position, obj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                selectedTarget = obj.transform;
            }
        }

        return selectedTarget;
    }

    /// <summary>
    /// ユニットをカメラの範囲内に留め、Obstacleレイヤーが設定されたオブジェクトの範囲に進入不可にするメソッドです。
    /// また、EnemyやAllyタグを持つオブジェクトの範囲に入った場合は、回避方向に移動します。
    /// </summary>
    private Vector2 KeepWithinCameraBounds(Vector2 position)
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // カメラのビュー範囲を取得
        Vector3 minScreenBounds = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.transform.position.z));
        Vector3 maxScreenBounds = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.transform.position.z));

        // "ArenaLimit" レイヤーのコライダーを取得
        LayerMask arenaLimitLayerMask = LayerMask.GetMask("ArenaLimit");
        Collider2D arenaLimitCollider = Physics2D.OverlapCircle(position, Mathf.Infinity, arenaLimitLayerMask);

        if (arenaLimitCollider != null)
        {
            // "ArenaLimit" の外縁を取得
            Bounds arenaBounds = arenaLimitCollider.bounds;

            // ポジションを "ArenaLimit" 内部にクランプ
            float clampedX = Mathf.Clamp(position.x, arenaBounds.min.x, arenaBounds.max.x);
            float clampedY = Mathf.Clamp(position.y, arenaBounds.min.y, arenaBounds.max.y);

            position = new Vector2(clampedX, clampedY);
        }

        // 最終的にカメラ範囲でさらに制限をかける
        float clampedCameraX = Mathf.Clamp(position.x, minScreenBounds.x, maxScreenBounds.x);
        float clampedCameraY = Mathf.Clamp(position.y, minScreenBounds.y, maxScreenBounds.y);

        Vector2 clampedPosition = new Vector2(clampedCameraX, clampedCameraY);

        // Unitの回避チェックの間隔が経過している場合はUnit回避を行う
        if (Time.time - lastAvoidanceCheckTime >= avoidanceCheckInterval)
        {
            // 回避チェックを行った時間を更新
            lastAvoidanceCheckTime = Time.time;

            // EnemyやAllyタグを持つオブジェクトの回避**
            float avoidRadius = 0.1f;
            string[] tagsToAvoid = { "Enemy", "Ally" };

            foreach (string tag in tagsToAvoid)
            {
                Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(clampedPosition, avoidRadius);
                foreach (Collider2D col in nearbyObjects)
                {
                    if (col.gameObject != this.gameObject && col.CompareTag(tag))
                    {
                        avoidanceDirection = (clampedPosition - (Vector2)col.bounds.center).normalized;
                        isAvoiding = true; // 回避フラグを立てる
                        if (showDebugInfo)
                        {
                            Debug.Log($"{tag}タグのオブジェクトを回避: {col.name}, 回避先: {clampedPosition + avoidanceDirection * avoidRadius}");
                        }
                        clampedPosition = (Vector2)transform.position; // 回避のため現在の位置を維持
                    }
                }
            }
        }

        // 障害物と接触する場合は手前で回避を実行 (優先度が高いため最後に行う)
        clampedPosition = AvoidObstacles(clampedPosition);

        return clampedPosition;
    }

    /// <summary>
    /// 障害物を回避するためのメソッドです。
    /// </summary>
    private Vector2 AvoidObstacles(Vector2 targetPosition)
    {
        float avoidDistance = movementSpeed * Time.deltaTime; // 回避する距離
        float scanAngleStep = 10f;  // スキャンする角度のステップ (度)
        int maxScanAngle = 90;      // スキャンする角度範囲の最大値 (±90度)
        LayerMask obstacleLayerMask = LayerMask.GetMask("Obstacle");

        // 移動先（targetPosition）が障害物にぶつかるか確認
        Collider2D colliderAtPosition = Physics2D.OverlapCircle(targetPosition, 0.1f, obstacleLayerMask);

        if (colliderAtPosition != null)
        {
            if (showDebugInfo) Debug.LogWarning("移動先に障害物があります。回避処理を実行します。");

            // 障害物がある場合のみ回避処理を実行
            Vector2 bestDirection = Vector2.zero;
            float smallestAngle = float.MaxValue;

            // スキャン範囲を設定（-maxScanAngleから+maxScanAngleまで）
            for (float angle = -maxScanAngle; angle <= maxScanAngle; angle += scanAngleStep)
            {
                // previousDirectionを基準に角度を回転させた方向を計算
                Vector2 calcDirection = (targetPosition - previousPosition).normalized;
                if (previousAvoidanceDir != Vector2.zero) calcDirection = previousAvoidanceDir; // 前回の回避方向を使用
                Vector2 scanDirection = Quaternion.Euler(0, 0, angle) * calcDirection;

                // スキャン方向に障害物があるかを確認
                Collider2D scanHit = Physics2D.OverlapPoint(previousPosition + scanDirection * avoidDistance, obstacleLayerMask);
                if (scanHit == null)
                {
                    // 障害物がない場合、方向を候補として記録
                    float angleDifference = Mathf.Abs(angle); // 中心方向との差を計算
                    if (angleDifference < smallestAngle)
                    {
                        smallestAngle = angleDifference;
                        bestDirection = scanDirection;
                    }
                }
            }

            // 障害物を回避する方向が見つかった場合
            if (bestDirection != Vector2.zero)
            {
                previousAvoidanceDir = bestDirection; // 回避方向を記憶
                return previousPosition + bestDirection.normalized * avoidDistance; // 最適な方向に移動
            }

            // すべての方向に障害物がある場合（例外的なケース）
            if (showDebugInfo) Debug.LogWarning("障害物を回避できる方向が見つかりません。現在位置を維持します。");
            return previousPosition; // 現在の位置を維持
        }

        // 障害物がない場合でも安全確認後に回避方向をリセット
        if (previousAvoidanceDir != Vector2.zero && Physics2D.OverlapCircle(targetPosition, 0.2f, obstacleLayerMask) == null)
        {
            if (showDebugInfo) Debug.Log("障害物を回避しました。回避方向をリセットします。");
            previousAvoidanceDir = Vector2.zero; // 安全を確認してからリセット
        }

        return targetPosition;
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
        if(isEscaping || enableTeleport) smoothedDirection = direction; // 逃走中はスムージングを無効にする

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
