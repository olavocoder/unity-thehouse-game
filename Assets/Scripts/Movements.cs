using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moviments : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed = 5f;
    public Rigidbody person;
    
    void Start()
    {
        person = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0f, vertical).normalized;
        person.MovePosition(transform.position + movement * speed * Time.fixedDeltaTime);
    }
}
