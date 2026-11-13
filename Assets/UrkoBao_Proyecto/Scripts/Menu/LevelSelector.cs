using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    // Carga la escena del nivel que le pases desde el boton
    public void CargarNivel(string nombreNivel)
    {
        SceneManager.LoadScene(nombreNivel);
    }

    // Vuelve al menu inicial
    public void VolverAlMenu(string nombreMenu = "MenuInicial")
    {
        SceneManager.LoadScene(nombreMenu);
    }
}