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

        // Autoconfigurar el componente físico
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // Fundamental que sea Kinematic para que aplique fuerza pero no caiga
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void FixedUpdate()
    {
        // Aplicamos velocidad real al Rigidbody para que PlayerMov pueda leerla matemáticamente
        Vector2 direccionHaciaObjetivo = (destinoActual - rb.position).normalized;
        rb.linearVelocity = direccionHaciaObjetivo * velocidad;

        // Comprobamos la distancia de forma precisa para evitar desincronizaciones y pasarnos de largo
        float distanciaAlObjetivo = Vector2.Distance(rb.position, destinoActual);
        if (distanciaAlObjetivo < (velocidad * Time.fixedDeltaTime))
        {
            // Clavamos la posición para no derrapar, y cambiamos de objetivo
            rb.position = destinoActual;
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