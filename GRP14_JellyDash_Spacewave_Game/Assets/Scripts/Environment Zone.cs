using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnvironmentZone : MonoBehaviour
{
    public string environmentId = "Environment2";

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.SetEnvironment(environmentId);
    }
}