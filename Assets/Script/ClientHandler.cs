using Unity.Netcode;
using UnityEngine;

public class ClientHandler : MonoBehaviour
{
    private void Start(){
        if (NetworkManager.Singleton.IsHost){
            Destroy(this);
            return;
        }

        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void OnClientDisconnected(ulong clientId){
        if (clientId == NetworkManager.Singleton.LocalClientId){
            Debug.Log("Disconnected from host. Returning to main menu...");
            HandleHostDisconnection();
        }
    }

    private void HandleHostDisconnection(){
        // Implement logic to return to the main menu or inform the player
        UnityEngine.SceneManagement.SceneManager.LoadScene(0);
    }

    private void OnDestroy(){
        if (NetworkManager.Singleton != null){
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }
}
