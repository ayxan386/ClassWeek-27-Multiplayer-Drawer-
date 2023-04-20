using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    private NetworkVariable<FixedString64Bytes> playerName = new();
    private NetworkVariable<Vector3> points = new();

    private TextMeshProUGUI joinLog;

    public override void OnNetworkSpawn()
    {
        playerName.OnValueChanged += OnPlayerNameChanged;
        points.OnValueChanged += newPointAdded;
        transform.parent = GameObject.Find("Join log").transform;
    }

    private void newPointAdded(Vector3 previousvalue, Vector3 newvalue)
    {
    }

    private void OnPlayerNameChanged(FixedString64Bytes previousvalue, FixedString64Bytes newvalue)
    {
        playerNameText.text = newvalue.ToString();
    }

    void Update()
    {
        if (string.IsNullOrEmpty(playerName.Value.ToString()))
        {
            UpdatePlayerNameServerRpc(NetworkButtonManager.Instance.PlayerName);
        }

        if (IsOwner && Input.GetMouseButton(0))
        {
            UpdatePointsServerRpc(Random.insideUnitSphere * 3);
        }
    }

    [ServerRpc]
    private void UpdatePlayerNameServerRpc(FixedString64Bytes newName)
    {
        playerName.Value = newName;
    }

    [ServerRpc]
    private void UpdatePointsServerRpc(Vector3 newPoint)
    {
        points.Value = newPoint;
    }
}