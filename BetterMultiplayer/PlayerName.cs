using System;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Wish;

namespace BetterMultiplayer;

public class PlayerName : MonoBehaviour
{
    private TextMeshProUGUI _playerNameText;
    private Player _player;
    private NetworkGamePlayer _netPlayer;

    public void Initialize(NetworkGamePlayer netPlayer)
    {
        _playerNameText = transform.GetComponent<TextMeshProUGUI>();
        _netPlayer = netPlayer;
        _player = netPlayer.player;

        gameObject.SetActive(true);
    }
    
    public void Update()
    {
        _playerNameText.enabled = Plugin.ShowPlayerNames.Value;
        _playerNameText.text = _netPlayer.playerName;
        transform.localPosition = new Vector3(0, 0.4f, 0);
    }
}