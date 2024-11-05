using System.Collections;
using System.Collections.Generic;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityEngine;

// デバッグモードで表示するデバッグページの設定
public sealed class AddRuneDebugPage : DefaultDebugPageBase
{
    GameManager gameManager;
    IventryUI iventryUI;
    protected override string Title { get; } = "AddRune Debug Page";

    public override IEnumerator Initialize()
    {
        // Get the GameManager instance.
        gameManager = GameManager.Instance;
        if (gameManager != null)
        {
            // Get the IventryUI instance.
            iventryUI = gameManager.GetComponent<IventryUI>();
        }
        
         // ルーンリストから全ルーンIDを取得し、プルダウンに表示、選択したIDのルーンをイベントリに追加
        foreach (var runeList in gameManager.itemData.runeList)
        {
            AddButton(runeList.name, clicked: () => { 
                // 選択したIDのルーンをイベントリに追加
                iventryUI.AddItem(runeList.ID);
            });
        }

        yield break;
    }
}