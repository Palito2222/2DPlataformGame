using Mirror;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InGameUI : NetworkBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    [Header("Referencias UI in Game")]
    //Referencias para la UI In Game
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject player;
    [SerializeField] private Button uiButton;
    [SerializeField] private GameObject uiButtonGO;
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private Rigidbody2D rb;

    [Header("Referencias Chat in Game")]
    //Referencias para el Chat In Game
    [SerializeField] private PlayerNameInput playerNameInput;
    [SerializeField] private GameObject chatUI;
    [SerializeField] private TMP_Text chatText;
    [SerializeField] private TMP_InputField inputField;

    private static event Action<string> onMessage;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Obtener referencia al NetworkManager desde la propiedad singleton
        networkManager = NetworkManager.singleton;

        //Activar la UI del Chat al iniciar el cliente Local
        chatUI.SetActive(true);

        //Activar el botón de la UI al iniciar el cliente Local
        uiButtonGO.SetActive(true);

        //Nos suscribimos al evento OnMessage en el cliente Local
        onMessage += HandleNewMessage;
    }

    private void HandleNewMessage(string message)
    {
        chatText.text += message;
    }

    [Client]
    public void Send(string message)
    {
        //Solo si presionas el Enter
        if (!Input.GetKeyDown(KeyCode.Return))
        {
            return;
        }

        //Solo si el mensaje no es Nulo o en Blanco
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        //Le decimos al servidor que envíe el mensaje
        CmdSendMessage(message);

        //Una vez se envíe el mensaje, limpiamos el área de escritura
        inputField.text = string.Empty;
    }

    [Command]
    private void CmdSendMessage(string message)
    {
        //Formato de mensaje
        RpcHandleMessage($"[{playerNameInput.playerName}]: {message}");
    }

    [ClientRpc]
    private void RpcHandleMessage(string message)
    {
        onMessage?.Invoke($"\n{message}");
    }

    //Al Presionar el boton del engranaje inGame
    public void OnClickUIButton()
    {
        uiButtonGO.SetActive(false);
        chatUI.SetActive(false);
        uiPanel.SetActive(true);

        playerController.OnDisableMovement();
        rb.velocity = Vector2.zero;
    }

    //Al presionar el boton de Atras en el Menu inGame
    public void OnClickBackButton()
    {
        uiPanel.SetActive(false);
        chatUI.SetActive(true);
        uiButtonGO.SetActive(true);

        playerController.OnEnableMovement();
    }

    //Al presionar te desconectas de la partida
    public void OnClickDisconectButton()
    {
        //Si es Host y Cliente
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            SceneManager.LoadSceneAsync("WaitingScreen");
            networkManager.StopHost();
        }
        //Si solo es cliente
        else if (NetworkClient.isConnected)
        {
            SceneManager.LoadSceneAsync("WaitingScreen");
            networkManager.StopClient();
        }
    }

    public void OnSelectChatInput()
    {
        playerController.OnDisableMovement();
        rb.velocity = Vector2.zero;
    }

    public void OnDeselectChatInput()
    {
        playerController.OnEnableMovement();
    }
}
