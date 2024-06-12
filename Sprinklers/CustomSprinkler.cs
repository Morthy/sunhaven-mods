
using UnityEngine;
using UnityEngine.Events;
using Wish;
using ZeroFormatter;

namespace Sprinklers;

public class CustomSprinkler : Decoration
{
    public int range;

    public override int UpdateOrder => 101;

    public override void LateUpdateMetaOvernight(ref DecorationPositionData decorationData)
    {
      base.LateUpdateMetaOvernight(ref decorationData);
      for (int index1 = -range; index1 <= range; ++index1)
      {
        for (int index2 = -range; index2 <= range; ++index2)
        {
          Vector2Int vector2Int = new Vector2Int(decorationData.x + index1 * 6, decorationData.y + index2 * 6);
          DecorationPositionData decorationPositionData;
          if (SingletonBehaviour<GameSave>.Instance.CurrentWorld.Decorations[decorationData.sceneID].TryGetValue(new KeyTuple<ushort, ushort, sbyte>((ushort) vector2Int.x, (ushort) vector2Int.y, decorationData.z), out decorationPositionData))
          {
            if (ItemDatabase.ValidID(decorationPositionData.id))
            {
              if (ItemDatabase.GetItemData(decorationPositionData.id).useItem is Seeds)
              {
                CropSaveData data = null;
                if (DeserializeMeta(decorationPositionData.meta, ref data) && (!data.watered || data.onFire))
                {

                  data.onFire = false;
                  data.watered = true;
                  byte[] meta = ZeroFormatterSerializer.Serialize(data);
                  Vector3Int position = new Vector3Int(decorationPositionData.x, decorationPositionData.y, decorationPositionData.z);
                  short sceneId = decorationPositionData.sceneID;
                  decorationPositionData.meta = meta;
                  UnityAction<Vector3Int, short, byte[]> decorationMetaUpdated = GameManager.onDecorationMetaUpdated;
                  if (decorationMetaUpdated != null)
                    decorationMetaUpdated(position, sceneId, meta);
                  SingletonBehaviour<GameManager>.Instance.UpdateDecorationMetaFromRPC(position, sceneId, meta);
                }
              }
            }
            else
              continue;
          }
          Vector2Int position1 = new Vector2Int(decorationData.x / 6 + index1, decorationData.y / 6 + index2);
          if (SingletonBehaviour<TileManager>.Instance.IsWaterable(position1, decorationData.sceneID))
            SingletonBehaviour<TileManager>.Instance.Water(position1, decorationData.sceneID);
        }
      }
    }
}