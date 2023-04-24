using System;
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
            OnAcceptButtonPressed?.Invoke(true);
            startGame.interactable = false;
        });
        PlayerLobbyController.OnDrawerSelect += OnDrawerSelect;
        PlayerLobbyController.OnPlayerVictory += OnPlayerVictory;
    }

    private void OnPlayerVictory(string arg1, ulong arg2)
    {
        startGame.interactable = true;
    }

    private void OnDrawerSelect(ulong obj)
    {
        startGame.interactable = true;
    }
}