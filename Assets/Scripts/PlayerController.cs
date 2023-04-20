using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    private NetworkVariable<Vector3> points = new();

    private TextMeshProUGUI joinLog;
    public PlayerNetworkController PlayerNetwork { get; set; }

    private void Start()
    {
        PlayerNetwork.playerName.OnValueChanged += OnPlayerNameChanged;
    }

    private void OnPlayerNameChanged(FixedString64Bytes previousvalue, FixedString64Bytes newvalue)
    {
        print("Update event recieved");
        playerNameText.text = newvalue.ToString();
    }

    void Update()
    {
        if (IsOwner && string.IsNullOrEmpty(playerNameText.text))
        {
            PlayerNetwork.UpdatePlayerNameServerRpc(NetworkButtonManager.Instance.PlayerName);
        }

        if (IsOwner && Input.GetMouseButton(0))
        {
            // UpdatePointsServerRpc(Random.insideUnitSphere * 3);
        }
    }
}