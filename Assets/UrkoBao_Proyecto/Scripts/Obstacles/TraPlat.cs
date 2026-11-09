using UnityEngine;

public class PlaTrap : MonoBehaviour
{
    [Header("Configuración de Tiempos")]
    [SerializeField] private float tiempoLimite = 2f;

    [Header("Colores")]
    [SerializeField] private Color colorAdvertenciaInicio = Color.yellow; // Color al pisar
    [SerializeField] private Color colorAdvertenciaFin = new Color(1f, 0.4f, 0f); // Color justo antes de morir (Naranja)
    [SerializeField] private Color colorPeligro = Color.red; // Cambio brusco instantáneo al matar

    private SpriteRenderer spriteRenderer;
    private Color colorOriginal;

    private float temporizador = 0f;
    private bool jugadorEnContacto = false;
    private bool esHazard = false;
    private PlayerMuerte playerMuerteRef;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            colorOriginal = spriteRenderer.color;
        }
    }

    private void Update()
    {
        if (jugadorEnContacto && !esHazard)
        {
            temporizador += Time.deltaTime;

            float progreso = Mathf.Clamp01(temporizador / tiempoLimite);

            // Transición progresiva para mostrar visualmente el tiempo restante
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(colorAdvertenciaInicio, colorAdvertenciaFin, progreso);
            }

            // Al cumplirse el tiempo limite, salta al color mortal y activa la muerte
            if (temporizador >= tiempoLimite)
            {
                esHazard = true;

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = colorPeligro;
                }

                if (playerMuerteRef != null)
                {
                    playerMuerteRef.Morir();
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerMuerte muerte = collision.gameObject.GetComponent<PlayerMuerte>();

        if (muerte != null)
        {
            jugadorEnContacto = true;
            playerMuerteRef = muerte;

            if (esHazard)
            {
                muerte.Morir();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<PlayerMuerte>() != null)
        {
            jugadorEnContacto = false;
            playerMuerteRef = null;

            // Si el jugador salta a tiempo, se limpia el contador y vuelve al color original
            if (!esHazard)
            {
                temporizador = 0f;
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = colorOriginal;
                }
            }
        }
    }
}