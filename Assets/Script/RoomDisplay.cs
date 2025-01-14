using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class RoomDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text roomName;
    [SerializeField] private TMP_Text playerCount;
    [SerializeField] private TMP_Text mapName;
    [SerializeField] private GameObject joinButton;

    private Lobby roomDisplayed;

    public void DisplayRoom(Lobby toDisplay){
        this.roomDisplayed = toDisplay;

        roomName.text = toDisplay.Data["RoomName"].Value;
        playerCount.text = $"Players: {toDisplay.Players.Count}/{toDisplay.MaxPlayers}";
        mapName.text = toDisplay.Data["MapName"].Value;

        joinButton.SetActive(toDisplay.Players.Count < toDisplay.MaxPlayers);
    }

    public void JoinRoom(){
        LobbyManager.Instance.JoinRoom(roomDisplayed, false);
    }
}
