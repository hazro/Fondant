using System.Collections;
using System.Collections.Generic;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityEngine;

// デバッグモードで表示するデバッグページの設定
public sealed class AddEqpDebugPage : DefaultDebugPageBase
{
    GameManager gameManager;
    IventryUI iventryUI;
    protected override string Title { get; } = "Add Eqp Debug Page";

    public override IEnumerator Initialize()
    {
        // Get the GameManager instance.
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            // Get the IventryUI instance.
            iventryUI = gameManager.GetComponent<IventryUI>();
        }
        
         // 装備リストから全装備IDを取得し、プルダウンに表示、選択したIDの装備をイベントリに追加
        foreach (var eqpList in gameManager.itemData.eqpList)
        {
            AddButton(eqpList.name, clicked: () => { 
                // 選択したIDのルーンをイベントリに追加
                iventryUI.AddItem(eqpList.ID);
            });
        }

        yield break;
    }
}