using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerNetworkController : NetworkBehaviour
{
    [SerializeField] private PlayerController playerPrefab;

    public NetworkVariable<FixedString64Bytes> playerName = new();
    private NetworkVariable<Vector2> pointHolder = new();
    private NetworkVariable<bool> resetBrushEvent = new();
    private NetworkVariable<bool> createBrushEvent = new();

    private PlayerController controller;
    private bool isDrawer;
    Vector2 lastPos;

    public override void OnNetworkSpawn()
    {
        controller = Instantiate(playerPrefab, GameObject.Find("Players").transform);
        controller.SetUIName(playerName.Value.ToString());
        playerName.OnValueChanged += OnNameChanged;
        pointHolder.OnValueChanged += OnPointAdded;
        resetBrushEvent.OnValueChanged += OnResetBrush;
        createBrushEvent.OnValueChanged += OnBrushCreate;

        if (IsOwner && string.IsNullOrEmpty(controller.playerNameText.text))
        {
            UpdatePlayerNameServerRpc(NetworkButtonManager.Instance.PlayerName);
        }

        if (IsOwner)
        {
            NetworkButtonManager.OnDrawerSelect += OnDrawerSelect;
        }
    }

    private void OnBrushCreate(bool previousvalue, bool newvalue)
    {
        print("Brush create event received: " + newvalue);
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        DrawingSingleton.Instance.CreateBrush(mousePos);
    }

    private void OnResetBrush(bool previousvalue, bool newvalue)
    {
        print("Brush reset event received: " + newvalue);
        DrawingSingleton.Instance.ResetBrush();
    }

    private void OnPointAdded(Vector2 previousvalue, Vector2 point)
    {
        // print("Point add event received: " + point);
        lastPos = point;
        DrawingSingleton.Instance.AddAPoint(lastPos);
    }

    private void OnDrawerSelect(ulong drawerId)
    {
        print("Drawer selection event received: " + drawerId);
        isDrawer = OwnerClientId == drawerId;
    }

    private void OnNameChanged(FixedString64Bytes previousvalue, FixedString64Bytes newvalue)
    {
        controller.SetUIName(playerName.Value.ToString());
    }

    [ServerRpc]
    private void UpdatePlayerNameServerRpc(FixedString64Bytes newName)
    {
        print("RPC method called");
        playerName.Value = newName;
    }

    public void Draw()
    {
        if (Input.GetMouseButtonDown(0))
        {
            CreateBrushServerRpc();
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

    void PointToMousePos()
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
        resetBrushEvent.Value = true;
    }

    [ServerRpc]
    private void CreateBrushServerRpc()
    {
        createBrushEvent.Value = true;
    }

    private void Update()
    {
        if (IsOwner && isDrawer)
        {
            Draw();
        }
    }
}