using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wish;

namespace WhatItemsCanICraft;

public class CraftingUI : CustomUI
{
    private bool _isCreated = false;
    private TextMeshProUGUI _titleText;

    public void Create()
    {
        if (_isCreated)
        {
            return;
        }

        var bg = this.CreateBg();
        var title = this.CreateTitle(bg.transform);
        _titleText = CreateTitleText(title.transform, "Craftable Items");
        var exit = this.CreateExitButton(this.transform);
        exit.onClick.AddListener(() => {
            UIHandler.Instance.CloseExternalUI();
        });
        
        _scrollRect = this.CreateScrollView(
            this.transform, 
            new Vector3(-15, 0, 0), 
            new Vector2(270f, 30f),
            new Vector2(0, 0),
            new Vector2(270f, 330f),
            new Vector2(270, 310),
            new Vector2(10f, 200f),
            new Vector2(142f, -8f)
        );
        
        _isCreated = true;
    }
    
    public void Populate(List<(ItemData, string)> data, string title)
    {
        _titleText.text = title;
        
        foreach (var item in data)
        {
            AddItem(item.Item1, item.Item2);
        }
        
        _scrollRect.verticalNormalizedPosition = 1f;
    }

    private void AddItem(ItemData item, string recipeList)
    {
        GameObject gameObject = new GameObject("Item " + item.id);
        gameObject.layer = LayerMask.NameToLayer("UI");
        var rootTransform = gameObject.AddComponent<RectTransform>();
        rootTransform.SetParent(_scrollRect.content);
        rootTransform.localScale = Vector3.one;
        
        Image image = new GameObject("ItemImage").AddComponent<Image>();
        image.sprite = item.icon;
        image.color = Color.white;
        image.preserveAspect = true;
        image.gameObject.layer = LayerMask.NameToLayer("UI");
        image.transform.SetParent(gameObject.transform);

        RectTransform imageTransform = image.rectTransform;
        imageTransform.localScale = Vector3.one;
        imageTransform.localPosition = (Vector3) new Vector2(-83f, 0.0f);
        imageTransform.sizeDelta = new Vector2(25, 25);

        TextMeshProUGUI titleText = this.CreateText(rootTransform, "Item name");
        titleText.transform.localScale = Vector3.one;
        titleText.transform.localPosition = new Vector3(-20, 0f, 0.0f);
        titleText.transform.localRotation = Quaternion.identity;
        titleText.rectTransform.sizeDelta = new Vector2(90, 25);
        titleText.alignment = TextAlignmentOptions.Left;
        titleText.fontSize = item.Name.Length <= 20 ? 15f : 10f;
        titleText.enableWordWrapping = false;
        titleText.text = item.FormattedName;

        
        TextMeshProUGUI percentageText = Instantiate(titleText, rootTransform);
        percentageText.gameObject.name = "Recipe list";
        percentageText.fontSize = 11f;
        percentageText.fontSizeMax = 14f;
        percentageText.fontSizeMin = 10f;
        percentageText.enableAutoSizing = true;
        percentageText.transform.localPosition = new Vector3(90f, 0f, 0.0f);

        percentageText.text = recipeList.Replace("RecipeList_", "");
        
        Popup popup = image.gameObject.AddComponent<Popup>();
        popup.name = "Popup";
        popup.text = item.FormattedName;
        popup.description = item.FormattedDescription;
    }
}