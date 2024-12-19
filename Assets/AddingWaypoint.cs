using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;

public class AddingWaypoint : MonoBehaviour
{

    public NavMeshAgent thisAgent;
    public Vector3 goal;
    public float deleteDistance = 2;
    //public static int agentCount = 0;
    public int agntCountVisable = 0;
    public void addTarget(Vector3 goal)
    {
        this.goal = goal;
    }

    private void Update()
    {
        if (Vector3.Distance(this.transform.position, goal) <= deleteDistance)
        {
            //agentCount--;
            this.gameObject.SetActive(false);

        }
    }
    void Start()
    {

    }

    private void Awake()
    {
        thisAgent.SetDestination(goal);
        thisAgent.speed = Random.Range(2, 5);
        thisAgent.avoidancePriority = Random.Range(1, 50);
        //agentCount++;
        StartCoroutine(repath());
    }

    public IEnumerator repath()
    { 
        yield return new WaitForSeconds(1);
        //thisAgent.nextPosition = this.transform.position;
        thisAgent.autoRepath = true;
        StartCoroutine(repath());
    }
}
