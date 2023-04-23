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
    }

    private void OnDrawerSelect(ulong obj)
    {
        startGame.interactable = true;
    }
}