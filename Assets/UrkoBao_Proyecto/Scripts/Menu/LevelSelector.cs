using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Añadimos esto para el nuevo sistema de controles

public class SelectorNiveles : MonoBehaviour
{
    [Header("Configuracion")]
    [SerializeField] private string nombreMenuInicial = "MenuInicial";

    private void Update()
    {
        // Comprobamos que haya un teclado conectado por seguridad
        if (Keyboard.current == null) return;

        // Si pulsamos Escape, ejecutamos la funcion de volver
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            VolverAlMenu(nombreMenuInicial);
        }
    }

    // Carga la escena del nivel que le pases desde el boton
    public void CargarNivel(string nombreNivel)
    {
        SceneManager.LoadScene(nombreNivel);
    }

    // Vuelve al menu inicial (se puede llamar desde el boton UI o desde el Update al pulsar Escape)
    public void VolverAlMenu(string nombreMenu)
    {
        SceneManager.LoadScene(nombreMenu);
    }
}