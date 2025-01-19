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

    public float smoothingLength = 1;

    public Vector3[] pathLocations = new Vector3[0];
    [SerializeField]
    private int pathIndex = 0;


    private bool debug = false;

    void Update()
    {
        if (Vector3.Distance(this.transform.position, target) <= deleteDistance)
        {
            this.gameObject.SetActive(false);
        }
        else if (pathLocations.Length > 0)
        {
            DrawPath();
        }
    }

    void Start()
    {
        pathRenderer = GetComponent<LineRenderer>();
        thisAgent = GetComponent<NavMeshAgent>();
        agentPath = new NavMeshPath();

        SetDestination();

        Debug.Log("Setting destination to " + target);
        thisAgent.speed = Random.Range(2, 5);
    }

    private void SetDestination()
    {
        thisAgent.ResetPath();
        NavMesh.CalculatePath(this.transform.position, target, thisAgent.areaMask, agentPath);
        Vector3[] corners = agentPath.corners;

        if (corners.Length < 2)
        {
            pathLocations = corners;
            pathIndex = 0;
            return;
        }

        BezierCurve[] curves = new BezierCurve[corners.Length - 1];
        SmoothPath(curves, corners);
        pathLocations = GetPathLocations(curves);
        pathIndex = 0;
    }

    private void SmoothPath(BezierCurve[] curves, Vector3[] corners)
    {
        for (int i = 0; i < curves.Length; i++)
        {
            if (curves[i] == null)
            {
                curves[i] = new BezierCurve();
            }

            Vector3 position = corners[i];
            Vector3 lastPosition = i == 0 ? position : corners[i - 1];
            Vector3 nextPosition = corners[i + 1];

            Vector3 lastDirection = (position - lastPosition).normalized;
            Vector3 nextDirection = (nextPosition - position).normalized;

            Vector3 startTangent = (lastDirection + nextDirection) * smoothingLength;
            Vector3 endTangent = (lastDirection + nextDirection) * -1 * smoothingLength;

            curves[i].Points[0] = position;
            curves[i].Points[1] = position + startTangent;
            curves[i].Points[2] = nextPosition + endTangent;
            curves[i].Points[3] = nextPosition;
        }

        {
            Vector3 nextDirection = (curves[1].EndPosition - curves[1].StartPosition).normalized;
            Vector3 lastDirection = (curves[0].EndPosition - curves[0].StartPosition).normalized;

            curves[0].Points[2] = curves[0].Points[3] + (nextDirection + lastDirection) * -1 * smoothingLength;
        }
    }

    private Vector3[] GetPathLocations(BezierCurve[] curves)
    {
        Vector3[] pathLocations = new Vector3[curves.Length * 10];

        int index = 0;
        for (int i = 0; i < curves.Length; i++)
        {
            Vector3[] segments = curves[i].GetSegments(10);
            for (int j = 0; j < segments.Length; j++)
            {
                pathLocations[index] = segments[j];
                index++;
            }
        }

        return pathLocations;
    }

    void DrawPath()
    {
        int numLocations = pathLocations.Length;
        pathRenderer.positionCount = numLocations;
        pathRenderer.SetPosition(0, transform.position);

        if(numLocations < 2) { return; }

        for(int i = 1; i < numLocations; i++)
        {
            Vector3 pointPosition = new Vector3(pathLocations[i].x, pathLocations[i].y, pathLocations[i].z);
            pathRenderer.SetPosition(i, pointPosition);
        }
    }
}
