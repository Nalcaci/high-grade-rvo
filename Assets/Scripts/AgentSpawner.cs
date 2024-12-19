using System.Collections;
using UnityEngine;

public class AgentSpawner:MonoBehaviour
{
    public GameObject agentPrefab;
    public GameObject target;
    public Color32 color;
    public float timer = 0.5f;
    public int spawnAmount = 500;
    private int spawnCount = 0;

    void SpawnAgent(GameObject agentPrefab, Vector3 position, Quaternion rotation, Vector3 target, Color32 color)
    {
        GameObject agent = Object.Instantiate(agentPrefab, position+new Vector3(Random.Range(-10,10),0, Random.Range(-10, 10)), rotation);
        agent.name = "Agent";
        agent.GetComponent<AddingWaypoint>().target = target;
        Renderer render = agent.GetComponent<Renderer>();
        render.material.color = color;
    }

    void Start()
    {
        StartCoroutine(Spawning());
    }

    public IEnumerator Spawning()
    {
        yield return new WaitForSeconds(timer);
        if (spawnCount < spawnAmount)
        {
            SpawnAgent(agentPrefab, this.transform.position, Quaternion.identity, target.transform.position, color);
            spawnCount += 1;
            StartCoroutine(Spawning());
        }
    }
}
