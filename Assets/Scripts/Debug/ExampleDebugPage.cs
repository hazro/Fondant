using System.Collections;
using System.Collections.Generic;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityEngine;
using UnityDebugSheet.Runtime.Extensions.Unity;
using IngameDebugConsole;
using Tayx.Graphy;
//using UnityDebugSheet.Runtime.Core.Scripts;
using UnityDebugSheet.Runtime.Core.Scripts.DefaultImpl.Cells;
using UnityDebugSheet.Runtime.Extensions.IngameDebugConsole;
using UnityDebugSheet.Runtime.Extensions.Graphy;
//using UnityDebugSheet.Unity;
//using UnityDebugSheet.IngameDebugConsole;
//using UnityDebugSheet.Graphy;


// デバッグモードで表示するデバッグページの設定
public sealed class ExampleDebugPage : DefaultDebugPageBase
{
    GameManager gameManager;
    IventryUI iventryUI;
    protected override string Title { get; } = "Example Debug Page";

    public override IEnumerator Initialize()
    {
        // Get the GameManager instance.
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            // Get the IventryUI instance.
            iventryUI = gameManager.GetComponent<IventryUI>();
        }

        // システム情報ページへのリンクボタンを追加
        AddPageLinkButton<SystemInfoDebugPage>("System Info");
        // インゲームデバッグコンソールページへのリンクボタンを追加
        AddPageLinkButton<IngameDebugConsoleDebugPage>("In-Game Debug Console", onLoad: x => x.page.Setup(DebugLogManager.Instance));
        // Graphyデバッグページへのリンクボタンを追加
        AddPageLinkButton<GraphyDebugPage>("Graphy", onLoad: x => x.page.Setup(GraphyManager.Instance));

        //AddButton("Example Button", clicked: () => { Debug.Log("Clicked"); });
        // バトルシーンへの遷移ボタンを追加
        AddButton("Battle Scene", clicked: () => { gameManager.LoadScene("InToBattleScene"); });
        // プレイヤー02をデフォルトの位置に移動
        AddButton("Player02_Move_DefaultPosition", clicked: () => { 
            GameObject P2 = gameManager.livingUnits[1];
            P2.transform.position = new Vector3(-2.0f, 0, 0);
         });
         // プレイヤー02を移動&攻撃可能にする
        AddButton("Player02_Move&Attack", clicked: () => { 
            GameObject P2 = gameManager.livingUnits[1];
            P2.GetComponent<UnitController>().enabled = true;
            P2.GetComponent<AttackController>().enabled = true;
         });
        // プレイヤー02を移動&攻撃不可能にする
        AddButton("Player02_Move&Attack_Disable", clicked: () => { 
            GameObject P2 = gameManager.livingUnits[1];
            P2.GetComponent<UnitController>().enabled = false;
            P2.GetComponent<AttackController>().enabled = false;
         });
        // プレイヤー02のHPを回復
        AddButton("Player02_Recover_HP", clicked: () => { 
            GameObject P2 = gameManager.livingUnits[1];
            P2.GetComponent<Unit>().InitHp();
         });
        AddPageLinkButton<AddRuneDebugPage>(nameof(AddRuneDebugPage));
        AddPageLinkButton<AddEqpDebugPage>(nameof(AddEqpDebugPage));
        AddPageLinkButton<AddWpnDebugPage>(nameof(AddWpnDebugPage));
        AddPageLinkButton<DropMonsterDebugPage>(nameof(DropMonsterDebugPage));
        // EnemyGroup内のエネミーのHpを回復
        AddButton("Recover Enemy HP", clicked: () => { 
            Transform enemyGroup = GameObject.Find("EnemyGroup")?.transform;
            if (enemyGroup != null)
            {
                foreach (Transform enemy in enemyGroup)
                {
                    enemy.GetComponent<Unit>().InitHp();
                }
            }
         });

        yield break;
    }
}