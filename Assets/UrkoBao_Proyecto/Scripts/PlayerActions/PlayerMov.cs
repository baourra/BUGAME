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
    [SerializeField] private float tiempoBufferSalto = 0.2f;

    [Header("Doble Salto")]
    [SerializeField] private bool habilitarDobleSalto = true;
    [SerializeField] private int maxSaltosExtra = 1;
    [SerializeField] private float fuerzaDobleSalto = 14.5f;
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
    private bool esDobleSalto;

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
                esDobleSalto = false;

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

        enSuelo = Physics2D.OverlapBox(
            sueloCheck.position,
            new Vector2(0.7f, 0.12f),
            0f,
            capaSuelo
        ) != null && rb.linearVelocity.y <= 0.05f;

        if (enSuelo)
        {
            coyote = 0.09f;
            saltosExtraRestantes = maxSaltosExtra;
            esDobleSalto = false;
        }
        else
        {
            coyote = Mathf.Max(0f, coyote - dt);
        }

        Vector2 v = rb.linearVelocity;
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
            esDobleSalto = false;
        }

        if (roofWalkActivo && (!mantieneSalto || !tocandoTecho || enSuelo || tiempoTecho <= 0f))
        {
            roofWalkActivo = false;
        }

        if (roofWalkActivo)
        {
            v.x = Mathf.MoveTowards(v.x, direccion * velocidad, (Mathf.Abs(direccion) > 0.01f ? aceleracionSuelo : frenadoSuelo) * dt);
            v.y = 0f;
            tiempoTecho -= dt;
            if (tiempoTecho <= 0f) roofWalkActivo = false;
        }
        else
        {
            float aceleracion = enSuelo ? (Mathf.Abs(direccion) > 0.01f ? aceleracionSuelo : frenadoSuelo) : controlAire;
            v.x = Mathf.MoveTowards(v.x, direccion * velocidad, aceleracion * dt);
        }

        if (!roofWalkActivo && !enSuelo && tocandoPared && v.y < 0f && !agarreUsado)
        {
            deslizandoPared = true;
            agarreUsado = true;
            tiempoPared = tiempoAgarrePared;
            saltosExtraRestantes = maxSaltosExtra;
            esDobleSalto = false;
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
            esDobleSalto = false;
        }
        else if (!enSuelo && tocandoPared && buffer > 0f)
        {
            v.x = -direccionPared * fuerzaSaltoPared;
            v.y = Gravedad * tiempoCima;

            buffer = 0f;
            deslizandoPared = false;
            coyote = 0f;
            saltosExtraRestantes = maxSaltosExtra;
            esDobleSalto = false;
        }
        else if (deslizandoPared)
        {
            tiempoPared -= dt;
            if (tiempoPared > 0f)
            {
                v.y = Mathf.Max(v.y, -velocidadBajadaPared);
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
            esDobleSalto = true;
        }

        float multiplicador = 1f;

        if (roofWalkActivo)
            multiplicador = 0f;
        else if (v.y > 0f && !mantieneSalto && !esDobleSalto)
            multiplicador = corteSalto;
        else if (!enSuelo && Mathf.Abs(v.y) < 3f)
            multiplicador = 0.6f;
        else if (v.y < 0f)
        {
            multiplicador = gravedadCaida;
            esDobleSalto = false;
        }

        rb.gravityScale = Gravedad * multiplicador / Mathf.Max(0.01f, Mathf.Abs(Physics2D.gravity.y));
        rb.linearVelocity = v;

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

        saltosExtraRestantes = maxSaltosExtra;
    }

    private void OnDrawGizmosSelected()
    {
        if (sueloCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(sueloCheck.position, new Vector2(0.7f, 0.12f));
    }
}