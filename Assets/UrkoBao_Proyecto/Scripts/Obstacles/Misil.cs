using UnityEngine;

public class Misil : MonoBehaviour
{
    [Header("Configuración del Misil")]
    [SerializeField] private float velocidad = 9f;
    [SerializeField] private float velocidadGiro = 250f;
    [SerializeField] private float tiempoVidaMaximo = 3f;
    [SerializeField] private GameObject prefabExplosion; // Opcional: si está vacío, genera partículas automáticas

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

        // Ignora la propia torreta al salir
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

        if (prefabExplosion != null)
        {
            Instantiate(prefabExplosion, transform.position, Quaternion.identity);
        }
        else
        {
            GenerarExplosionAutomatica();
        }

        Destroy(gameObject);
    }

    private void GenerarExplosionAutomatica()
    {
        GameObject fx = new GameObject("ExplosionFX");
        fx.transform.position = transform.position;

        ParticleSystem ps = fx.AddComponent<ParticleSystem>();
        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = ps.main;
        main.duration = 0.25f;
        main.loop = false;
        main.startLifetime = 0.25f;
        main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 10f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.25f, 0.55f);
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.25f, 0f), new Color(1f, 0.75f, 0f));
        main.stopAction = ParticleSystemStopAction.Destroy;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.1f;

        var renderer = fx.GetComponent<ParticleSystemRenderer>();
        Shader shader2D = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader2D == null) shader2D = Shader.Find("Sprites/Default");
        renderer.material = new Material(shader2D);
        renderer.sortingOrder = 10; // Asegura que se dibuje por delante de plataformas y fondo

        ps.Play();
    }
}