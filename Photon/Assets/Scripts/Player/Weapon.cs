using UnityEngine;
using System.Collections;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;
using DG.Tweening;

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

    [Header("References")]
    public Camera fpsCam;

    private float nextTimeToFire = 0f;

    [Header("Ammo")]
    [SerializeField] private int magSize = 30;
    [SerializeField] private int reserveAmmo = 120;
    private int currentAmmo;
    private bool isReloading = false;

    [Header("Aim Settings")]
    [SerializeField] private Vector3 hipLocalPos = new Vector3(0.2f, -0.2f, 0.6f);  // hip-fire position
    [SerializeField] private Vector3 aimOffset = new Vector3(0f, -0.05f, 0.3f);   // aimed position
    [SerializeField] private float aimDuration = 0.15f;
    [SerializeField] private float aimedFOV = 50f;
    private float defaultFOV;
    private bool isAiming = false;

    [SerializeField] private bool aimTest =true;

    [Header("Sway Settings")]
    [SerializeField] private float swayClamp = 0.02f;
    [SerializeField] private float swaySmooth = 6f;

    [Header("Effects")]
    // [SerializeField] private GameObject hitParticleFlesh;
    // [SerializeField] private GameObject hitParticle;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoDisplay;
    [SerializeField] private Image reloadProgressImage;
    private CanvasGroup reloadUIGroup;

    private Tween aimTween;
    private Tween fovTween;
    
    [Header("SFX")]
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (fpsCam != null && transform.parent != fpsCam.transform)
            transform.SetParent(fpsCam.transform);

        transform.localPosition = hipLocalPos;

        currentAmmo = magSize;

        if (fpsCam != null)
            defaultFOV = fpsCam.fieldOfView;

        if (reloadProgressImage != null)
        {
            reloadUIGroup = reloadProgressImage.GetComponentInParent<CanvasGroup>();
            if (reloadUIGroup != null)
                reloadUIGroup.alpha = 0f;
        }

        UpdateAmmoUI();
    }

    private void OnEnable()
    {
        if (reloadProgressImage != null)
            reloadProgressImage.fillAmount = 0f;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        if (reloadProgressImage != null)
            reloadProgressImage.fillAmount = 0f;

    }

    public void GunUpdate()
    {
        HandleAiming();
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

    // ------------------- AIM -------------------
    private void HandleAiming()
    {
        if (Input.GetMouseButtonDown(1) || aimTest)
            SetAiming(true);
        else if (Input.GetMouseButtonUp(1))
            SetAiming(false);
    }

    private void SetAiming(bool state)
    {
        if (isAiming == state) return;
        isAiming = state;

        aimTween?.Kill();
        fovTween?.Kill();

        Vector3 targetPos = isAiming ? aimOffset : hipLocalPos;
        aimTween = transform.DOLocalMove(targetPos, aimDuration).SetEase(Ease.OutSine);

        if (fpsCam != null)
        {
            float targetFOV = isAiming ? aimedFOV : defaultFOV;
            fovTween = fpsCam.DOFieldOfView(targetFOV, aimDuration);
        }
    }

    // ------------------- SWAY -------------------
    private void HandleSway()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * swayClamp;
        float mouseY = Input.GetAxisRaw("Mouse Y") * swayClamp;

        Vector3 basePos = isAiming ? aimOffset : hipLocalPos;
        Vector3 targetPos = basePos + new Vector3(-mouseX, -mouseY, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * swaySmooth);
    }

    // ------------------- SHOOT -------------------
    private void Shoot()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
        
        if (currentAmmo <= 0 || isReloading) return;

        currentAmmo--;
        UpdateAmmoUI();

        nextTimeToFire = Time.time + 1f / fireRate;

        Recoil();

        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out RaycastHit hit, range))
        {
            if (hit.transform == null) return;

            if (hit.transform.TryGetComponent(out Health target))
            {
                PhotonView targetView = target.GetComponent<PhotonView>();
                if (targetView != null)
                {
                    targetView.RPC("TakeDamage", RpcTarget.All, damage);

                    if (damage > target.health)
                    {
                        RoomManager.Singleton.AddKill(PhotonNetwork.LocalPlayer.NickName);
                    }

                    // if (hitParticleFlesh != null)
                    //     PhotonNetwork.Instantiate(hitParticleFlesh.name, hit.point, Quaternion.LookRotation(hit.normal));
                    ObjectPool.Instance.SpawnFromPool("BodyHit", hit.point, Quaternion.LookRotation(hit.normal));

                }
            }
            else
            {
                // if (hitParticle != null)
                //     PhotonNetwork.Instantiate(hitParticle.name, hit.point, Quaternion.LookRotation(hit.normal));
                ObjectPool.Instance.SpawnFromPool("NormalHit", hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }

    // ------------------- RECOIL -------------------
    private void Recoil()
    {
        transform.DOKill();

        Vector3 basePos = isAiming ? aimOffset : hipLocalPos;
        float recoilMultiplier = isAiming ? 0.3f : 1f;

        Vector3 recoilPos = basePos + new Vector3(0, recoilUp * recoilMultiplier, -recoilBack * recoilMultiplier);

        transform.DOLocalMove(recoilPos, 0.05f).SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOLocalMove(basePos, recoverSpeed).SetEase(Ease.OutQuad);
            });
    }

    // ------------------- RELOAD -------------------
    private IEnumerator Reload()
    {
        if (isReloading || currentAmmo == magSize || reserveAmmo <= 0) yield break;

        isReloading = true;

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

        if (reloadProgressImage != null)
            reloadProgressImage.fillAmount = 0f;

        if (reloadUIGroup != null)
            reloadUIGroup.DOFade(0f, 0.2f);

        isReloading = false;
    }

    // ------------------- AMMO UI -------------------
    private void UpdateAmmoUI()
    {
        if (ammoDisplay != null)
            ammoDisplay.text = $"{currentAmmo} / {reserveAmmo}";
    }

    public bool IsAiming => isAiming;
}
