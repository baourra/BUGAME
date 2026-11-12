using UnityEngine;

public class Misil : MonoBehaviour
{
    [Header("Configuracion del Misil")]
    [SerializeField] private float velocidad = 9f;
    [SerializeField] private float velocidadGiro = 250f;
    [SerializeField] private float tiempoVidaMaximo = 3f;
    [SerializeField] private float radioExplosion = 2f;
    [SerializeField] private GameObject prefabExplosion;

    private Transform objetivo;
    private Rigidbody2D rb;
    private Lanzamisiles lanzadorOriginal;
    private float tiempoActual;
    private int layerSuelo;
    private bool haExplotado;

    public void Inicializar(Transform target, Lanzamisiles lanzador)
    {
        objetivo = target;
        lanzadorOriginal = lanzador;
        rb = GetComponent<Rigidbody2D>();
        layerSuelo = LayerMask.NameToLayer("Suelo");
        tiempoActual = 0f;

        ConfigurarTrailRendererEnCola();
    }

    private void ConfigurarTrailRendererEnCola()
    {
        // Buscamos si ya existe un objeto llamado "PuntoTrail" en los hijos, si no, lo creamos en la cola
        Transform puntoTrail = transform.Find("PuntoTrail");
        if (puntoTrail == null)
        {
            GameObject objTrail = new GameObject("PuntoTrail");
            objTrail.transform.SetParent(transform);

            // Intentamos localizar el objeto "ColaMisil" para colocarlo ahi exacto, si no, se queda abajo
            Transform cola = transform.Find("MisilCuerpo/ColaMisil");
            if (cola != null)
            {
                objTrail.transform.position = cola.position;
            }
            else
            {
                objTrail.transform.localPosition = new Vector3(0f, -0.8f, 0f); // Ajusta esto si prefieres otra altura
            }
            puntoTrail = objTrail.transform;
        }

        TrailRenderer trail = puntoTrail.GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = puntoTrail.gameObject.AddComponent<TrailRenderer>();
        }

        trail.time = 0.25f;
        trail.startWidth = 0.35f;
        trail.endWidth = 0.05f;
        trail.sortingOrder = 9;

        Shader shader2D = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader2D == null) shader2D = Shader.Find("Sprites/Default");
        trail.material = new Material(shader2D);

        Gradient gradiente = new Gradient();
        gradiente.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.red, 0f), new GradientColorKey(new Color(1f, 0.5f, 0f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        trail.colorGradient = gradiente;
    }

    private void FixedUpdate()
    {
        if (rb == null || haExplotado) return;

        tiempoActual += Time.fixedDeltaTime;
        if (tiempoActual >= tiempoVidaMaximo)
        {
            Explotar();
            return;
        }

        if (objetivo != null)
        {
            Vector3 direccion = (objetivo.position - transform.position).normalized;
            float giro = Vector3.Cross(direccion, transform.up).z;
            rb.angularVelocity = -giro * velocidadGiro;
        }

        rb.linearVelocity = transform.up * velocidad;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (haExplotado) return;

        if (lanzadorOriginal != null && other.transform.IsChildOf(lanzadorOriginal.transform)) return;

        if (other.CompareTag("Player") || other.gameObject.layer == layerSuelo)
        {
            Explotar();
        }
    }

    private void Explotar()
    {
        if (haExplotado) return;
        haExplotado = true;

        if (lanzadorOriginal != null) lanzadorOriginal.NotificarExplosionMisil();

        GameObject fx = prefabExplosion != null
            ? Instantiate(prefabExplosion, transform.position, Quaternion.identity)
            : GenerarExplosionAutomatica();

        CircleCollider2D ondaExpansiva = fx.AddComponent<CircleCollider2D>();
        ondaExpansiva.isTrigger = true;
        ondaExpansiva.radius = radioExplosion;
        fx.AddComponent<Hazard>();

        Destroy(ondaExpansiva, 0.08f);
        Destroy(gameObject);
    }

    private GameObject GenerarExplosionAutomatica()
    {
        GameObject fx = new GameObject("ExplosionFX");
        fx.transform.position = transform.position;

        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.25f;
        main.loop = false;
        main.startLifetime = 0.25f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(radioExplosion * 2f, radioExplosion * 4.5f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.50f, 0f), new Color(1f, 0.85f, 0f));
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 22) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        var renderer = fx.GetComponent<ParticleSystemRenderer>();
        Shader shader2D = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader2D == null) shader2D = Shader.Find("Sprites/Default");
        renderer.material = new Material(shader2D);
        renderer.sortingOrder = 10;

        ps.Play();
        return fx;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radioExplosion);
    }
}