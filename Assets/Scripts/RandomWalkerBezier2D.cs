using UnityEngine;

/// <summary>
/// ランダムウォーカーのベジェ曲線による移動
/// </summary>
public class RandomWalkerBezier2D : MonoBehaviour
{
    private float moveSpeed = 0.03f; // 移動速度
    private float changeInterval = 3.0f; // 新しい目標地点を設定する間隔
    private Vector2 targetPosition; // 目標地点
    private Vector2 startPoint; // 開始点
    private Vector2 controlPoint1; // 制御点1
    private Vector2 controlPoint2; // 制御点2
    private float t; // ベジェ曲線のパラメータ
    private float changeTimer = 0; // タイマー

    // Start is called before the first frame update
    void Start()
    {
        startPoint = transform.position;
        SetNewTargetPosition();
        changeTimer = 0;
    }

    // Update is called once per frame
    public Vector2 GetBezierPosition(float movementSpeed, Vector2 currentPosition)
    {
        changeTimer += Time.deltaTime;

        if (changeTimer >= changeInterval)
        {
            startPoint = currentPosition;
            SetNewTargetPosition();
            changeTimer = 0;
            t = 0;
        }

        // ベジェ曲線に沿って移動
        t += Time.deltaTime * moveSpeed * movementSpeed;
        Vector2 position = CalculateBezierPoint(t, startPoint, controlPoint1, controlPoint2, targetPosition);
        return position;
    }

    // 新しい目標地点を設定
    public void SetNewTargetPosition()
    {
        targetPosition = new Vector2(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f));
        controlPoint1 = startPoint + (targetPosition - startPoint) / 3 + Random.insideUnitCircle * 2;
        controlPoint2 = startPoint + 2 * (targetPosition - startPoint) / 3 + Random.insideUnitCircle * 2;
    }

    // ベジェ曲線の計算
    Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
        changeTimer += Time.deltaTime;
        
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector2 p = uuu * p0; // 初期点の寄与
        p += 3 * uu * t * p1; // 制御点1の寄与
        p += 3 * u * tt * p2; // 制御点2の寄与
        p += ttt * p3; // 終点の寄与

        return p;
    }
}
