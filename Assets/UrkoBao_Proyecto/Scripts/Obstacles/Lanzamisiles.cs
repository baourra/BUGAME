using UnityEngine;

public class Lanzamisiles : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject prefabMisil;
    [SerializeField] private Transform transformCannon;
    [SerializeField] private Transform puntoDeDisparo;
    [SerializeField] private LayerMask capaObstaculos;

    [Header("Configuracion de Vision y Grados")]
    [SerializeField] private float rangoVision = 15f;
    [Range(0f, 360f)]
    [SerializeField] private float anguloVisionTotal = 360f; // 360 para apuntar a todo alrededor, o menor (ej. 180) para un arco frontal
    [SerializeField] private float velocidadGiroCannon = 10f;
    [SerializeField] private float tiempoRecargaPostExplosion = 2f;

    private Transform jugador;
    private bool puedeDisparar = true;
    private bool misilActivoEnEscena = false;

    private void Start()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null) jugador = jugadorObj.transform;
    }

    private void Update()
    {
        if (jugador == null) return;

        Vector2 vectorAlJugador = (Vector2)jugador.position - (Vector2)transform.position;
        float distancia = vectorAlJugador.magnitude;

        if (distancia <= rangoVision)
        {
            // Comprobamos si esta dentro del angulo de vision permitido
            Vector2 direccionAlJugador = vectorAlJugador.normalized;

            // Calculamos el angulo respecto hacia donde apunta la torreta (por defecto hacia arriba transform.up)
            float anguloConFrente = Vector2.Angle(transform.up, direccionAlJugador);

            if (anguloConFrente <= anguloVisionTotal / 2f)
            {
                // Comprobamos linea de vision (obstaculos)
                RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionAlJugador, distancia, capaObstaculos);

                if (hit.collider == null)
                {
                    RotarHaciaJugador(direccionAlJugador);

                    if (puedeDisparar && !misilActivoEnEscena) DispararMisil();
                }
            }
        }
    }

    private void RotarHaciaJugador(Vector2 direccion)
    {
        if (transformCannon == null) return;

        float anguloZ = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg - 90f;
        Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, anguloZ);
        transformCannon.rotation = Quaternion.Slerp(transformCannon.rotation, rotacionObjetivo, velocidadGiroCannon * Time.deltaTime);
    }

    private void DispararMisil()
    {
        if (prefabMisil == null || puntoDeDisparo == null) return;

        misilActivoEnEscena = true;
        puedeDisparar = false;

        GameObject misilInstancia = Instantiate(prefabMisil, puntoDeDisparo.position, puntoDeDisparo.rotation);
        misilInstancia.GetComponent<Misil>()?.Inicializar(jugador, this);
    }

    public void NotificarExplosionMisil()
    {
        misilActivoEnEscena = false;
        Invoke(nameof(HabilitarDisparo), tiempoRecargaPostExplosion);
    }

    private void HabilitarDisparo() => puedeDisparar = true;

    private void OnDrawGizmosSelected()
    {
        // Dibuja el radio de alcance maximo en amarillo
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoVision);

        // Dibuja el cono de vision en la escena para ver exactamente que grados abarca
        Gizmos.color = Color.cyan;
        int segmentos = 20;
        float medioAngulo = anguloVisionTotal / 2f;
        Quaternion rotIzq = Quaternion.Euler(0, 0, -medioAngulo);
        Quaternion rotDer = Quaternion.Euler(0, 0, medioAngulo);

        Vector3 dirIzq = rotIzq * transform.up * rangoVision;
        Vector3 dirDer = rotDer * transform.up * rangoVision;

        Gizmos.DrawLine(transform.position, transform.position + dirIzq);
        Gizmos.DrawLine(transform.position, transform.position + dirDer);
    }
}