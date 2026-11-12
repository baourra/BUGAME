using UnityEngine;

public class MisilTeledirigido : MonoBehaviour
{
    [Header("Configuración del Misil")]
    [SerializeField] private float velocidad = 9f;
    [SerializeField] private float velocidadGiro = 300f;
    [SerializeField] private float tiempoVidaMaximo = 3f;

    private Transform objetivo;
    private Rigidbody2D rb;
    private TorretaLanzamisiles lanzadorOriginal;
    private float tiempoActual;
    private bool haExplotado = false;

    public void Inicializar(Transform target, TorretaLanzamisiles lanzador)
    {
        objetivo = target;
        lanzadorOriginal = lanzador;
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true; // Evita que caiga volcado
        }
        tiempoActual = 0f;
    }

    private void FixedUpdate()
    {
        if (haExplotado || rb == null) return;

        tiempoActual += Time.fixedDeltaTime;

        if (tiempoActual >= tiempoVidaMaximo)
        {
            Explotar();
            return;
        }

        if (objetivo != null)
        {
            Vector2 direccion = (objetivo.position - transform.position).normalized;
            float anguloMeta = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg - 90f; // Ajuste si el misil apunta hacia arriba

            float anguloActual = Mathf.MoveTowardsAngle(rb.rotation, anguloMeta, velocidadGiro * Time.fixedDeltaTime);
            rb.MoveRotation(anguloActual);

            // Forzamos el avance hacia donde apunta el misil
            rb.linearVelocity = transform.up * velocidad;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (haExplotado) return;

        if (collision.gameObject.GetComponent<PlayerMov>() != null || collision.gameObject.CompareTag("Suelo"))
        {
            Explotar();
        }
    }

    private void Explotar()
    {
        if (haExplotado) return;
        haExplotado = true;

        if (lanzadorOriginal != null)
        {
            lanzadorOriginal.NotificarExplosionMisil();
        }

        Destroy(gameObject);
    }
}