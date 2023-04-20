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
        NetworkManager.Singleton.StartHost();

        joinPanel.SetActive(true);
        hostIpText.text = GetLocalIPAddress();
    }

    private void OnClientButtonClick()
    {
        buttonPanel.SetActive(false);
        PlayerName = nameInput.text;
        GetComponent<UnityTransport>().ConnectionData.Address = ipInput.text;
        NetworkManager.Singleton.StartClient();
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
}