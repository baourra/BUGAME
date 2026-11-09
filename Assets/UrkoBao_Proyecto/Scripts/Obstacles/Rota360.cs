using UnityEngine;

public class Rota360 : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    public float velocidadRotacion = 100f; // Grados por segundo (positivo = antihorario, negativo = horario)

    private void Update()
    {
        // Gira el objeto en el eje Z constantemente
        transform.Rotate(0f, 0f, velocidadRotacion * Time.deltaTime);
    }
}