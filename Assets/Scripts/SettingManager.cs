using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AK.Wwise;

// ゲームの設定画面を管理するクラス
public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; }
    public GameObject SettingPanel;

    [Header("menu")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button exitButton;

    [Header("setting")]
    [SerializeField] private Button setKeyButton;
    [SerializeField] private TMP_Dropdown resolutionList;
    [SerializeField] private Toggle fullScreenToggle;
    [SerializeField] private TMP_Dropdown qualityList;
    [SerializeField] private Toggle vSyncToggle;

    [Header("volume")]
    [SerializeField] private Sprite[] playButtonSprite;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private TextMeshProUGUI masterVolumeValue;
    [SerializeField] private Button masterPlayButton;
    [SerializeField] private Image masterPlayImage;
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private TextMeshProUGUI bgmVolumeValue;
    [SerializeField] private Button bgmPlayButton;
    [SerializeField] private Image bgmPlayImage;
    [SerializeField] private Slider seVolumeSlider;
    [SerializeField] private TextMeshProUGUI seVolumeValue;
    [SerializeField] private Button sePlayButton;
    [SerializeField] private Image sePlayImage;
    [SerializeField] private Slider systemVolumeSlider;
    [SerializeField] private TextMeshProUGUI systemVolumeValue;
    [SerializeField] private Button systemPlayButton;
    [SerializeField] private Image systemPlayImage;

    [Header("RTPCs")]
    public RTPC masterVolumeRTPC;
    public RTPC bgmVolumeRTPC;
    public RTPC seVolumeRTPC;
    public RTPC systemVolumeRTPC;

    private uint playingID = 0; // 再生中のイベントIDを格納

    private SaveSetting settings; // 設定クラスのインスタンス

    // Start is called before the first frame update
    void Start()
    {
        // シングルトンパターンの設定
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SettingPanel.SetActive(false);

        // 設定をロード
        settings = SaveSetting.Load();
        ApplySettingsToUI();

        // ボタンのメソッド割り当て
        exitButton.onClick.AddListener(() => SettingPanel.SetActive(false));
        saveButton.onClick.AddListener(SaveSettings);
        //masterVolumeSlider.onValueChanged.AddListener((value) => masterVolumeRTPC.SetGlobalValue(value * 80));
        //bgmVolumeSlider.onValueChanged.AddListener((value) => bgmVolumeRTPC.SetGlobalValue(value * 100));
        //seVolumeSlider.onValueChanged.AddListener((value) => seVolumeRTPC.SetGlobalValue(value * 100));
        //systemVolumeSlider.onValueChanged.AddListener((value) => systemVolumeRTPC.SetGlobalValue(value * 100));
        masterPlayButton.onClick.AddListener(() => PlayButton(masterPlayImage, masterVolumeSlider, masterVolumeRTPC));
        bgmPlayButton.onClick.AddListener(() => PlayButton(bgmPlayImage, bgmVolumeSlider, bgmVolumeRTPC));
        sePlayButton.onClick.AddListener(() => PlayButton(sePlayImage, seVolumeSlider, seVolumeRTPC));
        systemPlayButton.onClick.AddListener(() => PlayButton(systemPlayImage, systemVolumeSlider, systemVolumeRTPC));
        // スライダーの値変更時にVolumeValueテキストを更新
        masterVolumeSlider.onValueChanged.AddListener((value) => 
        {
            masterVolumeRTPC.SetGlobalValue(value * 80);
            masterVolumeValue.text = Mathf.RoundToInt(value * 100).ToString(); // 0.0～1.0 をパーセントに変換
        });
        bgmVolumeSlider.onValueChanged.AddListener((value) => 
        {
            bgmVolumeRTPC.SetGlobalValue(value * 100);
            bgmVolumeValue.text = Mathf.RoundToInt(value * 100).ToString(); // 0.0～1.0 をパーセントに変換
        });
        seVolumeSlider.onValueChanged.AddListener((value) => 
        {
            seVolumeRTPC.SetGlobalValue(value * 100);
            seVolumeValue.text = Mathf.RoundToInt(value * 100).ToString(); // 0.0～1.0 をパーセントに変換
        });
        systemVolumeSlider.onValueChanged.AddListener((value) => 
        {
            systemVolumeRTPC.SetGlobalValue(value * 100);
            systemVolumeValue.text = Mathf.RoundToInt(value * 100).ToString(); // 0.0～1.0 をパーセントに変換
        });

    }

    /// <summary>
    /// 設定画面を開く (gameManagerから呼び出し)
    /// </summary>
    public void OpenSettingPanel()
    {
        SettingPanel.SetActive(true);
    }

    /// <summary>
    /// ボリュームの再生ボタン
    /// </summary>
    private void PlayButton(Image image, Slider slider, RTPC rtpc)
    {
        if (image.sprite == playButtonSprite[0])
        {
            image.sprite = playButtonSprite[1];
            if (rtpc == bgmVolumeRTPC)
            {
                AkSoundEngine.StopPlayingID(playingID);
                playingID = AkSoundEngine.PostEvent("BGM_BattleSettings", gameObject);
            }
            if (rtpc == systemVolumeRTPC)
            {
                AkSoundEngine.StopPlayingID(playingID);
                playingID = AkSoundEngine.PostEvent("ST_Click", gameObject);
            }
            if (rtpc == seVolumeRTPC)
            {
                AkSoundEngine.StopPlayingID(playingID);
                playingID = AkSoundEngine.PostEvent("SE_Hit", gameObject);
            }
        }
        else
        {
            image.sprite = playButtonSprite[0];
            AkSoundEngine.StopPlayingID(playingID);
        }
    }

    /// <summary>
    /// 閉じるボタンが押されたときに呼び出される関数です。
    /// </summary>
    public void OnExitButtonClicked()
    {
        if (playingID != 0)
        {
            AkSoundEngine.StopPlayingID(playingID);
        }
        SettingPanel.SetActive(false);
    }

    /// <summary>
    /// 設定を保存
    /// </summary>
    public void SaveSettings()
    {
        // UIの値をSaveSettingクラスに反映
        settings.masterVolume = masterVolumeSlider.value;
        settings.bgmVolume = bgmVolumeSlider.value;
        settings.seVolume = seVolumeSlider.value;
        settings.systemVolume = systemVolumeSlider.value;
        settings.resolutionIndex = resolutionList.value;
        settings.fullScreen = fullScreenToggle.isOn;
        settings.quality = qualityList.value;
        settings.vsync = vSyncToggle.isOn;

        // 設定を保存
        settings.Save();
    }

    /// <summary>
    /// ロードした設定をUIに反映
    /// </summary>
    private void ApplySettingsToUI()
    {
        masterVolumeSlider.value = settings.masterVolume;
        bgmVolumeSlider.value = settings.bgmVolume;
        seVolumeSlider.value = settings.seVolume;
        systemVolumeSlider.value = settings.systemVolume;
        resolutionList.value = settings.resolutionIndex;
        fullScreenToggle.isOn = settings.fullScreen;
        qualityList.value = settings.quality;
        vSyncToggle.isOn = settings.vsync;

        // sliderのvalueをパーセントに変換してVolumeValueに反映
        masterVolumeValue.text = Mathf.RoundToInt(settings.masterVolume * 100).ToString();
        bgmVolumeValue.text = Mathf.RoundToInt(settings.bgmVolume * 100).ToString();
        seVolumeValue.text = Mathf.RoundToInt(settings.seVolume * 100).ToString();
        systemVolumeValue.text = Mathf.RoundToInt(settings.systemVolume * 100).ToString();
    }

}
