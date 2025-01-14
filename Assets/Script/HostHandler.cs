using Unity.Services.Lobbies;
using Unity.Netcode;
using UnityEngine;

public class HostHandler : MonoBehaviour
{
    public static string currentLobbyId;

    private void Awake() {
        if (!NetworkManager.Singleton.IsHost) Destroy(this);
    }

    private void OnApplicationQuit() {
        CloseRoom();
    }

    public void CloseRoom()
    {
        Debug.Log("Closing room...");

        // Notify all clients
        NotifyClientsRoomClosing();

        // Stop the host
        NetworkManager.Singleton.Shutdown();

        // Remove the lobby
        DeleteLobby();
    }

    private async void DeleteLobby()
    {
        try
        {
            if (!string.IsNullOrEmpty(currentLobbyId))
            {
                await Lobbies.Instance.DeleteLobbyAsync(currentLobbyId);
                Debug.Log("Lobby deleted successfully.");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Failed to delete lobby: {e.Message}");
        }
    }

    private void NotifyClientsRoomClosing()
    {
        // Implement a custom message to notify clients
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId) // Exclude the host
            {
                // Example: Send a custom message (requires custom messaging setup)
                // CustomMessagingManager.SendRoomClosingMessage(clientId);
            }
        }
    }
}
