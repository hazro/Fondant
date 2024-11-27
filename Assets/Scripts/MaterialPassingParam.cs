using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// エディターでも実行する
[ExecuteInEditMode]
public class MaterialPassingParam : MonoBehaviour
{
    public Texture2D MainTex;
    // HDRカラー
    [ColorUsage(true, true)] public Color HDRColor = Color.HSVToRGB(0.78f,0.98f,0.75f) * 4.5f * 2;
    private Material material;

    // Start is called before the first frame update
    void Start()
    {
        // マテリアルを取得
        material = GetComponent<Renderer>().material;
        // マテリアルにテクスチャを設定
        material.SetTexture("_MainTex", MainTex);
        // マテリアルにHDRカラーを設定
        material.SetColor("_Color", HDRColor);
    }

    // Update is called once per frame
    void Update()
    {
        // editorタグ
        #if UNITY_EDITOR
        //HDRColor = Color.HSVToRGB(0.78f,0.98f,0.75f) * 4.5f * 2;
        // マテリアルにテクスチャを設定
        material.SetTexture("_MainTex", MainTex);
        // マテリアルにHDRカラーを設定
        material.SetColor("_Color", HDRColor);
        #endif
        
    }
}
