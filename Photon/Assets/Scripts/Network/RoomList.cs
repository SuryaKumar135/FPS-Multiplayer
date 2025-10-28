using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomList : MonoBehaviourPunCallbacks
{

    public static RoomList Singleton;

    [Header("UI References")]
    [SerializeField] private Transform content;               // Parent for room buttons
    [SerializeField] private GameObject roomButtonPrefab;


    // Prefab for each room entry
    [Header("References")]
    [SerializeField] private GameObject roomManagerGameobject;
    [SerializeField] private RoomManager roomManager;

    // Cache of current rooms in the lobby
    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    private void Awake()
    {
        if(Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }
        Singleton = this;
    }

    private IEnumerator Start()
    {
        // Make sure we start from a clean state
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);

        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Master Server");
        PhotonNetwork.JoinLobby();
    }

    //public override void OnJoinedLobby()
    //{
    //    Debug.Log("Joined Lobby");
    //}

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // Update our cached list
        foreach (RoomInfo info in roomList)
        {
            if (info.RemovedFromList || info.PlayerCount == 0)
            {
                // Room closed or empty -> remove from cache
                cachedRoomList.Remove(info.Name);
            }
            else
            {
                // Add or update
                cachedRoomList[info.Name] = info;
            }
        }

        // Refresh UI
        UpdateRoomListUI();
    }

    private void UpdateRoomListUI()
    {
        // Clear old buttons
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // Create new buttons
        foreach (RoomInfo room in cachedRoomList.Values)
        {
            GameObject buttonObj = Instantiate(roomButtonPrefab, content);
            buttonObj.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                $"{room.Name}";

            buttonObj.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text =
              $"({room.PlayerCount}/{room.MaxPlayers})";

            Button btn = buttonObj.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                PhotonNetwork.JoinRoom(room.Name);
            });

            if(buttonObj.transform.TryGetComponent(out RoomListButton roomListButton))
            {
                roomListButton.SetRoomName(room.Name);
            }
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogWarning("Join room failed: " + message);
    }

    public void ChangeRoomToCreateRoom(string roomName)
    {
        roomManager.roomNameToJoin = roomName;
    }

    public void JoinRoomByName(string roomName)
    {
        roomManager.roomNameToJoin = roomName;
        roomManagerGameobject.SetActive(true);
        gameObject.SetActive(false);
    }
}
