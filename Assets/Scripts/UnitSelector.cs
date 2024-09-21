using UnityEngine;
using UnityEngine.EventSystems;

public class UnitSelector : MonoBehaviour, IPointerClickHandler
{
    private IventryUI iventryUI;

    private void Start()
    {
        // GameManagerのインスタンスを取得し、そこからIventryUIを取得
        iventryUI = GameManager.Instance.IventryUI;
        if (iventryUI == null)
        {
            Debug.LogError("IventryUI is not assigned in GameManager.");
        }
    }

    // クリックしたオブジェクトに対して実行されるメソッド
    public void OnPointerClick(PointerEventData eventData)
    {
        //IventryUIのUnitSkillUIを更新するメソッドを実行する
        //TagがAllyの場合
        if (this.gameObject.tag == "Ally")
        {
            iventryUI.UpdateUnitSkillUI(this.gameObject);
        }
    }
}