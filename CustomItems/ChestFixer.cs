using HarmonyLib;
using TMPro;
using UnityEngine;
using Wish;
using Button = UnityEngine.UI.Button;

namespace CustomItems;

public class ChestFixer : MonoBehaviour
{
    public Chest chest;
    
    public void Awake()
    {
        var exit = this.transform.Find("ExitButton").GetComponent<Button>();
        exit.onClick = new Button.ButtonClickedEvent();
        exit.onClick.AddListener(() =>
        {
            chest.EndInteract(0);
        });

        var name = Traverse.Create(this.chest).Field("chestName").GetValue<SunHavenInputField>();
        name.onEndEdit.AddListener(delegate(string text)
        {
            chest.SetChestNameAndUpdate(text);
        });
    }
}