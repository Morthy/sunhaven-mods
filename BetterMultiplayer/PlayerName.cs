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
    private Transform _chatBubble;
    private Player _player;
    private NetworkGamePlayer _netPlayer;

    public void Initialize(NetworkGamePlayer netPlayer)
    {
        _playerNameText = transform.GetComponent<TextMeshProUGUI>();
        _netPlayer = netPlayer;
        _player = netPlayer.player;
        _chatBubble = Traverse.Create(_player).Field("chatBubble").GetValue<Transform>();
        
        gameObject.SetActive(true);
    }
    
    public void Update()
    {
        _playerNameText.enabled = Plugin.ShowPlayerNames.Value;
        _playerNameText.text = _netPlayer.playerName;
        transform.localPosition = _chatBubble.gameObject.activeSelf ? new Vector3(0, 1, 0) : new Vector3(0, 0.21f, 0);
    }
}