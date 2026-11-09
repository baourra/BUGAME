using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMuerte : MonoBehaviour
{
    [Header("Configuración Muerte")]
    [SerializeField] private float retrasoReaparicion = 0.15f; // Tiempo de espera antes de reiniciar

    private SpriteRenderer spriteRenderer;
    private PlayerMov playerMov;
    private Rigidbody2D rb;
    private bool estaMuerto = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerMov = GetComponent<PlayerMov>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Morir()
    {
        if (estaMuerto) return;
        estaMuerto = true;

        // Feedback visual: Cambiar el sprite a rojo
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
        }

        // Desactivar el control y frenar al personaje al instante
        if (playerMov != null)
        {
            playerMov.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false; // Congela la física durante el impacto
        }

        StartCoroutine(ReiniciarNivel());
    }

    private IEnumerator ReiniciarNivel()
    {
        yield return new WaitForSeconds(retrasoReaparicion);

        // Recarga la escena actual de forma instantánea
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}