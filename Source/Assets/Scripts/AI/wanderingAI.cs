using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class wanderingAI : MonoBehaviour
{
    
    public float wanderRadius;
    public float wanderTimer;

    private AudioSource walkingSound;
    private Transform target;
    private Animator anim;
    private NavMeshAgent agent;
    private float timer;
    private bool move;

    // Use this for initialization
    void Awake()
    {
        move = false;
        walkingSound = GetComponent<AudioSource>();
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        timer = wanderTimer;
    }

    // Update is called once per frame
    void Update()
    {
            timer += Time.deltaTime;

        if (move)
            walkingSound.enabled = true;
        else
            walkingSound.enabled = false;

            if (timer >= wanderTimer)
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                agent.SetDestination(newPos);
                move = true;
                timer = 0;
            }
    }


    void FixedUpdate()
    {
        //Stop the walking animation, if the agent reached his destination
        if (Mathf.Round( (Mathf.Abs(GetComponent<Transform>().position.magnitude - agent.destination.magnitude))) == 0  )
            move = false;
        else
            move = true;

        anim.SetBool("move", move);
    }

    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        //Return a new position inside the chooseable radius
        Vector3 randDirection = Random.insideUnitSphere * dist;

        randDirection += origin;

        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);

        return navHit.position;
    }



}
