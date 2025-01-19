using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetACar : MonoBehaviour
{
    // Start is called before the first frame update
    //private PlayerMovementScript playerMovementScript;
    //private MouseLookScript mouseLookScript;
    //private GunInventory gunInventory;
    public GameObject cameraCar;
    private Movements movements;
    
    void Start()
    {
        // Pega scripts do personagem quando o código inicia
        movements = GetComponent<Movements>();
        //mouseLookScript = GetComponent<MouseLookScript>();
        //gunInventory = GetComponent<GunInventory>();

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKey("k")){
            //Debug.Log("apertou k");
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        string pattern = @"\bCar\b";
        Match match = Regex.Match(collision.gameObject.name, pattern);

        if(match.Success){
            // Desativa scripts de controle do personagem
            //playerMovementScript.enabled = false;
            //mouseLookScript.enabled = false;
            //gunInventory.enabled = false;

            // Ativa camera do carro
            cameraCar.SetActive(true);

            // Desativa Personagem
            gameObject.SetActive(false);

            // Ativa script de controle do carro
            CarController carController = collision.gameObject.GetComponent<CarController>();
            carController.enabled = true;

            // Filtra qualquer objeto atrelado ao personagem para desativar
            GameObject[] guns = GameObject.FindGameObjectsWithTag("Weapon");
            foreach(GameObject gun in guns){
                gun.SetActive(false);
            }

        }
    }
}
