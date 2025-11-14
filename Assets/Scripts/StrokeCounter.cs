using UnityEngine;
using UnityEngine.UI;

public class StrokeCounter : MonoBehaviour
{
    public Text strokeText;
    private int strokeCount;

    public void AddStroke()
    {
        strokeCount++;
        strokeText.text = "ë≈êî: " + strokeCount;
    }
}
