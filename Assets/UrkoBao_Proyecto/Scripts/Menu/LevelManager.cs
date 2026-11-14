using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Interfaz de Victoria")]
    [SerializeField] private GameObject panelVictoria;

    [Header("Navegacion de Escenas")]
    [SerializeField] private string nombreSiguienteNivel = "";
    [SerializeField] private string nombreEscenaSelector = "LvlSelect";

    private bool nivelCompletado = false;

    private void Start()
    {
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(false);
        }
        Time.timeScale = 1f;
    }

    public void CompletarNivel()
    {
        if (nivelCompletado) return;

        nivelCompletado = true;
        Time.timeScale = 0f;

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SiguienteNivel()
    {
        Time.timeScale = 1f;

        // Comprueba que hayas escrito un nombre y que la escena exista en Build Profiles
        if (!string.IsNullOrEmpty(nombreSiguienteNivel) && Application.CanStreamedLevelBeLoaded(nombreSiguienteNivel))
        {
            SceneManager.LoadScene(nombreSiguienteNivel);
        }
        else
        {
            Debug.LogWarning("No se encontro la siguiente escena o el campo esta vacio. Volviendo al selector.");
            IrAlSelector();
        }
    }

    public void IrAlSelector()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaSelector);
    }
}