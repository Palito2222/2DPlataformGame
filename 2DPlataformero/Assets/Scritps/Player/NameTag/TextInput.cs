using TMPro;
using UnityEngine;

public class TextInput : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private GameObject inputFieldGO;
    [SerializeField] private PlayerNameTag playerNameTag;
    [SerializeField] private PlayerMovement playerMovement;

    private void Awake()
    {
        playerMovement.enabled = false;
    }

    private void Start()
    {
        // Configura el evento para manejar el cambio de texto
        inputField.onEndEdit.AddListener(OnInputEndEdit);
    }

    private void OnInputEndEdit(string inputText)
    {
        playerNameTag.SetPlayerName(inputText);
        inputFieldGO.SetActive(false);
        playerMovement.enabled = true;
    }
}
