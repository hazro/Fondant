using UnityEngine;

/// <summary>
/// ユニットをランダムな目標地点に向かって移動させ、目標地点に到達したら新しい目標地点を設定します。
/// 追従移動、テレポート機能、および逃走機能を追加しました。
/// </summary>
public class UnitController : MonoBehaviour
{
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

    [Header("通常移動設定")]
    public float movementSpeed = 1.0f;
    public float followRange = 7.0f; // 追従範囲
    public float approachRange = 0.3f; // 近づきすぎたら止まる範囲
    [Range(0, 100)] public float followWeight = 99f; // 追従移動の反映ウェイト（%）

    [Header("攻撃モード設定")]
    public bool enableAttackStance = false; // 攻撃モードの有効化
    public float attackStanceDuration = 10.0f; // 攻撃モードでその場にとどまる時間（秒）
    public float attackDelay = 5.0f; // 攻撃後のディレイ時間（秒）

    // インターナルステートの追跡
    private float lastTargetUpdateTime;
    public float targetUpdateInterval = 0.5f; // ターゲット再評価の間隔
    private float lastTeleportTime; // 最後にテレポートした時間を記録
    private bool isEscaping = false; // 逃走中かどうかのフラグ

    // スプライト関連のフィールド
    [Header("移動方向に応じたスプライト")]
    public Sprite[] directionSprites;
    public bool useDiagonalSprites = false;
    public bool useFlippedSpritesForLeft = false;

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private Transform targetTransform;
    private RandomWalkerBezier2D randomWalker; // RandomWalkerBezier2Dの参照
    private Vector2 startPosition;
    private float startTime;
    private Vector2 previousPosition;
    private Vector2 previousDirection; // 前回のスプライト方向を記憶

    private bool inAttackStance = false; // 攻撃モード中かどうか
    public bool InAttackStance => inAttackStance; // 攻撃モード中かどうかを取得するプロパティ
    private float attackStanceStartTime; // 攻撃モードの開始時間
    private bool inAttackDelay = false; // 攻撃ディレイ中かどうか
    private float attackDelayStartTime; // 攻撃ディレイの開始時間
    private Vector2 escapePosition; //　逃走先の記録

    private AttackController attackController; // AttackControllerの参照

    /// <summary>
    /// 初期設定を行います。
    /// </summary>
    void Start()
    {
        // RandomWalkerBezier2Dコンポーネントがアタッチされていない場合はアタッチする
        if (GetComponent<RandomWalkerBezier2D>() == null)
        {
            gameObject.AddComponent<RandomWalkerBezier2D>();
        }
        
        spriteRenderer = GetComponent<SpriteRenderer>(); // スプライトレンダラーの参照を取得
        randomWalker = GetComponent<RandomWalkerBezier2D>(); // RandomWalkerBezier2Dの参照を取得

        if (randomWalker == null)
        {
            Debug.LogError("RandomWalkerBezier2D component is missing on this GameObject.");
        }

        // 最初の目標地点を設定
        startPosition = transform.position;
        startTime = Time.time;

        SetClosestTarget();
        lastTargetUpdateTime = Time.time;
        lastTeleportTime = Time.time; // テレポートの初期時間を設定

        previousPosition = transform.position;
        previousDirection = Vector2.zero; // 初期化

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

            newPosition = KeepWithinCameraBounds(newPosition);

            transform.position = newPosition;

            // ターゲットがapproachRangeの範囲内にいるときは、逃走中以外で常にターゲット方向を向く
            if (targetTransform != null && Vector2.Distance(currentPosition, targetTransform.position) <= approachRange && !isEscaping)
            {
                Vector2 directionToTarget = ((Vector2)targetTransform.position - currentPosition).normalized;
                UpdateSpriteBasedOnDirection(directionToTarget, directionSprites, spriteRenderer, useDiagonalSprites, useFlippedSpritesForLeft);
            }
            else
            {
                Vector2 averageDirection = CalculateAverageDirection(previousPosition, currentPosition, newPosition);
                UpdateSpriteBasedOnDirection(averageDirection, directionSprites, spriteRenderer, useDiagonalSprites, useFlippedSpritesForLeft);
            }

            previousPosition = currentPosition;
        }
        else
        {
            Debug.LogError("RandomWalkerBezier2D is missing on this GameObject.");
        }
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
        UpdateSpriteBasedOnDirection(escapeDirection, directionSprites, spriteRenderer, useDiagonalSprites, useFlippedSpritesForLeft);

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
                UpdateSpriteBasedOnDirection(directionToTarget, directionSprites, spriteRenderer, useDiagonalSprites, useFlippedSpritesForLeft);
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
        string targetTag = targetSameTag ? gameObject.tag : (gameObject.CompareTag("Enemy") ? "Ally" : "Enemy");
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (GameObject potentialTarget in GameObject.FindGameObjectsWithTag(targetTag))
        {
            if (potentialTarget == gameObject) continue;

            float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = potentialTarget.transform;
            }
        }

        targetTransform = closestTarget;

        if (attackController != null && targetTransform != null)
        {
            attackController.SetTargetObject(targetTransform);
        }

        if (showDebugInfo)
        {
            if (targetTransform != null)
            {
                Debug.Log($"ターゲット設定: {targetTransform.name} 距離: {closestDistance}");
            }
            else
            {
                Debug.Log("ターゲットが見つかりませんでした。");
            }
        }
    }

    /// <summary>
    /// ユニットをカメラの範囲内に留めるメソッドです。
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

        return new Vector2(clampedX, clampedY);
    }

    /// <summary>
    /// 移動方向に基づいてスプライトを更新します。
    /// </summary>
    private void UpdateSpriteBasedOnDirection(Vector2 direction, Sprite[] sprites, SpriteRenderer spriteRenderer, bool useDiagonalSprites, bool useFlippedSpritesForLeft)
    {

        // スプライトが8枚以下の場合でも処理をスキップ
        if (sprites.Length < 8)
        {
            return;
        }

        // スプライトの方向をスムージングするために補間
        float smoothingFactor = 0.1f; // スムージングの度合いを調整する係数
        Vector2 smoothedDirection = Vector2.Lerp(previousDirection, direction, smoothingFactor).normalized;
        previousDirection = smoothedDirection; // 次のフレームのために現在の方向を記憶

        float angle = Mathf.Atan2(smoothedDirection.y, smoothedDirection.x) * Mathf.Rad2Deg;
        int spriteIndex = 0;

        if (angle >= -22.5f && angle < 22.5f) spriteIndex = 0;
        else if (angle >= 22.5f && angle < 67.5f) spriteIndex = useDiagonalSprites ? 1 : 0;
        else if (angle >= 67.5f && angle < 112.5f) spriteIndex = 2;
        else if (angle >= 112.5f && angle < 157.5f) spriteIndex = useDiagonalSprites ? 3 : 4;
        else if (angle >= 157.5f || angle < -157.5f) spriteIndex = useFlippedSpritesForLeft ? 0 : 4;
        else if (angle >= -157.5f && angle < -112.5f) spriteIndex = useDiagonalSprites ? 5 : 4;
        else if (angle >= -112.5f && angle < -67.5f) spriteIndex = 6;
        else if (angle >= -67.5f && angle < -22.5f) spriteIndex = useDiagonalSprites ? 7 : 0;

        spriteRenderer.sprite = sprites[spriteIndex];
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
