using System;
using System.Linq;
using HarmonyLib;
using I2.Loc;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Wish;
using Button = UnityEngine.UI.Button;

namespace CreativeMode;

public class CustomUI : MonoBehaviour
{
    private const string BgTextureData = "iVBORw0KGgoAAAANSUhEUgAAAVwAAAGuCAYAAADcTL4+AAAAGXRFWHRTb2Z0d2FyZQBBZG9iZSBJbWFnZVJlYWR5ccllPAAAAyZpVFh0WE1MOmNvbS5hZG9iZS54bXAAAAAAADw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+IDx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDkuMC1jMDAwIDc5LmRhNGE3ZTVlZiwgMjAyMi8xMS8yMi0xMzo1MDowNyAgICAgICAgIj4gPHJkZjpSREYgeG1sbnM6cmRmPSJodHRwOi8vd3d3LnczLm9yZy8xOTk5LzAyLzIyLXJkZi1zeW50YXgtbnMjIj4gPHJkZjpEZXNjcmlwdGlvbiByZGY6YWJvdXQ9IiIgeG1sbnM6eG1wPSJodHRwOi8vbnMuYWRvYmUuY29tL3hhcC8xLjAvIiB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIgeG1sbnM6c3RSZWY9Imh0dHA6Ly9ucy5hZG9iZS5jb20veGFwLzEuMC9zVHlwZS9SZXNvdXJjZVJlZiMiIHhtcDpDcmVhdG9yVG9vbD0iQWRvYmUgUGhvdG9zaG9wIDI0LjEgKFdpbmRvd3MpIiB4bXBNTTpJbnN0YW5jZUlEPSJ4bXAuaWlkOkRCRTJCMEJDRjA1RTExRUQ5ODNCRUMxMjY5NUU0QzQyIiB4bXBNTTpEb2N1bWVudElEPSJ4bXAuZGlkOkRCRTJCMEJERjA1RTExRUQ5ODNCRUMxMjY5NUU0QzQyIj4gPHhtcE1NOkRlcml2ZWRGcm9tIHN0UmVmOmluc3RhbmNlSUQ9InhtcC5paWQ6REJFMkIwQkFGMDVFMTFFRDk4M0JFQzEyNjk1RTRDNDIiIHN0UmVmOmRvY3VtZW50SUQ9InhtcC5kaWQ6REJFMkIwQkJGMDVFMTFFRDk4M0JFQzEyNjk1RTRDNDIiLz4gPC9yZGY6RGVzY3JpcHRpb24+IDwvcmRmOlJERj4gPC94OnhtcG1ldGE+IDw/eHBhY2tldCBlbmQ9InIiPz7z0+IYAAAJE0lEQVR42uzdP29TVxjA4ZvgSmQpWSsqBgZAqHRgYMnEGCQ+BGJhYWBi6CfIxJAlHRAfIpIZmbJkyFAqFDJkQEWsoVOlCqW8N36d45sb/yME7D7P4vT6OrYc6dfDucfHC6tLS1XPYXU2FiqA+XCmXezkL3389PKJM3a2L1W373wc+ZvyvPW192f5AgG+uWjjpC1s+tzG6OJCJ2MbJ6atV2/q25W7l8Z6QfEE8fjHT+tfXN1aXvZXAmba64ODiWJbxLXXz5tFtI+i22mG8+j2cu+Bb6rffn801pNcvdGtrq/c//yYjfqFAsy61Yf3+20bL7Yb1fFswcf+qDd1BqN54fMvvle9fL7ZH0rfu7ZRPXtxpX/O/u6nuuDlsTxeVd365+Z9ALPmyYN31dut7kDbyvuij9HM5rEy1vH4ne2qPbghTogTM7rNqOaUQfzyDGscjyfOYxFtgFn27EW337RsXIY1GhgtbEa3GdumzuB0woXW6MYTpJyfzei2RXl/d9NfC5ibkW6pbGDO1w6LbTn/u7C6tHSYk7txR1nrGKnGlMIkF8HM3wLzYtL2dfceDcQ2BqA5hxuLETplgXNUe1ztjZFP8suPP1R//v3vVC8QYFY0W9cmBqgpphuiqVW12QtuVS02HxAj3JybjXgOC2i8gPIWYF5jO6p1ZS+joW1zu51pn3ic46P+bwDwvcZ1nPsnbVznW75wgP+TxbaDueyhGU8BBRjexdNWcLUGty22AIzvtOieCG5cWbO0C2B6uQ/DyODGlbVmdE0lAJyubGTGtm2VQuscbnmi2AJMFt222PaDW+5mE442azhiaRfAaGUry4aWja2Dm/vZ5s44zTqLLsB4sc0RbrT0aLew4/106+DGR3njQOydkLt+ATCdaGhud1t8G069eU3cDnwtTm7AUC4RM5cLMHyEGxfMcovacl+FnoX+RbMMa9tSBgAmky0tP9ew6G0BOB+CCyC4AIILgOACCC6A4HoLAM7HyG98yF3DXtuxEeDrBvefqz95lwDGcHH/w9D7TSkAfC8j3PTrz752B6DNH3+NN+dqhAtwTgQXQHABBBcAwQUQXADB9RYACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIguACCCyC4AAgugOACILgAggsguAAILoDgAiC4AIILILgACC6A4AIIrrcAQHABBBcAwQUQXADBBUBwAQQXAMEFEFwAwQVAcAEEFwDBBRBcAMEFQHABBBcAwQUQXADBBUBwAQQXAMEFEFwAwQVAcAEEFwDBBRBcAMEFQHABBBcAwQUQXADBBUBwAQQXAMEFEFwAwQVAcAEEFwDBBRBcAMEFQHABBBcAwQUQXADBBUBwAQQXAMEFEFwAwQVAcAEEFwDBBRBcAMEFQHABBBcAwQUQXADBBUBwAQQXAMEFEFwAwQVAcAEEFwDBBRBcAMEFQHABBBcAwQUQXADBBUBwAQQXAMEFEFwAwQVAcAEEFwDBBRBcAMEFQHABBBcAwQUQXADBBUBwAQQXAMEFEFwAwQVAcAEEFwDBBfgudEadcHH/Q32717sFoNHJswrureVl7ybAGF4fHAy935QCwDkRXADBBRBcAAQXQHABBDd/yOUM62vvvSsAXyhbWi4VW1hdWjqMHx4/vVytPrxfvd3qVk8evKvvtAYXYDIZ2GcvrlTXV+5VL59v9uO7mLHd2b5Ux3Z/91N9IgDTiYZGS6Op0dZobD+4ceD2nY91ja/euFCfCMB0oqHR0mhqtDUa2w9uHCjFiQBMp9nQbKxVCgDnpDW4cdHMBTOAyUU7c+HByOCKLcDXie6J4MbVtFFbjAFwumhorkwYGtyY7BVdgC+Lbdvig9Y5XKsUAKZ3WkOtUgA4Jye+Yqec6M1pBRfRAIYrp2Gzo929luDmpyDW1zYGJnpjb4V71zamflKAWTbpYLO796jeOyFFP1fu3uz91/uj4ManIGJzhWZs43PAzYiWL6AtrvZhAOZF29KuYQ2MZkY7M7rR1PW1N/Xt1qtTvrW3jG0zorlOt7wSl58bjvvis8MA82GjvxFNti4Gp9nAZhvboluqt2fM3cJipNuMbT5Rs/rNZQ9ldNvWnwHMkghrGduydXlfqXlebs2YbY3HnBjhNmMbAT45T9E6e9Eb3W7YxByYC0dN6w78y/36SswCtJ1b1YHN6JYtTQPBjRLvbB/9fLyl2OZYL6yu+O7miTkOgFkUUwb9Eeru+B2squPtGNumFOL2sG0aIIfCY8W2N2QGmCfllOu4LWybnojeZnDr6J7R61vwJwLmxJl28T8BBgCTBGp+p9sDxAAAAABJRU5ErkJggg==";

    private static Sprite _bgSprite;

    private const string TitleTextureData = "iVBORw0KGgoAAAANSUhEUgAAAHAAAAAVCAYAAACe2WqiAAABBklEQVRoBe2aMQrCQBBFZ4NNEE/gAXKKgG2OIWhpYSWCpSDWll5A8AC2QjxE0lgIOYFY6spEFoxzgf34p8ju/jSP/7ZLnG8mXnReA5Hk3m75wGnAVYfMZ3neEtdlKavlA4f+D0nXm758+0rm45uoOB3Ki/9GBEfqTN0linytnvGTk7DTQHDWCuy84QGqAQqE0mVhKdB2ApVQIJQuC0uBthOohAKhdFlYCrSdQCUUCKXLwlKg7QQqoUAoXRaWAm0nUAkFQumysBRoO4FKOgL1WxMn7gZ+HfUC7ml/lmI6kuMlJFxjbUBdhXFFmurezxbDkHEFaGC3bZTSBYF6+PwbozsOQgNOId8BxTDZWfVzgwAAAABJRU5ErkJggg==";
    private static Sprite _titleSprite;
    
    protected ScrollRect ScrollRect;
    
    public void ClearContent()
    {
        foreach(Transform child in ScrollRect.content.transform)
        {
            Destroy(child.gameObject);
        }
    }

    protected static ScrollRect CreateScrollView(
      Transform parent,
      Vector3 position,
      Vector3 viewportPosition,
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
      var scrollRect = new GameObject("Scroll View").AddComponent<ScrollRect>();
      scrollRect.gameObject.layer = LayerMask.NameToLayer("UI");
      Transform transform1;
      (transform1 = scrollRect.transform).SetParent(parent);
      transform1.localScale = Vector3.one;
      transform1.localPosition = position;
      scrollRect.scrollSensitivity = 15f;
      scrollRect.movementType = ScrollRect.MovementType.Clamped;

      var viewport = new GameObject("Viewport")
      {
          layer = LayerMask.NameToLayer("UI")
      };
      
      var viewportTransform = viewport.AddComponent<RectTransform>();
      viewportTransform.SetParent(scrollRect.transform);
      viewportTransform.localScale = Vector3.one;
      viewportTransform.localPosition = viewportPosition;
      viewportTransform.sizeDelta = contentSizeDel;

      viewport.AddComponent<Image>().rectTransform.sizeDelta = contentSizeDel;
      viewport.AddComponent<Mask>().showMaskGraphic = false;

      var gridLayoutGroup = new GameObject("Content").AddComponent<GridLayoutGroup>();
      var fitter = gridLayoutGroup.gameObject.AddComponent<ContentSizeFitter>();
      fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
      gridLayoutGroup.gameObject.layer = LayerMask.NameToLayer("UI");
      ((RectTransform)gridLayoutGroup.transform).pivot = new Vector2(0.5f, 1f);
      
      Transform transform2;
      (transform2 = gridLayoutGroup.transform).SetParent(viewportTransform);
      transform2.localScale = Vector3.one;
      transform2.localPosition = Vector3.zero;
      gridLayoutGroup.constraint = constraint;
      gridLayoutGroup.padding = new RectOffset(0, 0, 4, 4);

      scrollRect.content = (RectTransform)transform2;
      scrollRect.content.sizeDelta = contentSizeDel;
      scrollRect.viewport = (RectTransform)viewport.transform;
      gridLayoutGroup.childAlignment = layoutDirection;
      gridLayoutGroup.cellSize = cellSize;
      gridLayoutGroup.constraintCount = 2;
      gridLayoutGroup.spacing = spacing;
      
      scrollRect.horizontal = false;
      scrollRect.verticalScrollbar = CreateScrollBar(scrollRect.transform, barDirection, barPosition);
      ((RectTransform) scrollRect.verticalScrollbar.transform).sizeDelta = barSizeDel;
      scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent;
      return scrollRect;
    }

    private static Scrollbar CreateScrollBar(Transform parent, Scrollbar.Direction direction, Vector3 position = default (Vector3))
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
        if (!_bgSprite)
        {
            var bgTexture = CreateTexture(BgTextureData);
            _bgSprite = Sprite.Create(bgTexture, new Rect(0, 0, bgTexture.width, bgTexture.height), new Vector2(0.5f, 0.5f), 24);
        }
        
        var bg = new GameObject("Background").AddComponent<Image>();
        bg.sprite = _bgSprite;
        bg.gameObject.layer = LayerMask.NameToLayer("UI");
        var bgTransform = bg.rectTransform;
        bgTransform.SetParent(this.transform);
        bgTransform.localScale = Vector3.one;
        bgTransform.localPosition = Vector3.zero;
        bgTransform.localRotation = Quaternion.identity;
        bgTransform.sizeDelta = new Vector2(348, 430);
        
        return bg;
    }

    protected Image CreateTitle(Transform parent)
    {
        if (!_titleSprite)
        {
            var tex = CreateTexture(TitleTextureData);
            _titleSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 24);
        }
        
        var title = new GameObject("Title").AddComponent<Image>();
        title.sprite = _titleSprite;
        title.gameObject.layer = LayerMask.NameToLayer("UI");
        title.transform.SetParent(parent);
        title.preserveAspect = true;
        var titleTransform = title.rectTransform;
        titleTransform.localScale = Vector3.one;
        titleTransform.localPosition = new Vector3(0, 200, 0);
        titleTransform.localRotation = Quaternion.identity;
        return title;
    }
    
    protected static TextMeshProUGUI CreateTitleText(Transform parent, string name)
    {
        TextMeshProUGUI titleText = CreateText(parent, "Title text");
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

    private static TextMeshProUGUI CreateText(Transform parent, string name)
    {
        var text = Instantiate(Traverse.Create(UIHandler.Instance.endOfDayScreen).Field("coinsTotalTMP").GetValue<TextMeshProUGUI>(), parent);
        text.gameObject.layer = LayerMask.NameToLayer("UI");
        text.name = name;
        return text;
    }

    protected static Button CreateExitButton(Transform parent)
    {
        var buttonTransform = Instantiate(Traverse.Create(UIHandler.Instance).Field("_inventoryUI").GetValue<GameObject>().transform.Find("Inventory/ExitButton"), parent);
        buttonTransform.localPosition = new Vector3(177, 219, 0);
        return buttonTransform.gameObject.GetComponent<Button>();
    }

    protected static SunHavenInputField CreateSearchBar(Transform parent, Vector3 position)
    {
        var table = Resources.FindObjectsOfTypeAll<CraftingTable>().First();
        var craftingTableUI = Traverse.Create(table).Field("ui").GetValue<GameObject>();
        var searchBar = Instantiate(craftingTableUI.transform.Find("Filter/Filter").gameObject, parent);
        ((RectTransform)searchBar.transform).sizeDelta = new Vector2(96, 18);
        searchBar.transform.localPosition = position;
        var input = searchBar.transform.Find("SearchInput").GetComponent<SunHavenInputField>();
        input.onValueChanged = new SunHavenInputField.OnChangeEvent();
        return input;
    }

    protected static TMP_Dropdown CreateDropDown(Transform parent, Vector3 position)
    {
        var settings = Traverse.Create(UIHandler.Instance).Field("_settings").GetValue<PlayerSettings>();
        var dropdown = Instantiate(settings.transform.Find("SettingsScroll View_Video/Viewport/Content/Setting_Resolution/SettingButton/Resolution_SettingsDropdown").GetComponent<TMP_Dropdown>(), parent);

        var gameObject = dropdown.gameObject;
        ((RectTransform)gameObject.transform).sizeDelta = new Vector2(100, 18);
        gameObject.transform.localPosition = position;
        dropdown.ClearOptions();
        return dropdown;
    }

    protected static Button CreateButton(Transform parent, Vector3 position, Vector2 sizeDelta, string name)
    {
        var settings = Traverse.Create(UIHandler.Instance).Field("_settings").GetValue<PlayerSettings>();
        var btn = Instantiate(settings.transform.Find("ExitButton").GetComponent<Button>(), parent);
        var gameObject = btn.gameObject;
        gameObject.name = "Modded button";
        btn.onClick = new Button.ButtonClickedEvent();
        ((RectTransform)gameObject.transform).sizeDelta = sizeDelta;
        gameObject.transform.localPosition = position;

        var txt = btn.transform.Find("Text (TMP)");
        Destroy(txt.GetComponent<Localize>());
        txt.GetComponent<TextMeshProUGUI>().text = name;
        return btn;
    }

    private static Texture2D CreateTexture(string data)
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