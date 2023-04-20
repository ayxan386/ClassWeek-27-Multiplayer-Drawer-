using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.VisualScripting;

public class StartButton : MonoBehaviour
{
    [SerializeField] private Button startGame;
    private int numberofPlayers;

   public void StartGame()
    {
        
        numberofPlayers++;
    }
    


   
}

