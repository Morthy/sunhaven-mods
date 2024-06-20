using System;
using System.Linq;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wish;
using Button = UnityEngine.UI.Button;
using System.Collections;
using I2.Loc;
using PSS;

namespace CreativeMode;

public class ItemUI : CustomUI
{
    private bool _isCreated;
    private BuyableItem _itemPrefab;

    private DecorationCategory _categoryId = DecorationCategory.None;
    private string _filter = "";
    private int _filterCooldown = -1;
    private int _page;

    private Button _prevButton;
    private Button _nextButton;

    private Coroutine _addItemsRoutine;
    private bool _firstOpen;

    public void Create()
    {
        if (_isCreated)
        {
            return;
        }
        
        Plugin.logger.LogInfo("Create");

        CreatePrefab();
        
        var bg = this.CreateBg();
        var title = this.CreateTitle(bg.transform);
        CreateTitleText(title.transform, "Decorations");
        var exit = CreateExitButton(this.transform);
        exit.onClick.AddListener(() => {
            UIHandler.Instance.CloseExternalUI();
        });
        
        var filter = CreateSearchBar(bg.transform, new Vector3(106, 196, 0));
        filter.onValueChanged.AddListener(delegate
        {
            _filter = filter.text;
            _filterCooldown = 30;
        });
        
        var dropdown = CreateDropDown(bg.transform, new Vector3(-157, 196, 0));
        dropdown.onValueChanged = new TMP_Dropdown.DropdownEvent();
        dropdown.onValueChanged.AddListener(delegate
        {
            SelectCategory(dropdown.options[dropdown.value].text);
        });

        var options = DecorationCategorization.CategoryNames.Select(x => new TMP_Dropdown.OptionData(x.Value)).ToList();
        options.Insert(0, new TMP_Dropdown.OptionData("All"));
        dropdown.AddOptions(options);
        
        // buttons
        _prevButton = CreateButton(bg.transform, new Vector3(-126, -197, 0), new Vector2(75, 18), "Prev page");
        _prevButton.onClick.AddListener(delegate
        {
            _page--;
            Populate();
            AudioManager.Instance.PlayAudio(Prefabs.Instance.changeTabsSound, 0.5f);
        });
        _nextButton = CreateButton(bg.transform, new Vector3(126, -197, 0), new Vector2(75, 18), "Next page");
        _nextButton.onClick.AddListener(delegate
        {
            _page++;
            Populate();
            AudioManager.Instance.PlayAudio(Prefabs.Instance.changeTabsSound, 0.5f);
        });
        
        ScrollRect = CreateScrollView(
            transform, 
            new Vector3(8, 9, 0),
            new Vector3(5, -10.5f),
            new Vector2(154f, 62f),
            new Vector2(2, 2),
            new Vector2(348f, 430f),
            new Vector2(348f, 369f),
            new Vector2(10f, 240f),
            new Vector2(154f, -10f)
        );
        
        _isCreated = true;
    }

    private void CreatePrefab()
    {
        var shop = Resources.FindObjectsOfTypeAll<Shop>().FirstOrDefault(s => s.name.Equals("Solon"));

        if (!shop)
        {
            Plugin.logger.LogError("Failed to find shop when creating prefab");
            return;
        }
        
        var actualShop = shop.GetComponent<Shop>();
        _itemPrefab = Instantiate(Traverse.Create(actualShop).Field("_buyableItemPrefab").GetValue<BuyableItem>());
        DontDestroyOnLoad(_itemPrefab);
        _itemPrefab.priceTMP.gameObject.SetActive(false);
        Destroy(_itemPrefab.transform.Find("Panel/BuyTMP").GetComponent<Localize>());
        _itemPrefab.transform.Find("Panel/BuyTMP").GetComponent<TextMeshProUGUI>().text = "Get";

        RectTransform itemImage = (RectTransform)_itemPrefab.transform.Find("Panel/ItemImage");
        itemImage.GetComponent<Image>().enabled = false;
        itemImage.sizeDelta = new Vector2(40, 40);
        ((RectTransform)itemImage.Find("ItemImage")).sizeDelta = new Vector2(36, 36);
        
        _itemPrefab.itemImage.transform.localPosition = new Vector3(-39, -8, 0);
        _itemPrefab.itemNameTMP.rectTransform.sizeDelta = new Vector2(-12, 32);
        _itemPrefab.itemNameTMP.rectTransform.localPosition = new Vector3(-2, 22, 0);
    }
    
    public void LateUpdate()
    {
        if (_filterCooldown < 0) return;
        _filterCooldown--;

        if (_filterCooldown != 0) return;
        _page = 0;
        Populate();
    }

    private void SelectCategory(string categoryName)
    {
        _page = 0;
        _categoryId = DecorationCategorization.GetCategoryId(categoryName);
        Populate();
    }

    public void Opened()
    {
        Plugin.logger.LogInfo("Opened");
        if (!_firstOpen)
        {
            Populate();
            _firstOpen = true;
        }
    }

    public void Populate()
    {
        if (_addItemsRoutine != null)
            StopCoroutine(_addItemsRoutine);
        
        ClearContent();
        _addItemsRoutine = StartCoroutine(AddItemsRoutine());
    }

    private IEnumerator AddItemsRoutine()
    {
        var items = DecorationCategorization.GetMatchingItemIds(_categoryId, _filter, _page*Plugin.CataloguePageSize.Value, Plugin.CataloguePageSize.Value);
        _prevButton.gameObject.SetActive(_page > 0);
        _nextButton.gameObject.SetActive(items.Count > Plugin.CataloguePageSize.Value);
        
        foreach (var i in items.Take(Plugin.CataloguePageSize.Value))
        {
            AddItem(i, ItemInfoDatabase.Instance.allItemSellInfos[i]);

            yield return new WaitForSecondsRealtime(0.001f);
        }

        //_scrollRect.verticalNormalizedPosition = 1f;
        yield return null;
    }
    
    private void AddItem(int id, ItemSellInfo item)
    {
        if (!_itemPrefab)
        {
            return;
        }
        
        BuyableItem newBuyableItem = Instantiate(_itemPrefab, ScrollRect.content);
        newBuyableItem.itemImage.enabled = false;
        newBuyableItem.itemImage.ShopItem = true;
        newBuyableItem.itemNameTMP.text = item.name;
        newBuyableItem.qtyTMP.text = "";
        newBuyableItem.Qty = 9999;

        Database.GetData<ItemData>(id, itemData =>
        {
            if (newBuyableItem.itemImage == null)
            {
                return;
            }

            if (!Plugin.ShowUnownedDLCItems.Value && itemData.isDLCItem)
            {
                Destroy(newBuyableItem.gameObject);
                return;
            }
            
            if (false)
            {
                // newBuyableItem.itemNameTMP.text += $" <color=green><size=80%>({item.id})</size></color>";
            }

            newBuyableItem.itemImage.Initialize(itemData.GetItem());
            newBuyableItem.itemImage.enabled = true;

            var realItemImage = Traverse.Create(newBuyableItem.itemImage).Field("_image").GetValue<Image>();

            if (itemData is WallpaperData data)
            {
                realItemImage.sprite = data.wallpaper;
            }

            if (realItemImage.preferredHeight < realItemImage.rectTransform.sizeDelta.y && realItemImage.preferredWidth < realItemImage.rectTransform.sizeDelta.x)
            {
                realItemImage.rectTransform.sizeDelta = new Vector2(realItemImage.preferredWidth, realItemImage.preferredWidth);
            }

            if (itemData.isDLCItem && !DecorationCategorization.canUseDlcItem(id))
            {
                newBuyableItem.buyButton.gameObject.SetActive(false);
                newBuyableItem.buy5Button.gameObject.SetActive(false);
                newBuyableItem.buy20Button.gameObject.SetActive(false);
                newBuyableItem.transform.Find("Panel/BuyTMP").gameObject.SetActive(false);
                newBuyableItem.qtyTMP.text = "<color=red>DLC required</color>";
            }
            else
            {
                newBuyableItem.buyButton.onClick.AddListener(() => { Player.Instance.Inventory.AddItem(id, 1, true); });

                newBuyableItem.buy5Button.onClick.AddListener(() => { Player.Instance.Inventory.AddItem(id, 5, true); });

                newBuyableItem.buy20Button.onClick.AddListener(() => { Player.Instance.Inventory.AddItem(id, 20, true); });
            }
        });


        newBuyableItem.itemToUseAsCurrency = null;
        newBuyableItem.coinsPrice = 0;
        newBuyableItem.ticketsPrice = 0;
        newBuyableItem.orbsPrice = 0;
        newBuyableItem.priceTMP.text = "";
    }
}