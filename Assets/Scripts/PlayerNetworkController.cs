using TMPro;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour
{
    [SerializeField] private PlayerController playerPrefab;

    public NetworkVariable<FixedString64Bytes> playerName = new();
    private NetworkVariable<Vector2> pointHolder = new();
    private NetworkVariable<int> resetBrushEvent = new();
    private NetworkVariable<Vector2> createBrushEvent = new();

    private PlayerController controller;
    private bool isDrawer;
    Vector2 lastPos;
    private TextMeshProUGUI eventMessageText;
    private NetworkAnimator eventMessageAnimator;

    public override void OnNetworkSpawn()
    {
        controller = Instantiate(playerPrefab, GameObject.Find("Players").transform);
        controller.SetUIName(playerName.Value.ToString());
        playerName.OnValueChanged += OnNameChanged;
        pointHolder.OnValueChanged += OnPointAdded;
        resetBrushEvent.OnValueChanged += OnResetBrush;
        createBrushEvent.OnValueChanged += OnBrushCreate;
        StartButton.OnAcceptButtonPressed += OnAcceptButtonPressed;

        eventMessageText = GameObject.Find("Event message").GetComponent<TextMeshProUGUI>();

        if (IsOwner && string.IsNullOrEmpty(controller.playerNameText.text))
        {
            UpdatePlayerNameServerRpc(NetworkButtonManager.Instance.PlayerName);
        }

        PlayerLobbyController.OnDrawerSelect += OnDrawerSelect;
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
        if (isDrawer)
        {
            eventMessageText.text =IsOwner ? "You are the drawer" :  $"{playerName.Value} is the drawer!!!"; 
            eventMessageText.gameObject.GetComponent<Animator>().SetTrigger("scale");
            DrawingSingleton.Instance.ResetCanvas();
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

    private void Update()
    {
        if (IsOwner && isDrawer)
        {
            Draw();
        }
    }
}