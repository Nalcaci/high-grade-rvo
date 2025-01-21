using UnityEngine;
using UnityEngine.AI;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;

public class BasePath : MonoBehaviour
{
    private NavMeshAgent thisAgent;
    private LineRenderer pathRenderer;

    public Vector3 target;
    public float deleteDistance = 1;

    public Vector3[] pathLocations = new Vector3[0];
    [SerializeField]
    private int pathIndex = 0;


    void Update()
    {
        if (Vector3.Distance(this.transform.position, target) <= deleteDistance)
        {
            this.gameObject.SetActive(false);
        }
        else if (thisAgent.hasPath)
        {
            DrawPath();
        }
    }

    void Start()
    {
        pathRenderer = GetComponent<LineRenderer>();
        thisAgent = GetComponent<NavMeshAgent>();

        thisAgent.SetDestination(target);
        //SetDestination();

        Debug.Log("Setting destination to " + target);
        thisAgent.speed = Random.Range(2, 5);
    }

    void DrawPath()
    {

        if (thisAgent.path.corners.Length < 2) { return; }

        pathRenderer.positionCount = thisAgent.path.corners.Length;
        pathRenderer.SetPosition(0, transform.position);

        for (int i = 1; i < thisAgent.path.corners.Length; i++)
        {
            Vector3 pointPosition = new Vector3(thisAgent.path.corners[i].x, thisAgent.path.corners[i].y, thisAgent.path.corners[i].z);
            pathRenderer.SetPosition(i, pointPosition);
        }
    }

}
