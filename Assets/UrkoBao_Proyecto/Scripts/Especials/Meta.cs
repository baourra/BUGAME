using UnityEngine;

public class Meta : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            LevelManager manager = FindFirstObjectByType<LevelManager>();
            if (manager != null)
            {
                manager.CompletarNivel();
            }
        }
    }
}