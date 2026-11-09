using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BalaTorreta : MonoBehaviour
{
    [Header("Configuración")]
    public float velocidad = 15f;
    public LayerMask capaSuelo; // Aquí asignarás tu layer "Suelo"

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Lo hacemos cinemático para que la gravedad no le afecte, pero siga detectando colisiones
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void Start()
    {
        // La bala avanza en la dirección "Derecha" local (transform.right)
        rb.linearVelocity = transform.right * velocidad;

        // Autodestrucción de seguridad por si sale de la pantalla
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // 1. Si toca al jugador, lo mata
        PlayerMuerte muerte = col.GetComponent<PlayerMuerte>();
        if (muerte != null)
        {
            muerte.Morir();
        }

        // 2. Si toca una pared o el suelo, se destruye (compara la layer del objeto tocado)
        if ((capaSuelo.value & (1 << col.gameObject.layer)) > 0)
        {
            Destroy(gameObject);
        }
    }
}