using System.Collections.Generic;
using UnityEngine;
using Wish;

namespace TeleportItems;

public class TeleportItem : UseItem
{
    public string teleportScene;
    public Vector2 teleportLocation;

    public override void Use1()
    {
        DialogueController.Instance.SetDefaultBox();
        DialogueController.Instance.PushDialogue(new DialogueNode()
        {
            dialogueText = new List<string>() { $"Are you sure you wish to use your teleporter? This will consume the item." },
            responses = new Dictionary<int, Response>()
            {
                {
                    1,
                    new Response
                    {
                        responseText = () => "Yes",
                        action = () =>
                        {
                            ScenePortalManager.Instance.ChangeScene(teleportLocation, teleportScene);
                            Player.Instance.Inventory.RemoveItemAt(Player.Instance.ItemIndex, 1);
                        }
                    }
                },
                {
                    2,
                    new Response
                    {
                        responseText = () => "No",
                        action = null
                    }
                },
            }
        });
    }
}