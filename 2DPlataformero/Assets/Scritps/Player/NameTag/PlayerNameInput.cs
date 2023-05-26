using Mirror;
using TMPro;
using UnityEngine;

public class PlayerNameInput : NetworkBehaviour
{
    [SerializeField] private TextMeshPro playerNameText;

    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private PlayerMovement playerMovement;

    [SyncVar(hook = nameof(OnPlayerNameChanged))]
    private string playerName = "";

    public override void OnStartLocalPlayer()
    {
        playerMovement.enabled = false;
        inputField.gameObject.SetActive(true);
        inputField.onEndEdit.AddListener(OnInputEndEdit);
    }

    private void OnInputEndEdit(string inputText)
    {
        playerName = inputText;
        playerNameText.text = playerName;
        inputField.gameObject.SetActive(false);

        CmdSetPlayerName(playerName);
        playerMovement.enabled = true;
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