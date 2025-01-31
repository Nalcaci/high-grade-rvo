using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    public Toggle pathToggle;
    public Toggle vectorToggle;

    public Slider numberAgents;
    public Slider smoothingSections;
    public Slider displacementAmount;

    public Toggle personalityToggle;

    [SerializeField]
    private GameObject spawnerParent;

    [SerializeField]
    private GameObject agentPrefab;

    private int defaultSmoothing;
    private float defaultDisplacement;

    public TMP_Text currentAgents;
    public TMP_Text currentSmoothing;
    public TMP_Text displacement;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        defaultSmoothing = agentPrefab.GetComponent<AddingWaypoint>().smoothingSections;
        defaultDisplacement = agentPrefab.GetComponent<AddingWaypoint>().maxDisplacement;
        SetDisplacement();
        SetAgents();
        SetSmoothing();
        Time.timeScale = 0;
    }
    private void Update()
    {
        currentAgents.text = numberAgents.value.ToString();
        currentSmoothing.text = smoothingSections.value.ToString();
        displacement.text = displacementAmount.value.ToString();
    }
    public void OnResetButtonClicked()
    {
        ResetAgent();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    public void SetAgents()
    {
        foreach(AgentSpawner spawner in spawnerParent.GetComponentsInChildren<AgentSpawner>())
        {
            spawner.spawnAmount = (int)numberAgents.value;
        }
    }


    public void SetSmoothing()
    {
        agentPrefab.GetComponent<AddingWaypoint>().ChangeSmoothing((int)smoothingSections.value);
    }

    public void SetDisplacement()
    {
        agentPrefab.GetComponent<AddingWaypoint>().ChangeDisplacement((int)displacementAmount.value);
    }

    public void TogglePath()
    {
        if (pathToggle.isOn == false)
        {
            agentPrefab.GetComponent<AddingWaypoint>().drawPath = false;
            foreach (GameObject aliveAgent in GameObject.FindGameObjectsWithTag("Agent"))
            {
                aliveAgent.GetComponent<AddingWaypoint>().drawPath = false;
                aliveAgent.GetComponent<LineRenderer>().enabled = false;
            }
        }
        else
        {
            agentPrefab.GetComponent<AddingWaypoint>().drawPath = true;
            foreach (GameObject aliveAgent in GameObject.FindGameObjectsWithTag("Agent"))
            {
                aliveAgent.GetComponent<AddingWaypoint>().drawPath = true;
                aliveAgent.GetComponent<LineRenderer>().enabled = true;
            }
        }
    }

    public void ToggleVector()
    {
        if (vectorToggle.isOn == false)
        {
            agentPrefab.GetComponent<AddingWaypoint>().drawVector = false;
            foreach (GameObject aliveAgent in GameObject.FindGameObjectsWithTag("Agent"))
            {
                aliveAgent.GetComponent<AddingWaypoint>().drawVector = false;
            }
        }
        else
        {
            agentPrefab.GetComponent<AddingWaypoint>().drawVector = true;
            foreach (GameObject aliveAgent in GameObject.FindGameObjectsWithTag("Agent"))
            {
                aliveAgent.GetComponent<AddingWaypoint>().drawVector = true;
            }
        }
    }

    public void ToggleDisplacement()
    {
        if (displacementAmount.value > 1)
        {
            agentPrefab.GetComponent<AddingWaypoint>().displaceCorner = false;
        }
        else
        {
            agentPrefab.GetComponent<AddingWaypoint>().displaceCorner = true;
        }
    }

    public void TogglePersonality()
    {
        if (displacementAmount.value > 1)
        {
            agentPrefab.GetComponent<AddingWaypoint>().enablePersonality = false;
        }
        else
        {
            agentPrefab.GetComponent<AddingWaypoint>().enablePersonality = true;
        }
    }

    void OnApplicationQuit()
    {
        ResetAgent();
    }

    public void ResetAgent()
    {
        agentPrefab.GetComponent<AddingWaypoint>().drawPath = true;
        agentPrefab.GetComponent<AddingWaypoint>().drawVector = true;
        agentPrefab.GetComponent<AddingWaypoint>().smoothingSections = defaultSmoothing;
        agentPrefab.GetComponent<AddingWaypoint>().maxDisplacement = defaultDisplacement;
        agentPrefab.GetComponent<AddingWaypoint>().displaceCorner = false;
        agentPrefab.GetComponent<AddingWaypoint>().enablePersonality = true;
    }

    public void StartSimulation()
    {
        Time.timeScale = 1;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
