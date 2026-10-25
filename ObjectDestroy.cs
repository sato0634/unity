using UnityEngine;

public class ObjectDestroy : MonoBehaviour
{
    [SerializeField] private int pointValue = 10;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ScoreManager.Instance.AddScore(pointValue);
            Destroy(gameObject);
        }
    }
}