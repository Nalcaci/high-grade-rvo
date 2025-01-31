using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CornerCollider : MonoBehaviour
{
    public TMP_Text collisionCounter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Agent")
        {
            int counterValue = int.Parse(collisionCounter.text);
            collisionCounter.text = (counterValue + 1).ToString();
        }
    }

}
