using Mirror;
using TMPro;
using UnityEngine;

public class PlayerNameTag : NetworkBehaviour
{
    [SerializeField] private TextMeshPro playerNameText;

    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    private string playerName = "";

    private void OnPlayerNameChanged(string oldValue, string newValue)
    {
        playerNameText.text = newValue;
    }

    [Server]
    public void SetPlayerName(string newName)
    {
        playerName = newName;
    }
}
