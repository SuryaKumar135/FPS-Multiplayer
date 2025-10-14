using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Network Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Game States")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject nameSelectScreen;
    [SerializeField] private GameObject lobbyCamera;

    public static RoomManager Singleton;

    private string playerNickname = "Noob";

    public int kills;
    public int deaths;

    private void Awake()
    {
        // Singleton setup
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
            return;
        }

        Singleton = this;
    }

    // Called when user enters nickname in UI
    public void ChangeNickname(string name)
    {
        playerNickname = string.IsNullOrEmpty(name) ? "Noob" : name;
        PhotonNetwork.NickName = playerNickname;
    }

    // Called by Join button
    public void JoinRoomButtonPressed()
    {
        if (nameSelectScreen != null) nameSelectScreen.SetActive(false);
        if (loadingScreen != null) loadingScreen.SetActive(true);

        Debug.Log("Connecting to Photon...");
        PhotonNetwork.NickName = playerNickname; // assign nickname before connecting
        PhotonNetwork.ConnectUsingSettings();
    }

    // ------------------- PHOTON CALLBACKS -------------------

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master Server");
        PhotonNetwork.NickName = playerNickname;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");

        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 10,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.JoinOrCreateRoom("Room1", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room: " + PhotonNetwork.CurrentRoom.Name);

        if (loadingScreen != null) loadingScreen.SetActive(false);
        if (lobbyCamera != null) lobbyCamera.SetActive(false);

        SpawnPlayer();
        InitializePlayerStats();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected from Photon: " + cause);

        if (loadingScreen != null) loadingScreen.SetActive(false);
        if (nameSelectScreen != null) nameSelectScreen.SetActive(true);
        if (lobbyCamera != null) lobbyCamera.SetActive(true);
    }

    // ------------------- PLAYER SPAWN -------------------

    public void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("Player Prefab not assigned in RoomManager!");
            return;
        }

        Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        GameObject player = PhotonNetwork.Instantiate(playerPrefab.name, spawnPos, Quaternion.identity);

        if (player.TryGetComponent(out PlayerSetup setup))
            setup.IsLocalPlayer();

        if (player.TryGetComponent(out Health health))
            health.isLocalPlayer = true;

        if (player.TryGetComponent(out PhotonView view))
            view.RPC("SetNicknameRPC", RpcTarget.AllBuffered, playerNickname);

        Debug.Log("Local Player Spawned");
    }

    // ------------------- CUSTOM PROPERTIES (Kills/Deaths) -------------------

    private void InitializePlayerStats()
    {
        // Create initial values for every player
        Hashtable stats = new Hashtable
        {
            { "Kills", 0 },
            { "Deaths", 0 }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(stats);
    }

    //public void SetKills(int value)
    //{
    //    kills = value;
    //    UpdatePlayerStats();
    //}

    //public void SetDeaths(int value)
    //{
    //    deaths = value;
    //    UpdatePlayerStats();
    //}

    //public void AddKill()
    //{
    //    //if(PhotonNetwork.LocalPlayer == null)
    //    //    return;
    //    kills++;
    //    UpdatePlayerStats();
    //}

    public void AddKill(string playerName = "")
    {
        if (PhotonNetwork.LocalPlayer == null)
            return;
        kills++;
        UpdatePlayerStats();
        Debug.Log($"{playerName} got a kill! Total kills: {kills}");
    }

    public void AddDeath()
    {

        if (PhotonNetwork.LocalPlayer == null)
            return;
        deaths++;
        UpdatePlayerStats();
    }

    private void UpdatePlayerStats()
    {
        Hashtable hash = new Hashtable
        {
            { "Kills", kills },
            { "Deaths", deaths }
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
    }
}
