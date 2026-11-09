using UnityEngine;

public class Pincho : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Busca el componente de muerte en el objeto que ha entrado en la hitbox
        PlayerMuerte muerte = collision.GetComponent<PlayerMuerte>();

        if (muerte != null)
        {
            muerte.Morir();
        }
    }
}