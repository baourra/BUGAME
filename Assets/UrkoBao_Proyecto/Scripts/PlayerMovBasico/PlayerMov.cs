
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

    private float Gravedad => 2f * alturaSalto / (tiempoCima * tiempoCima);

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
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                direccion--;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                direccion++;

            pulsarSalto = Keyboard.current.spaceKey.wasPressedThisFrame ||
                          Keyboard.current.wKey.wasPressedThisFrame;

            mantieneSalto = Keyboard.current.spaceKey.isPressed ||
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

        // Detectar paredes
        Bounds limites = col.bounds;

        Collider2D paredIzquierda = DetectarPared(
            new Vector2(limites.min.x - 0.05f, limites.center.y),
            new Vector2(0.12f, limites.size.y * 0.75f)
        );

        Collider2D paredDerecha = DetectarPared(
            new Vector2(limites.max.x + 0.05f, limites.center.y),
            new Vector2(0.12f, limites.size.y * 0.75f)
        );

        bool tocandoPared = paredIzquierda != null || paredDerecha != null;

        Collider2D paredActual = paredDerecha != null
            ? paredDerecha
            : paredIzquierda;

        direccionPared = paredDerecha != null ? 1 : -1;

        // Reiniciar el agarre al separarse de la pared
        if (!tocandoPared)
        {
            deslizandoPared = false;
            agarreUsado = false;
        }

        // Comenzar el agarre al caer contra una pared
        if (!enSuelo && tocandoPared && v.y < 0f && !agarreUsado)
        {
            deslizandoPared = true;
            agarreUsado = true;
            tiempoPared = tiempoAgarrePared;
        }

        // Movimiento horizontal
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

        // Wall Slide y Wall Jump
        if (deslizandoPared)
        {
            tiempoPared -= dt;

            if (tiempoPared > 0f)
            {
                v.y = Mathf.Max(v.y, -velocidadBajadaPared);

                if (buffer > 0f)
                {
                    BoostWall boost = paredActual != null ? paredActual.GetComponent<BoostWall>() : null;

                    float fuerza = boost != null ? boost.fuerzaHorizontal : fuerzaSaltoPared;

                    v.x = -direccionPared * fuerza;
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

        // Salto normal
        else if (buffer > 0f && coyote > 0f)
        {
            v.y = Gravedad * tiempoCima;
            buffer = 0f;
            coyote = 0f;
            enSuelo = false;
        }

        // Gravedad
        float multiplicador = 1f;

        if (deslizandoPared)
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

        buffer = Mathf.Max(0f, buffer - dt);
    }

    // Detectar una pared y dar prioridad a las Super Walls
    private Collider2D DetectarPared(Vector2 centro, Vector2 tamano)
    {
        Collider2D[] paredes = Physics2D.OverlapBoxAll(
            centro,
            tamano,
            0f,
            capaSuelo
        );

        Collider2D paredEncontrada = null;

        foreach (Collider2D pared in paredes)
        {
            if (pared.GetComponent<BoostWall>() != null)
                return pared;

            if (paredEncontrada == null)
                paredEncontrada = pared;
        }

        return paredEncontrada;
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
