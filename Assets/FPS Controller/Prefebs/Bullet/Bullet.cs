using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public float Speed;
    public ParticleSystem OnHitAffect;
    Rigidbody RB;
    public float Damage;
    public FPSCharacterManager[] NetObjects;
    public NetworkVariable<int> OwnerID = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public AudioClip FireSound;

    void Awake()
    {
        RB = GetComponent<Rigidbody>();
        if (FireSound != null)
        {
            // TODO: Multiply with SfxVolume
            // TODO: Cap volume to 1
            AudioSource.PlayClipAtPoint(FireSound, transform.position, 1.0f * SettingsManager.MasterVolume);
        }
    }

    void Start()
    {
        if (IsServer)
        {
            SetInitialVelocityServerRpc(transform.forward, Speed);
        }
    }

    [ServerRpc]
    private void SetInitialVelocityServerRpc(Vector3 direction, float speed)
    {
        // Apply velocity on the server
        RB.velocity = direction * speed;
        // Synchronize the velocity with clients
        SetInitialVelocityClientRpc(direction, speed);
    }

    [ClientRpc]
    private void SetInitialVelocityClientRpc(Vector3 direction, float speed)
    {
        // Apply velocity on the clients
        RB.velocity = direction * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        print("Hit to : " + collision.transform.name);

        if (collision.transform.tag == "Bullet")
            print("<color=red>Caution: Hit to A Bullet!!!");

        if (collision.gameObject.GetComponent<Health>() != null)
        {
            if (IsServer)
            {
                print("Damage");
                collision.gameObject.GetComponent<Health>().TakeDamageServerRpc(Damage);

                if (collision.gameObject.GetComponent<Health>().CurrentHealth.Value <= 0 && !collision.gameObject.GetComponent<Health>().isDeadCounted)
                {
                    print("Eliminated!!");
                    collision.gameObject.GetComponent<Health>().isDeadCounted = true;
                    AddEliminationServerRpc(OwnerID.Value);
                }
            }
        }
        else
        {
            print("Health Didn't found");
        }

        if (OnHitAffect != null)
        {
            Instantiate(OnHitAffect, collision.contacts[0].point, Quaternion.LookRotation(collision.contacts[0].normal));
        }



        if (IsHost)
        {
            DestroyBulletClientRpc();
            Destroy(this.gameObject);
        }
    }

    [ServerRpc]
    private void AddEliminationServerRpc(int ownerId)
    {
        FPSCharacterManager[] objs = FindObjectsOfType<FPSCharacterManager>();
        foreach (var obj in objs)
        {
            if (obj.OwnerClientId == (ulong)ownerId)
            {
                obj.AddEliminations(1);
            }
        }
    }

    [ClientRpc]
    private void DestroyBulletClientRpc()
    {
        Destroy(this.gameObject);
    }
}
