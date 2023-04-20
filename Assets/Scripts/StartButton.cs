using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.VisualScripting;

public class StartButton : MonoBehaviour
{
    [SerializeField] private Button startGame;
    
    private int currentNumberOfPlayers;
    private int currentNumberOfAccepted;
    public void Start()
    {
        startGame.onClick.AddListener(OnStartButtonClickedServerRpc);
    }
    [ServerRpc]
    private void OnStartButtonClickedServerRpc()
    {
        currentNumberOfAccepted++;
        Debug.Log("start");
    }

}

