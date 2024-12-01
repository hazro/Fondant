using UnityEngine;

public class UnitMaterialPassingParam : MonoBehaviour
{
    public int VerticalCount = 3;
    public Color HardLightColor = Color.gray;
    public bool isTeleport = false;
    public float TeleportSpeed = 0.5f;
    public float TeleportOut = 0.0f;

    private Material material;
    public SpriteRenderer materialSpriteRenderer;

    void Start()
    {
        SetMaterial();
        Initialization();
    }

    // マテリアルをセットする
    public void SetMaterial()
    {
        try
        {
            if (materialSpriteRenderer != null)
            {
                if (Application.isPlaying)
                {
                    // materialプロパティを使用してマテリアルをインスタンス化
                    material = materialSpriteRenderer.material;
                }
                else
                {
                    // Prefabの場合はsharedMaterialを使用
                    material = materialSpriteRenderer.sharedMaterial;
                }
            }
            else
            {
                // エラーメッセージを出力しない
                // Debug.LogError("materialSpriteRenderer is not assigned.");
            }
        }
        catch (System.Exception ex)
        {
            // エラーメッセージを出力しない
        }
    }

    // パラメータをセットする
    public void SetParam()
    {
        if (material != null)
        {
            material.SetColor("_HardlightColor", HardLightColor);
            material.SetFloat("_isTeleport", isTeleport ? 1.0f : 0.0f);
            material.SetFloat("_tSpeed", TeleportSpeed);
            material.SetFloat("_tOut", TeleportOut);
        }
    }

    // パラメータに変更があった場合に呼び出される(editor上でのみ実行される)
    #if UNITY_EDITOR
    void OnValidate()
    {
        if (!Application.isPlaying)
        {
            SetMaterial();
            SetParam();
        }
    }
    #endif

    // パラメータの初期化
    public void Initialization()
    {
        HardLightColor = Color.gray;
        isTeleport = false;
        TeleportSpeed = 0.5f;
        TeleportOut = 0.0f;
        SetParam();
    }
}