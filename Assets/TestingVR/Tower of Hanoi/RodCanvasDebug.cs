using TMPro;
using UnityEngine;

public class RodCanvasDebug : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _value;

    public void SetTextValue(string value)
    {
        _value.text = value;
    }
}
