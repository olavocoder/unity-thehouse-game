using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarPlayer : MonoBehaviour
{
    public GameObject player;
    private CarController carController;
    public GameObject cameraCar;

    // Start is called before the first frame update
    void Start()
    {
        carController = GetComponent<CarController>();
    }

    // Update is called once per frame
    void Update()
    {        
        if(Input.GetKey("k")){
            cameraCar.SetActive(false);
            player.SetActive(true);
            // Filtra qualquer objeto atrelado ao personagem para desativar
            GameObject[] guns = GameObject.FindGameObjectsWithTag("Weapon");
            foreach(GameObject gun in guns){
                Debug.Log("Entrou aqui");
                gun.SetActive(true);
                //gun.transform.position = gameObject.transform.position + new Vector3(2,1,2);
            }

            player.transform.position = gameObject.transform.position + new Vector3(2,1,2);
            PlayerMovementScript playerMovementScript = player.GetComponent<PlayerMovementScript>();
            MouseLookScript mouseLookScript = player.GetComponent<MouseLookScript>();
            GunInventory gunInventory = player.GetComponent<GunInventory>();

            // Desativa scripts de controle do personagem
            playerMovementScript.enabled = true;
            mouseLookScript.enabled = true;
            gunInventory.enabled = true;
            carController.enabled = false;
        }
    }
}
