using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelectorWindow : EditorWindow
{
    private int selectedSceneIndex = 0;
    private List<string> sceneNames = new List<string>();
    private List<string> scenePaths = new List<string>();
    private bool loadTargetSceneOnPlay = false;
    private string targetScenePath;
    private const string gameStartScenePath = "Assets/Scenes/GameStartScene.unity";

    private Vector2 scrollPosition; // スクロール位置

    [MenuItem("Window/Scene Selector")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneSelectorWindow>("Scene Selector");
        window.position = new Rect(100, 100, 250, 450); // UIの縦のサイズを1.5倍に設定
    }

    private void OnEnable()
    {
        LoadScenes();
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
    }

    private void LoadScenes()
    {
        sceneNames.Clear();
        scenePaths.Clear();

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                string path = scene.path;
                string name = System.IO.Path.GetFileNameWithoutExtension(path);
                sceneNames.Add(name);
                scenePaths.Add(path);
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Select Scene to Play:", EditorStyles.boldLabel);

        if (sceneNames.Count > 0)
        {
            selectedSceneIndex = EditorGUILayout.Popup(selectedSceneIndex, sceneNames.ToArray());

            if (GUILayout.Button("Load and Play Scene"))
            {
                if (selectedSceneIndex >= 0 && selectedSceneIndex < scenePaths.Count)
                {
                    OpenStartSceneAndPlayWithSavePrompt();
                }
            }

            // ビルド設定に登録されたシーン一覧をスクロールバー付きで表示
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scenes in Build Settings:", EditorStyles.boldLabel);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));

            for (int i = 0; i < sceneNames.Count; i++)
            {
                if (GUILayout.Button(sceneNames[i], EditorStyles.miniButton))
                {
                    OpenSceneWithSavePrompt(scenePaths[i]);
                }
            }

            EditorGUILayout.EndScrollView();
        }
        else
        {
            EditorGUILayout.HelpBox("Please add scenes to Build Settings", MessageType.Warning);
        }
    }

    private void OpenStartSceneAndPlayWithSavePrompt()
    {
        // 保存確認のポップアップ
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            // "GameStartScene"をロード
            EditorSceneManager.OpenScene(gameStartScenePath);

            // 次に開くシーンのパスを保存し、再生モードが開始されたらロードする
            targetScenePath = scenePaths[selectedSceneIndex];
            loadTargetSceneOnPlay = true;

            // 再生モードに入る
            EditorApplication.isPlaying = true;
        }
        else
        {
            Debug.Log("ユーザーは保存せずに続行を選択しました。");
            EditorSceneManager.OpenScene(gameStartScenePath);
            targetScenePath = scenePaths[selectedSceneIndex];
            loadTargetSceneOnPlay = true;

            EditorApplication.isPlaying = true;
        }
    }

    private void OpenSceneWithSavePrompt(string scenePath)
    {
        if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            EditorSceneManager.OpenScene(scenePath);
        }
        else
        {
            EditorSceneManager.OpenScene(scenePath);
            Debug.Log("User chose to open the scene without saving the current changes.");
        }
    }

    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && loadTargetSceneOnPlay)
        {
            AsyncOperation asyncLoad = EditorSceneManager.LoadSceneAsyncInPlayMode(targetScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            asyncLoad.completed += (operation) =>
            {
                if (asyncLoad.isDone)
                {
                    Debug.Log("Target scene loaded successfully.");
                }
                else
                {
                    Debug.LogError("Failed to load the target scene.");
                }
            };
            loadTargetSceneOnPlay = false;
        }
    }
}
