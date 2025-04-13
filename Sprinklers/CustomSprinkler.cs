
using System.Collections.Generic;
using PSS;
using UnityEngine;
using UnityEngine.Events;
using Wish;
using ZeroFormatter;

namespace Sprinklers;

public class CustomSprinkler : Decoration
{
  private static Dictionary<int, int> Ranges = new()
  {
    { ItemHandler.SmallSprinklerId, 1 },
    { ItemHandler.LargeSprinklerId, 2 },
    { ItemHandler.NelvariSprinklerId, 3 },
    { ItemHandler.WithergateSprinklerId, 4 },
  };

    public override int UpdateOrder => 101;

    public static void SprinkleSprinkle(ref DecorationPositionData decorationData)
    {
      var range = Ranges[decorationData.id];
      for (int index1 = -range; index1 <= range; ++index1)
      {
        for (int index2 = -range; index2 <= range; ++index2)
        {
          Vector2Int vector2Int = new Vector2Int(decorationData.x + index1 * 6, decorationData.y + index2 * 6);
          DecorationPositionData decorationPositionData;
          
          if (SingletonBehaviour<GameSave>.Instance.CurrentWorld.Decorations[decorationData.sceneID].TryGetValue(new DecorationKey((ushort)vector2Int.x, (ushort)vector2Int.y, decorationData.z), out decorationPositionData))
          {
            if (Database.ValidID(decorationPositionData.id))
            {
              
              Database.GetData<ItemData>(decorationPositionData.id, itemData =>
              {
                if (itemData.useItem is not Seeds) return;
                
                CropSaveData data = null;
                if (!DeserializeMeta(decorationPositionData.meta, ref data) || (data.watered && !data.onFire)) return;
                
                data.onFire = false;
                data.watered = true;
                byte[] meta = ZeroFormatterSerializer.Serialize(data);
                Vector3Int position = new Vector3Int(decorationPositionData.x, decorationPositionData.y, decorationPositionData.z);
                short sceneId = decorationPositionData.sceneID;
                decorationPositionData.meta = meta;
                UnityAction<Vector3Int, short, byte[]> decorationMetaUpdated = GameManager.onDecorationMetaUpdated;
                decorationMetaUpdated?.Invoke(position, sceneId, meta);
                SingletonBehaviour<GameManager>.Instance.UpdateDecorationMetaFromRPC(position, sceneId, meta);
              });
            }
            else
              continue;
          }
          Vector2Int position1 = new Vector2Int(decorationData.x / 6 + index1, decorationData.y / 6 + index2);
          if (SingletonBehaviour<TileManager>.Instance.IsWaterable(position1, decorationData.sceneID))
          {
            SingletonBehaviour<TileManager>.Instance.Water(position1, decorationData.sceneID);
          }
        }
      }
    }
}