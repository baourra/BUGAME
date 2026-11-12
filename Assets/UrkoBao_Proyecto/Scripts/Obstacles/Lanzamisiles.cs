using UnityEngine;

public class Lanzamisiles : MonoBehaviour
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
        if (jugadorObj != null) jugador = jugadorObj.transform;
    }

    private void Update()
    {
        if (jugador == null) return;

        if (Vector2.Distance(transform.position, jugador.position) <= rangoVision)
        {
            Vector2 direccionAlJugador = (jugador.position - transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direccionAlJugador, rangoVision, capaObstaculos);

            if (hit.collider == null)
            {
                RotarHaciaJugador(direccionAlJugador);

                if (puedeDisparar && !misilActivoEnEscena) DispararMisil();
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoVision);
    }
}