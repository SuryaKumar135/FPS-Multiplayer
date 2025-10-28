using UnityEngine;
using System.Linq;
using Photon.Pun;
using TMPro;
using Photon.Pun.UtilityScripts;

public class LeaderBoard : MonoBehaviour
{
    [SerializeField] private GameObject leaderBoardUI;

    [Header("Settings")]
    [SerializeField] private float refreshRate = 1f;

    [Header("UI References")]
    [SerializeField] private GameObject[] slots; // Each slot has TMP text children

    private TextMeshProUGUI[] names;
    private TextMeshProUGUI[] kd;
    private TextMeshProUGUI[] scores;

    public static LeaderBoard Singelton;

    private void Awake()
    {
        if(Singelton == null)
        {
            Singelton = this;
        }
        else
        {
            Destroy(this);
        }

        // Automatically find name and score texts inside each slot
        names = new TextMeshProUGUI[slots.Length];
        kd = new TextMeshProUGUI[slots.Length];
        scores = new TextMeshProUGUI[slots.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            // Get all TMP texts inside the slot
            var tmps = slots[i].GetComponentsInChildren<TextMeshProUGUI>();

            // Assume first TMP is name, second is score (you can swap if needed)
            if (tmps.Length >= 2)
            {
                names[i] = tmps[0];
                kd[i] = tmps[1];
                scores[i] = tmps[2];
            }
            else
            {
                Debug.LogWarning($"Slot {i} is missing name/score TMP components!");
            }
        }
    }

    private void Start()
    {
        InvokeRepeating(nameof(RefreshLeaderBoard), 1f, refreshRate);
    }

    private void RefreshLeaderBoard()
    {
        // Hide all slots
        foreach (var slot in slots)
            slot.SetActive(false);

        // Sort players by score (highest first)
        var sortedPlayers = PhotonNetwork.PlayerList
            .OrderByDescending(p => p.GetScore())
            .ToArray();

        for (int i = 0; i < sortedPlayers.Length && i < slots.Length; i++)
        {
            var player = sortedPlayers[i];
            slots[i].SetActive(true);

            string playerName = string.IsNullOrEmpty(player.NickName)
                ? $"Player {player.ActorNumber}"
                : player.NickName;

            names[i].text = playerName;
            scores[i].text = player.GetScore().ToString();

            if (player.CustomProperties["Kills"] !=null)
            {
                kd[i].text = player.CustomProperties["Kills"].ToString() + "/" + player.CustomProperties["Deaths"].ToString();
            }
            else
            {
                kd[i].text = "0/0";
            }
        }
    }


    public void ToggleLeaderBoard(bool togle)
    {
        leaderBoardUI.SetActive(togle);
    }

}
