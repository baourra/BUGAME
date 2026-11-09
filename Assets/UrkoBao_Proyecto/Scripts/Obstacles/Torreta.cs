using UnityEngine;

public class Torreta : MonoBehaviour
{
    [Header("Configuración de Torreta")]
    public GameObject prefabBala;
    public Transform puntoDisparo; // El lugar exacto de donde sale la bala
    public float tiempoEntreDisparos = 2f;

    // Sirve para desfasar torretas (que no disparen todas a la vez)
    public float retrasoInicial = 0f;

    private float temporizador;

    private void Start()
    {
        temporizador = tiempoEntreDisparos - retrasoInicial;
    }

    private void Update()
    {
        temporizador -= Time.deltaTime;

        if (temporizador <= 0f)
        {
            Disparar();
            temporizador = tiempoEntreDisparos;
        }
    }

    private void Disparar()
    {
        if (prefabBala != null && puntoDisparo != null)
        {
            // Crea la bala en la posición y con la rotación del Punto de Disparo
            Instantiate(prefabBala, puntoDisparo.position, puntoDisparo.rotation);
        }
    }
}