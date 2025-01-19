using UnityEngine;
using UnityEngine.AI;
using Unity.Collections;
using System.Collections.Generic;

public class AddingWaypoint : MonoBehaviour
{
    private NavMeshAgent thisAgent;
    private NavMeshPath agentPath;

    private LineRenderer pathRenderer;

    public Vector3 target;
    public float deleteDistance = 1;

    private bool debug = false;

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
        agentPath = thisAgent.path;

        thisAgent.CalculatePath(target, agentPath);


        thisAgent.SetDestination(target);

        SmoothPath();

        Debug.Log("Setting destination to " + target);
        thisAgent.speed = Random.Range(2, 5);
    }


    void SmoothPath()
    {
        List<Vector3> newCorners = new List<Vector3>();

        if (thisAgent.path.corners.Length < 2) { return; }

        for (int i = 1; i < thisAgent.path.corners.Length-1; i++) //for each corner, smooth edge by splitting if over a certain degree threshold.
        {
            Vector3 pointPosition = new Vector3(thisAgent.path.corners[i].x, thisAgent.path.corners[i].y, thisAgent.path.corners[i].z);

            var t1 = (thisAgent.path.corners[i] - thisAgent.path.corners[i - 1]).normalized;
            var t2 = (thisAgent.path.corners[i+1] - thisAgent.path.corners[i]).normalized;

            var avgTangent = Vector3.Lerp(t1, t2, 0.5f);

            var split1 = pointPosition + avgTangent;
            var split2 = pointPosition - avgTangent;

            newCorners.Add(split1);
            newCorners.Add(split2);
        }

        if (debug) //debug drawing of new Path
        { 
            pathRenderer.positionCount = newCorners.Count;
            pathRenderer.SetPosition(0, transform.position);

            for (int i = 1; i < newCorners.Count; i++)
            {
                pathRenderer.SetPosition(i, newCorners[i]);
            }
        }
        //thisAgent.path.corners = newCorners;
    }

    void DrawPath()
    {
        pathRenderer.positionCount = thisAgent.path.corners.Length;
        pathRenderer.SetPosition(0, transform.position);

        if(thisAgent.path.corners.Length < 2) { return; }

        for(int i = 1; i < thisAgent.path.corners.Length; i++)
        {
            Vector3 pointPosition = new Vector3(thisAgent.path.corners[i].x, thisAgent.path.corners[i].y, thisAgent.path.corners[i].z);
            pathRenderer.SetPosition(i, pointPosition);
        }
    }
}
