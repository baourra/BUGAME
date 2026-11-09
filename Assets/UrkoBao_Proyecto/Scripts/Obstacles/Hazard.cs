using UnityEngine;

public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Busca si el objeto que toca la hitbox tiene el componente PlayerMuerte
        PlayerMuerte muerte = collision.GetComponent<PlayerMuerte>();

        if (muerte != null)
        {
            muerte.Morir();
        }
    }
}