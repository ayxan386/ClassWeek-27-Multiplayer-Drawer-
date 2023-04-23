using System;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerLobbyController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI numberOfPlayers;
    [SerializeField] private GameObject joinPanel;

    private NetworkVariable<int> currentNumberOfPlayers = new();
    private HashSet<ulong> acceptedPlayers = new();
    public static PlayerLobbyController Instance { get; private set; }

    public static Action<ulong> OnDrawerSelect;
    public static Action<string> OnWordSelection;
    public static Action<string, ulong> OnPlayerVictory;
    private string theAnswer;

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
                NetworkManager.Singleton.ConnectedClientsIds[Random.Range(0, acceptedPlayers.Count)];
            acceptedPlayers.Clear();
            SelectPlayerAsDrawerClientRpc(drawer);
            Invoke("SelectWord", 3);
        }
    }

    private void SelectWord()
    {
        var words = Resources.Load<TextAsset>("Words").text.Split("\n");
        var wordIndex = Random.Range(0, words.Length);
        theAnswer = words[wordIndex].ToLower();
        SendSelectedWordClientRpc(theAnswer);
    }

    public void CompareAnswers(string word, ulong playerId)
    {
        print($"the submitted word : {word} the answer was {theAnswer}");
        if (word.Trim() == theAnswer.Trim())
        {
            print("Words match");
            SendPlayerWonClientRpc(word, playerId);
        }
    }

    [ClientRpc]
    private void SendSelectedWordClientRpc(FixedString64Bytes word)
    {
        OnWordSelection?.Invoke(word.ToString());
    }

    [ClientRpc]
    private void SelectPlayerAsDrawerClientRpc(ulong playerId)
    {
        joinPanel.SetActive(false);
        OnDrawerSelect?.Invoke(playerId);
    }

    [ClientRpc]
    private void SendPlayerWonClientRpc(FixedString64Bytes answer, ulong playerId)
    {
        print("Received victory event");
        OnPlayerVictory?.Invoke(answer.ToString(), playerId);
    }


    public void OnStartButtonClicked(ulong playerId)
    {
        acceptedPlayers.Add(playerId);
        print("Current number of accepted players: " + acceptedPlayers.Count);
        if (acceptedPlayers.Count == currentNumberOfPlayers.Value)
            StartGame();
    }
}