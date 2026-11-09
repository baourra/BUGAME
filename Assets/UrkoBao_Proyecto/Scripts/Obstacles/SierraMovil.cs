using UnityEngine;

public class SierraMovil : MonoBehaviour
{
    [Header("Ruta")]
    public Transform[] puntosDeRuta;
    public float velocidadMovimiento = 5f;

    [Header("Visual")]
    public float velocidadGiroSprite = 300f; // Para que la sierra ruede visualmente
    public Transform spriteSierra; // Asigna aquí el objeto hijo que tiene el dibujo de la sierra

    private int indicePuntoActual = 0;

    private void Update()
    {
        if (puntosDeRuta.Length == 0) return;

        // 1. Mover físicamente la sierra hacia el punto actual
        Transform objetivo = puntosDeRuta[indicePuntoActual];
        transform.position = Vector2.MoveTowards(transform.position, objetivo.position, velocidadMovimiento * Time.deltaTime);

        // 2. Si ha llegado al punto exacto, cambiar al siguiente
        if (Vector2.Distance(transform.position, objetivo.position) < 0.01f)
        {
            indicePuntoActual++;

            // Si llega al final de la lista, vuelve al punto 0 (bucle continuo)
            if (indicePuntoActual >= puntosDeRuta.Length)
            {
                indicePuntoActual = 0;
            }
        }

        // 3. Rotar visualmente el sprite de la sierra
        if (spriteSierra != null)
        {
            spriteSierra.Rotate(0f, 0f, velocidadGiroSprite * Time.deltaTime);
        }
    }
}