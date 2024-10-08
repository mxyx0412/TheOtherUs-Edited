using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace TheOtherRoles.Modules;

public class BepInExUpdater : MonoBehaviour
{
    public const string RequiredBepInExVersion = "6.0.0-be.692+851521cc126e4f9d841d2d9bfe857558f0395939";
    public const string BepInExDownloadURL = "https://builds.bepinex.dev/projects/bepinex_be/692/BepInEx-Unity.IL2CPP-win-x86-6.0.0-be.692%2B851521c.zip";
    public static bool UpdateRequired => Paths.BepInExVersion.ToString() != RequiredBepInExVersion;

    public void Awake()
    {
        Message("BepInEx Update Required...");
        Message($"{Paths.BepInExVersion}, {RequiredBepInExVersion} ");
        this.StartCoroutine(CoUpdate());

    }

    [HideFromIl2Cpp]
    public IEnumerator CoUpdate()
    {
        Task.Run(() => MessageBox(GetForegroundWindow(), "正在更新BepinEx模组框架, 请稍后...", "The Other Us - Edited", 0));
        UnityWebRequest www = UnityWebRequest.Get(BepInExDownloadURL);
        yield return www.Send();
        if (www.isNetworkError || www.isHttpError)
        {
            Error(www.error);
            yield break;
        }

        var zipPath = Path.Combine(Paths.GameRootPath, ".bepinex_update");
        File.WriteAllBytes(zipPath, www.downloadHandler.data);


        var tempPath = Path.Combine(Path.GetTempPath(), "TheOtherUpdater.exe");
        var asm = Assembly.GetExecutingAssembly();
        var exeName = asm.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith("TheOtherUpdater.exe"));

        using (var resource = asm.GetManifestResourceStream(exeName))
        {
            using var file = new FileStream(tempPath, FileMode.OpenOrCreate, FileAccess.Write);
            resource!.CopyTo(file);
        }

        var startInfo = new ProcessStartInfo(tempPath, $"--game-path \"{Paths.GameRootPath}\" --zip \"{zipPath}\"");
        startInfo.UseShellExecute = false;
        Process.Start(startInfo);
        Application.Quit();
    }

    [DllImport("user32.dll")]
    public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, int options);
    [DllImport("user32.dll")]
    public static extern int MessageBoxTimeout(IntPtr hwnd, string text, string title, uint type, short wLanguageId, int milliseconds);
}

[HarmonyPatch(typeof(SplashManager), nameof(SplashManager.Update))]
public static class StopLoadingMainMenu
{
    public static bool Prefix()
    {
        return !BepInExUpdater.UpdateRequired;
    }
}