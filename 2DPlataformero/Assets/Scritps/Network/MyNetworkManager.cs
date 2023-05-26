using UnityEngine;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    public override void OnClientConnect()
    {
        base.OnClientConnect();

        Debug.Log("Me conecté al servidor");
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        Debug.Log("Un Jugador ha entrado " + numPlayers);
    }
}
