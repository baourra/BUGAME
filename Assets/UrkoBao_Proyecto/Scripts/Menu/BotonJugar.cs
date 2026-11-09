using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonJugar : MonoBehaviour
{
    [SerializeField] private string JuegoTests = "JuegoTests";

    public void CargarEscenaJuego()
    {
        SceneManager.LoadScene(JuegoTests);
    }



    public void SalirDelJuego()
    {
        Application.Quit();
    }
}
