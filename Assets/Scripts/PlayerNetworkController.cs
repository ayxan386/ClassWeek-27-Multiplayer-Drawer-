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
        controller.PlayerNetwork = this;
    }


    [ServerRpc]
    public void UpdatePlayerNameServerRpc(FixedString64Bytes newName)
    {
        print("RPC method called");
        playerName.Value = newName;
    }
}