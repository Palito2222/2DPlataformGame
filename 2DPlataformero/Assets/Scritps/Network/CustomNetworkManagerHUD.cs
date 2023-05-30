using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class CustomNetworkManagerHUD : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;
    [SerializeField] private Button hostClientConnectButton;
    [SerializeField] private Button clientConnectButton;
    [SerializeField] private TMP_InputField ipAddressInput;

    private void Awake()
    {
        // Obtener referencia al NetworkManager desde la propiedad singleton
        networkManager = NetworkManager.singleton;
    }

    public void onClickHostAndClientButton()
    {
        if (NetworkServer.active || NetworkClient.isConnected)
        {
            return; // Evitar iniciar el servidor o el cliente si ya está activo/conectado
        }

        SceneManager.LoadSceneAsync("WaitingScreen");
        SceneManager.LoadSceneAsync("InGame", LoadSceneMode.Additive).completed += OnInGameHostLoadComplete;
    }

    private void OnInGameHostLoadComplete(AsyncOperation asyncOperation)
    {
        // La escena "InGame" se ha cargado completamente
        networkManager.StartHost();
        SceneManager.UnloadSceneAsync("WaitingScreen");
        asyncOperation.completed -= OnInGameHostLoadComplete; // Remover el evento completado para evitar llamadas duplicadas
    }

    public void onEndEditIP(string newIpAdress)
    {
        networkManager.networkAddress = newIpAdress;
        Debug.Log("La IP es: " + networkManager.networkAddress);
    }

    public void onClickClientButton()
    {
        if (NetworkServer.active || NetworkClient.isConnected)
        {
            return; // Evitar iniciar el servidor o el cliente si ya está activo/conectado
        }

        SceneManager.LoadSceneAsync("WaitingScreen");
        SceneManager.LoadSceneAsync("InGame", LoadSceneMode.Additive).completed += OnInGameClientLoadComplete;
    }

    private void OnInGameClientLoadComplete(AsyncOperation asyncOperation)
    {
        // La escena "InGame" se ha cargado completamente
        networkManager.StartClient();
        SceneManager.UnloadSceneAsync("WaitingScreen");
        asyncOperation.completed -= OnInGameClientLoadComplete; // Remover el evento completado para evitar llamadas duplicadas
    }
}
