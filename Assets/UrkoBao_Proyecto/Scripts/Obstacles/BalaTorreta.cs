using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BalaTorreta : MonoBehaviour
{
    [Header("Configuracion")]
    public float velocidad = 15f;
    public LayerMask capaSuelo;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        ConfigurarTrailRendererEnCola();
    }

    private void ConfigurarTrailRendererEnCola()
    {
        // Creamos un hijo trasero para que la estela salga desde la parte posterior de la bala
        GameObject objTrail = new GameObject("PuntoTrailBala");
        objTrail.transform.SetParent(transform);

        // Colocamos el emisor hacia atras en el eje X (izquierda local) y centrado en Y (0)
        objTrail.transform.localPosition = new Vector3(-0.4f, 0f, 0f);

        TrailRenderer trail = objTrail.AddComponent<TrailRenderer>();
        trail.time = 0.2f;
        trail.startWidth = 0.25f;
        trail.endWidth = 0.03f;

        // Lo ponemos en orden -1 para que la estela quede por debajo del grafico de la bala
        trail.sortingOrder = -1;

        Shader shader2D = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (shader2D == null) shader2D = Shader.Find("Sprites/Default");
        trail.material = new Material(shader2D);

        Gradient gradiente = new Gradient();
        gradiente.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.yellow, 0f), new GradientColorKey(new Color(1f, 0.4f, 0f), 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        trail.colorGradient = gradiente;
    }

    private void Start()
    {
        rb.linearVelocity = transform.right * velocidad;
        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        PlayerMuerte muerte = col.GetComponent<PlayerMuerte>();
        if (muerte != null)
        {
            muerte.Morir();
            Destroy(gameObject);
        }

        if ((capaSuelo.value & (1 << col.gameObject.layer)) > 0)
        {
            Destroy(gameObject);
        }
    }
}