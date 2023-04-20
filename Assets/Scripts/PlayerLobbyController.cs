using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerLobbyController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI numberOfPlayers;
    [SerializeField] private GameObject joinPanel;

    private NetworkVariable<int> currentNumberOfPlayers = new();

    public static PlayerLobbyController Instance { get; private set; }

    public static Action<ulong> OnDrawerSelect;

    public override void OnNetworkSpawn()
    {
        Instance = this;
        currentNumberOfPlayers.OnValueChanged += OnPlayerCountChange;
        if (IsServer || IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectionChanged;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientConnectionChanged;
        }
    }

    private void OnPlayerCountChange(int previousvalue, int newvalue)
    {
        numberOfPlayers.text = currentNumberOfPlayers.Value.ToString();
    }

    private void OnClientConnectionChanged(ulong obj)
    {
        print("Player connection changed");
        currentNumberOfPlayers.Value = NetworkManager.Singleton.ConnectedClientsIds.Count;
    }

    [ContextMenu("Start game")]
    public void StartGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var drawer =
                NetworkManager.Singleton.ConnectedClientsIds[Random.Range(0, currentNumberOfPlayers.Value)];
            joinPanel.SetActive(false);
            SelectPlayerAsDrawerClientRpc(drawer);
        }
    }

    [ClientRpc]
    private void SelectPlayerAsDrawerClientRpc(ulong playerId)
    {
        OnDrawerSelect?.Invoke(playerId);
    }
}