using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CameraMovement : NetworkBehaviour
{
    public float Sensitivity;
    [Header("Other Refs")]
    public Transform HorizontalTarget;
    public Transform VerticalTarget;
    public Transform LookAtTarget;
    float X_Axis;
    float Y_Axis;

    void Start()
    {
        if (HorizontalTarget == null)
            HorizontalTarget = transform;
    }

    float X_Rot;
    float Y_Rot;

    Vector3 LookAtPoint()
    {
        RaycastHit hit;
        if (Physics.Raycast(VerticalTarget.position, VerticalTarget.forward, out hit))
        {
            return hit.point;
        }
        return Vector3.zero;
    }

    void Update()
    {        
        // Only execute for the local player
        if (!IsLocalPlayer){
            return;
        }

        X_Axis = Input.GetAxis("Mouse X") * Sensitivity * Time.deltaTime;
        Y_Axis = Input.GetAxis("Mouse Y") * Sensitivity * Time.deltaTime;

        X_Rot += X_Axis;
        Y_Rot -= Y_Axis;

        if (Y_Rot > 40)
        {
            Y_Rot = 40;
        }
        if (Y_Rot < -40)
        {
            Y_Rot = -40;
        }

        // Call the RPC to update the rotation on all clients
        UpdateRotationServerRpc(X_Rot, Y_Rot);
    }

    // Server RPC to update the rotation
    [ServerRpc]
    void UpdateRotationServerRpc(float xRot, float yRot, ServerRpcParams rpcParams = default)
    {
        // Call the client RPC to update the rotation on all clients
        UpdateRotationClientRpc(xRot, yRot);
    }

    // Client RPC to update the rotation
    [ClientRpc]
    void UpdateRotationClientRpc(float xRot, float yRot, ClientRpcParams rpcParams = default)
    {
        HorizontalTarget.rotation = Quaternion.Euler(0, xRot, 0);
        VerticalTarget.eulerAngles = new Vector3(yRot, VerticalTarget.eulerAngles.y, VerticalTarget.eulerAngles.z);

        LookAtTarget.position = new Vector3(LookAtPoint().x, LookAtPoint().y, LookAtPoint().z);
    }
}