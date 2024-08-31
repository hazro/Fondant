using UnityEngine;

/// <summary>
/// ユニットをランダムな目標地点に向かって移動させ、目標地点に到達したら新しい目標地点を設定します。
/// 追従移動、テレポート機能、および逃走機能を追加しました。
/// </summary>
public class UnitController : MonoBehaviour
{
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
    public float minTargetDistance = 0.5f;
    public float maxTargetDistance = 1.0f;
    public float followRange = 7.0f; // 追従範囲
    public float approachRange = 0.3f; // 近づきすぎたら止まる範囲
    [Range(0, 100)] public float followWeight = 99f; // 追従移動の反映ウェイト（%）

    [Header("攻撃モード設定")]
    public bool enableAttackStance = false; // 攻撃モードの有効化
    public float attackStanceDuration = 10.0f; // 攻撃モードでその場にとどまる時間（秒）
    public float attackDelay = 5.0f; // 攻撃後のディレイ時間（秒）

    private float lastTargetUpdateTime;
    public float targetUpdateInterval = 0.5f; // ターゲット再評価の間隔

    private float lastTeleportTime; // 最後にテレポートした時間を記録
    private bool isEscaping = false; // 逃走中かどうかのフラグ

    [Header("移動方向に応じたスプライト")]
    public Sprite[] directionSprites;
    public bool useDiagonalSprites = false;
    public bool useFlippedSpritesForLeft = false;

    private SpriteRenderer spriteRenderer;
    private Vector2 currentTarget;
    private Camera mainCamera;
    private Transform targetTransform;

    private Vector2 startPosition;
    private float journeyTime;
    private float startTime;

    private Vector2 previousPosition;
    private Vector2 previousDirection; // 前回のスプライト方向を記憶

    private bool inAttackStance = false; // 攻撃モード中かどうか
    private float attackStanceStartTime; // 攻撃モードの開始時間
    private bool inAttackDelay = false; // 攻撃ディレイ中かどうか
    private float attackDelayStartTime; // 攻撃ディレイの開始時間

    private Vector2 escapePosition; //　逃走先の記録

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 最初の目標地点を設定
        SetNewRandomTarget();
        startPosition = transform.position;
        journeyTime = GetRandomJourneyTime();
        startTime = Time.time;

        SetClosestTarget();
        lastTargetUpdateTime = Time.time;
        lastTeleportTime = Time.time; // テレポートの初期時間を設定

        previousPosition = transform.position;
        previousDirection = Vector2.zero; // 初期化
    }

    void Update()
    {
        Vector2 currentPosition = transform.position;
        Vector2 newPosition;

        // 攻撃モードが有効の場合の処理
        if (enableAttackStance)
        {
            // 攻撃モードまたはディレイ中の場合の処理
            if (inAttackStance || inAttackDelay)
            {
                HandleAttackStance(currentPosition);
                if (showDebugInfo) DisplayDebugInfo(); // デバッグ情報の表示
                if (!inAttackDelay)
                {
                    return; // 攻撃モード中は通常の移動処理を行わない
                }
            }
        }

        // 逃走機能が有効かつ逃走中でない場合に逃走をチェック
        if (enableEscape && !isEscaping)
        {
            CheckAndPerformEscape(currentPosition);
        }

        // 逃走中であれば他の移動を行わない
        if (isEscaping)
        {
            PerformEscape(currentPosition);
            return;
        }

        // テレポート機能が有効かつ一定時間経過している場合、テレポートを実行
        if (enableTeleport && Time.time - lastTeleportTime >= teleportInterval)
        {
            PerformTeleport(currentPosition);
            lastTeleportTime = Time.time; // テレポートの実行時間を更新

            // テレポート後はランダム移動のターゲットをリセット
            SetNewRandomTarget();

            // テレポート後は全ての移動処理をスキップ
            return;
        }

        // 一定間隔でターゲットを再評価する
        if (Time.time - lastTargetUpdateTime > targetUpdateInterval)
        {
            SetClosestTarget();
            lastTargetUpdateTime = Time.time;
        }

        // ランダム移動と追従移動を計算
        Vector2 randomMovePosition = CalculateRandomMove(currentPosition);
        Vector2 followMovePosition = CalculateFollowMove(currentPosition);

        // ランダム移動と追従移動の影響を個別に計算して合成
        float followRatio = followWeight / 100f;
        float randomRatio = 1f - followRatio;

        newPosition = (randomMovePosition * randomRatio) + (followMovePosition * followRatio);

        // カメラの範囲内に制限
        newPosition = KeepWithinCameraBounds(newPosition);

        // ユニットを移動
        transform.position = newPosition;

        // スプライトの切り替え
        Vector2 averageDirection = CalculateAverageDirection(previousPosition, currentPosition, newPosition);
        UpdateSpriteBasedOnDirection(averageDirection, directionSprites, spriteRenderer, useDiagonalSprites, useFlippedSpritesForLeft);

        // 前回の位置を更新
        previousPosition = currentPosition;
    }

    /// <summary>
    /// 攻撃範囲内に別タグのユニットがいる場合に逃走するためのチェックを行う関数
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
            // 逃走先の計算
            Vector2 directionAway = (currentPosition - (Vector2)closestThreat.position).normalized;
            float escapeDistance = threatApproachRange * escapeDistanceMultiplier;
            escapePosition = currentPosition + directionAway * escapeDistance; // 逃走先を設定

            if (showDebugInfo)
            {
                Debug.Log($"逃走先計算: 新しい位置 {escapePosition}, 逃走元: {closestThreat.name}");
            }
            // 逃走フラグをセット
            isEscaping = true;
        }
    }

    /// <summary>
    /// 実際に逃走する処理を行う関数
    /// </summary>
    private void PerformEscape(Vector2 currentPosition)
    {
        // 逃走の実行
        Vector2 newEscapePosition = Vector2.MoveTowards(currentPosition, escapePosition, movementSpeed * escapeSpeedMultiplier * Time.deltaTime);

        // 逃走中も画面内に制限
        newEscapePosition = KeepWithinCameraBounds(newEscapePosition);

        // 逃走のスプライト切り替え
        Vector2 escapeDirection = (newEscapePosition - currentPosition).normalized;
        UpdateSpriteBasedOnDirection(escapeDirection, directionSprites, spriteRenderer, useDiagonalSprites, useFlippedSpritesForLeft);

        transform.position = newEscapePosition;

        // 逃走が完了したか確認
        if (Vector2.Distance(transform.position, escapePosition) < 0.1f)
        {
            // 逃走完了後はランダム移動のターゲットをリセット
            SetNewRandomTarget();
            isEscaping = false; // 逃走フラグをリセット
        }
    }

    /// <summary>
    /// テレポート機能を実行する関数
    /// </summary>
    private void PerformTeleport(Vector2 currentPosition)
    {
        // 攻撃中はテレポートしない
        if (inAttackStance) return;

        // ターゲットが設定されていない場合はテレポートしない
        if (targetTransform == null) return;

        Vector2 targetPosition = targetTransform.position;
        Vector2 directionToTarget = (targetPosition - currentPosition).normalized;
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

        // テレポートの距離を計算し、攻撃範囲よりも近づかないようにする
        float teleportDistanceLimited = Mathf.Min(teleportDistance, distanceToTarget - approachRange);

        // テレポート距離が無効（負の値またはゼロ）の場合は終了
        if (teleportDistanceLimited <= 0)
        {
            if (showDebugInfo)
            {
                Debug.Log("テレポートが必要ないか、ターゲットに近すぎるため、テレポートを実行しません。");
            }
            return;
        }

        // テレポート先の位置を計算
        Vector2 teleportPosition = currentPosition + directionToTarget * teleportDistanceLimited;

        // テレポート位置が有効かどうかの確認を追加
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
    /// 攻撃モードを処理する関数
    /// </summary>
    private void HandleAttackStance(Vector2 currentPosition)
    {
        if (targetTransform != null)
        {
            // 攻撃スタンス中の場合
            if (inAttackStance)
            {
                if (Time.time - attackStanceStartTime < attackStanceDuration)
                {
                    // 攻撃モード中はその場で向きを固定
                    return;
                }
                else
                {
                    // 攻撃モード終了、ディレイに移行
                    inAttackStance = false;
                    inAttackDelay = true;
                    attackDelayStartTime = Time.time;
                    return;
                }
            }

            // ディレイ中の場合
            if (inAttackDelay)
            {
                if (Time.time - attackDelayStartTime < attackDelay)
                {
                    // ディレイ中は通常の移動を行う（攻撃しない）
                    return;
                }
                else
                {
                    // ディレイ終了、再び攻撃スタンスのチェックを開始
                    inAttackDelay = false;
                }
            }

            // 攻撃モードやディレイが終了した場合、新たにターゲットを確認
            float distanceToTarget = Vector2.Distance(currentPosition, targetTransform.position);
            if (distanceToTarget <= approachRange)
            {
                // 攻撃範囲内に入った場合、攻撃モードに移行
                inAttackStance = true;
                attackStanceStartTime = Time.time;

                // ターゲットの向きを向く
                Vector2 directionToTarget = (targetTransform.position - (Vector3)currentPosition).normalized;
                previousDirection = directionToTarget; // 現在のターゲット方向を記憶
                UpdateSpriteBasedOnDirection(directionToTarget, directionSprites, spriteRenderer, useDiagonalSprites, useFlippedSpritesForLeft);
            }
        }
    }

    /// <summary>
    /// 追従移動を計算する関数
    /// </summary>
    private Vector2 CalculateFollowMove(Vector2 currentPosition)
    {
        if (targetTransform == null)
        {
            // ターゲットが見つからない場合は現在の位置を返す
            return currentPosition;
        }

        Vector2 targetPosition = targetTransform.position;
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

        // ターゲットがfollowRangeの範囲外にいる場合、追従しない
        if (distanceToTarget > followRange)
        {
            if (showDebugInfo)
            {
                Debug.Log($"ターゲットが追従範囲外です: 距離 = {distanceToTarget}");
            }
            return currentPosition; // 追従しない場合は現在の位置を返す
        }

        // 攻撃範囲内にいる場合、攻撃モードを処理
        HandleAttackStance(currentPosition);

        // ターゲットがapproachRange内にいる場合、近づきすぎないようにする
        if (distanceToTarget <= approachRange)
        {
            if (showDebugInfo)
            {
                Debug.Log($"ターゲットに近づきすぎています: {distanceToTarget}");
            }
            return currentPosition; // ターゲットとの距離が近すぎるので、現在の位置を維持する
        }

        // ターゲットに向かって移動する
        return Vector2.MoveTowards(currentPosition, targetPosition, movementSpeed * Time.deltaTime);
    }

    /// <summary>
    /// ランダム移動を計算する関数
    /// </summary>
    private Vector2 CalculateRandomMove(Vector2 currentPosition)
    {
        float elapsed = Time.time - startTime;
        float t = Mathf.Clamp01(elapsed / journeyTime);
        Vector2 newPosition = Vector2.Lerp(startPosition, currentTarget, t);

        if (t >= 1.0f)
        {
            SetNewRandomTarget();
            startPosition = currentPosition;
            journeyTime = GetRandomJourneyTime();
            startTime = Time.time;
        }

        return newPosition;
    }

    /// <summary>
    /// ランダムな目標地点を設定します。目標地点はカメラの範囲内に制限されます。
    /// </summary>
    private void SetNewRandomTarget()
    {
        Vector2 currentPosition = transform.position;
        float randomDistance = Random.Range(minTargetDistance, maxTargetDistance);
        float randomAngle = Random.Range(0, 360);

        Vector2 newTarget = currentPosition + new Vector2(
            Mathf.Cos(randomAngle * Mathf.Deg2Rad) * randomDistance,
            Mathf.Sin(randomAngle * Mathf.Deg2Rad) * randomDistance
        );

        newTarget = KeepWithinCameraBounds(newTarget);
        currentTarget = newTarget;
    }

/// <summary>
/// ターゲットとの距離を評価し、最も近いターゲットを設定します。
/// </summary>
public void SetClosestTarget()
{
    // ターゲットタグを設定する
    string targetTag = targetSameTag ? gameObject.tag : (gameObject.CompareTag("Enemy") ? "Ally" : "Enemy");
    float closestDistance = Mathf.Infinity;
    Transform closestTarget = null;

    // 指定したタグのターゲットを探索
    foreach (GameObject potentialTarget in GameObject.FindGameObjectsWithTag(targetTag))
    {
        // 自分自身をターゲットにしない
        if (potentialTarget == gameObject) continue;

        float distance = Vector2.Distance(transform.position, potentialTarget.transform.position);
        if (distance < closestDistance)
        {
            closestDistance = distance;
            closestTarget = potentialTarget.transform;
        }
    }

    // ターゲットを設定（見つからなかった場合はnullのまま）
    targetTransform = closestTarget;

    // デバッグ情報を表示
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
    /// ユニットをカメラの範囲内に留めるメソッド
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
        if (sprites.Length < 8)
        {
            Debug.LogWarning("directionSprites 配列には8つのスプライトが必要です。");
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
    /// ランダムな移動時間を取得します。
    /// </summary>
    private float GetRandomJourneyTime()
    {
        return Random.Range(0.5f, 2.0f); // 移動時間の範囲を設定
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
