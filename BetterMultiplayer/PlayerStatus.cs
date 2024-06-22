using System;
using System.Linq;
using BepInEx;
using HarmonyLib;
using TMPro;
using UnityEngine;
using Wish;

namespace BetterMultiplayer;

public class PlayerStatus : MonoBehaviour
{
    private TextMeshProUGUI _text;
    private int _lastForceShow;

    public void Awake()
    {
        var questPosition = Player.Instance.transform.Find("UI_Quests/QuestTracker/Scroll View").localPosition;
        
        _text = Instantiate(Player.Instance.transform.Find("UI_Quests/QuestTracker/SkillTreeVisibilityToggle/SkillTreeNameTMP").GetComponent<TextMeshProUGUI>(), transform);
        _text.rectTransform.sizeDelta = new Vector2(120, 250);
        _text.rectTransform.localPosition = new Vector3(questPosition.x-47, -188, 0);
        _text.fontSize = 15;
        _text.alignment = TextAlignmentOptions.TopRight;
        _text.name = "Status";
    }

    public void Update()
    {
        if (Input.GetKeyDown((KeyCode)Enum.Parse(typeof(KeyCode), Plugin.PlayerStatusHotkey.Value)))
        {
            _text.enabled = !_text.enabled;

            if (_text.enabled)
            {
                UpdateStatus();
            }
        }
    }


    private void UpdateStatus()
    {
        var players = NetworkLobbyManager.Instance.GamePlayers;
        var key = Plugin.PlayerStatusHotkey.Value;
        var title = "";
        var playerStatus = "";
        var loadingText = "<i>Loading...</i>";
        
        if (DayCycle.Instance.Time.Hour < 22)
        {
            title = $"Players: {players.Count} <color=green>[{key}]</color>";
            playerStatus = players.Aggregate("", (current, p) => current + $"{(!p.Value.playerName.IsNullOrWhiteSpace()?p.Value.playerName:loadingText)}" + "\n");
        }
        else
        {
            if (_lastForceShow != DayCycle.Instance.Time.Day)
            {
                _lastForceShow = DayCycle.Instance.Time.Day;
                _text.enabled = true;
            }
            
            var bed = players.Count(p => p.Value.sleeping);
            title = $"Players in bed: {bed}/{players.Count} <color=green>[{key}]</color>";
            playerStatus = players.Aggregate("", (current, p) => current + (p.Value.sleeping?$"<color=green>{(!p.Value.playerName.IsNullOrWhiteSpace()?p.Value.playerName:loadingText)}</color>":$"<color=red>{(!p.Value.playerName.IsNullOrWhiteSpace()?p.Value.playerName:loadingText)}</color>") + "\n");
        }
        
        _text.text = $"<line-height=40%>{title}\n\n{playerStatus}</line-height>";

    }
    
    public void LateUpdate()
    {
        if (Time.frameCount % 60 != 8)
        {
            return;
        }
        
        UpdateStatus();
    }
}