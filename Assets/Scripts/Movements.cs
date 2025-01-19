using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movements : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 5f;
    public float rotationSpeed = 60f;

    public Rigidbody person;
    private Animator animator;
    
    void Start()
    {
        person = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
 // Captura os eixos de entrada
        float horizontal = Input.GetAxis("Horizontal"); // A/D para rotação
        float vertical = Input.GetAxis("Vertical");     // W/S para movimento
        float verticalRounded = Mathf.RoundToInt(vertical);
        // Define o estado de caminhada no Animator
        animator.SetInteger("isWalk", (int) verticalRounded);

        // Movimento para frente e para trás (W e S)
        Vector3 forwardMovement = transform.forward * vertical * speed * Time.fixedDeltaTime;
        person.MovePosition(transform.position + forwardMovement);

        // Rotação do personagem (A e D)
        if (horizontal != 0)
        {
            float rotation = horizontal * rotationSpeed * Time.fixedDeltaTime;
            transform.Rotate(0, rotation, 0); // Rotaciona no eixo Y
        }
    }
}
