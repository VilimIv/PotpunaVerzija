using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerManager : MonoBehaviour
{
    NetworkObject networkObject;
    public Camera PlayerCamera;
    public Canvas CanvasUI;
    private void Awake(){
        networkObject = GetComponent<NetworkObject>();
    }

    private void Start(){
        if (!networkObject.IsOwner){
            PlayerCamera.gameObject.GetComponent<AudioListener>().enabled = false;
            PlayerCamera.enabled = false;
            GetComponent<PlayerHud>().enabled = false;
            CanvasUI.gameObject.SetActive(false);
            GetComponent<CameraMovement>().enabled = false;
        }
        else print(transform.name + "Is the Owner");
    }
}