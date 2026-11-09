
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMov : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 10f;
    [SerializeField] private float aceleracionSuelo = 110f;
    [SerializeField] private float frenadoSuelo = 230f;
    [SerializeField] private float controlAire = 45f;

    [Header("Salto")]
    [SerializeField] private float alturaSalto = 3.2f;
    [SerializeField] private float tiempoCima = 0.43f;
    [SerializeField] private float gravedadCaida = 1.15f;
    [SerializeField] private float corteSalto = 2.2f;

    [Header("Pared")]
    [SerializeField] private float tiempoAgarrePared = 0.25f;
    [SerializeField] private float velocidadBajadaPared = 2.5f;
    [SerializeField] private float fuerzaSaltoPared = 10f;

    [Header("RoofWalk")]
    [SerializeField] private float tiempoAgarreTecho = 1.2f;

    [Header("Suelo")]
    [SerializeField] private Transform sueloCheck;
    [SerializeField] private LayerMask capaSuelo;

    private Rigidbody2D rb;
    private Collider2D col;

    private float direccion;
    private bool mantieneSalto;
    private bool enSuelo;

    private float coyote;
    private float buffer;

    private bool deslizandoPared;
    private bool agarreUsado;
    private float tiempoPared;
    private int direccionPared;

    private bool roofWalkActivo;
    private bool subiendoAntesDeColision;
    private float tiempoTecho;

    private float Gravedad =>
        2f * alturaSalto / (tiempoCima * tiempoCima);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        direccion = 0f;
        bool pulsarSalto = false;
        mantieneSalto = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
                direccion--;

            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
                direccion++;

            pulsarSalto =
                Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.wKey.wasPressedThisFrame;

            mantieneSalto =
                Keyboard.current.spaceKey.isPressed ||
                Keyboard.current.wKey.isPressed;
        }

        if (Gamepad.current != null)
        {
            float stick = Gamepad.current.leftStick.x.ReadValue();

            if (Mathf.Abs(stick) > Mathf.Abs(direccion))
                direccion = stick;

            pulsarSalto |= Gamepad.current.buttonSouth.wasPressedThisFrame;
            mantieneSalto |= Gamepad.current.buttonSouth.isPressed;
        }

        if (pulsarSalto)
            buffer = 0.2f;
    }

    private void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        enSuelo = Physics2D.OverlapBox(
            sueloCheck.position,
            new Vector2(0.7f, 0.12f),
            0f,
            capaSuelo
        ) != null && rb.linearVelocity.y <= 0.05f;

        coyote = enSuelo ? 0.09f : Mathf.Max(0f, coyote - dt);

        Vector2 v = rb.linearVelocity;
        Bounds limites = col.bounds;

        // Detectar paredes
        bool paredIzquierda = Physics2D.OverlapBox(
            new Vector2(limites.min.x - 0.05f, limites.center.y),
            new Vector2(0.12f, limites.size.y * 0.75f),
            0f,
            capaSuelo
        ) != null;

        bool paredDerecha = Physics2D.OverlapBox(
            new Vector2(limites.max.x + 0.05f, limites.center.y),
            new Vector2(0.12f, limites.size.y * 0.75f),
            0f,
            capaSuelo
        ) != null;

        bool tocandoPared = paredIzquierda || paredDerecha;
        direccionPared = paredDerecha ? 1 : -1;

        // Detectar techo
        bool tocandoTecho = Physics2D.OverlapBox(
            new Vector2(limites.center.x, limites.max.y + 0.005f),
            new Vector2(limites.size.x * 0.8f, 0.02f),
            0f,
            capaSuelo
        ) != null;

        // Reiniciar agarre de pared al separarse
        if (enSuelo || !tocandoPared)
        {
            deslizandoPared = false;
            agarreUsado = false;
        }

        // Activar RoofWalk al tocar un techo mientras subimos
        if (!roofWalkActivo &&
            mantieneSalto &&
            !enSuelo &&
            tocandoTecho &&
            (v.y > 0.01f || subiendoAntesDeColision))
        {
            roofWalkActivo = true;
            tiempoTecho = tiempoAgarreTecho;
            buffer = 0f;
            coyote = 0f;
            deslizandoPared = false;
        }

        // Comprobar si debemos soltar el techo
        if (roofWalkActivo &&
            (!mantieneSalto || !tocandoTecho ||
             enSuelo || tiempoTecho <= 0f))
        {
            roofWalkActivo = false;
        }

        // Movimiento horizontal
        if (roofWalkActivo)
        {
            v.x = Mathf.MoveTowards(
                v.x,
                direccion * velocidad,
                (Mathf.Abs(direccion) > 0.01f
                    ? aceleracionSuelo
                    : frenadoSuelo) * dt
            );

            v.y = 0f;
            tiempoTecho -= dt;

            if (tiempoTecho <= 0f)
                roofWalkActivo = false;
        }
        else
        {
            float aceleracion = enSuelo
                ? (Mathf.Abs(direccion) > 0.01f
                    ? aceleracionSuelo
                    : frenadoSuelo)
                : controlAire;

            v.x = Mathf.MoveTowards(
                v.x,
                direccion * velocidad,
                aceleracion * dt
            );
        }

        // Wall Slide
        if (!roofWalkActivo &&
            !enSuelo &&
            tocandoPared &&
            v.y < 0f &&
            !agarreUsado)
        {
            deslizandoPared = true;
            agarreUsado = true;
            tiempoPared = tiempoAgarrePared;
        }

        // Wall Jump
        if (roofWalkActivo)
        {
            deslizandoPared = false;
        }
        else if (deslizandoPared)
        {
            tiempoPared -= dt;

            if (tiempoPared > 0f)
            {
                v.y = Mathf.Max(v.y, -velocidadBajadaPared);

                if (buffer > 0f)
                {
                    v.x = -direccionPared * fuerzaSaltoPared;
                    v.y = Gravedad * tiempoCima;

                    buffer = 0f;
                    deslizandoPared = false;
                    coyote = 0f;
                }
            }
            else
            {
                deslizandoPared = false;
            }
        }
        else if (!roofWalkActivo && buffer > 0f && coyote > 0f)
        {
            // Salto normal
            v.y = Gravedad * tiempoCima;
            buffer = 0f;
            coyote = 0f;
            enSuelo = false;
        }

        // Gravedad
        float multiplicador = 1f;

        if (roofWalkActivo)
            multiplicador = 0f;
        else if (v.y > 0f && !mantieneSalto)
            multiplicador = corteSalto;
        else if (!enSuelo && Mathf.Abs(v.y) < 3f)
            multiplicador = 0.6f;
        else if (v.y < 0f)
            multiplicador = gravedadCaida;

        rb.gravityScale = Gravedad * multiplicador /
            Mathf.Max(0.01f, Mathf.Abs(Physics2D.gravity.y));

        rb.linearVelocity = v;

        subiendoAntesDeColision = !roofWalkActivo && v.y > 0.01f;

        buffer = Mathf.Max(0f, buffer - dt);
    }

    private void OnDrawGizmosSelected()
    {
        if (sueloCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            sueloCheck.position,
            new Vector2(0.7f, 0.12f)
        );
    }
}
