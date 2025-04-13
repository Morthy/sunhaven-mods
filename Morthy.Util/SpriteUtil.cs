using UnityEngine;

namespace Morthy.Util;

public class SpriteUtil
{
    private static Texture2D CreateTexture(byte[] data)
    {
        var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        texture.LoadImage(data);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.wrapModeU = TextureWrapMode.Clamp;
        texture.wrapModeV = TextureWrapMode.Clamp;
        texture.wrapModeW = TextureWrapMode.Clamp;
        return texture;
    }

    public static Sprite CreateSprite(byte[] textureData, string name)
    {
        var texture = CreateTexture(textureData);
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 24);
        sprite.name = name;
        texture.name = name;
        return sprite;
    }
    
    public static Sprite CreateSpriteDecoration(byte[] textureData, string name)
    {
        var texture = CreateTexture(textureData);
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0f, 0f), 24,  0, SpriteMeshType.FullRect);
        sprite.name = name;
        texture.name = name;
        return sprite;
    }
    
    public static Sprite CreateSprite(byte[] textureData, Vector2 pivot, string name)
    {
        var texture = CreateTexture(textureData);
        var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), pivot, 24,  0, SpriteMeshType.FullRect);
        sprite.name = name;
        texture.name = name;
        return sprite;
    }
}