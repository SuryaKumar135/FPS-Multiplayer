using UnityEngine;
using Photon.Pun;
using TMPro;


public class PlayerSetup : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject playerUI;
    [SerializeField] private GunSwitcher gunSwitcher;

    [SerializeField] private string playerNickname;

    [SerializeField] private TextMeshPro playerNameText;

    [SerializeField] private Transform nameTransform;

    [SerializeField] private GameObject nonLocalModel;
    
    [SerializeField] private NonLocalPlayerAnimation nonLocalPlayerAnimation ;
    public void IsLocalPlayer()
    {
        playerMovement.enabled = true;
        playerCamera.SetActive(true);
        playerUI.SetActive(true);
        gunSwitcher.enabled = true;

        //disable for your self
        playerNameText.enabled = false;
        
        //Non local 
        nonLocalModel.gameObject.SetActive(false);
        
        nonLocalPlayerAnimation.enabled = false;
    }

    [PunRPC]
    public void SetNicknameRPC(string nickname)
    {
        playerNickname = nickname;
        gameObject.name = nickname;
        playerNameText.text = nickname;
    }

    private void LateUpdate()
    {
        nameTransform.LookAt(Camera.main.transform);

        if(LeaderBoard.Singelton != null)
            LeaderBoard.Singelton.ToggleLeaderBoard(Input.GetKey(KeyCode.Tab));
    }
}
