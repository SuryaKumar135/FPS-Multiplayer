using UnityEngine;
using System.Collections;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun.UtilityScripts;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Settings")]
    public float damage = 10f;
    public float range = 100f;
    public float fireRate = 5f;
    [SerializeField] private float reloadTime = 2f;

    [Header("Recoil Settings")]
    [SerializeField, Range(0f, 2f)] private float recoilUp = 0.5f;
    [SerializeField, Range(0f, 2f)] private float recoilBack = 0.5f;
    [SerializeField, Range(0f, 1f)] private float recoverSpeed = 0.15f;

    private Vector3 originalLocalPos;

    [Header("Ammo")]
    [SerializeField] private int magSize = 30;
    [SerializeField] private int reserveAmmo = 120;
    private int currentAmmo;
    private int maxAmmo;
    private bool isReloading = false;

    [Header("References")]
    public Camera fpsCam;

    private float nextTimeToFire = 0f;

    [Header("Sway Settings")]
    [SerializeField] private float swayClamp = 0.02f;
    [SerializeField] private float swaySmooth = 6f;

    [Header("Effects")]
    [SerializeField] private GameObject hitParticleFlesh;
    [SerializeField] private GameObject hitParticle;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoDisplay;
    [SerializeField] private Image reloadProgressImage;
    private CanvasGroup reloadUIGroup;

    private void Awake()
    {
        originalLocalPos = transform.localPosition;
        maxAmmo = reserveAmmo;

        currentAmmo = magSize;
        UpdateAmmoUI();

        if (reloadProgressImage != null)
        {
            reloadUIGroup = reloadProgressImage.GetComponentInParent<CanvasGroup>();
            if (reloadUIGroup != null)
                reloadUIGroup.alpha = 0f;
        }
    }

    private void OnEnable()
    {
        reloadProgressImage.fillAmount = 0f;
    }

    private void OnDisable()
    {
        reloadProgressImage.fillAmount = 0f;
        StopAllCoroutines();
    }

    public void GunUpdate()
    {
        HandleSway();

        if (isReloading) return;

        UpdateAmmoUI();

        if (currentAmmo <= 0)
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire)
        {
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
        }
    }

    private void Shoot()
    {
        if (currentAmmo <= 0 || isReloading) return;

        currentAmmo--;
        UpdateAmmoUI();

        nextTimeToFire = Time.time + 1f / fireRate;

        // Cancel previous tweens before applying new recoil
        Recoil();

        // Raycast hit logic
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit hit, range))
        {
            if(hit.transform == null) return;

            if (hit.transform.TryGetComponent(out Health target))
            {
                PhotonView targetView = target.GetComponent<PhotonView>();
                if (targetView != null)
                {
                    // Send damage + shooter’s ActorNumber or NickName
                    targetView.RPC("TakeDamage", RpcTarget.All, damage);//, PhotonNetwork.LocalPlayer.NickName);

                    // Instantiate hit particle effect
                    if (hitParticleFlesh != null)
                    {
                        PhotonNetwork.Instantiate(hitParticleFlesh.name, hit.point, Quaternion.LookRotation(hit.normal));
                    }

                    if(damage > target.health)
                    {
                        RoomManager.Singleton.AddKill(PhotonNetwork.LocalPlayer.NickName);
                    }
                }
                
            }
            else
            {
                if (hitParticle != null)
                {
                    PhotonNetwork.Instantiate(hitParticle.name, hit.point, Quaternion.LookRotation(hit.normal));
                }
            }
        }

    }
    // // ------------------- Recoil -------------------
    private void Recoil()
    {
        transform.DOKill();

        // Apply recoil: move slightly up and back
        transform.DOLocalMove(originalLocalPos + new Vector3(0, recoilUp, -recoilBack), 0.05f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                // Return smoothly to original position
                transform.DOLocalMove(originalLocalPos, recoverSpeed).SetEase(Ease.OutQuad);
            });
    }

    // ------------------- RELOAD -------------------
    private IEnumerator Reload()
    {
        if (isReloading || currentAmmo == magSize || reserveAmmo <= 0) yield break;

        isReloading = true;
        Debug.Log("Reloading...");

        if (reloadUIGroup != null)
            reloadUIGroup.DOFade(1f, 0.2f);

        if (reloadProgressImage != null)
        {
            reloadProgressImage.DOKill();
            reloadProgressImage.fillAmount = 0f;
            reloadProgressImage.DOFillAmount(1f, reloadTime).SetEase(Ease.Linear);
        }

        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = magSize - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);
        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;

        Debug.Log($"Reloaded. Current Ammo: {currentAmmo} | Reserve: {reserveAmmo}");

        if (reloadProgressImage != null)
            reloadProgressImage.fillAmount = 0f;

        if (reloadUIGroup != null)
            reloadUIGroup.DOFade(0f, 0.2f);

        isReloading = false;
    }

    // ------------------- SWAY -------------------
    private void HandleSway()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * swayClamp;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayClamp;

        Vector3 targetPos = originalLocalPos + new Vector3(-mouseX, -mouseY, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * swaySmooth);
    }

    // ------------------- AMMO UI -------------------
    private void UpdateAmmoUI()
    {
        if (ammoDisplay != null)
            ammoDisplay.text = $"{currentAmmo} / {reserveAmmo}";
    }
}
