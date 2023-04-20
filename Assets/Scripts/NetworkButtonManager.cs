using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class NetworkButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TextMeshProUGUI nameInput;
    [SerializeField] private TextMeshProUGUI ipInput;
    [SerializeField] private TextMeshProUGUI numberOfPlayers;

    [Header("After join UI")] [SerializeField]
    private GameObject joinPanel;

    [SerializeField] private TextMeshProUGUI hostIpText;

    private int currentNumberOfPlayers;
    
    private int currentNumberOfAccepted;

    public static NetworkButtonManager Instance { get; private set; }

    public static Action<ulong> OnDrawerSelect;

    public string PlayerName { get; private set; }

    private void Start()
    {
        Instance = this;
        hostButton.onClick.AddListener(OnHostButtonClick);
        clientButton.onClick.AddListener(OnClientButtonClick);
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectionChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientConnectionChanged;
    }

    private void OnClientConnectionChanged(ulong obj)
    {
        currentNumberOfPlayers = NetworkManager.Singleton.ConnectedClientsIds.Count;
        numberOfPlayers.text = currentNumberOfPlayers.ToString();
    }

    private void OnHostButtonClick()
    {
        buttonPanel.SetActive(false);
        PlayerName = nameInput.text;

        hostIpText.text = GetLocalIPAddress();

        // GetComponent<UnityTransport>().ConnectionData.Address = hostIpText.text;
        NetworkManager.Singleton.StartHost();
        joinPanel.SetActive(true);
    }

    private void OnClientButtonClick()
    {
        buttonPanel.SetActive(false);
        PlayerName = nameInput.text;

        var ipAddress = ipInput.text[..^1];
        GetComponent<UnityTransport>().ConnectionData.Address = ipAddress;
        NetworkManager.Singleton.StartClient();

        hostIpText.text = ipAddress;
        joinPanel.SetActive(true);
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }

        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    [ContextMenu("Start game")]
    public void StartGame()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            var drawer = NetworkManager.Singleton.ConnectedClientsIds[Random.Range(0, currentNumberOfPlayers)];
            SelectPlayerAsDrawerClientRpc(drawer);
        }
    }

    [ClientRpc]
    private void SelectPlayerAsDrawerClientRpc(ulong playerId)
    {
        OnDrawerSelect?.Invoke(playerId);
    }
}