using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicial : MonoBehaviour
{
    [SerializeField] private string nombreEscenaSelector = "LvlSelect";

    public void IrAlSelector()
    {
        SceneManager.LoadScene(nombreEscenaSelector);
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Detiene el juego en el editor
#else
        Application.Quit(); // Cierra el juego de verdad en el .exe final
#endif
    }
}