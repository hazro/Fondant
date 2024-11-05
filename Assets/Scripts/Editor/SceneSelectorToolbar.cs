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

    [MenuItem("Window/Scene Selector")]
    public static void ShowWindow()
    {
        var window = GetWindow<SceneSelectorWindow>("Scene Selector");
        window.position = new Rect(100, 100, 250, 60);
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
                    // 保存確認のポップアップを表示
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        // ユーザーが保存を選択した場合
                        OpenStartSceneAndPlay();
                    }
                    else
                    {
                        // ユーザーが保存せずに再生する場合
                        Debug.Log("ユーザーは保存せずに続行を選択しました。");
                        OpenStartSceneAndPlay();
                    }
                }
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Please add scenes to Build Settings", MessageType.Warning);
        }
    }

    private void OpenStartSceneAndPlay()
    {
        // まず"GameStartScene"をロード
        EditorSceneManager.OpenScene("Assets/Scenes/GameStartScene.unity");

        // 次に開くシーンのパスを保存し、再生モードが開始されたらロードする
        targetScenePath = scenePaths[selectedSceneIndex];
        loadTargetSceneOnPlay = true;

        // 再生モードに入る
        EditorApplication.isPlaying = true;
    }

    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        // 再生モードに入った際に、"GameStartScene"から選択されたシーンに遷移
        if (state == PlayModeStateChange.EnteredPlayMode && loadTargetSceneOnPlay)
        {
            // 選択シーンをロードし、フラグをリセット
            EditorSceneManager.LoadSceneAsyncInPlayMode(targetScenePath, new LoadSceneParameters(LoadSceneMode.Single));
            loadTargetSceneOnPlay = false;
        }
    }
}
