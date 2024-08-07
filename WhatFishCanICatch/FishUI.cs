using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wish;

namespace WhatFishCanICatch;

public class FishUI : CustomUI
{
    private bool _isCreated = false;
    private float _lastHide;

    public void OnDisable()
    {
        _lastHide = Time.time;
    }

    public bool DidJustDisable()
    {
        return Time.time - _lastHide < 0.25;
    }

    public void Create()
    {
        if (_isCreated)
        {
            return;
        }

        var bg = this.CreateBg();
        var title = this.CreateTitle(bg.transform);
        CreateTitleText(title.transform, "Catchable fish");
        var exit = this.CreateExitButton(this.transform);
        exit.onClick.AddListener(() => {
            UIHandler.Instance.CloseExternalUI();
        });
        
        _scrollRect = this.CreateScrollView(
            this.transform, 
            new Vector3(0, 0, 0), 
            new Vector2(210f, 30f),
            new Vector2(0, 0),
            new Vector2(210f, 330f),
            new Vector2(210, 310),
            new Vector2(10f, 200f),
            new Vector2(100f, -8f)
        );
        
        _isCreated = true;
    }
    
    public void Populate(Dictionary<FishData, float> data)
    {
        foreach (var item in data)
        {
            AddFish(item.Key, item.Value);
        }
        
        _scrollRect.verticalNormalizedPosition = 1f;
    }

    private void AddFish(FishData fish, float chance)
    {
        GameObject gameObject = new GameObject("Fish " + fish.id);
        gameObject.layer = LayerMask.NameToLayer("UI");
        var rootTransform = gameObject.AddComponent<RectTransform>();
        rootTransform.SetParent(_scrollRect.content);
        rootTransform.localScale = Vector3.one;
        
        Image image = new GameObject("ItemImage").AddComponent<Image>();
        image.sprite = fish.icon;
        image.color = Color.white;
        image.preserveAspect = true;
        image.gameObject.layer = LayerMask.NameToLayer("UI");
        image.transform.SetParent(gameObject.transform);

        RectTransform imageTransform = image.rectTransform;
        imageTransform.localScale = Vector3.one;
        imageTransform.localPosition = (Vector3) new Vector2(-83f, 0.0f);
        imageTransform.sizeDelta = new Vector2(25, 25);

        TextMeshProUGUI titleText = this.CreateText(rootTransform, "Fish name");
        titleText.transform.localScale = Vector3.one;
        titleText.transform.localPosition = new Vector3(-20, 0f, 0.0f);
        titleText.transform.localRotation = Quaternion.identity;
        titleText.rectTransform.sizeDelta = new Vector2(90, 25);
        titleText.alignment = TextAlignmentOptions.Left;
        titleText.fontSize = 18f;
        titleText.fontSizeMax = 18f;
        titleText.fontSizeMin = 16f;
        titleText.enableWordWrapping = false;
        titleText.text = fish.FormattedName;

        TextMeshProUGUI percentageText = Instantiate(titleText, rootTransform);
        percentageText.gameObject.name = "Percentage text";
        percentageText.transform.localPosition = new Vector3(90f, 0f, 0.0f);
        percentageText.text = Math.Round(chance*100, 2) + "%";
        
        Popup popup = image.gameObject.AddComponent<Popup>();
        popup.name = "Popup";
        popup.text = fish.FormattedName;
        popup.description = fish.FormattedDescription;
    }
}