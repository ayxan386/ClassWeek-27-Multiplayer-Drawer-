using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

public class NetworkButtonManager : MonoBehaviour
{
    [SerializeField] private GameObject buttonPanel;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private TextMeshProUGUI nameInput;
    [SerializeField] private TextMeshProUGUI ipInput;

    [Header("After join UI")] [SerializeField]
    private GameObject joinPanel;

    [SerializeField] private TextMeshProUGUI hostIpText;
    
    [Header("On going game UI")]
    [SerializeField] private GameObject gameStartMenu;

    public GameObject GameStartMenu => gameStartMenu;

    public static NetworkButtonManager Instance { get; private set; }

    public string PlayerName { get; private set; }

    private void Start()
    {
        Instance = this;
        hostButton.onClick.AddListener(OnHostButtonClick);
        clientButton.onClick.AddListener(OnClientButtonClick);
    }

    private void OnHostButtonClick()
    {
        buttonPanel.SetActive(false);
        PlayerName = nameInput.text;

        hostIpText.text = GetLocalIPAddress();

        GetComponent<UnityTransport>().ConnectionData.Address = hostIpText.text;
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

    public void SetPlayerWaitingLayout()
    {
        gameStartMenu.SetActive(false);
        joinPanel.SetActive(true);
    }

}