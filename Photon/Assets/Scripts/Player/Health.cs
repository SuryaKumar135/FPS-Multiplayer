using DG.Tweening;
using Photon.Pun;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
public class Health : MonoBehaviour
{
    public float health = 100f;

    [SerializeField] private Image healthBar;

    public bool isLocalPlayer = false;

    //private void OnEnable()
    //{
    //    maxHealth = 100f;
    //    if (healthBar != null)
    //    {
    //        healthBar.fillAmount = maxHealth / 100f;
    //    }
    //}

    [PunRPC]
    public void TakeDamage(float damage) //string shooterName)
    {
        if (health <= 0) return;

        health -= damage;

        // Check if this client is the one who owns this player (the one who is dying)
        if (health <= 0)
        {
            // Only the shooter increments kills
            //if (PhotonNetwork.LocalPlayer.NickName == shooterName)
            //{
            //    RoomManager.Singleton.AddKill(shooterName);
            //}

            // Only the dead player increments deaths
            if (isLocalPlayer)
            {
                RoomManager.Singleton.AddDeath();
                RoomManager.Singleton.SpawnPlayer();
                PhotonNetwork.Destroy(gameObject); 
            }

            // Destroy or respawn
           // if (isLocalPlayer)
                

           // Destroy(gameObject);
        }

        if (healthBar != null)
            healthBar.fillAmount = health / 100f;
    }


}
