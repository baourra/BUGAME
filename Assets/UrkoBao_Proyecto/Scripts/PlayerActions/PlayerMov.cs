using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMov : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 15f;
    [SerializeField] private float aceleracionSuelo = 200f;
    [SerializeField] private float frenadoSuelo = 300f;
    [SerializeField] private float controlAire = 130f;

    [Header("Salto")]
    [SerializeField] private float alturaSalto = 5f;
    [SerializeField] private float tiempoCima = 0.4f;
    [SerializeField] private float gravedadCaida = 2f;
    [SerializeField] private float corteSalto = 2.2f;
    [SerializeField] private float tiempoBufferSalto = 0.2f;

    [Header("Doble Salto")]
    [SerializeField] private bool habilitarDobleSalto = true;
    [SerializeField] private int maxSaltosExtra = 1;
    [SerializeField] private float fuerzaDobleSalto = 22f;
    [SerializeField] private bool redireccionInstantanea = true;

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
    private bool pulsarSalto;
    private bool enSuelo;

    private float coyote;
    private float buffer;

    private int saltosExtraRestantes;

    private bool deslizandoPared;
    private bool agarreUsado;
    private float tiempoPared;
    private int direccionPared;

    private bool roofWalkActivo;
    private bool subiendoAntesDeColision;
    private float tiempoTecho;

    private bool enCannon;
    private bool dispararCannon;
    private Transform transformCannon;
    private float fuerzaCannon;

    private float Gravedad => 2f * alturaSalto / (tiempoCima * tiempoCima);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
    }

    private void Update()
    {
        direccion = 0f;
        pulsarSalto = false;
        mantieneSalto = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) direccion--;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) direccion++;

            pulsarSalto = Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame;
            mantieneSalto = Keyboard.current.spaceKey.isPressed || Keyboard.current.wKey.isPressed;
        }

        if (Gamepad.current != null)
        {
            float stick = Gamepad.current.leftStick.x.ReadValue();
            if (Mathf.Abs(stick) > Mathf.Abs(direccion)) direccion = stick;

            pulsarSalto |= Gamepad.current.buttonSouth.wasPressedThisFrame;
            mantieneSalto |= Gamepad.current.buttonSouth.isPressed;
        }

        if (enCannon && pulsarSalto)
        {
            dispararCannon = true;
        }
        else if (pulsarSalto)
        {
            buffer = tiempoBufferSalto;
        }
    }

    private void FixedUpdate()
    {
        if (enCannon)
        {
            if (dispararCannon)
            {
                enCannon = false;
                dispararCannon = false;
                buffer = 0f;

                rb.linearVelocity = transformCannon.right * fuerzaCannon;
                return;
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
                return;
            }
        }

        float dt = Time.fixedDeltaTime;

        // --- SISTEMA RELATIVO DE ENTORNO (Limpio y Matemático) ---
        Vector2 velocidadEntorno = Vector2.zero;
        if (transform.parent != null)
        {
            Rigidbody2D rbPadre = transform.parent.GetComponent<Rigidbody2D>();
            if (rbPadre != null) velocidadEntorno = rbPadre.linearVelocity;
        }

        // Aislamos la velocidad pura del jugador restando el impulso de la plataforma
        Vector2 v = rb.linearVelocity - velocidadEntorno;

        // Ahora podemos saber si cae de verdad respecto a la plataforma, no respecto al mundo
        bool cayendo = v.y <= 0.05f;
        // ---------------------------------------------------------

        enSuelo = Physics2D.OverlapBox(
            sueloCheck.position,
            new Vector2(0.7f, 0.12f),
            0f,
            capaSuelo
        ) != null && cayendo;

        if (enSuelo)
        {
            coyote = 0.09f;
            saltosExtraRestantes = maxSaltosExtra;
        }
        else
        {
            coyote = Mathf.Max(0f, coyote - dt);
        }

        Bounds limites = col.bounds;

        Collider2D colIzq = Physics2D.OverlapBox(new Vector2(limites.min.x - 0.05f, limites.center.y), new Vector2(0.12f, limites.size.y * 0.75f), 0f, capaSuelo);
        Collider2D colDer = Physics2D.OverlapBox(new Vector2(limites.max.x + 0.05f, limites.center.y), new Vector2(0.12f, limites.size.y * 0.75f), 0f, capaSuelo);

        bool tocandoPared = colIzq != null || colDer != null;
        direccionPared = colDer != null ? 1 : -1;

        bool tocandoTecho = Physics2D.OverlapBox(new Vector2(limites.center.x, limites.max.y + 0.005f), new Vector2(limites.size.x * 0.8f, 0.02f), 0f, capaSuelo) != null;

        if (enSuelo || !tocandoPared)
        {
            deslizandoPared = false;
            agarreUsado = false;
        }

        if (!roofWalkActivo && mantieneSalto && !enSuelo && tocandoTecho && (v.y > 0.01f || subiendoAntesDeColision))
        {
            roofWalkActivo = true;
            tiempoTecho = tiempoAgarreTecho;
            buffer = 0f;
            coyote = 0f;
            deslizandoPared = false;
            saltosExtraRestantes = maxSaltosExtra;
        }

        if (roofWalkActivo && (!mantieneSalto || !tocandoTecho || enSuelo || tiempoTecho <= 0f))
        {
            roofWalkActivo = false;
        }

        if (roofWalkActivo)
        {
            if (Mathf.Abs(direccion) < 0.01f) v.x = 0f;
            else v.x = Mathf.MoveTowards(v.x, direccion * velocidad, aceleracionSuelo * dt);

            v.y = 0f;
            tiempoTecho -= dt;
            if (tiempoTecho <= 0f) roofWalkActivo = false;
        }
        else
        {
            float aceleracion = enSuelo ? (Mathf.Abs(direccion) > 0.01f ? aceleracionSuelo : frenadoSuelo) : controlAire;
            v.x = Mathf.MoveTowards(v.x, direccion * velocidad, aceleracion * dt);
        }

        if (!enSuelo && tocandoPared)
        {
            if (colIzq != null && v.x < 0f) v.x = 0f;
            if (colDer != null && v.x > 0f) v.x = 0f;
        }

        // Aquí usamos "cayendo" (relativo) para que deje agarrarse aunque la plataforma suba
        if (!roofWalkActivo && !enSuelo && tocandoPared && cayendo && !agarreUsado)
        {
            deslizandoPared = true;
            agarreUsado = true;
            tiempoPared = tiempoAgarrePared;
            saltosExtraRestantes = maxSaltosExtra;
        }

        if (roofWalkActivo)
        {
            deslizandoPared = false;
        }
        else if (buffer > 0f && coyote > 0f)
        {
            v.y = Gravedad * tiempoCima;
            buffer = 0f;
            coyote = 0f;
            enSuelo = false;
        }
        else if (!enSuelo && tocandoPared && buffer > 0f)
        {
            v.x = -direccionPared * fuerzaSaltoPared;
            v.y = Gravedad * tiempoCima;

            buffer = 0f;
            deslizandoPared = false;
            coyote = 0f;
            saltosExtraRestantes = maxSaltosExtra;
        }
        else if (deslizandoPared)
        {
            tiempoPared -= dt;
            if (tiempoPared > 0f)
            {
                v.y = -velocidadBajadaPared; // Velocidad de caída RELATIVA a la plataforma
            }
            else
            {
                deslizandoPared = false;
            }
        }
        else if (habilitarDobleSalto && buffer > 0f && saltosExtraRestantes > 0)
        {
            v.y = fuerzaDobleSalto;
            if (redireccionInstantanea && Mathf.Abs(direccion) > 0.01f) v.x = direccion * velocidad;
            saltosExtraRestantes--;
            buffer = 0f;
            coyote = 0f;
        }

        float multiplicador = 1f;

        if (roofWalkActivo) multiplicador = 0f;
        else if (v.y > 0f && !mantieneSalto) multiplicador = corteSalto;
        else if (!enSuelo && Mathf.Abs(v.y) < 3f) multiplicador = 0.6f;
        else if (v.y < 0f) multiplicador = gravedadCaida;

        rb.gravityScale = Gravedad * multiplicador / Mathf.Max(0.01f, Mathf.Abs(Physics2D.gravity.y));

        // Sumamos la velocidad del entorno al final. Si saltas, sumarás el momentum de la plataforma (Super fluido)
        rb.linearVelocity = v + velocidadEntorno;

        subiendoAntesDeColision = !roofWalkActivo && v.y > 0.01f;
        buffer = Mathf.Max(0f, buffer - dt);
    }

    public void EnterCannon(Transform cannon, float fuerza, Vector3 posicionCentro)
    {
        if (enCannon) return;

        enCannon = true;
        transformCannon = cannon;
        fuerzaCannon = fuerza;
        transform.position = posicionCentro;

        saltosExtraRestantes = 0;
    }

    private void OnDrawGizmosSelected()
    {
        if (sueloCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(sueloCheck.position, new Vector2(0.7f, 0.12f));
    }
}