using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI playerNameText;
    private TextMeshProUGUI joinLog;

    public void SetUIName(string name)
    {
        playerNameText.text = name;
    }
}