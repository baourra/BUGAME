using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Importante: Añadimos la libreria del nuevo sistema

public class MenuPausa : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private string nombreEscenaSelector = "LvlSelect";

    private bool estaPausado = false;

    private void Start()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    private void Update()
    {
        // Seguridad: comprobar si hay un teclado conectado para evitar errores
        if (Keyboard.current == null) return;

        // Abrir pausa o ir al selector con Escape
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (estaPausado)
            {
                IrAlSelector();
            }
            else
            {
                Pausar();
            }
        }

        // Atajos de teclado solo disponibles si el juego esta pausado
        if (estaPausado)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Reanudar();
            }
            else if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                ReiniciarNivel();
            }
        }
    }

    public void Pausar()
    {
        estaPausado = true;
        if (panelPausa != null) panelPausa.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Reanudar()
    {
        estaPausado = false;
        if (panelPausa != null) panelPausa.SetActive(false);
        Time.timeScale = 1f;
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlSelector()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaSelector);
    }
}