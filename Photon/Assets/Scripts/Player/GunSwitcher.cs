using UnityEngine;
using System.Collections.Generic;

public class GunSwitcher : MonoBehaviour
{
    [Header("Weapon List")]
    [SerializeField] private List<Weapon> weapons = new List<Weapon>();
    private int currentWeaponIndex = 0;

    [Space]
    [Header("Sway Settings")]
    [SerializeField] private float swayClamp = 0.09f;
    [SerializeField] private float smoothing = 5f;

    private Vector3 original;

    
    private void Start()
    {
        SelectWeapon(currentWeaponIndex);
        original = transform.localPosition; //  Use local position
    }

    private void Update()
    {
        HandleSway();
        HandleWeaponSwitch();
        weapons[currentWeaponIndex].GunUpdate();
    }

    private void HandleSway()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        // Invert for natural sway
        Vector3 targetPos = new Vector3(-mouseX, -mouseY, 0) * swayClamp;

        // Smoothly interpolate back to target
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            original + targetPos,
            Time.deltaTime * smoothing
        );
    }

    private void HandleWeaponSwitch()
    {
        if (weapons.Count <= 1) return;

        if (Input.GetKeyDown(KeyCode.E))
            NextWeapon();

        if (Input.GetKeyDown(KeyCode.Q))
            PreviousWeapon();
    }

    private void NextWeapon()
    {
        weapons[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex = (currentWeaponIndex + 1) % weapons.Count;

        weapons[currentWeaponIndex].gameObject.SetActive(true);
        weapons[currentWeaponIndex].transform.localPosition = original; //  reset
    }

    private void PreviousWeapon()
    {
        weapons[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex--;
        if (currentWeaponIndex < 0)
            currentWeaponIndex = weapons.Count - 1;

        weapons[currentWeaponIndex].gameObject.SetActive(true);
        weapons[currentWeaponIndex].transform.localPosition = original; //  reset
    }

    private void SelectWeapon(int index)
    {
        for (int i = 0; i < weapons.Count; i++)
            weapons[i].gameObject.SetActive(i == index);
    }
}
