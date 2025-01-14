using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : NetworkBehaviour
{

    [SerializeField]
    private Button startHostButton;

    [SerializeField]
    private Button startClientButton;

    [SerializeField]
    private TextMeshProUGUI playersInGameText;
    private void Awake()
    {
        Cursor.visible = true;
    }

    private void Update()
    {
        playersInGameText.text = $"Players in game: {PlayerManager.Instance.PlayersInGame}";
    }

    private void Start()
    {
        startHostButton.onClick.AddListener(() => 
        {
            if(NetworkManager.Singleton.StartHost()){
                Debug.Log("Host started...");
            }
            else{
                Debug.Log("Host could not be started.");
            }
        });

        startClientButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient()){
                Debug.Log("Client started...");
            }
            else{
                Debug.Log("Client could not be started.");
            }
        });
    }
}
