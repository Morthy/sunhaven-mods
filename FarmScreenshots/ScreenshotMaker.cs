using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using Wish;

namespace FarmScreenshots;

public class ScreenshotMaker : MonoBehaviour
{
    public Vector3 cameraPosition;
    public float orthographicSize;
    public float aspectRatio;
    public int screenshotWidth;
    public int screenshotHeight;
    private GameObject _cameraObject;
    private readonly List<GameObject> _temporaryHidden = new();
    private int _waitCount = 0;
    
    public void Awake()
    {
        Plugin.logger.LogInfo("Awake");
    }

    public void TakeScreenshot()
    {
        //HideStuff();
        _cameraObject = Instantiate(Player.Instance.Camera.gameObject, this.transform);
        _cameraObject.transform.eulerAngles = new Vector3(315f, 0, 0);
        _cameraObject.transform.position = cameraPosition;
        var renderCamera = _cameraObject.GetComponent<Camera>();
        renderCamera.enabled = false;
        renderCamera.forceIntoRenderTexture = true;
        renderCamera.orthographic = true;
        renderCamera.orthographicSize = orthographicSize;
        renderCamera.aspect = aspectRatio;
        renderCamera.targetDisplay = 0;
        renderCamera.backgroundColor = new Color(0.0198f, 0.0249f, 0.0377f, 1f);
        
        Camera.onPostRender += OnCameraPostRender;

        renderCamera.enabled = true;
        
        var renderTexture = new RenderTexture(screenshotWidth, screenshotHeight, 24);
        renderCamera.targetTexture = renderTexture;
        renderCamera.Render();
    }

    private void OnCameraPostRender(Camera renderCamera)
    {
        if (renderCamera != _cameraObject.GetComponent<Camera>())
        {
            return;
        }

        _waitCount++;

        if (_waitCount < 10)
        {
            return;
        }

        try
        {
            var image = new Texture2D(renderCamera.targetTexture.width, renderCamera.targetTexture.height, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, renderCamera.targetTexture.width, renderCamera.targetTexture.height), 0, 0);
            image.Apply();

            var bytes = image.EncodeToPNG();

            var screenshotDir = Path.Combine(BepInEx.Paths.PluginPath, "Screenshots");

            if (!Directory.Exists(screenshotDir))
            {
                Directory.CreateDirectory(screenshotDir);
            }

            var screenshotFile = Path.Combine(screenshotDir, SingletonBehaviour<GameSave>.Instance.CurrentSave.characterData.characterName + " " + DateTime.Now.ToString("yyyyMMddhhmmss") + ".png");

            Plugin.logger.LogInfo(screenshotFile);

            if (!File.Exists(screenshotFile))
            {
                File.WriteAllBytes(screenshotFile, bytes);
                NotificationStack.Instance.SendNotification("Screenshot saved successfully!");
            }

            RenderTexture.active = null;
            renderCamera.targetTexture = null;
            renderCamera.enabled = false;
        }
        catch (Exception e)
        {
            Plugin.logger.LogError(e);
        }

        Camera.onPostRender -= OnCameraPostRender;
        
        //ShowStuff();
        Destroy(gameObject);
    }

    private void HideStuff()
    {

    }

    private void ShowStuff()
    {
        
    }
}