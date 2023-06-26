using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Wish;
using ZeroFormatter;

namespace CustomItems;

public class DecorationScrubber : UseItem
{
    private bool _created;
    private List<GameObject> _grids = new ();

    public override void Use1()
    {
        if (_grids.Count == 0)
        {
            NotificationStack.Instance.SendNotification("No invalid decorations found in this scene");
            return;
        }
        
        DialogueController.Instance.SetDefaultBox();
        DialogueController.Instance.PushDialogue(new DialogueNode
        {
            dialogueText = new List<string>
            {
                $"Remove {_grids.Count} ghost decoration tiles?"
            },
            responses = new Dictionary<int, Response>
            {
                {
                    1,
                    new Response
                    {
                        responseText = () => "Yes, I've backed up my save file",
                        action = Clean
                    }
                },
                {
                    2,
                    new Response
                    {
                        responseText = () => "Cancel",
                        action = null
                    }
                }
            }
        });
        
    }

    private void Clean()
    {
        var decorations = GameSave.Instance.CurrentWorld.decorations[ScenePortalManager.ActiveSceneIndex].ToList();

        foreach (var d in decorations)
        {
            var key = new Vector3Int(d.Value.x, d.Value.y, d.Value.z);
            if (!SingletonBehaviour<GameManager>.Instance.objects.ContainsKey(key))
            {
                GameSave.Instance.CurrentWorld.decorations[ScenePortalManager.ActiveSceneIndex].Remove(d.Key);
            }
        }

        OnDisable();
        NotificationStack.Instance.SendNotification("Ghost decorations removed!");
    }

    protected virtual void LateUpdate()
    {
        if (!player || !player.IsOwner)
            return;

        var decorations = GameSave.Instance.CurrentWorld.decorations[ScenePortalManager.ActiveSceneIndex];

        if (decorations == null)
        {
            return;
        }
        
        if (!_created)
        {
            foreach (var d in decorations)
            {
                var key = new Vector3Int(d.Value.x, d.Value.y, d.Value.z);
                if (!SingletonBehaviour<GameManager>.Instance.objects.ContainsKey(key))
                {
                    _grids.Add(MakeDecorationGrid(key));
                }
            }

            _created = true;
        }
        
    }

    protected virtual void OnDisable()
    {
        foreach (var o in _grids)
        {
            Destroy(o);
        }

        _grids.Clear();
        _created = false;
    }
    
    
    private GameObject MakeDecorationGrid(Vector3 position, float alpha = 0.25f)
    {
        var go = new GameObject();
        var spriteRenderer = go.AddComponent<SpriteRenderer>();
        --spriteRenderer.sortingOrder;
        spriteRenderer.sprite = SingletonBehaviour<Prefabs>.Instance.decorationGrid;
        spriteRenderer.drawMode = SpriteDrawMode.Simple;
        go.transform.localScale = new Vector3(0.5f, 1.4142135f/2, 1f);
        go.transform.eulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        go.transform.position = new Vector3(position.x / 6f, position.y / 6f * 1.4142135381698608f, position.z - 0.015f);
        Plugin.logger.LogInfo($"{position.x / 6f} {position.y / 6f * 1.4142135381698608f}");
        spriteRenderer.size = new Vector2(1, 1);
        spriteRenderer.color = new Color(1f, 0.33f, 0.33f, 1f);
        return go;
    }
    
}