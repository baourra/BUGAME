using UnityEngine;

public class TorretaLanzamisiles : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject prefabMisil;
    [SerializeField] private Transform transformCannon;
    [SerializeField] private Transform puntoDeDisparo;
    [SerializeField] private LayerMask capaObstaculos;

    [Header("Configuración")]
    [SerializeField] private float rangoVision = 15f;
    [SerializeField] private float velocidadGiroCannon = 10f;
    [SerializeField] private float tiempoRecargaPostExplosion = 2f;

    private Transform jugador;
    private bool puedeDisparar = true;
    private bool misilActivoEnEscena = false;

    private void Start()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
        else
        {
            Debug.LogError("¡La torreta no encuentra al jugador! Asegúrate de que el jugador tiene la Tag 'Player'.");
        }
    }

    private void Update()
    {
        if (jugador == null) return;

        float distancia = Vector2.Distance(transform.position, jugador.position);

        if (distancia <= rangoVision)
        {
            Vector2 direccionAlJugador = (jugador.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionAlJugador, rangoVision, capaObstaculos);

            bool veAlJugador = (hit.collider == null || hit.collider.GetComponent<PlayerMov>() != null);

            if (veAlJugador)
            {
                RotarHaciaJugador(direccionAlJugador);

                if (puedeDisparar && !misilActivoEnEscena)
                {
                    DispararMisil();
                }
            }
        }
    }

    private void RotarHaciaJugador(Vector2 direccion)
    {
        if (transformCannon == null) return;

        float anguloZ = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;

        // CORRECCIÓN: Si tu cañón está dibujado apuntando hacia arriba, restamos 90 grados.
        // (Si al probarlo sigue apuntando de lado, cambia esto a anguloZ ;)
        anguloZ -= 90f;

        Quaternion rotacionObjetivo = Quaternion.Euler(0f, 0f, anguloZ);
        transformCannon.rotation = Quaternion.Slerp(transformCannon.rotation, rotacionObjetivo, velocidadGiroCannon * Time.deltaTime);
    }

    private void DispararMisil()
    {
        if (prefabMisil == null || puntoDeDisparo == null) return;

        misilActivoEnEscena = true;
        puedeDisparar = false;

        GameObject misilInstancia = Instantiate(prefabMisil, puntoDeDisparo.position, puntoDeDisparo.rotation);
        MisilTeledirigido scriptMisil = misilInstancia.GetComponent<MisilTeledirigido>();

        if (scriptMisil != null)
        {
            scriptMisil.Inicializar(jugador, this);
        }
    }

    public void NotificarExplosionMisil()
    {
        misilActivoEnEscena = false;
        Invoke(nameof(HabilitarDisparo), tiempoRecargaPostExplosion);
    }

    private void HabilitarDisparo()
    {
        puedeDisparar = true;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoVision);
    }
}