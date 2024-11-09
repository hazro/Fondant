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
            systemImage.sprite = systemImages[0];
            Debug.Log("Battle Resumed");
        }
        else
        {
            // Updateメソッドを停止
            Time.timeScale = 0;
            systemImage.sprite = systemImages[1];
            Debug.Log("Battle Stopped");
        }

    }

}
