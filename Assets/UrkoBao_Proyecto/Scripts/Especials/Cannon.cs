using UnityEngine;

public class Cannon : MonoBehaviour
{
    [Header("Configuración del Cañón")]
    public float fuerzaDisparo = 100f;
    public float velocidadRotacion = 3f;
    public float anguloMaximo = 45f;

    private float rotacionBase;

    private void Start()
    {
        rotacionBase = transform.eulerAngles.z;
    }

    private void Update()
    {
        float anguloActual = rotacionBase + Mathf.Sin(Time.time * velocidadRotacion) * anguloMaximo;
        transform.rotation = Quaternion.Euler(0, 0, anguloActual);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        PlayerMov player = col.GetComponent<PlayerMov>();

        if (player != null)
        {
            player.EnterCannon(transform, fuerzaDisparo, transform.position);
        }
    }
}