using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class FPSCharacterManager : NetworkBehaviour
{
    public List<GunDetails> Weapons;
    public List<ThrowableDetails> Throwables;
    public OtherRefs Refrences;
    public NetworkVariable<int> CurruntWeapon = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public NetworkVariable<int> Eliminations = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [HideInInspector] public bool CanFire;
    [HideInInspector] public bool CanSwitch;

    float DefaultFOV;

    void Awake()
    {
        CanFire = true;
        CanSwitch = true;
        DefaultFOV = Refrences.PlayerCamera.gameObject.GetComponent<Camera>().fieldOfView;
        UpdateEliminations(0, 0);
    }
    private void LateUpdate()
    {
        Refrences.WeaponRootPose.LookAt(Refrences.TargetPos);
        Refrences.WeaponRootPose.localEulerAngles = new Vector3(0, Refrences.WeaponRootPose.eulerAngles.x, 0);
        Refrences.WeaponRootObjcts.LookAt(Refrences.TargetPos);

    }

    public override void OnNetworkSpawn()
    {
        CurruntWeapon.OnValueChanged += WeaponValueChaged;
        Eliminations.OnValueChanged += UpdateEliminations;
    }

    void UpdateEliminations(int OldValue, int NewValue)
    {
        Refrences.KillsText.text = "<color=yellow>Eliminations : </color>" + Eliminations.Value.ToString();
    }

    public void AddEliminations(int Count)
    {
        print(Count + " Elimination has been added");
        if (IsServer)
        {
            Eliminations.Value += Count;
        }
    }

    void Start()
    {
        if (!GetComponent<NetworkObject>().IsOwner)
            return;

        Cursor.lockState = CursorLockMode.Locked;
        FillAmmo();
        GrabWeapon(0);
        CurruntWeapon.Value = 0;
        UpdateThrowAblesAmmo();
        UpdateAmmo();
    }
    public void WeaponValueChaged(int OldValue, int NewValue)
    {
        DIsableAllWeapons();
        Weapons[CurruntWeapon.Value].GunObject.gameObject.SetActive(true);
        UpdateAmmo();
        CanFire = true;
    }
    public void HealthValueChaged(float OldValue, float NewValue)
    {
        DIsableAllWeapons();
        Weapons[CurruntWeapon.Value].GunObject.gameObject.SetActive(true);
        UpdateAmmo();
        CanFire = true;
    }
    void FillAmmo()
    {
        foreach(GunDetails weapon in Weapons)
        {
            weapon.CurruntMagAmmo = weapon.MagCapacity;
        }
    }
    void Update()
    {
        if (!GetComponent<NetworkObject>().IsOwner || GameManager.MatchEnded)
            return;

        SwitchGunChecks();
        ShootChecks();
        ThrowwableInputs();

        if(Refrences.CharcaterAniamtor.transform.localPosition != Vector3.zero)
        {
            Refrences.CharcaterAniamtor.transform.localPosition = Vector3.zero;
        }
    }

    #region Throables Input Checks
    static GameObject ToThrough;
    static Transform ThrowPoint;
    void ThrowwableInputs()
    {
        for (int i = 0; i < Throwables.Count; i++)
        {
            if (CanFire)
            {
                if (Input.GetKeyDown(Throwables[i].InputKey))
                {
                    if (Throwables[i].Amount <= 0)
                        return;

                    ToThrough = Throwables[i].ThrowablePrefeb;
                    ThrowPoint = Throwables[i].ThrowPoint;

                    Refrences.CharcaterAniamtor.SetInteger("WeaponType_int", Throwables[i].AnimatorType);
                    Refrences.CharcaterAniamtor.SetBool("Shoot_b", true);
                    DIsableAllWeapons();
                    Throwables[i].ThrowableObject.gameObject.SetActive(true);
                    CanSwitch = false;
                    Throwables[i].Amount--;
                    UpdateThrowAblesAmmo();
                    CanFire = false;
                }
            }
        }
    }

    public void Throw()
    {
        if (ToThrough == null || ThrowPoint == null)
            return;

        SpawnThrowableServerRpc(ThrowPoint.position, ThrowPoint.rotation);

        ToThrough = null;
        ThrowPoint = null;
    }
    #endregion

    public void GrabWeapon(int Index)
    {
        DIsableAllWeapons();
        Weapons[Index].GunObject.gameObject.SetActive(true);
        Refrences.CharcaterAniamtor.SetInteger("WeaponType_int", Weapons[Index].AnimatorType);
        Refrences.WeaponsAnimator.SetInteger("WeaponType_int", Weapons[Index].AnimatorType);
        Refrences.CharcaterAniamtor.SetBool("Shoot_b", false);
        Refrences.CharcaterAniamtor.SetBool("FullAuto_b", false);
        Refrences.WeaponsAnimator.SetBool("Shoot_b", false);
        Refrences.WeaponsAnimator.SetBool("FullAuto_b", false);
        Refrences.CharcaterAniamtor.SetBool("Reload_b", false);
        Refrences.WeaponsAnimator.SetBool("Reload_b", false);
        UpdateAmmo();
        CanFire = true;
       // AdjustCamera();
    }

    void AdjustCamera()
    {
        switch (Weapons[CurruntWeapon.Value].AnimatorType)
        {
            case 0:
                Refrences.PlayerCamera.position = Refrences.CameraPositions.IdlePos.position;
                Refrences.PlayerCamera.rotation = Refrences.CameraPositions.IdlePos.rotation;
                break;
            case 1:
                Refrences.PlayerCamera.position = Refrences.CameraPositions.HandGunPos.position;
                Refrences.PlayerCamera.rotation = Refrences.CameraPositions.HandGunPos.rotation;
                break;
            case 2:
                Refrences.PlayerCamera.position = Refrences.CameraPositions.AssultPos.position;
                Refrences.PlayerCamera.rotation = Refrences.CameraPositions.AssultPos.rotation;
                break;
            case 3:
                Refrences.PlayerCamera.position = Refrences.CameraPositions.AssultPos.position;
                Refrences.PlayerCamera.rotation = Refrences.CameraPositions.AssultPos.rotation;
                break;
            case 4:
                Refrences.PlayerCamera.position = Refrences.CameraPositions.AssultPos.position;
                Refrences.PlayerCamera.rotation = Refrences.CameraPositions.AssultPos.rotation;
                break;
            case 5:
                Refrences.PlayerCamera.position = Refrences.CameraPositions.SniperPos.position;
                Refrences.PlayerCamera.rotation = Refrences.CameraPositions.SniperPos.rotation;
                break;
            case 6:
                Refrences.PlayerCamera.position = Refrences.CameraPositions.SniperPos.position;
                Refrences.PlayerCamera.rotation = Refrences.CameraPositions.SniperPos.rotation;
                break;
            case 7:
                Refrences.PlayerCamera.position = Refrences.CameraPositions.AssultPos.position;
                Refrences.PlayerCamera.rotation = Refrences.CameraPositions.AssultPos.rotation;
                break;
        }
    }
    public void DIsableAllWeapons()
    {
        for (int i = 0; i < Weapons.Count; i++)
        {
            Weapons[i].GunObject.gameObject.SetActive(false);
        }
    }
    void ShowWeapon(int Index)
    {
        Weapons[Index].GunObject.gameObject.SetActive(true);
    }
    #region Inputs
    void SwitchGunChecks()
    {
        if (!CanSwitch)
            return;
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
        {
            if (CurruntWeapon.Value + 1 < Weapons.Count)
            {
                CurruntWeapon.Value++;
            }
            else
            {
                CurruntWeapon.Value = 0;
            }
            GrabWeapon(CurruntWeapon.Value);
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f)
        {
            if (CurruntWeapon.Value - 1 >= 0)
            {
                CurruntWeapon.Value--;
            }
            else
            {
                CurruntWeapon.Value = Weapons.Count - 1;
            }
            GrabWeapon(CurruntWeapon.Value);
        }
    }
    float ShootTimer;
    IEnumerator StopSingleAnimas()
    {
        yield return new WaitForEndOfFrame();
        Refrences.CharcaterAniamtor.SetBool("Shoot_b", false);
        Refrences.CharcaterAniamtor.SetBool("FullAuto_b", false);
        Refrences.WeaponsAnimator.SetBool("Shoot_b", false);
        Refrences.WeaponsAnimator.SetBool("FullAuto_b", false);
    }
    void ShootChecks()
    {
        if (Weapons[CurruntWeapon.Value].HasScope)
        {
            if (Input.GetMouseButtonDown(1))
            {
                Refrences.ScopeAimImage.SetActive(true);
                Refrences.PlayerCamera.GetComponent<Camera>().fieldOfView = 20;
                DIsableAllWeapons();
            }

            if (Input.GetMouseButtonUp(1))
            {
                Refrences.ScopeAimImage.SetActive(false);
                Refrences.PlayerCamera.GetComponent<Camera>().fieldOfView = DefaultFOV;
                ShowWeapon(CurruntWeapon.Value);
            }
        }

        if (!CanFire)
            return;

        if (Input.GetMouseButtonDown(0) && Weapons[CurruntWeapon.Value].CurruntMagAmmo > 0)
        {
            Refrences.CharcaterAniamtor.SetBool("Shoot_b", true);
            Refrences.WeaponsAnimator.SetBool("Shoot_b", true);
            if (Weapons[CurruntWeapon.Value].Type == GunType.Auto)
            {
                Refrences.CharcaterAniamtor.SetBool("FullAuto_b", true);
                Refrences.WeaponsAnimator.SetBool("FullAuto_b", true);
            }
            if(Weapons[CurruntWeapon.Value].CurruntMagAmmo > 0)
            {
                Shoot();
                if(Weapons[CurruntWeapon.Value].Type == GunType.Single)
                {
                    StartCoroutine(StopSingleAnimas());
                    switch (Weapons[CurruntWeapon.Value].AnimatorType)
                    {
                        case 1:
                            Refrences.CharcaterAniamtor.Play("Handgun_Shoot", 0, 0);
                            Refrences.WeaponsAnimator.Play("Handgun_Shoot", 0, 0);
                            break;
                        case 2:
                            Refrences.CharcaterAniamtor.Play("AK47_Auto_SingleShot", 0, 0);
                            Refrences.WeaponsAnimator.Play("AR01_Auto_SingleShot", 0, 0.4f);
                            break;
                        case 4:
                            Refrences.CharcaterAniamtor.Play("Shotgun_Shoot", 0, 0);
                            Refrences.WeaponsAnimator.Play("Shotgun_Shoot", 0, 0.4f);
                            break;

                    }
                }else if (Weapons[CurruntWeapon.Value].Type == GunType.Sniper)
                {
                    CanFire = false;
                }
            }
            else
            {
                StartReload();
            }
        }
        else if (Input.GetMouseButton(0))
        {
            if (Weapons[CurruntWeapon.Value].Type == GunType.Auto)
            {
                if (Weapons[CurruntWeapon.Value].CurruntMagAmmo > 0)
                {
                    ShootTimer += Time.deltaTime;
                    if (ShootTimer > Weapons[CurruntWeapon.Value].firerate)
                    {
                        Shoot();
                        ShootTimer = 0;
                    }
                }
                else if (Weapons[CurruntWeapon.Value].AmmoCapacity > 0)
                {
                    StartReload();
                }
                else
                {
                    Refrences.CharcaterAniamtor.SetBool("Shoot_b", false);
                    Refrences.CharcaterAniamtor.SetBool("FullAuto_b", false);
                    Refrences.WeaponsAnimator.SetBool("Shoot_b", false);
                    Refrences.WeaponsAnimator.SetBool("FullAuto_b", false);
                }
            }
        }else if(!Input.GetMouseButtonUp(0))
        {
            Refrences.CharcaterAniamtor.SetBool("Shoot_b", false);
            Refrences.CharcaterAniamtor.SetBool("FullAuto_b", false);
            Refrences.WeaponsAnimator.SetBool("Shoot_b", false);
            Refrences.WeaponsAnimator.SetBool("FullAuto_b", false);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            if (Weapons[CurruntWeapon.Value].AmmoCapacity > 0)
            {
                StartReload();
            }
        }
    }
    #endregion

    #region Shooting System
    GameObject Bullet;
    void Shoot()
    {
        Weapons[CurruntWeapon.Value].CurruntMagAmmo--;
        if(Weapons[CurruntWeapon.Value].Bullet != null && Weapons[CurruntWeapon.Value].ShootPoint != null)
        {
            SpawnBulletServerRpc(Weapons[CurruntWeapon.Value].ShootPoint.position, Weapons[CurruntWeapon.Value].ShootPoint.rotation);
        }

        UpdateAmmo();
    }
    void StartReload()
    {
        Refrences.CharcaterAniamtor.SetBool("Shoot_b", false);
        Refrences.CharcaterAniamtor.SetBool("FullAuto_b", false);
        Refrences.WeaponsAnimator.SetBool("Shoot_b", false);
        Refrences.WeaponsAnimator.SetBool("FullAuto_b", false);
        Refrences.CharcaterAniamtor.SetBool("Reload_b", true);
        Refrences.WeaponsAnimator.SetBool("Reload_b", true);
    }
    public void Reload()
    {
        int missingAmmo = Weapons[CurruntWeapon.Value].MagCapacity - Weapons[CurruntWeapon.Value].CurruntMagAmmo;
        if (Weapons[CurruntWeapon.Value].AmmoCapacity >= missingAmmo)
        {
            Weapons[CurruntWeapon.Value].CurruntMagAmmo = Weapons[CurruntWeapon.Value].MagCapacity;
            Weapons[CurruntWeapon.Value].AmmoCapacity -= missingAmmo;
        }
        else if (Weapons[CurruntWeapon.Value].AmmoCapacity > 0)
        {
            Weapons[CurruntWeapon.Value].CurruntMagAmmo += Weapons[CurruntWeapon.Value].AmmoCapacity;
            Weapons[CurruntWeapon.Value].AmmoCapacity = 0;
        }
        UpdateAmmo();
        Refrences.CharcaterAniamtor.SetBool("Reload_b", false);
        Refrences.WeaponsAnimator.SetBool("Reload_b", false);

        if (Input.GetMouseButton(0) && Weapons[CurruntWeapon.Value].CurruntMagAmmo > 0)
        {
            Refrences.CharcaterAniamtor.SetBool("Shoot_b", true);
            Refrences.CharcaterAniamtor.SetBool("FullAuto_b", true);
            Refrences.WeaponsAnimator.SetBool("Shoot_b", true);
            Refrences.WeaponsAnimator.SetBool("FullAuto_b", true);
        }
    }
    #endregion

    #region Ui Update
    void UpdateAmmo()
    {
        if(Refrences.AmmoText != null)
        {
            Refrences.AmmoText.text = Weapons[CurruntWeapon.Value].CurruntMagAmmo.ToString() + "/" + Weapons[CurruntWeapon.Value].AmmoCapacity.ToString();
        }
    }

    void UpdateThrowAblesAmmo()
    {
        for (int i = 0; i < Throwables.Count; i++)
        {
            if (Throwables[i].UiText != null)
            {
                Throwables[i].UiText.text = Throwables[i].Amount.ToString();
            }
        }
    }
    #endregion

    [ServerRpc]
    private void SpawnBulletServerRpc(Vector3 position, Quaternion rotation)
    {
        // Instantiate the bullet on the server
        GameObject bullet = Instantiate(Weapons[CurruntWeapon.Value].Bullet.gameObject, position, rotation);

        // Assign the owner ID
        if (int.TryParse(OwnerClientId.ToString(), out int Id))
        {
            bullet.GetComponent<Bullet>().OwnerID.Value = Id;
        }

        // Set the bullet's direction if necessary
        // Here we're assuming the shooter is aiming at the target when firing
        // It's better to ensure the rotation is properly synchronized or derived from consistent data
        if (Refrences.TargetPos != null)
        {
            bullet.transform.LookAt(Refrences.TargetPos.position);
        }

        // Ensure the bullet has a NetworkObject component and spawn it
        NetworkObject networkObject = bullet.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
    }

    [ServerRpc]
    private void SpawnThrowableServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject Throable = Instantiate(Throwables[0].ThrowablePrefeb.gameObject, position, rotation);
        int Id;
        int.TryParse(OwnerClientId.ToString(), out Id);
        Throable.gameObject.GetComponent<Grenade>().OwnerID.Value = Id;
        NetworkObject networkObject = Throable.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            networkObject.Spawn();
        }
    }
}
[System.Serializable]
public class GunDetails
{
    public string WeaponName;
    public Transform GunObject;
    public GunType Type;
    public float firerate;
    public int MagCapacity;
    public int AmmoCapacity;
    public Transform ShootPoint;
    public GameObject Bullet;
    public float Aim_OffsetAngle;
    public bool HasScope;
    public int AnimatorType;
    [HideInInspector] public int CurruntMagAmmo;
}
public enum GunType
{
    Auto,
    Single,
    Sniper
}

[System.Serializable]
public class OtherRefs
{
    public Transform PlayerCamera;
    public Animator CharcaterAniamtor, WeaponsAnimator;
    public TextMeshProUGUI AmmoText;
    public TextMeshProUGUI KillsText;
    public CameraSetup CameraPositions;
    public Transform WeaponRootPose;
    public Transform TargetPos;
    public Transform WeaponRootObjcts;
    public GameObject ScopeAimImage;
}
[System.Serializable]
public class CameraSetup
{
    public Transform AssultPos,
        HandGunPos,
        IdlePos,
        SniperPos;
}

[System.Serializable]
public class ThrowableDetails
{
    public string Name;
    public GameObject ThrowablePrefeb;
    public Transform ThrowPoint;
    public Transform ThrowableObject;
    public float Amount;
    public int AnimatorType;
    public KeyCode InputKey;
    public TextMeshProUGUI UiText;
}