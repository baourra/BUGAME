
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PrecisionPlayerController2D : MonoBehaviour
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

    [Header("Suelo")]
    [SerializeField] private Transform sueloCheck;
    [SerializeField] private LayerMask capaSuelo;

    private Rigidbody2D rb;
    private float direccion;
    private bool mantieneSalto;
    private bool enSuelo;

    private float coyote;
    private float buffer;

    private float Gravedad =>
        2f * alturaSalto / (tiempoCima * tiempoCima);

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

            pulsarSalto = Keyboard.current.spaceKey.wasPressedThisFrame;
            mantieneSalto = Keyboard.current.spaceKey.isPressed;
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
            buffer = 0.1f;
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

        // Salto
        if (buffer > 0f && coyote > 0f)
        {
            v.y = Gravedad * tiempoCima;
            buffer = 0f;
            coyote = 0f;
            enSuelo = false;
        }

        // Gravedad según la fase del salto
        float multiplicador = 1f;

        if (v.y > 0f && !mantieneSalto)
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
