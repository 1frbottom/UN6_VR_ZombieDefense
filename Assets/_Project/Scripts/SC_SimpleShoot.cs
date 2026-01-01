using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Interaction;
using System.Linq;

public class SC_SimpleShoot : MonoBehaviour
{
    [Header("Interaction SDK")]
    public GrabInteractable grabInteractable;

    [Header("Prefab Refrences")]
    public GameObject bulletPrefab;
    public GameObject casingPrefab;
    public GameObject muzzleFlashPrefab;

    [Header("Location Refrences")]
    [SerializeField] private Animator gunAnimator;
    [SerializeField] private Transform barrelLocation;
    [SerializeField] private Transform casingExitLocation;

    [Header("Settings")]
    [SerializeField] private float destroyTimer = 2f;
    [SerializeField] private float shotPower = 500f;
    [SerializeField] private float ejectPower = 150f;

    [SerializeField] private float fireRate = 0.01f;
    private float nextFireTime;

    [Header("Sound")]
    public AudioClip fireSound;
    private AudioSource audioSource;

    [Header("Haptics")]
    public float hapticAmplitude = 0.5f;
    public float hapticDuration = 0.1f;

    private OVRInput.Controller _cachedController;
    private IInteractor _lastInteractor;

    [Header("Ammo Settings")]
    public int maxAmmo = 7;
    private int currentAmmo;
    public AudioClip reloadSound;
    public AudioClip emptySound;



    void Start()
    {
        if (barrelLocation == null) barrelLocation = transform;
        if (gunAnimator == null) gunAnimator = GetComponentInChildren<Animator>();
        if (grabInteractable == null) grabInteractable = GetComponentInParent<GrabInteractable>();

        audioSource = GetComponent<AudioSource>();
        currentAmmo = maxAmmo;

    }

    void Update()
    {
        if (grabInteractable != null && grabInteractable.State == InteractableState.Select)
        {
            var interactor = grabInteractable.SelectingInteractors.FirstOrDefault();
            if (interactor == null) return;

            if (_lastInteractor != interactor)
            {
                _lastInteractor = interactor;
                _cachedController = IsLeftHand(interactor.transform) ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;
            }

            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, _cachedController) && Time.time > nextFireTime)
            {
                if (currentAmmo > 0)
                {
                    nextFireTime = Time.time + fireRate;
                    ShootLogic();
                }
                else
                {
                    if (audioSource != null && emptySound != null)
                        audioSource.PlayOneShot(emptySound);
                }
            }
        }
        else { _lastInteractor = null; }

    }

    // 부모를 타고 올라가며 "Left" 키워드를 찾는 함수
    bool IsLeftHand(Transform t)
    {
        if (t == null) return false;
        if (t.name.ToLower().Contains("left") || t.name.ToLower().Contains("_l")) return true;

        // 부모가 있다면 부모에게 물어봄 (재귀 호출)
        return t.parent != null && IsLeftHand(t.parent);
    }

    void Shoot()
    {
        if (muzzleFlashPrefab)
        {
            GameObject tempFlash = Instantiate(muzzleFlashPrefab, barrelLocation.position, barrelLocation.rotation);
            Destroy(tempFlash, destroyTimer);
        }
        if (!bulletPrefab) return;
        GameObject bullet = Instantiate(bulletPrefab, barrelLocation.position, barrelLocation.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(barrelLocation.forward * shotPower);

    }
    void ShootLogic()
    {
        currentAmmo--; // 탄수 감소
        if (audioSource != null && fireSound != null) audioSource.PlayOneShot(fireSound);
        TriggerHaptic();
        gunAnimator.SetTrigger("Fire");
    }

    // 탄창 충돌 감지
    private void OnTriggerEnter(Collider other)
    {
        // 탄창 태그를 가진 물체가 닿았을 때
        if (other.CompareTag("Magazine"))
        {
            Reload(other.gameObject);
        }

    }

    void Reload(GameObject magazine)
    {
        currentAmmo = maxAmmo;
        if (audioSource != null && reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        Destroy(magazine);
    }

    void CasingRelease()
    {
        if (!casingExitLocation || !casingPrefab) return;
        GameObject tempCasing = Instantiate(casingPrefab, casingExitLocation.position, casingExitLocation.rotation);
        tempCasing.GetComponent<Rigidbody>().AddExplosionForce(Random.Range(ejectPower * 0.7f, ejectPower), (casingExitLocation.position - casingExitLocation.right * 0.3f - casingExitLocation.up * 0.6f), 1f);
        tempCasing.GetComponent<Rigidbody>().AddTorque(new Vector3(0, Random.Range(100f, 500f), Random.Range(100f, 1000f)), ForceMode.Impulse);
        Destroy(tempCasing, destroyTimer);
    }
    void TriggerHaptic()
    {
        if (this.gameObject.activeInHierarchy)
            StartCoroutine(HapticRoutine());

    }
    IEnumerator HapticRoutine()
    {
        // _cachedController를 그대로 사용
        OVRInput.SetControllerVibration(1f, hapticAmplitude, _cachedController);
        yield return new WaitForSeconds(hapticDuration);
        OVRInput.SetControllerVibration(0f, 0f, _cachedController);

    }


}