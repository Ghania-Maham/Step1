using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_FSM : MonoBehaviour
{
    //enums are nice to keep states
    public enum ENEMY_STATE { PATROL, CHASE, ATTACK };

   
    //We need a property to access the current state

    public ENEMY_STATE CurrentState
    {
        get { return currentState; }
        set
        {
            currentState = value;
            //stop all coroutines
            StopAllCoroutines();

            switch (currentState)
            {
                case ENEMY_STATE.PATROL:
                    StartCoroutine(EnemyPatrol());
                    break;
                case ENEMY_STATE.CHASE:
                    StartCoroutine(EnemyChase());
                    break;
                case ENEMY_STATE.ATTACK:
                    StartCoroutine(EnemyAttack());
                    break;
            }
        }
    }

    [SerializeField]
    private ENEMY_STATE currentState;

    //What about some references?
    private CheckMyVision checkMyVision; //This is our previous file

    private NavMeshAgent agent = null;

    private Health playerHealth = null; //TODO

    private Transform playerTransform = null;

    //Reference to patrol destination
    private Transform PatrolDestination = null;

   

    public float maxDamage = 10f;
    private void Awake()
    {
        checkMyVision = GetComponent<CheckMyVision>();
        agent = GetComponent<NavMeshAgent>();
        playerHealth = GameObject.FindGameObjectWithTag("Player").
        GetComponent<Health>();
        //Do something about player transform to
        playerTransform = playerHealth.GetComponent<Transform>();
    }
    
    // Start is called before the first frame update
    void Start()
    {
        //Find a random destination
        GameObject[] destinations = GameObject.FindGameObjectsWithTag("Dest");
        PatrolDestination = destinations[Random.Range(0,destinations.Length)].GetComponent<Transform>();
        currentState = ENEMY_STATE.PATROL;

    }
    public IEnumerator EnemyPatrol()
    {
        while(currentState == ENEMY_STATE.PATROL)
        {
            checkMyVision.sensitivity = CheckMyVision.enmSensitivity.HIGH;
            agent.isStopped=false;
            agent.SetDestination(PatrolDestination.position);

            while (agent.pathPending)
                yield return null; //This is to ensure we wait for path completion
            if(checkMyVision.targetInSight)
            {
                Debug.Log("Find you, changing to CHASE state");
                agent.isStopped=true;
                currentState = ENEMY_STATE.CHASE;
                yield break;
            }
            yield break;
        }
        
    }
    public IEnumerator EnemyChase()
    {
        Debug.Log("Enemy Chase starting");
        //Again we shall start with aloop
        while(currentState == ENEMY_STATE.CHASE)
        {
            //In this case, let us keep sensitivity LOW
            checkMyVision.sensitivity = CheckMyVision.enmSensitivity.LOW;

            //The idea of the chase is to go to the last known position
            agent.isStopped = false;
            agent.SetDestination(checkMyVision.lastKnownSighting);

            //Again we need to yield if path is yet incomplete
            while(agent.pathPending)
            {
                yield return null;
            }
            //while chasing we need to keep checking if we reached
            if(agent.remainingDistance <= agent.stoppingDistance)
            {
                agent.isStopped =true;
                //what if we reached destination but cannot see the player?
                if (!checkMyVision.targetInSight)
                {
                    Debug.Log("Target not in sight so patrolling");
                    currentState = ENEMY_STATE.PATROL;

                }

                else
                {
                    Debug.Log("Target not in sight so patrolling");
                    currentState = ENEMY_STATE.ATTACK;
                }
                yield break;
            }
            //Till next frame
            yield break;
        }
        
    }
    public IEnumerator EnemyAttack()
    {
        //Like the other start with the loop
        while (currentState == ENEMY_STATE.ATTACK)
        {
            Debug.Log("I am attacking");
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);

            while (agent.pathPending)
                yield return null;

            if (agent.remainingDistance > agent.stoppingDistance)
            {
                CurrentState = ENEMY_STATE.CHASE;
            }
            else
            {
                //attack
                //Do something here later on about player hwalth  
                playerHealth.HealthPoints -= maxDamage*Time.deltaTime;
            }
            yield return null;
        }
        yield break;
    }

    // Update is called once per frame
    void Update()
    {

    }
}

