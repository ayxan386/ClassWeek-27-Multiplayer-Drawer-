using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PlayerLobbyController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI numberOfPlayers;
    [SerializeField] private GameObject joinPanel;
    [SerializeField] private Button startGame;

    private NetworkVariable<int> currentNumberOfPlayers = new();
    private int currentNumberOfAccepted = 0;
    public static PlayerLobbyController Instance { get; private set; }

    public static Action<ulong> OnDrawerSelect;

    public override void OnNetworkSpawn()
    {
        Instance = this;
        currentNumberOfPlayers.OnValueChanged += OnPlayerCountChange;
        startGame.onClick.AddListener(OnStartButtonClickedServerRpc);
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
            currentNumberOfAccepted = 0;
            SelectPlayerAsDrawerClientRpc(drawer);
        }
    }

    [ClientRpc]
    private void SelectPlayerAsDrawerClientRpc(ulong playerId)
    {
        OnDrawerSelect?.Invoke(playerId);
    }

    [ServerRpc]
    private void OnStartButtonClickedServerRpc()
    {
        currentNumberOfAccepted++;
        if (IsServer && currentNumberOfAccepted == currentNumberOfPlayers.Value)
        {
            StartGame();
        }
    }
}