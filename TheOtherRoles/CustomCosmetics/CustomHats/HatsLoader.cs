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

    private IEnumerator CoFetchHats()
    {
        isRunning = true;
        string localFilePath = Path.Combine(HatsDirectory, ManifestFileName);

        if (ModOption.localHats)
        {
            LoadLocalHats(localFilePath);
            isRunning = false;
            yield break;
        }

        yield return DownloadHatsConfig(localFilePath);
        isRunning = false;
    }

    private void LoadLocalHats(string Path)
    {
        Message("加载本地帽子文件");
        if (File.Exists(Path))
        {
            var localFileContent = File.ReadAllText(Path);
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
    }

    private IEnumerator DownloadHatsConfig(string Path)
    {
        var www = new UnityWebRequest
        {
            method = UnityWebRequest.kHttpVerbGET,
            downloadHandler = new DownloadHandlerBuffer()
        };

        Message($"正在下载帽子配置文件: {RepositoryUrl}/{ManifestFileName}");
        www.url = $"{RepositoryUrl}/{ManifestFileName}";

        var operation = www.SendWebRequest();

        while (!operation.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (www.isNetworkError || www.isHttpError)
        {
            Error($"下载帽子配置文件时出错: {www.error}");
            Message("正在尝试以本地方式加载帽子...");
            LoadLocalHats(Path);
            isSuccessful = false;
            yield break;
        }

        try
        {
            if (!Directory.Exists(HatsDirectory))
            {
                Directory.CreateDirectory(HatsDirectory);
            }

            File.WriteAllBytes(Path, www.downloadHandler.data);
            Message($"帽子清单已保存到: {Path}");

            var downloadedFileContent = File.ReadAllText(Path);
            var response = JsonSerializer.Deserialize<SkinsConfigFile>(downloadedFileContent, new JsonSerializerOptions
            {
                AllowTrailingCommas = true
            });

            ProcessHatsData(response);
        }
        catch (Exception ex)
        {
            isSuccessful = false;
            Error($"未能保存或加载帽子配置文件: {ex.Message}");
        }
        finally
        {
            www.downloadHandler.Dispose();
            www.Dispose();
        }
    }

    private void ProcessHatsData(SkinsConfigFile response)
    {
        if (!Directory.Exists(HatsDirectory))
        {
            Directory.CreateDirectory(HatsDirectory);
        }

        UnregisteredHats.AddRange(SanitizeHats(response));
        Message($"读取了 {UnregisteredHats.Count} 项帽子");

        if (!isSuccessful || ModOption.localHats)
        {
            Message("在线配置文件无效，取消下载任务。");
            return;
        };

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
            Message($"正在下载: {fileName}");
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