using System;
using System.Collections;
using System.IO;
using System.Text.Json;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using UnityEngine.Networking;
using static TheOtherRoles.CustomCosmetics.CustomHats.CustomHatManager;

namespace TheOtherRoles.CustomCosmetics.CustomHats;

public class HatsLoader : MonoBehaviour
{
    private bool isRunning;
    private bool isSuccessful = true;
    public void FetchHats()
    {
        if (isRunning) return;
        this.StartCoroutine(CoFetchHats());
    }

    [HideFromIl2Cpp]
    private IEnumerator CoFetchHats()
    {
        isRunning = true;
        var www = new UnityWebRequest();
        www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
        string localFilePath = Path.Combine(ResourcesDirectory, ManifestFileName);

        Message($"正在下载帽子配置文件: {RepositoryUrl}/{ManifestFileName}");
        www.SetUrl($"{RepositoryUrl}/{ManifestFileName}");
        www.downloadHandler = new DownloadHandlerBuffer();
        var operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (www.isNetworkError || www.isHttpError)
        {
            Error($"下载帽子配置文件时出错: {www.error}");
            Message("正在尝试以本地加载帽子...");

            isSuccessful = false;
            if (File.Exists(localFilePath))
            {
                var localFileContent = File.ReadAllText(localFilePath);
                var response = JsonSerializer.Deserialize<SkinsConfigFile>(localFileContent, new JsonSerializerOptions
                {
                    AllowTrailingCommas = true
                });
                ProcessHatsData(response);
            }
            else
            {
                Error("不存在本地帽子配置文件.");
            }

            isRunning = false;
            yield break;
        }

        try
        {
            if (!Directory.Exists(ResourcesDirectory))
            {
                Directory.CreateDirectory(ResourcesDirectory);
            }

            File.WriteAllBytes(localFilePath, www.downloadHandler.data);
            Message($"帽子配置文件已保存到: {localFilePath}");

            var downloadedFileContent = File.ReadAllText(localFilePath);
            var response = JsonSerializer.Deserialize<SkinsConfigFile>(downloadedFileContent, new JsonSerializerOptions
            {
                AllowTrailingCommas = true
            });

            ProcessHatsData(response);
        }
        catch (Exception ex)
        {
            Error($"未能保存或加载帽子配置文件: {ex.Message}");
        }

        www.downloadHandler.Dispose();
        www.Dispose();
        isRunning = false;
    }

    private void ProcessHatsData(SkinsConfigFile response)
    {
        if (!Directory.Exists(HatsDirectory))
        {
            Directory.CreateDirectory(HatsDirectory);
        }

        if (!isSuccessful)
        {
            Error("在线配置文件无效，取消下载任务。");
            return;
        };

        UnregisteredHats.AddRange(SanitizeHats(response));
        var toDownload = GenerateDownloadList(UnregisteredHats);

        Message($"准备下载 {toDownload.Count} 项帽子文件");

        foreach (var fileName in toDownload)
        {
            this.StartCoroutine(CoDownloadHatAsset(fileName));
        }
    }

    private static IEnumerator CoDownloadHatAsset(string fileName)
    {
        var www = new UnityWebRequest();
        www.SetMethod(UnityWebRequest.UnityWebRequestMethod.Get);
        fileName = fileName.Replace(" ", "%20");
        www.SetUrl($"{RepositoryUrl}/hats/{fileName}");
        Message($"正在下载: {fileName}");
        www.downloadHandler = new DownloadHandlerBuffer();
        var operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (www.isNetworkError || www.isHttpError)
        {
            Error(www.error);
            yield break;
        }

        var filePath = Path.Combine(HatsDirectory, fileName);
        filePath = filePath.Replace("%20", " ");
        var persistTask = File.WriteAllBytesAsync(filePath, www.downloadHandler.data);
        while (!persistTask.IsCompleted)
        {
            if (persistTask.Exception != null)
            {
                Error(persistTask.Exception.Message);
                break;
            }
            yield return new WaitForEndOfFrame();
        }

        www.downloadHandler.Dispose();
        www.Dispose();
    }
}