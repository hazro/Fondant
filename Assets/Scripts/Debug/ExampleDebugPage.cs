using System.Collections;
using UnityDebugSheet.Runtime.Core.Scripts;
using UnityEngine;

public sealed class ExampleDebugPage : DefaultDebugPageBase
{
    GameManager gameManager;
    protected override string Title { get; } = "Example Debug Page";

    public override IEnumerator Initialize()
    {
        // Get the GameManager instance.
        gameManager = GameManager.Instance;
        // Add a button to this page.
        AddButton("Example Button", clicked: () => { Debug.Log("Clicked"); });
        AddButton("Player02_Move_DefaultPosition", clicked: () => { 
            GameObject P2 = gameManager.livingUnits[1];
            P2.transform.position = new Vector3(-2.0f, 0, 0);
         });
        AddButton("Player02_Move&Attack", clicked: () => { 
            GameObject P2 = gameManager.livingUnits[1];
            P2.GetComponent<UnitController>().enabled = true;
            P2.GetComponent<AttackController>().enabled = true;
         });

        yield break;
    }
}