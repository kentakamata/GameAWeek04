using UnityEngine;

public class CupGoal : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("カップイン！");
        }
    }
}
