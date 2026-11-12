using UnityEngine;

public class BreakablePlat : MonoBehaviour
{
    public enum EstadoDestruccion { Impecable, Deteriorado, CasiRoto }

    [Header("Configuración")]
    [SerializeField] private EstadoDestruccion estadoActual = EstadoDestruccion.Impecable;
    [SerializeField] private float tiempoRotura = 0.3f;

    [Header("Sprites Visuales")]
    [SerializeField] private Sprite spriteImpecable;
    [SerializeField] private Sprite spriteDeteriorado;
    [SerializeField] private Sprite spriteCasiRoto;

    private SpriteRenderer spriteRenderer;
    private bool esperandoRotura = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ActualizarVisual();
    }

    // OnValidate hace que el sprite cambie en tiempo real en el editor de Unity
    // cuando eliges la fase en el Inspector, sin tener que darle a Play.
    private void OnValidate()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        ActualizarVisual();
    }

    private void ActualizarVisual()
    {
        if (spriteRenderer == null) return;

        switch (estadoActual)
        {
            case EstadoDestruccion.Impecable:
                if (spriteImpecable != null) spriteRenderer.sprite = spriteImpecable;
                break;
            case EstadoDestruccion.Deteriorado:
                if (spriteDeteriorado != null) spriteRenderer.sprite = spriteDeteriorado;
                break;
            case EstadoDestruccion.CasiRoto:
                if (spriteCasiRoto != null) spriteRenderer.sprite = spriteCasiRoto;
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Solo actuamos si es el jugador y la plataforma no está ya a punto de romperse
        if (!esperandoRotura && collision.gameObject.GetComponent<PlayerMov>() != null)
        {
            // Comprobamos que el jugador viene desde arriba (pisándola) y no tocándola por debajo
            if (collision.transform.position.y > transform.position.y)
            {
                AvanzarEstado();
            }
        }
    }

    private void AvanzarEstado()
    {
        if (estadoActual == EstadoDestruccion.Impecable)
        {
            estadoActual = EstadoDestruccion.Deteriorado;
            ActualizarVisual();
        }
        else if (estadoActual == EstadoDestruccion.Deteriorado)
        {
            estadoActual = EstadoDestruccion.CasiRoto;
            ActualizarVisual();
        }
        else if (estadoActual == EstadoDestruccion.CasiRoto)
        {
            esperandoRotura = true;
            // Invoke llama a la función Romper tras el tiempo exacto que le indiques
            Invoke(nameof(Romper), tiempoRotura);
        }
    }

    private void Romper()
    {
        // En lugar de usar Destroy(), la desactivamos. 
        // Es una práctica mucho más limpia por si en el futuro quieres que 
        // las plataformas reaparezcan cuando el jugador muere y reinicia el nivel.
        gameObject.SetActive(false);
    }
}