using Wish;

namespace CreativeMode;

public class DecorationCatalogue : UseItem
{
    public override void Use1()
    {
        if (UIHandler.InventoryOpen) return;
        var itemUI = Plugin.ItemUI.gameObject;
        UIHandler.Instance.OpenUI(itemUI, itemUI.transform.parent);
        Plugin.ItemUI.Opened();
    }
}