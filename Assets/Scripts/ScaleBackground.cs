using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class ScaleBackground : MonoBehaviour
{
    [SerializeField, Range(0f, 2f)]
    private float verticalScaleMultiplier = 1.0f; // 縦方向のスケール調整用スライダー

    [SerializeField]
    private bool alignToBottom = true; // 画面下部に合わせるかどうか

    private void Update()
    {
        AdjustScaleAndPosition();
    }

    private void AdjustScaleAndPosition()
    {
        // スプライトレンダラーを取得
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

        // カメラのサイズを取得
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("Main Camera is not found.");
            return;
        }

        float screenHeight = 2f * mainCamera.orthographicSize; // カメラの縦の描画範囲
        float screenWidth = screenHeight * mainCamera.aspect;  // カメラの横の描画範囲

        // スプライトのサイズを取得
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        // スプライトのスケールを計算
        float scaleY = (screenHeight / spriteSize.y) * verticalScaleMultiplier; // 高さに合わせるスケール
        float scaleX = screenWidth / spriteSize.x;  // 幅に合わせるスケール

        // スプライトのアスペクト比を保ちながら縦に合わせる
        float scale = Mathf.Max(scaleX, scaleY); // アスペクト比を保つためのスケールを決定

        // Transformにスケールを適用
        transform.localScale = new Vector3(scale, scale, 1);

        if (alignToBottom)
        {
            /*
            // 画像を画面の下に合わせる
            Vector3 cameraBottomPosition = mainCamera.transform.position - new Vector3(0, mainCamera.orthographicSize, 0);
            float spriteBottomOffset = spriteRenderer.bounds.size.y / 2 * transform.localScale.y;

            // 位置を画面の下に揃える
            transform.position = new Vector3(mainCamera.transform.position.x, cameraBottomPosition.y + spriteBottomOffset, transform.position.z);
            */
            // 自身のポジションYを自身のスケールYに合わせる
            transform.position = new Vector3(transform.position.x, transform.localScale.y, transform.position.z);
        }
    }
}
