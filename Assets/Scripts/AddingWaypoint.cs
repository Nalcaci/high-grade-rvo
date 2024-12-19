using UnityEngine;
using UnityEngine.AI;

public class AddingWaypoint : MonoBehaviour
{
    private NavMeshAgent thisAgent;
    public Vector3 target;
    public float deleteDistance = 1;

    void Update()
    {
        if (Vector3.Distance(this.transform.position, target) <= deleteDistance)
        {
            this.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        thisAgent = GetComponent<NavMeshAgent>();
        thisAgent.SetDestination(target);
        Debug.Log("Setting destination to " + target);
        thisAgent.speed = Random.Range(2, 5);
    }
}
