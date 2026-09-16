using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] private Transform checkpoint;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            transform.position = checkpoint.position;
        }
    }
}