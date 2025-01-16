using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Enemy : MonoBehaviour
{
    public GameObject player;
    private NavMeshAgent agent;
    private Boolean isCollide = false;
    private Animator animator;

    void FinishAnimation(){
        HealthSystemForDummies playerHealth = player.GetComponent<HealthSystemForDummies>();
		playerHealth.AddToCurrentHealth(-100);			
    }

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.SetDestination(player.transform.position);
        
        bool isPlayer = agent.remainingDistance <= agent.stoppingDistance;
        animator.SetBool("isWalk", !isPlayer);
        animator.SetBool("isPlayer", isPlayer);
        
    }
}