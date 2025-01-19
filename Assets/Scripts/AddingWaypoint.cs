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

    [SerializeField]
    public float smoothingLength = 1;
    [SerializeField]
    private int smoothingSections = 10;

    public Vector3[] pathLocations = new Vector3[0];
    [SerializeField]
    private int pathIndex = 0;

    [Header("Movement Configuration")]
    [SerializeField]
    [Range(0, 0.99f)]
    private float smoothing = 0.25f;
    [SerializeField]
    private float targetLerpSpeed = 1;

    [SerializeField]
    private Vector3 targetDirection;
    private float lerpTime = 0;
    [SerializeField]
    private Vector3 movementVector;

    private bool debug = false;

    void Update()
    {
        MoveAgent();

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

    private void MoveAgent()
    {
        if (pathIndex >= pathLocations.Length)
        {
            return;
        }

        if (Vector3.Distance(transform.position, pathLocations[pathIndex] + (thisAgent.baseOffset * Vector3.up)) <= thisAgent.radius)
        {
            pathIndex++;
            lerpTime = 0;

            if (pathIndex >= pathLocations.Length)
            {
                return;
            }
        }

        movementVector = (pathLocations[pathIndex] + (thisAgent.baseOffset * Vector3.up) - transform.position).normalized;

        targetDirection = Vector3.Lerp(
            targetDirection,
            movementVector,
            Mathf.Clamp01(lerpTime * targetLerpSpeed * (1 - smoothing))
        );

        Vector3 lookDirection = movementVector;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.LookRotation(lookDirection),
                Mathf.Clamp01(lerpTime * targetLerpSpeed * (1 - smoothing))
            );
        }

        thisAgent.Move(targetDirection * thisAgent.speed * Time.deltaTime);

        lerpTime += Time.deltaTime;
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
        Vector3[] pathLocations = new Vector3[curves.Length * smoothingSections];

        int index = 0;
        for (int i = 0; i < curves.Length; i++)
        {
            Vector3[] segments = curves[i].GetSegments(smoothingSections);
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
