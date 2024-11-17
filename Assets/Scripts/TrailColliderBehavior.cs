using UnityEngine;

/// <summary>
/// トレイル用コライダーの挙動を制御するクラス
/// </summary>
public class TrailColliderBehavior : MonoBehaviour
{
    private ProjectileBehavior projectile; // 親ProjectileBehaviorへの参照

    /// <summary>
    /// 親ProjectileBehaviorを設定する
    /// </summary>
    /// <param name="projectile">ProjectileBehaviorの参照</param>
    public void Initialize(ProjectileBehavior projectile)
    {
        this.projectile = projectile;
    }

    /// <summary>
    /// 接触したオブジェクトを親ProjectileBehaviorに通知する
    /// </summary>
    /// <param name="collision">接触したオブジェクト</param>
    private void OnTriggerStay2D(Collider2D collision)
    {
        Debug.Log("Unitが接触した: " + collision.gameObject.name + "親ProjectileBehavior" + projectile);
        if (projectile != null)
        {
            projectile.OnTrailColliderTriggerStay(this, collision);
        }
    }
}
