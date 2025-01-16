using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
   public Transform target; // O carro ou veículo a ser seguido
    public Vector3 offset = new Vector3(0, 5, -10); // Posição da câmera em relação ao carro
    public float followSpeed = 10f; // Velocidade para a câmera alcançar a posição
    public float rotationSpeed = 5f; // Velocidade de suavização da rotação
    public float lookAheadFactor = 5f; // Distância à frente do carro para onde a câmera deve olhar

    private Vector3 velocity = Vector3.zero;

    void FixedUpdate()
    {
        if (target == null) return;
        // Calcula a posição desejada da câmera com base no alvo e no offset
        Vector3 desiredPosition = target.position + target.TransformDirection(offset);

        // Suaviza o movimento da câmera para a posição desejada
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 1 / followSpeed);

        // Calcula o ponto de visão à frente do carro
        Vector3 lookAheadPosition = target.position + target.forward * lookAheadFactor;

        // Suaviza a rotação da câmera para olhar para o ponto à frente
        Quaternion targetRotation = Quaternion.LookRotation(lookAheadPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}
