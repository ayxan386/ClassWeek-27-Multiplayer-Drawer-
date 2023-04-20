using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI playerNameText;
    private NetworkVariable<Vector3> points = new();

    private TextMeshProUGUI joinLog;

    public void SetUIName(string name)
    {
        print("Update event recieved");
        playerNameText.text = name;
    }
}