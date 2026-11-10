using UnityEngine;

public class MovingPlats : MonoBehaviour
{
    public enum EjeMovimiento { Horizontal, Vertical }

    [Header("Configuración de Ruta")]
    [SerializeField] private EjeMovimiento eje = EjeMovimiento.Horizontal;
    [SerializeField] private float distancia = 5f;
    [SerializeField] private float velocidad = 3f;

    private Vector2 posInicial;
    private Vector2 posObjetivo;
    private Vector2 destinoActual;

    private Rigidbody2D rb;

    private void Start()
    {
        posInicial = transform.position;

        Vector2 direccion = (eje == EjeMovimiento.Horizontal) ? Vector2.right : Vector2.up;
        posObjetivo = posInicial + (direccion * distancia);

        destinoActual = posObjetivo;

        // Autoconfigurar el componente físico para sincronizarse con el jugador
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Lo hacemos cinemático para que no le afecte la gravedad, pero mueva a los cuerpos que pise
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // Usamos FixedUpdate para que el movimiento de la plataforma vaya al mismo ritmo que la caída del jugador
    private void FixedUpdate()
    {
        // MovePosition mueve la plataforma usando el motor de colisiones, eliminando el rebote
        Vector2 nuevaPos = Vector2.MoveTowards(rb.position, destinoActual, velocidad * Time.fixedDeltaTime);
        rb.MovePosition(nuevaPos);

        if (Vector2.Distance(rb.position, destinoActual) < 0.01f)
        {
            destinoActual = (destinoActual == posInicial) ? posObjetivo : posInicial;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerMov>() != null)
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerMov>() != null)
        {
            collision.transform.SetParent(null);
        }
    }
}