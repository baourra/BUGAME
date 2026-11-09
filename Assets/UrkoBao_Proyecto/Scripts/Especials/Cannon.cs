using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("Configuración del Cañón")]
    public float fuerzaDisparo = 100f;

    private void OnTriggerEnter2D(Collider2D col)
    {
        PlayerMov player = col.GetComponent<PlayerMov>();

        if (player != null)
        {
            // Usamos transform.right (eje X) ya que el cañón apunta a la derecha
            player.EnterCannon(transform.right, fuerzaDisparo, transform.position);
        }
    }
}