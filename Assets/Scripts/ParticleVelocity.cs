using UnityEngine;

/// <summary>
/// パーティクルの速度を設定するコンポーネント
/// </summary>
public class ParticleVelocity : MonoBehaviour
{
    public new ParticleSystem particleSystem;
    public Transform targetTransform;

    private ParticleSystem.VelocityOverLifetimeModule velocityModule;
    private Vector3 previousPosition;

    void Start()
    {
        if (particleSystem == null)
        {
            particleSystem = GetComponent<ParticleSystem>();
        }

        velocityModule = particleSystem.velocityOverLifetime;
        velocityModule.enabled = true;
        previousPosition = targetTransform.position;
    }

    void Update()
    {
        Vector3 deltaPosition = targetTransform.position - previousPosition;
        Vector3 velocity = deltaPosition / Time.deltaTime;

        // パーティクルのVelocityを設定
        velocityModule.x = new ParticleSystem.MinMaxCurve(velocity.x);
        velocityModule.y = new ParticleSystem.MinMaxCurve(velocity.y);
        velocityModule.z = new ParticleSystem.MinMaxCurve(velocity.z);

        previousPosition = targetTransform.position;
    }
}
