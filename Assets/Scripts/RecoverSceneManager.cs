using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecoverSceneManager : MonoBehaviour
{
    [SerializeField] private GameObject RotRootObj;
    private float rotateSpeed = 0.3f;
    private bool isRotating = false; // 回転中かどうかを判定するフラグ

    [SerializeField] private List<Image> DisplayImages;
    [SerializeField] private List<Image> WakuImages;
    [SerializeField] private List<Image> FaceImages;
    [SerializeField] private List<TextMeshProUGUI> NameTexts;
    [SerializeField] private List<TextMeshProUGUI> ConditionTexts;
    [SerializeField] private List<Sprite> FaceSprites;

    [SerializeField] private TextMeshProUGUI PriceText;
    [SerializeField] private GameObject InfoPanel;
    [SerializeField] private TextMeshProUGUI InfoText;
    [SerializeField] private GameObject OKButton;
    [SerializeField] private GameObject CancelButton;
    [SerializeField] private TextMeshProUGUI saleText; // セール情報表示用テキスト
    private int Price;
    public StatusLog statusLog; // ステータスログのインスタンスを取得するためのフィールド
    [SerializeField] private ParticleSystem unitGlowParticle; // キャラクターの周りに光らせるパーティクル

    GameManager gameManager;

    void Start()
    {
        gameManager = GameManager.Instance;
        gameManager.UpdateGoldAndExpUI();
        if(gameManager.roomOptions.salePercentage != 0)
        {
            saleText.text = gameManager.roomOptions.salePercentage + "% ON SALE ! ";
            saleText.gameObject.SetActive(true);
        }
        else
        {
            saleText.gameObject.SetActive(false);
        }
        // InfoPanelを非表示にする
        InfoPanel.SetActive(false);
        OKButton.SetActive(false);
        CancelButton.SetActive(false);
        UpdateAlphaValues();
        UpdateAllCharacterInfo();
    }

    // 右ボタンを押した時の処理
    public void OnRightButton()
    {
        if (!isRotating)
        {
            StartCoroutine(RotateOverTime(-72)); // -72度回転
        }
    }

    // 左ボタンを押した時の処理
    public void OnLeftButton()
    {
        if (!isRotating)
        {
            StartCoroutine(RotateOverTime(72)); // 72度回転（逆方向）
        }
    }

    // 回転中にのみアルファ値を更新
    private IEnumerator RotateOverTime(float angle)
    {
        isRotating = true; // 回転中に設定

        float elapsedTime = 0f;
        Quaternion startRotation = RotRootObj.transform.rotation; // 初期回転
        Quaternion endRotation = startRotation * Quaternion.Euler(0, angle, 0); // 回転後の目標角度

        while (elapsedTime < rotateSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotateSpeed;
            RotRootObj.transform.rotation = Quaternion.Slerp(startRotation, endRotation, t); // 回転の補間

            // アルファ値を更新
            UpdateAlphaValues();

            yield return null;
        }
        RotRootObj.transform.rotation = endRotation;
        UpdateAllCharacterInfo(); // キャラクター情報を更新
        isRotating = false; // 回転が完了したのでフラグを解除
    }

    // アルファ値の更新処理
    private void UpdateAlphaValues()
    {
        for (int i = 0; i < DisplayImages.Count; i++)
        {
            float alpha = CalcAlpha(i);

            DisplayImages[i].color = new Color(0.5f, 0.5f, 0.5f, alpha / 2.0f);
            WakuImages[i].color = new Color(0.5f, 0.5f, 0.5f, alpha);
            FaceImages[i].color = new Color(0.8f, 0.8f, 0.8f, alpha);
            NameTexts[i].color = new Color(1, 1, 1, alpha);
            ConditionTexts[i].color = new Color(1, 1, 1, alpha);
        }
    }

    // 指定の回転範囲の時のalpha値を計算する関数
    private float CalcAlpha(float offset)
    {
        float baseAngle = offset * 72f;

        // 回転角度を取得し、基準角度との差を -180 ~ 180 度の範囲に調整
        float yRotation = RotRootObj.transform.rotation.eulerAngles.y;
        float angleDifference = Mathf.DeltaAngle(yRotation, baseAngle);

        // アルファ値の計算
        float alpha = 0.0f;
        if (angleDifference >= 0 && angleDifference <= 72)
        {
            alpha = Mathf.Lerp(1, 0, angleDifference / 72f);
        }
        else if (angleDifference >= -72 && angleDifference < 0)
        {
            alpha = Mathf.Lerp(0, 1, (angleDifference + 72) / 72f);
        }

        return alpha;
    }

    // 全キャラの名前とコンディションと職業とpriceと顔グラフィックを更新
    public void UpdateAllCharacterInfo()
    {
        if (gameManager == null)
        {
            Debug.LogError("GameManagerが見つかりません");
            return;
        }

        for (int i = 0; i < gameManager.livingUnits.Count; i++)
        {
            GameObject unitObj = gameManager.livingUnits[i];
            Unit unit = unitObj.GetComponent<Unit>();
            // 職業と名前を取得
            ItemData.JobListData jobList = gameManager.itemData.jobList.Find(x => x.ID == unit.job);
            NameTexts[i].text = unit.unitName + "\n" + jobList.name;
            // jobに応じて顔グラフィックを変更
            FaceImages[i].sprite = FaceSprites[unit.job];
            // コンディションを取得
            string conditionTx = unit.condition == 0 ? "Normal" : unit.condition == 1 ? "Dead" : unit.condition == 2 ? "Poison" : unit.condition == 3 ? "Bleed" : unit.condition == 4 ? "Stun" : unit.condition == 5 ? "Paralysis" : unit.condition == 6 ? "Weaken" : "DefenceDown";
            float getAlpha = FaceImages[i].color.a;
            if (unit.condition == 1)
            {
                conditionTx = "<color=red>" + conditionTx + "</color>";
                // 顔グラフィックのRGBだけ変更
                FaceImages[i].color = new Color(0.5f, 0.0f, 0.0f, getAlpha);
            }
            else if (unit.condition != 0)
            {
                conditionTx = "<color=yellow>" + conditionTx + "</color>";
                FaceImages[i].color = new Color(0.8f, 0.6f, 0.3f, getAlpha);
            }
            else
            {
                FaceImages[i].color = new Color(0.8f, 0.8f, 0.8f, getAlpha);
            }
            ConditionTexts[i].text = "Condition\n" + conditionTx;
        }
        // Priceを更新
        int index = 0;
        // RotRootObjのRectTransformのY軸の回転角度を四捨五入して表示
        switch (Mathf.Round(RotRootObj.transform.rotation.eulerAngles.y))
        {
            case 0:
                index = 0;
                break;
            case 72:
                index = 1;
                break;
            case 144:
                index = 2;
                break;
            case 216:
                index = 3;
                break;
            case 288:
                index = 4;
                break;
        }
        if (gameManager.livingUnits[index].GetComponent<Unit>().condition == 1)
        {
            Price = gameManager.livingUnits[index].GetComponent<Unit>().currentLevel * 100;
            Price = Price * (100 - gameManager.roomOptions.salePercentage) / 100;
        }
        else if (gameManager.livingUnits[index].GetComponent<Unit>().condition != 0)
        {
            Price = gameManager.livingUnits[index].GetComponent<Unit>().currentLevel * 30;
            Price = Price * (100 - gameManager.roomOptions.salePercentage) / 100;
        }
        else
        {
            Price = 0;
        }
        // statusLogのcurrentGoldよりもPriceが低い場合赤字で表示
        if (statusLog.currentGold < Price)
        {
            PriceText.text = "Price <color=red>" + Price.ToString() + "</color>";
        }
        else
        {
            PriceText.text = "Price " + Price.ToString();
        }
    }

    // OKボタンを押した時の処理
    public void OnOKButton()
    {
        // お金を減らす
        statusLog.currentGold -= Price;

        // unitの周りを光らせるパーティクルを再生
        PlayGlowParticle();

        // アクティブなユニットを取得
        int index = 0;
        // RotRootObjのRectTransformのY軸の回転角度を四捨五入して表示
        switch (Mathf.Round(RotRootObj.transform.rotation.eulerAngles.y))
        {
            case 0:
                index = 0;
                break;
            case 72:
                index = 1;
                break;
            case 144:
                index = 2;
                break;
            case 216:
                index = 3;
                break;
            case 288:
                index = 4;
                break;
        }
        GameObject unitObj = gameManager.livingUnits[index];
        // GoldステータスをUIに反映
        gameManager.UpdateGoldAndExpUI();
        gameManager.RecoverUnit(unitObj);
        // キャラ情報を更新
        UpdateAllCharacterInfo();
        // InfoPanelを非表示にする
        InfoPanel.SetActive(false);
        OKButton.SetActive(false);
        CancelButton.SetActive(false);
    }

    // Cancelボタンを押した時の処理
    public void OnCancelButton()
    {
        // 何もせずにInfoPanelを非表示にする
        InfoPanel.SetActive(false);
        OKButton.SetActive(false);
        CancelButton.SetActive(false);
    }

    // Exitボタンを押した時の処理
    public void OnExitButton()
    {
        // StatusAdjustmentSceneに遷移
        gameManager.LoadScene("StatusAdjustmentScene");
    }

    // 治療(Recover)ボタンを押した時の処理
    public void OnRecoverButton()
    {
        // 回転中の場合は処理を行わない
        if (isRotating) return;

        // 金額が0の場合は処理を行わない
        if (Price == 0)
        {
            InfoPanel.SetActive(false);
            OKButton.SetActive(false);
            CancelButton.SetActive(false);
            return;
        }
        // お金が足りない場合
        else if (Price > statusLog.currentGold)
        {
            InfoPanel.SetActive(true);
            InfoText.text = "Not enough money.";
            OKButton.SetActive(false);
            CancelButton.SetActive(true);
            return;
        }
        // 治療を実行す治療を実行するか確認
        else
        {
            InfoPanel.SetActive(true);
            InfoText.text = "Do you want to recover?";
            OKButton.SetActive(true);
            CancelButton.SetActive(true);
        }
    }

    // キャラクターの周りを光らせるパーティクルを再生
    public void PlayGlowParticle()
    {
        if (unitGlowParticle != null)
        {
            unitGlowParticle.Play();
        }
    }
}
