using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using Modding;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace Screenshotter;

public class Screenshotter : Mod
{
    private static string _dir;
    private static readonly string _folder = "Screenshots";

    public Screenshotter() : base("Screenshotter")
    {
        _dir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new DirectoryNotFoundException("I have no idea how you did this, but good luck figuring it out."), _folder);

        if (!Directory.Exists(_dir))
        {
            Directory.CreateDirectory(_dir);
        }
    }

    public override void Initialize()
    {
        ScreenshotMb screenshotMb = GameManager.instance.cameraCtrl.cam.gameObject.AddComponent<ScreenshotMb>();
        screenshotMb.Dir = _dir;
    }

    private IEnumerator ListenToF11()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                yield return new WaitForEndOfFrame();
                CaptureScreen(GameManager.instance.cameraCtrl.cam);
            }
            yield return null;
        }
    }

    private void CaptureScreen(Camera cam)
    {
        cam.targetTexture = new RenderTexture(7680, 4320, 24, RenderTextureFormat.ARGB32);

        RenderTexture activeRenderTexture = RenderTexture.active;
        RenderTexture.active = cam.targetTexture;

        cam.Render();

        Texture2D image = new Texture2D(cam.targetTexture.width, cam.targetTexture.height);
        image.ReadPixels(new Rect(0, 0, cam.targetTexture.width, cam.targetTexture.height), 0, 0);
        image.Apply();
        RenderTexture.active = activeRenderTexture;

        byte[] bytes = image.EncodeToPNG();
        UObject.DestroyImmediate(image);

        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        File.WriteAllBytes(GetPath(sceneName, bytes), bytes);

        cam.targetTexture = null;
    }

    private static string GetPath(string sceneName, byte[] hash)
    {
        return Path.Combine(_dir, $"{sceneName}-{GetByteHash(hash)}.png");
    }
    private static string GetByteHash(byte[] bytes)
    {
        SHA512 hash = new SHA512Managed();
        return BitConverter.ToString(hash.ComputeHash(bytes)).Replace("-", "");
    }
}