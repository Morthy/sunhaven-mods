using System;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Wish;
using Button = UnityEngine.UI.Button;

namespace WhatFishCanICatch;


public class CustomUI : MonoBehaviour
{
    private static string bgTextureData =
        "iVBORw0KGgoAAAANSUhEUgAAAOAAAAF/CAYAAABQasXDAAAACXBIWXMAAA7EAAAOxAGVKw4bAAAJMElEQVR4nO3cMW/c5h3HcQpxAWepvQYuMmiIjaDp4MGLp4w24BcRZMniIZOHvoJMHrKoQ+AXYeAyevLiwUNdGIoHD0GMrHanAEWg6k/qOVEU5TvJV/za6+ezSOLx7jjwC5IPH+rSnY8/7o4cdJux08H22mgnl9qH3n9w7dQaz59d6W7eerfyk9p633/3ZpMbCP+VqpXztjF12Ep1slMBHrQPbJ4+edn/vP3llW4d9QX1/vsP+g/uvrh6tYNt8+Lt23PF1xwdmA57+ny57KiVg0vjFduH3rx17eiNL7u//u2btb5k98aiu3773uF79voNhW105+t7y319HdXD8dnl0Nf4YHciwN0bHx1+8N3uxx8e93/XG+9+ttc9fPTpcp3X+7/3RY+XteVdt+h/n74G2+Dbr37ufnq6OLGvj1+rXqqh6bKm4q33P392/L4TAZZaoVZsEY61+OoUsz68hVbL64vbsooYts3DR4vlPt72+VLLqolqYxph0+KbWgY4nH4Ob5xGWF/QtOu7FuFUbdjr/dPxwraY7vfjJtr13tg0vvH1Yx/g8Tnpu2W9LcI6hz1rUGVu+dwGwLZYNcA4fr3GQqbx1QHquLc3Q4CtyHbUO45ob+UG/fmPf+j+8c9/rb2BsC2m+/6cGkNp6vS0Guu64Qzx6ZOZa8A6AtY5bjuvXbUB624IbJN19v3WTx0JzxqYvNSdU/vidZaLkm1w1j4/9/p59/lzB3geqzYc/t/NBjh3+ikmWK11Mh0XGd+2GzsV4DrXfsD5tAgXr04uPxVgjdSYzwmb1eaRTs2Ogk4jdPoJ5zMeHW3xzc2Qmb0GHK8oPriYcYRz8ZXlTJjx9JhhsumgPkCEcH7jgZjx3NEyzIYZzYSpBfWYxXTF9kEihPVN7wdWU+0pinbAW86EGWZxd0fPOC1mJ1kDFzd+Uujm/vF0zwqw/jfFQS2oiddl8eqbfgKpWxLwYdo0tApvmBd6Yn71znIQpkI7a6gU+HDjuwvtv0b8R6eiAe8nQAgSIAQJEIJWBtguFl/4T4OwcSsD/G33kw64mMuvf33v605BIUiAECRACBIgBAkQggQIQQKEoLUD/MufPJYE6/r7L+vNXHEEhCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAECRCCBAhBAoQgAUKQACFIgBAkQAgSIAQJEIIECEEChCABQpAAIUiAECRACBIgBAkQggQIQQKEIAFCkAAhSIAQJEAIEiAELQN88fZt//P77950d77ugA2rtkprrVSAB/XL/QfXDsO71/30dNHd/Wyvf/G33U864OKqoW+/+vnwt73u4aNPu+u373Y//vC4xXjQHwErvufPrnS7Nxbd6/3f+xWHNwGbUE1VW1236Fu7/2A4IvYB1oKbt94d1nmvX2FYEdiUamr3xkf9EfDm/uO+ua47CrDiG6sVy+XXvy6XvRr9Drzf5cnframmmnv6ZMUo6BdXr3bAhxkPukzNBljXf+KDzaiWqqm6Dpw6FaD4YPNahItXJ5efCrBGRGt0RoSwOXUaWm1NnQqwLhZFCJvT4psOxJTZa8C5FYGLO6spc0EhaHYQpmnDp05F4fzGtx9aV7ODMMNd+Zoas3fiQrHmhrZ5oRf5Utg25z0YLV5908/9bKqn219+fvTXaCZMDbpM46uJ2WUc1XgD5mKbu9cB22JujvT7mqiGqqUW4TDA+bL/eeZMmHF8ZRxVu084Htlp89zqtZrrBttrbzmxuu377Y5BNTFtpUwjHFsGOBwW352Kb3rvoj0pMR5WbRtSr9Uhdu5+B/yvq9DG8ZXxbbvp2d+4g3GErbVy6gg4jq++qN40Nb2QPFp6dPTbWz54CNtm2McXJ870rt/uZh9ir+UVXIt13FZzIsAajHn+bPi9rguHwZnTh805/SNN+8O6Rk3ZRnWKWUGN9/VVhobeLQc6pyrAncMj1kE9IHhR7XnCuUfuYZsMg5XH+/y6pusetbLTjoB9hKff9qYfqVntxHo7HWyvgyGeC7Ux1nfyb/w5mjB20f/1AAAAAElFTkSuQmCC";
    private static Sprite bgSprite;

    private static string titleTextureData = "iVBORw0KGgoAAAANSUhEUgAAAHAAAAAVCAYAAACe2WqiAAABBklEQVRoBe2aMQrCQBBFZ4NNEE/gAXKKgG2OIWhpYSWCpSDWll5A8AC2QjxE0lgIOYFY6spEFoxzgf34p8ju/jSP/7ZLnG8mXnReA5Hk3m75wGnAVYfMZ3neEtdlKavlA4f+D0nXm758+0rm45uoOB3Ki/9GBEfqTN0linytnvGTk7DTQHDWCuy84QGqAQqE0mVhKdB2ApVQIJQuC0uBthOohAKhdFlYCrSdQCUUCKXLwlKg7QQqoUAoXRaWAm0nUAkFQumysBRoO4FKOgL1WxMn7gZ+HfUC7ml/lmI6kuMlJFxjbUBdhXFFmurezxbDkHEFaGC3bZTSBYF6+PwbozsOQgNOId8BxTDZWfVzgwAAAABJRU5ErkJggg==";
    private static Sprite titleSprite;
    
    protected ScrollRect _scrollRect;
    
    public void ClearContent()
    {
        foreach(Transform child in _scrollRect.content.transform)
        {
            Destroy(child.gameObject);
        }
    }

    protected ScrollRect CreateScrollView(
      Transform parent,
      Vector3 position,
      Vector2 cellSize,
      Vector2 spacing,
      Vector2 sizeDel,
      Vector2 contentSizeDel,
      Vector2 barSizeDel,
      Vector3 barPosition,
      GridLayoutGroup.Constraint constraint = GridLayoutGroup.Constraint.FixedColumnCount,
      Scrollbar.Direction barDirection = Scrollbar.Direction.BottomToTop,
      TextAnchor layoutDirection = TextAnchor.UpperLeft)
    {
      ScrollRect scrollRect = new GameObject("Scroll View").AddComponent<ScrollRect>();
      scrollRect.gameObject.layer = LayerMask.NameToLayer("UI");
      scrollRect.transform.SetParent(parent);
      scrollRect.transform.localScale = Vector3.one;
      scrollRect.transform.localPosition = position;
      scrollRect.scrollSensitivity = 5f;
      scrollRect.movementType = ScrollRect.MovementType.Clamped;

      GameObject viewport = new GameObject("Viewport");
      viewport.layer = LayerMask.NameToLayer("UI");
      RectTransform viewportTransform = viewport.AddComponent<RectTransform>();
      viewportTransform.SetParent(scrollRect.transform);
      viewportTransform.localScale = Vector3.one;
      viewportTransform.localPosition = new Vector3(position.x, position.y - 9, position.z);
      viewportTransform.sizeDelta = sizeDel;

      viewport.AddComponent<Image>().rectTransform.sizeDelta = sizeDel;
      viewport.AddComponent<Mask>().showMaskGraphic = false;

      GridLayoutGroup gridLayoutGroup = new GameObject("Content").AddComponent<GridLayoutGroup>();
      var fitter = gridLayoutGroup.gameObject.AddComponent<ContentSizeFitter>();
      fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
      gridLayoutGroup.gameObject.layer = LayerMask.NameToLayer("UI");
      ((RectTransform)gridLayoutGroup.transform).pivot = new Vector2(0.5f, 1f);
      gridLayoutGroup.transform.SetParent(viewportTransform);
      gridLayoutGroup.transform.localScale = Vector3.one;
      gridLayoutGroup.transform.localPosition = Vector3.zero;
      gridLayoutGroup.constraint = constraint;

      scrollRect.content = (RectTransform)gridLayoutGroup.transform;
      scrollRect.content.sizeDelta = contentSizeDel;
      scrollRect.viewport = (RectTransform)viewport.transform;
      gridLayoutGroup.childAlignment = layoutDirection;
      gridLayoutGroup.cellSize = cellSize;
      gridLayoutGroup.constraintCount = 1;
      gridLayoutGroup.spacing = spacing;
      
      scrollRect.horizontal = false;
      scrollRect.verticalScrollbar = CreateScrollBar(scrollRect.transform, barDirection, barPosition);
      ((RectTransform) scrollRect.verticalScrollbar.transform).sizeDelta = barSizeDel;
      scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
      return scrollRect;
    }

    private Scrollbar CreateScrollBar(Transform parent, Scrollbar.Direction direction, Vector3 position = default (Vector3))
    {
        var settings = Traverse.Create(UIHandler.Instance).Field("_settings").GetValue<PlayerSettings>();
        var scrollBar = Instantiate(settings.transform.Find("SettingsScroll View_Gameplay/Scrollbar Vertical").GetComponent<Scrollbar>(), parent);
        if (position != new Vector3())
            scrollBar.transform.localPosition = position;
        scrollBar.direction = direction;
        return scrollBar;
    }

    protected Image CreateBg()
    {
        if (!bgSprite)
        {
            var bgTexture = CreateTexture(bgTextureData);
            bgSprite = Sprite.Create(bgTexture, new Rect(0, 0, bgTexture.width, bgTexture.height), new Vector2(0.5f, 0.5f), 24);
        }
        
        Image bg = new GameObject("Background").AddComponent<Image>();
        bg.sprite = bgSprite;
        bg.gameObject.layer = LayerMask.NameToLayer("UI");
        RectTransform transform = bg.rectTransform;
        transform.SetParent(this.transform);
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.sizeDelta = new Vector2(224, 383);
        
        return bg;
    }

    protected Image CreateTitle(Transform parent)
    {
        if (!titleSprite)
        {
            var tex = CreateTexture(titleTextureData);
            titleSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 24);
        }
        
        Image title = new GameObject("Title").AddComponent<Image>();
        title.sprite = titleSprite;
        title.gameObject.layer = LayerMask.NameToLayer("UI");
        title.transform.SetParent(parent);
        title.preserveAspect = true;
        RectTransform transform = title.rectTransform;
        transform.localScale = Vector3.one;
        transform.localPosition = new Vector3(0, 177, 0);
        transform.localRotation = Quaternion.identity;
        return title;
    }
    
    protected TextMeshProUGUI CreateTitleText(Transform parent, string name)
    {
        TextMeshProUGUI titleText = this.CreateText(parent, "Title text");
        titleText.enableAutoSizing = true;
        titleText.gameObject.layer = LayerMask.NameToLayer("UI");
        titleText.rectTransform.sizeDelta = new Vector2(80, 30);
        titleText.transform.localScale = Vector3.one;
        titleText.transform.localPosition = new Vector3(0.0f, 0f, 0.0f);
        titleText.transform.localRotation = Quaternion.identity;
        titleText.alignment = TextAlignmentOptions.Midline;
        titleText.fontSizeMax = 30f;
        titleText.fontSizeMin = 10f;
        titleText.fontSize = 18f;
        titleText.text = name;
        return titleText;
    }

    protected TextMeshProUGUI CreateText(Transform parent, string name)
    {
        var baseText = Traverse.Create(UIHandler.Instance.endOfDayScreen).Field("coinsTotalTMP").GetValue<TextMeshProUGUI>();
        var text = Instantiate(baseText, parent);
        text.alignment = TextAlignmentOptions.Left;
        text.gameObject.layer = LayerMask.NameToLayer("UI");
        text.name = name;
        return text;
    }

    protected Button CreateExitButton(Transform parent)
    {
        var buttonTransform = Instantiate(Traverse.Create(UIHandler.Instance).Field("_inventoryUI").GetValue<GameObject>().transform.Find("Inventory/ExitButton"), parent);
        buttonTransform.localPosition = new Vector3(110, 190, 0);
        return buttonTransform.gameObject.GetComponent<Button>();
    }

    protected Texture2D CreateTexture(string data)
    {
        var texture = new Texture2D(1, 1);
        texture.LoadImage(Convert.FromBase64String(data));
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.wrapModeU = TextureWrapMode.Clamp;
        texture.wrapModeV = TextureWrapMode.Clamp;
        texture.wrapModeW = TextureWrapMode.Clamp;
        return texture;
    }
}