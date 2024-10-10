using System;
using System.IO;
using System.Security.Cryptography;
using JetBrains.Annotations;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Screenshotter;

public class ScreenshotMb : MonoBehaviour
{
    public string Dir;
    public int Width = 16384; // 1920, 3840, 7680, 15360, 16384
    public int Height = 9216; // 1080, 2160, 4320,  8640,  9216

    private bool _takeScreenshot = false;
    private RenderTexture _activeRenderTexture = null;
    private Camera _camera;

    private void Start()
    {
        _camera = gameObject.GetComponent<Camera>();
    }

    [UsedImplicitly]
    private void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        // Read pixels from the source RenderTexture, apply the material, copy the updated results to the destination RenderTexture
        Graphics.Blit(src, dest);

        if (_takeScreenshot)
        {
            DoScreenshot(dest);
            CleanupScreenshot();

            _takeScreenshot = false;
        }
        else if (Input.GetKeyDown(KeyCode.F11))
        {
            PrepareScreenshot();

            _takeScreenshot = true;
        }
    }

    private void PrepareScreenshot()
    {
        _camera.targetTexture = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32);

        _activeRenderTexture = RenderTexture.active;
        RenderTexture.active = _camera.targetTexture;
    }

    private void DoScreenshot(RenderTexture textureToSave)
    {
        Texture2D image = new Texture2D(textureToSave.width, textureToSave.height, TextureFormat.RGB24, false);
        image.ReadPixels(new Rect(0, 0, textureToSave.width, textureToSave.height), 0, 0);
        image.Apply();

        byte[] bytes = image.EncodeToPNG();
        UObject.DestroyImmediate(image);

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        File.WriteAllBytes(GetPath(sceneName, bytes), bytes);
    }

    private void CleanupScreenshot()
    {
        RenderTexture.active = _activeRenderTexture;
        _camera.targetTexture = null;
    }

    private string GetPath(string sceneName, byte[] hash)
    {
        return Path.Combine(Dir, $"{sceneName}-{GetByteHash(hash)}.png");
    }
    private string GetByteHash(byte[] bytes)
    {
        SHA512 hash = new SHA512Managed();
        return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "");
    }
}