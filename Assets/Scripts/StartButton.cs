using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class StartButton : MonoBehaviour
{
    [SerializeField] private Button startGame;

    public static Action<bool> OnAcceptButtonPressed;

    private void Start()
    {
        startGame.onClick.AddListener(() =>
        {
            Debug.LogError(NetworkManager.Singleton.LocalClientId);
            OnAcceptButtonPressed?.Invoke(true);
            startGame.interactable = false;
            // PlayerLobbyController.Instance.AcceptButtonPressed();
            PlayerLobbyController.Instance.OnStartButtonClickedServerRpc(NetworkManager.Singleton.LocalClientId);
        });
        PlayerLobbyController.OnDrawerSelect += OnDrawerSelect;
    }

    private void OnDrawerSelect(ulong obj)
    {
        startGame.interactable = true;
    }
}