using UnityEngine;
using UnityEngine.UI;

public class CursorUIController : MonoBehaviour
{
    public Image cursorImage;                 // カーソルとして使うImageコンポーネント
    [Range(0.1f, 2.0f)]
    public float cursorScale = 0.75f;          // カーソルのスケール
    private float previousCursorScale = 0.75f; // 前回のスケール値

    void Start()
    {
        Cursor.visible = false;               // デフォルトのOSカーソルを非表示にする
        UpdateCursorSize(cursorScale);        // 初期カーソルサイズを設定
    }

    void Update()
    {
        // カーソルが表示されている場合は非表示にする
        if(Cursor.visible) Cursor.visible = false;

        // マウス位置にカーソルを追従させる
        Vector3 cursorPosition = Input.mousePosition;
        cursorImage.transform.position = cursorPosition;

        // スライダーでサイズが変更された場合に更新
        if (cursorScale != previousCursorScale)
        {
            UpdateCursorSize(cursorScale);
            previousCursorScale = cursorScale;
        }
    }

    /// <summary>
    /// カーソルのサイズを更新する
    /// </summary>
    /// <param name="scale">カーソルのスケール</param>
    private void UpdateCursorSize(float scale)
    {
        cursorImage.rectTransform.localScale = new Vector3(scale, scale, 1);
    }
}
