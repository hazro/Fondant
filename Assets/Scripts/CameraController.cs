using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// カメラのズームとスローモーションを制御するクラス
/// </summary>
public class CameraController : MonoBehaviour
{
    private Camera mainCamera;
    public GameObject whiteoutMask; // ホワイトアウト用マスク
    private SpriteRenderer whiteoutMaskSpriteRenderer; // ホワイトアウト用マスクのスプライトレンダラー
    private float originalOGS; // カメラの元のorthographicSize
    private Vector3 originalPosition; // カメラの元の位置
    
    void Start()
    {
        mainCamera = Camera.main;
        originalOGS = mainCamera.orthographicSize;
        whiteoutMaskSpriteRenderer = whiteoutMask.GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// ズームとスローモーションを開始するコルーチン
    /// </summary>
    /// <param name="zoomSpeed"></param>
    /// <param name="slowMotionDuration"></param>
    /// <param name="targetObject"></param>
    /// <returns>コルーチン</returns>
    public IEnumerator StartZoomAndSlowMotion(float zoomSpeed, float slowMotionDuration, GameObject targetObject)
    {
        // 処理前のカメラの位置とサイズを保存
        originalPosition = mainCamera.transform.position;
        originalOGS = mainCamera.orthographicSize;

        yield return StartCoroutine(ZoomAndSlowCoroutine(zoomSpeed, slowMotionDuration, targetObject));
    }

    /// <summary>
    /// ズームとスローモーションを行うコルーチン
    /// </summary>
    /// <param name="zoomSpeed"></param>
    /// <param name="slowMotionDuration"></param>
    /// <param name="targetObject"></param>
    /// <returns>コルーチン</returns>
    private IEnumerator ZoomAndSlowCoroutine(float zoomSpeed, float slowMotionDuration, GameObject targetObject)
    {
        Debug.Log("敵全滅時のターゲット" + targetObject.name);
        Vector3 targetPosition = targetObject.transform.position;
        float startSize = mainCamera.orthographicSize;
        Vector3 startPosition = mainCamera.transform.position;
        float elapsedTime = 0f;
        float duration = Mathf.Abs(startSize - 1) / zoomSpeed; // ズームと移動にかける時間

        // カメラズームとターゲット中心移動
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            t = t * t * (3f - 2f * t); // イーズインアウトのためのスムースステップ

            // カメラのサイズをイーズインアウトで縮小
            mainCamera.orthographicSize = Mathf.Lerp(startSize, 1, t);

            // カメラの位置をイーズインアウトでターゲットに向かって移動
            mainCamera.transform.position = Vector3.Lerp(startPosition, new Vector3(targetPosition.x, targetPosition.y, mainCamera.transform.position.z), t);

            yield return null;
        }

        // スローモーション
        Time.timeScale = 0.1f;
        yield return new WaitForSecondsRealtime(slowMotionDuration);
        Time.timeScale = 1f;

        // Ef_dieをResourcesから読み込んで生成
        GameObject dieEffect = Resources.Load<GameObject>("Ef_die");
        if (dieEffect != null)
        {
            if(targetObject != null)
            {
                Instantiate(dieEffect, targetObject.transform.position, Quaternion.identity);
            }
        }
        Destroy(targetObject); // 最後の敵消滅

        // ホワイトアウトオブジェジェクトをアクティブにし、Scaleを0に初期化した後、0.5秒かけて10に拡大
        whiteoutMask.SetActive(true);
        whiteoutMask.transform.localScale = Vector3.zero;
        whiteoutMaskSpriteRenderer.color = new Color(1, 0.97f, 0.85f, 0.4f); // 透明にする
        whiteoutMask.transform.localScale = new Vector3(0, 0, 0); // Scaleを0に初期化
        elapsedTime = 0;
        duration = 0.5f;
        while (elapsedTime < duration)
        {
            // whiteoutMaskはtargetObjectの位置に合わせる
            whiteoutMask.transform.position = targetPosition;
            elapsedTime += Time.deltaTime;
            whiteoutMask.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(3, 3, 3), elapsedTime / duration);
            yield return null;
        }
        // alpha1の黒にして、Scaleを10にする
        whiteoutMaskSpriteRenderer.color = new Color(0, 0, 0, 1);
        whiteoutMask.transform.localScale = new Vector3(10, 10, 10);

        // カメラを元の位置とサイズに戻す
        mainCamera.transform.position = originalPosition;
        mainCamera.orthographicSize = originalOGS;
    }
}