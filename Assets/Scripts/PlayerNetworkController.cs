using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour
{
    [SerializeField] private PlayerController playerPrefab;

    public NetworkVariable<FixedString64Bytes> playerName = new();
    private NetworkVariable<Vector2> pointHolder = new();
    private NetworkVariable<int> resetBrushEvent = new();
    private NetworkVariable<Vector2> createBrushEvent = new();
    private NetworkVariable<FixedString64Bytes> lastAnswer = new();

    private PlayerController controller;
    private bool isDrawer;
    Vector2 lastPos;
    private TextMeshProUGUI eventMessageText;
    private Animator eventMessageAnimator;
    private TMP_InputField answerInputField;

    public override void OnNetworkSpawn()
    {
        controller = Instantiate(playerPrefab, GameObject.Find("Players").transform);
        controller.SetUIName(playerName.Value.ToString());
        playerName.OnValueChanged += OnNameChanged;
        pointHolder.OnValueChanged += OnPointAdded;
        resetBrushEvent.OnValueChanged += OnResetBrush;
        createBrushEvent.OnValueChanged += OnBrushCreate;
        StartButton.OnAcceptButtonPressed += OnAcceptButtonPressed;
        lastAnswer.OnValueChanged += OnAnswerSubmit;

        eventMessageText = GameObject.Find("Event message").GetComponent<TextMeshProUGUI>();
        eventMessageAnimator = eventMessageText.gameObject.GetComponent<Animator>();

        if (IsOwner && string.IsNullOrEmpty(controller.playerNameText.text))
        {
            UpdatePlayerNameServerRpc(NetworkButtonManager.Instance.PlayerName);
        }

        PlayerLobbyController.OnDrawerSelect += OnDrawerSelect;
        PlayerLobbyController.OnWordSelection += OnWordSelection;
        PlayerLobbyController.OnPlayerVictory += OnPlayerVictory;
    }

    private void OnPlayerVictory(string ans, ulong playerId)
    {
        if (playerId == OwnerClientId)
            ShowMessage(IsOwner ? "You won!!" : $"{playerName.Value} won\n the word was {ans}");

        if (IsOwner) NetworkButtonManager.Instance.SetPlayerWaitingLayout();
    }

    private void OnWordSelection(string word)
    {
        if (IsOwner && isDrawer)
        {
            ShowMessage($"The word is \n {word}");
        }
    }

    private void ShowMessage(string message)
    {
        eventMessageText.text = message;
        eventMessageAnimator.SetTrigger("scale");
    }

    private void OnAnswerSubmit(FixedString64Bytes previousvalue, FixedString64Bytes newvalue)
    {
        controller.AddAnswerToUI(newvalue.ToString());
    }

    private void OnAcceptButtonPressed(bool obj)
    {
        if (IsOwner)
        {
            OnPlayerAcceptServerRpc(OwnerClientId);
        }
    }

    private void OnBrushCreate(Vector2 previousvalue, Vector2 mousePos)
    {
        print("Brush create event received: " + mousePos);
        DrawingSingleton.Instance.CreateBrush(mousePos);
    }

    private void OnResetBrush(int previousvalue, int newvalue)
    {
        print("Brush reset event received: " + newvalue);
        DrawingSingleton.Instance.ResetBrush();
    }

    private void OnPointAdded(Vector2 previousvalue, Vector2 point)
    {
        lastPos = point;
        DrawingSingleton.Instance.AddAPoint(lastPos);
    }

    private void OnDrawerSelect(ulong drawerId)
    {
        print("Drawer selection event received: " + drawerId);
        isDrawer = OwnerClientId == drawerId;
        controller.UpdateBackgroundColor(isDrawer);
        controller.ResetAnswers();
        if (isDrawer)
        {
            ShowMessage(IsOwner ? "You are the drawer" : $"{playerName.Value} is the drawer!!!");
            DrawingSingleton.Instance.ResetCanvas();
        }

        if (IsOwner && !isDrawer)
        {
            var gameStartedMenu = NetworkButtonManager.Instance.GameStartMenu;
            gameStartedMenu.SetActive(true);
            answerInputField = gameStartedMenu.transform.GetComponentInChildren<TMP_InputField>();
            answerInputField.onSubmit.RemoveAllListeners();
            answerInputField.onSubmit.AddListener((ans) =>
            {
                answerInputField.text = "";
                answerInputField.Select();
                OnPlayerAnswerSubmitServerRpc(ans);
            });
        }
    }

    private void OnNameChanged(FixedString64Bytes previousvalue, FixedString64Bytes newvalue)
    {
        controller.SetUIName(playerName.Value.ToString());
    }

    [ServerRpc]
    private void UpdatePlayerNameServerRpc(FixedString64Bytes newName)
    {
        print("Updating player name method called");
        playerName.Value = newName;
    }

    private void Draw()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateBrushServerRpc(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
        else if (Input.GetMouseButton(0))
        {
            PointToMousePos();
        }
        else
        {
            ResetBrushServerRpc();
        }
    }

    private void PointToMousePos()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (lastPos != mousePos)
        {
            AddPointServerRpc(mousePos);
            lastPos = mousePos;
        }
    }

    [ServerRpc]
    private void AddPointServerRpc(Vector2 mousePos)
    {
        pointHolder.Value = mousePos;
    }

    [ServerRpc]
    private void ResetBrushServerRpc()
    {
        if (DrawingSingleton.Instance.BrushExists())
            resetBrushEvent.Value = Random.Range(0, 99999);
    }

    [ServerRpc]
    private void CreateBrushServerRpc(Vector3 mousePos)
    {
        createBrushEvent.Value = mousePos;
    }

    [ServerRpc]
    public void OnPlayerAcceptServerRpc(ulong playerId)
    {
        PlayerLobbyController.Instance.OnStartButtonClicked(playerId);
    }

    [ServerRpc]
    public void OnPlayerAnswerSubmitServerRpc(FixedString64Bytes word)
    {
        lastAnswer.Value = word;
        PlayerLobbyController.Instance.CompareAnswers(word.ToString().ToLower(), OwnerClientId);
    }

    private void Update()
    {
        if (IsOwner && isDrawer)
        {
            Draw();
        }
    }
}