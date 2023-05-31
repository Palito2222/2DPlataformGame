using Mirror;
using TMPro;
using UnityEngine;

public class PlayerNameInput : NetworkBehaviour
{
    [SerializeField] private TextMeshPro playerNameText;

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject namePanel;
    [SerializeField] private PlayerController playerMovement;

    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    public string playerName = "";

    public override void OnStartLocalPlayer()
    {
        playerMovement.OnDisableMovement();
        namePanel.gameObject.SetActive(true);
        inputField.onEndEdit.AddListener(OnInputEndEdit);
    }

    private void OnInputEndEdit(string inputText)
    {
        playerName = inputText;
        playerNameText.text = playerName;
        namePanel.gameObject.SetActive(false);

        CmdSetPlayerName(playerName);
        playerMovement.OnEnableMovement();
    }

    [Command]
    private void CmdSetPlayerName(string newName)
    {
        playerName = newName;
    }

    private void OnPlayerNameChanged(string oldValue, string newValue)
    {
        playerNameText.text = newValue;
    }
}