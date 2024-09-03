using UnityEngine;

public class RandomWalkerBezier2D : MonoBehaviour
{
    public float moveSpeed = 0.05f; // 移動速度
    public float changeInterval = 2.0f; // 新しい目標地点を設定する間隔
    private Vector2 targetPosition; // 目標地点
    private Vector2 startPoint; // 開始点
    private Vector2 controlPoint1; // 制御点1
    private Vector2 controlPoint2; // 制御点2
    private float t; // ベジェ曲線のパラメータ
    private float changeTimer; // タイマー

    void Start()
    {
        startPoint = transform.position;
        SetNewTargetPosition();
        changeTimer = 0;
    }

    void Update()
    {
        changeTimer += Time.deltaTime;

        if (changeTimer >= changeInterval)
        {
            startPoint = transform.position;
            SetNewTargetPosition();
            changeTimer = 0;
            t = 0;
        }

        // ベジェ曲線に沿って移動
        t += Time.deltaTime * moveSpeed;
        Vector2 position = CalculateBezierPoint(t, startPoint, controlPoint1, controlPoint2, targetPosition);
        transform.position = position;
    }

    // 新しい目標地点を設定
    void SetNewTargetPosition()
    {
        targetPosition = new Vector2(Random.Range(-10.0f, 10.0f), Random.Range(-10.0f, 10.0f));
        controlPoint1 = startPoint + (targetPosition - startPoint) / 3 + Random.insideUnitCircle * 2;
        controlPoint2 = startPoint + 2 * (targetPosition - startPoint) / 3 + Random.insideUnitCircle * 2;
    }

    // ベジェ曲線の計算
    Vector2 CalculateBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
    {
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
