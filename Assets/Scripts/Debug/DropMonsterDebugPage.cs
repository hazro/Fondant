using System.Collections;
using System.Collections.Generic;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityEngine;

// デバッグモードで表示するデバッグページの設定
public sealed class DropMonsterDebugPage : DefaultDebugPageBase
{
    GameManager gameManager;
    IventryUI iventryUI;
    private Transform enemyGroup; // エネミーグループのTransform
    protected override string Title { get; } = "Drop Monster Debug Page";

    public override IEnumerator Initialize()
    {
        // Get the GameManager instance.
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            // Get the IventryUI instance.
            iventryUI = gameManager.GetComponent<IventryUI>();
        }
        
         // リストから全IDを取得し、プルダウンに表示、選択したIDを追加
        foreach (var enemyList in gameManager.itemData.enemyList)
        {
            AddButton(enemyList.name, clicked: () => { 
                CheckEnemyGroup();
                GameObject enemyPrefab = Resources.Load<GameObject>("Prefabs/Unit/Enemy/Enemy" + enemyList.ID.ToString("00"));
                float randomX = Random.Range(1.5f, 4.5f);
                float randomY = Random.Range(-2.0f, 2.0f);
                GameObject enemy = Instantiate(enemyPrefab, new Vector3(randomX,randomY,-0.1f), Quaternion.identity, enemyGroup);
                enemy.transform.SetParent(enemyGroup);
                gameManager.enemyCount++;
            });
        }

        yield break;
    }

    /// <summary>
    /// シーンにEnemyGroupががあるか確認して、なければ作成する関数です。
    /// </summary>
    private void CheckEnemyGroup()
    {
        enemyGroup = GameObject.Find("EnemyGroup")?.transform;
        if (enemyGroup == null)
        {
            GameObject enemyGroupObj = new GameObject("EnemyGroup");
            enemyGroup = enemyGroupObj.transform;
            // gameManagerのenemyGroupにenemyGroupを設定
            gameManager.enemyGroup = enemyGroupObj;

            // 次のシーンに持っていくために、DontDestroyOnLoadを設定
            DontDestroyOnLoad(enemyGroupObj);
        }
    }
}