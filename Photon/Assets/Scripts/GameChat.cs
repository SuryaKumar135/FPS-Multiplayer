using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameChat : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject chatUI;             // Parent panel for chat UI
    [SerializeField] private TextMeshProUGUI chatDisplay;
    [SerializeField] private TMP_InputField chatInput;

    private bool isTyping;
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        chatUI.SetActive(false); // start hidden
    }

    private void Update()
    {
        // Open chat
        if (Input.GetKeyDown(KeyCode.CapsLock))
        {
            isTyping = !isTyping;
            if (isTyping)
            {
                //isTyping = true;
                chatUI.SetActive(true);

                EventSystem.current.SetSelectedGameObject(null);
                chatInput.Select();
                chatInput.ActivateInputField();
            }
            else
            {
                //isTyping = false;
                chatUI.SetActive(false);

                chatInput.text = string.Empty;
                EventSystem.current.SetSelectedGameObject(null);
            }
            
            
        }

        // // Close chat without sending
        // if (Input.GetKeyDown(KeyCode.Escape) && isTyping)
        // {
        //     
        // }

        // Send message
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) &&
            isTyping &&
            !string.IsNullOrEmpty(chatInput.text))
        {
            string formattedText = $"{PhotonNetwork.LocalPlayer.NickName}: {chatInput.text}";

            // Display locally
            AddMessage(formattedText);

            // Send to others
            photonView.RPC(nameof(ReceiveMessage), RpcTarget.Others, formattedText);

            chatInput.text = string.Empty;
            isTyping = false;
            chatUI.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    [PunRPC]
    private void ReceiveMessage(string msg)
    {
        AddMessage(msg);
    }

    private void AddMessage(string msg)
    {
        chatDisplay.text += "\n" + msg;
    }
}
