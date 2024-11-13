using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleStopManager : MonoBehaviour
{
    [SerializeField] private List<Sprite> systemImages = new List<Sprite>();
    [SerializeField] private Image systemImage;
    public void StopResumeBattleButton()
    {
        if (Time.timeScale == 0)
        {
            // Updateメソッドを再開
            Time.timeScale = 1;
            // 敵の移動を再開
            foreach (Transform enemy in GameManager.Instance.enemyGroup.transform)
            {
                enemy.gameObject.GetComponent<UnitController>().isMoving = true;
            }
            // プレイヤーの移動を再開
            foreach (GameObject player in GameManager.Instance.livingUnits)
            {
                player.GetComponent<UnitController>().isMoving = true;
            }
            systemImage.sprite = systemImages[0];
            Debug.Log("Battle Resumed");
        }
        else
        {
            // Updateメソッドを停止
            Time.timeScale = 0;
            // 敵の移動を停止
            foreach (Transform enemy in GameManager.Instance.enemyGroup.transform)
            {
                enemy.gameObject.GetComponent<UnitController>().isMoving = false;
            }
            // プレイヤーの移動を停止
            foreach (GameObject player in GameManager.Instance.livingUnits)
            {
                player.GetComponent<UnitController>().isMoving = false;
            }
            systemImage.sprite = systemImages[1];
            Debug.Log("Battle Stopped");

        }

    }

}
