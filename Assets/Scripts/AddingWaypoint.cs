using UnityEngine;
using UnityEngine.AI;
using Unity.Collections;
using System.Collections.Generic;
using System.Linq;

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
    [SerializeField]
    [Range(-1, 1)]
    private float smoothingFactor = 0;

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
    private Vector3 infinityVector = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
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
        pathLocations = PostProcessPath(curves, pathLocations);
        return pathLocations;
    }

    private Vector3[] PostProcessPath(BezierCurve[] Curves, Vector3[] Path)
    {
        Vector3[] path = RemoveOversmoothing(Curves, Path);

        path = RemoveTooClosePoints(path);

        path = SamplePathPositions(path);

        return path;
    }

    private Vector3[] RemoveOversmoothing(BezierCurve[] Curves, Vector3[] Path)
    {
        if (Path.Length <= 2)
        {
            return Path;
        }

        int index = 1;
        int lastIndex = 0;
        for (int i = 0; i < Curves.Length; i++)
        {
            Vector3 targetDirection = (Curves[i].EndPosition - Curves[i].StartPosition).normalized;

            for (int j = 0; j < smoothingSections - 1; j++)
            {
                Vector3 segmentDirection = (Path[index] - Path[lastIndex]).normalized;
                float dot = Vector3.Dot(targetDirection, segmentDirection);
                Debug.Log($"Target Direction: {targetDirection}. segment direction: {segmentDirection} = dot {dot} with index {index} & lastIndex {lastIndex}");
                if (dot <= smoothingFactor)
                {
                    Path[index] = infinityVector;
                }
                else
                {
                    lastIndex = index;
                }

                index++;
            }

            index++;
        }

        Path[Path.Length - 1] = Curves[Curves.Length - 1].EndPosition;

        Vector3[] TrimmedPath = Path.Except(new Vector3[] { infinityVector }).ToArray();

        Debug.Log($"Original Smoothed Path: {Path.Length}. Trimmed Path: {TrimmedPath.Length}");

        return TrimmedPath;
    }

    private Vector3[] SamplePathPositions(Vector3[] Path)
    {
        for (int i = 0; i < Path.Length; i++)
        {
            if (NavMesh.SamplePosition(Path[i], out NavMeshHit hit, thisAgent.radius * 1.5f, thisAgent.areaMask))
            {
                Path[i] = hit.position;
            }
            else
            {
                Debug.LogWarning($"No NavMesh point close to {Path[i]}. Check your smoothing settings!");
                Path[i] = infinityVector;
            }
        }

        return Path.Except(new Vector3[] { infinityVector }).ToArray();
    }

    private Vector3[] RemoveTooClosePoints(Vector3[] Path)
    {
        if (Path.Length <= 2)
        {
            return Path;
        }

        int lastIndex = 0;
        int index = 1;
        for (int i = 0; i < Path.Length - 1; i++)
        {
            if (Vector3.Distance(Path[index], Path[lastIndex]) <= thisAgent.radius)
            {
                Path[index] = infinityVector;
            }
            else
            {
                lastIndex = index;
            }
            index++;
        }

        return Path.Except(new Vector3[] { infinityVector }).ToArray();
    }

    void DrawPath()
    {
        if (pathIndex >= pathLocations.Length) return;

        int remainingPoints = pathLocations.Length - pathIndex;
        pathRenderer.positionCount = remainingPoints + 1; // Include current position
        pathRenderer.SetPosition(0, transform.position);

        for (int i = 0; i < remainingPoints; i++)
        {
            Vector3 pointPosition = pathLocations[pathIndex + i];
            pathRenderer.SetPosition(i + 1, pointPosition);
        }
    }

}
