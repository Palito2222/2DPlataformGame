using Mirror;
using UnityEngine;

public class CharacterSelector : NetworkBehaviour
{
    [SerializeField] private GameObject panelSelector;
    [SerializeField] private CharacterData character1;
    [SerializeField] private CharacterData character2;
    [SerializeField] private CharacterData character3;
    [SerializeField] private CharacterData character4;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private CharacterData currentCharacter;

    [SyncVar(hook = nameof(OnPlayerCharacterChanged))]
    private string playerCharacterID;

    public override void OnStartLocalPlayer()
    {
        panelSelector.SetActive(true);
    }

    public void onSelectRed()
    {
        currentCharacter = character1;
        playerMovement.SetCharacterData(currentCharacter);
        panelSelector.SetActive(false);

        // Llamar al método de comando para actualizar el ID en el servidor
        CmdSetPlayerCharacter(currentCharacter.uniqueID);
    }

    public void onSelectBlue()
    {
        currentCharacter = character2;
        playerMovement.SetCharacterData(currentCharacter);
        panelSelector.SetActive(false);

        // Llamar al método de comando para actualizar el ID en el servidor
        CmdSetPlayerCharacter(currentCharacter.uniqueID);
    }

    public void onSelectGreen()
    {
        currentCharacter = character3;
        playerMovement.SetCharacterData(currentCharacter);
        panelSelector.SetActive(false);

        // Llamar al método de comando para actualizar el ID en el servidor
        CmdSetPlayerCharacter(currentCharacter.uniqueID);
    }

    public void onSelectPink()
    {
        currentCharacter = character4;
        playerMovement.SetCharacterData(currentCharacter);
        panelSelector.SetActive(false);

        // Llamar al método de comando para actualizar el ID en el servidor
        CmdSetPlayerCharacter(currentCharacter.uniqueID);
    }


    private CharacterData GetCharacterDataByID(string characterID)
    {
        // Buscar el ScriptableObject por su ID en tu lista de personajes disponibles
        CharacterData[] characters = { character1, character2, character3, character4 };
        foreach (CharacterData character in characters)
        {
            if (character.uniqueID == characterID)
            {
                return character;
            }
        }

        return null;
    }

    [Command]
    private void CmdSetPlayerCharacter(string characterID)
    {
        // Actualizar el CharacterData en el servidor
        playerCharacterID = characterID;
    }

    private void OnPlayerCharacterChanged(string oldValue, string newValue)
    {
        // Obtener el ScriptableObject correspondiente al ID recibido
        CharacterData selectedCharacter = GetCharacterDataByID(newValue);

        // Actualizar el CharacterData en el cliente local
        playerMovement.SetCharacterData(selectedCharacter);

        // Actualizar el Sprite del personaje en el cliente local
        spriteRenderer.sprite = selectedCharacter.characterSprite;
    }
}