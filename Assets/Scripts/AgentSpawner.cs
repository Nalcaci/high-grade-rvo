using UnityEngine;

public class AgentSpawner:MonoBehaviour
{
    public GameObject agentPrefab;
    public static void SpawnAgent(GameObject agentPrefab, Vector3 position, Quaternion rotation)
    {
        GameObject agent = Object.Instantiate(agentPrefab, position, rotation);
        agent.name = "Agent";
    }

    void Start()
    {
        SpawnAgent(agentPrefab, this.transform.position, Quaternion.identity);
    }
}
