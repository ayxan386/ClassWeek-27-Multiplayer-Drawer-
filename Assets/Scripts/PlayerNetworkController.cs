using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour
{
    [SerializeField] private PlayerController playerPrefab;

    public NetworkVariable<FixedString64Bytes> playerName = new();
    private PlayerController controller;

    public override void OnNetworkSpawn()
    {
        controller = Instantiate(playerPrefab, GameObject.Find("Players").transform);
        controller.SetUIName(playerName.Value.ToString());
        playerName.OnValueChanged += OnValueChanged;

        if (IsOwner && string.IsNullOrEmpty(controller.playerNameText.text))
        {
            UpdatePlayerNameServerRpc(NetworkButtonManager.Instance.PlayerName);
        }
    }

    private void OnValueChanged(FixedString64Bytes previousvalue, FixedString64Bytes newvalue)
    {
        controller.SetUIName(playerName.Value.ToString());
    }

    [ServerRpc]
    private void UpdatePlayerNameServerRpc(FixedString64Bytes newName)
    {
        print("RPC method called");
        playerName.Value = newName;
    }
}