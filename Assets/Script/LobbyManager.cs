using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class LobbyManager : MonoBehaviour
{
    public bool Loading { get; private set; } = false;
    public static LobbyManager Instance { get; private set; }

    [SerializeField] private GameObject roomDisplayPrefab;
    [SerializeField] private Transform roomDisplayParent;
    [SerializeField] private GameObject refreshPanel;

    // This is automatically deleted if not in Editor
    [SerializeField] private GameObject deleteLobbiesButton;

    private List<GameObject> roomDisplays = new List<GameObject>();

    private async void Awake() {
        Instance = this;

        Loading = true;

        try{
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn){
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in anonymously.");
            }

            Debug.Log("Unity Services initialized successfully.");
        }
        catch (Exception ex){
            Debug.LogError($"Failed to initialize Unity Services: {ex.Message}");
        }
        
#if !UNITY_EDITOR
        Destroy(deleteLobbiesButton);
#endif

        Loading = false;
    }

    public void RefreshRooms(){
        RefreshRoomsAsync();
    }

    public void JoinRoom(Lobby lobby, bool isHost)
    {
        if (Loading) return;

        // TODO: Show loading or something..
        Loading = true;

        string hostIP = lobby.Data["HostIP"].Value;
        int port = int.Parse(lobby.Data["Port"].Value);
        string mapName = lobby.Data["MapName"].Value;

        // Configure NetworkManager to join the host
        var unityTransport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        unityTransport.ConnectionData.Address = hostIP;
        unityTransport.ConnectionData.Port = (ushort)port;

        if (!isHost){
            HostHandler.currentLobbyId = lobby.Id;
            NetworkManager.Singleton.StartClient();
        }
        
        Loading = false;
    }

    private async void RefreshRoomsAsync(){
        refreshPanel.SetActive(true);

        for (var i = 0; i < roomDisplays.Count; i++){
            Destroy(roomDisplays[i]);
        }

        roomDisplays.Clear();

        var roomResults = await GetAvailableRooms();

        foreach (var lobby in roomResults){
            Debug.Log($"Lobby: {lobby.Name}, Players: {lobby.Players.Count}/{lobby.MaxPlayers}");
            var display = Instantiate(roomDisplayPrefab, roomDisplayParent).GetComponent<RoomDisplay>();
            display.DisplayRoom(lobby);
            roomDisplays.Add(display.gameObject);
        }

        refreshPanel.SetActive(false);
    }

    public void CreateRoomButton(){
        CreateRoomAsync();
    }

    private async void CreateRoomAsync(){
        // TODO: Functionality to allow the player to add room name of their own
        // TODO: Functionality to allow the player to select max players of their own
        // TODO: Functionality to allow the player to select map of their own

        // First create the room..
        var room = await CreateRoom($"Room {Random.Range(0, 1000):D4}", 16, "DefaultMap");

        // Then join it..
        JoinRoom(room, true);

        Debug.Log($"Loading scene: {room.Data["MapName"].Value}");
        NetworkManager.Singleton.SceneManager.LoadScene(room.Data["MapName"].Value, LoadSceneMode.Single);
    }

    public async Task<Lobby> CreateRoom(string roomName, int maxPlayers, string mapName){
        try{
            NetworkManager.Singleton.StartHost();
            var options = new CreateLobbyOptions();

            var lobby = await Lobbies.Instance.CreateLobbyAsync(
                roomName,
                maxPlayers,
                new CreateLobbyOptions{
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>{
                        { "RoomName", new DataObject(DataObject.VisibilityOptions.Public, roomName) },
                        { "MapName", new DataObject(DataObject.VisibilityOptions.Public, mapName) },
                        { "HostIP", new DataObject(DataObject.VisibilityOptions.Public, GetLocalIPAddress()) },
                        { "Port", new DataObject(DataObject.VisibilityOptions.Public, "7777") }
                    }
                });

            Debug.Log($"Lobby created: {lobby.Id} Name: {lobby.Data["RoomName"].Value} Map: {lobby.Data["MapName"].Value}");
            return lobby;
        }
        catch (LobbyServiceException e){
            Debug.LogError($"Failed to create lobby: {e.Message}");
            return null;
        }
    }

    public async Task<List<Lobby>> GetAvailableRooms(){
        try{
            var queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            return queryResponse.Results;
        }
        catch (LobbyServiceException e){
            Debug.LogError($"Failed to query lobbies: {e.Message}");
            return null;
        }
    }

#if UNITY_EDITOR
    // FOR DEBUGGING _ DO NOT DEPLOY TO PLAYERS!
    public void DeleteLobbiesButton(){
        DeleteLobbies();
    }

    // FOR DEBUGGING _ DO NOT DEPLOY TO PLAYERS!
    public async void DeleteLobbies(){
        var lobbies = await GetAvailableRooms();

        foreach (var lobby in lobbies)
            await Lobbies.Instance.DeleteLobbyAsync(lobby.Id);
    }
#endif

    // TODO: Change to non-LAN logic.
    private string GetLocalIPAddress() {
        return "127.0.0.1";
        // return IPManager.GetIP(ADDRESSFAM.IPv4);
    }
}
