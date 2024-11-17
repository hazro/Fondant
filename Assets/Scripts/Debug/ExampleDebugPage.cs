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
using Tayx.Graphy.Utils.NumString;
//using UnityDebugSheet.Unity;
//using UnityDebugSheet.IngameDebugConsole;
//using UnityDebugSheet.Graphy;


// デバッグモードで表示するデバッグページの設定
public sealed class ExampleDebugPage : DefaultDebugPageBase
{
    GameManager gameManager;
    IventryUI iventryUI;
    protected override string Title { get; } = "Example Debug Page";
    private SelectPlayer selectedPlayer = SelectPlayer.Player02; // 選択されたプレイヤーを格納する変数

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
        AddButton("バトルシーンへの遷移", clicked: () => { gameManager.LoadScene("InToBattleScene"); });

        // Enum Picker
        var enumPickerData1 = new EnumPickerCellModel(SelectPlayer.Player02);
        enumPickerData1.Text = "プレイヤー選択";
        enumPickerData1.Clicked += () => Debug.Log("Clicked");
        enumPickerData1.Confirmed += () => Debug.Log("Picking Page Closed");
        enumPickerData1.ActiveValueChanged += value => 
        {
            selectedPlayer = (SelectPlayer)value;
            Debug.Log($"Selected Option Changed: {selectedPlayer}");
        };
        AddEnumPicker(enumPickerData1);

        // 選択したプレイヤー以外のプレイヤーを非表示にする
        AddButton("選択したプレイヤー以外を非表示", clicked: () => { 
            for (int i = 0; i < gameManager.livingUnits.Count; i++)
            {
                if (i != (int)selectedPlayer)
                {
                    gameManager.livingUnits[i].SetActive(false);
                }
            }
         });
        // 全プレイヤーを表示する
        AddButton("全プレイヤーを表示", clicked: () => { 
            for (int i = 0; i < gameManager.livingUnits.Count; i++)
            {
                gameManager.livingUnits[i].SetActive(true);
            }
         });

        // 選択したプレイヤーをデフォルトの位置に移動
        AddButton("選択したプレイヤーをデフォルト位置に移動", clicked: () => { 
            // 選択したプレイヤーを取得
            GameObject selectedUnit = gameManager.livingUnits[(int)selectedPlayer];
            selectedUnit.transform.position = new Vector3(-2.0f, 0, 0);
         });
         // 選択したプレイヤーを移動&攻撃可能にする
        AddButton("選択したプレイヤーを移動&攻撃可能にする", clicked: () => { 
            GameObject selectedUnit = gameManager.livingUnits[(int)selectedPlayer];
            selectedUnit.GetComponent<UnitController>().enabled = true;
            selectedUnit.GetComponent<AttackController>().enabled = true;
         });
        // 選択したプレイヤーを移動&攻撃不可能にする
        AddButton("選択したプレイヤーを移動&攻撃不可能にする", clicked: () => { 
            GameObject selectedUnit = gameManager.livingUnits[(int)selectedPlayer];
            selectedUnit.GetComponent<UnitController>().enabled = false;
            selectedUnit.GetComponent<AttackController>().enabled = false;
         });
         // すべての敵の移動&攻撃可能にする
        AddButton("すべての敵の移動&攻撃可能にする", clicked: () => { 
            foreach (Transform enemy in gameManager.enemyGroup.transform)
            {
                enemy.GetComponent<UnitController>().enabled = true;
                enemy.GetComponent<AttackController>().enabled = true;
            }
         });
        // すべての敵の移動&攻撃不可能にする
        AddButton("すべての敵の移動&攻撃不可能にする", clicked: () => {
            foreach (Transform enemy in gameManager.enemyGroup.transform)
            {
                enemy.GetComponent<UnitController>().enabled = false;
                enemy.GetComponent<AttackController>().enabled = false;
            }
         });
        // すべてのユニットのHPを回復
        AddButton("すべてのユニット（敵味方）のHPを回復", clicked: () => { 
            foreach (GameObject unit in gameManager.livingUnits)
            {
                unit.GetComponent<Unit>().InitHp();
            }
            foreach (Transform enemy in gameManager.enemyGroup.transform)
            {
                enemy.GetComponent<Unit>().InitHp();
            }
         });
         // すべてのユニットのHpが減少しないようにする
        AddButton("すべてのユニットのHpが減少しない", clicked: () => { 
            foreach (GameObject unit in gameManager.livingUnits)
            {
                unit.GetComponent<Unit>().isNoHpReduction = true;
            }
            // gameManager.enemyGroup.transformの子オブジェクトのUnitコンポーネントのisNoHpReductionをtrueにする
            foreach (Transform enemy in gameManager.enemyGroup.transform)
            {
                enemy.GetComponent<Unit>().isNoHpReduction = true;
            }
         });
         // すべてのユニットのHpが減少するようにする
        AddButton("すべてのユニットのHpが減少する", clicked: () => { 
            foreach (GameObject unit in gameManager.livingUnits)
            {
                unit.GetComponent<Unit>().isNoHpReduction = false;
            }
            foreach (Transform enemy in gameManager.enemyGroup.transform)
            {
                enemy.GetComponent<Unit>().isNoHpReduction = false;
            }
         });
        AddPageLinkButton<AddRuneDebugPage>(nameof(AddRuneDebugPage));
        AddPageLinkButton<AddEqpDebugPage>(nameof(AddEqpDebugPage));
        AddPageLinkButton<AddWpnDebugPage>(nameof(AddWpnDebugPage));
        AddPageLinkButton<DropMonsterDebugPage>(nameof(DropMonsterDebugPage));

        yield break;
    }
    private enum SelectPlayer
    {
        Player01 = 0,
        Player02 = 1,
        Player03 = 2,
        Player04 = 3,
        Player05 = 4
    }
}