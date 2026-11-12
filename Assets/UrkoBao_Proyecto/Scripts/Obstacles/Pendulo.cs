using UnityEngine;

public class Pendulo : MonoBehaviour
{
    [Header("Configuración del Péndulo")]
    [SerializeField] private float limiteAngulo = 75f; // Grados máximos hacia la izquierda y derecha
    [SerializeField] private float velocidad = 2.5f;    // Rapidez del balanceo
    [SerializeField] private float desfase = 0f;       // Modifica esto si pones varios juntos para que no se muevan exactamente a la vez

    private float rotacionBase;

    private void Start()
    {
        // Guarda la rotación inicial por si quieres colocar el péndulo inclinado en el nivel
        rotacionBase = transform.eulerAngles.z;
    }

    private void Update()
    {
        // La función Sin crea un bucle perfecto entre -1 y 1. Lo multiplicamos por el ángulo límite.
        float angulo = rotacionBase + Mathf.Sin((Time.time + desfase) * velocidad) * limiteAngulo;

        transform.rotation = Quaternion.Euler(0f, 0f, angulo);
    }
}