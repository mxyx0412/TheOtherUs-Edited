﻿using System;
using System.Collections.Generic;
using InnerNet;
using TheOtherRoles.CustomGameModes;
using TheOtherRoles.Utilities;
using TMPro;
using UnityEngine;

namespace TheOtherRoles.Patches;

[HarmonyPatch]
public static class CredentialsPatch
{
    public static string fullCredentialsVersion = $"{getString("fullCredentialsVersion")}v{TheOtherRolesPlugin.Version + (TheOtherRolesPlugin.betaDays > 0 ? "-BETA" : "")}";

    public static string fullCredentials = getString("fullCredentials");

    public static string mainMenuCredentials = getString("mainMenuCredentials");

    public static string contributorsCredentials = getString("contributorsCredentials");

    [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
    internal static class PingTrackerPatch
    {
        public static GameObject modStamp;

        private static void Postfix(PingTracker __instance)
        {
            __instance.text.alignment = TextAlignmentOptions.TopRight;
            if (AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started)
            {
                var gameModeText = "";
                if (HideNSeek.isHideNSeekGM) gameModeText = getString("isHideNSeekGM");
                else if (HandleGuesser.isGuesserGm) gameModeText = getString("isGuesserGm");
                else if (PropHunt.isPropHuntGM) gameModeText = getString("isPropHuntGM");
                if (gameModeText != "") gameModeText = Helpers.cs(Color.yellow, gameModeText) + "\n";
                __instance.text.text =
                    $"{getString("fullCredentialsVersion2")} v{TheOtherRolesPlugin.Version + "\n" + getString("gameTitle2")}\n<size=90%>{gameModeText}</size>" +
                    __instance.text.text;
                if (CachedPlayer.LocalPlayer.Data.IsDead || (!(CachedPlayer.LocalPlayer.PlayerControl == null) &&
                                                           (CachedPlayer.LocalPlayer.PlayerControl == Lovers.lover1 ||
                                                            CachedPlayer.LocalPlayer.PlayerControl == Lovers.lover2)))
                {
                    var transform = __instance.transform;
                    var localPosition = transform.localPosition;
                    localPosition = new Vector3(3.4f, localPosition.y, localPosition.z);
                    transform.localPosition = localPosition;
                }
                else
                {
                    var transform = __instance.transform;
                    var localPosition = transform.localPosition;
                    localPosition = new Vector3(4.2f, localPosition.y, localPosition.z);
                    transform.localPosition = localPosition;
                }
            }
            else
            {
                var gameModeText = MapOptions.gameMode switch
                {
                    CustomGamemodes.HideNSeek => getString("isHideNSeekGM"),
                    CustomGamemodes.Guesser => getString("isGuesserGm"),
                    CustomGamemodes.PropHunt => getString("isPropHuntGM"),
                    _ => ""
                };
                if (gameModeText != "") gameModeText = Helpers.cs(Color.yellow, gameModeText) + "\n";

                __instance.text.text =
                    $"{fullCredentialsVersion}\n  {gameModeText + fullCredentials}\n {__instance.text.text}";
                var transform = __instance.transform;
                var localPosition = transform.localPosition;
                localPosition = new Vector3(3.5f, localPosition.y, localPosition.z);
                transform.localPosition = localPosition;
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class LogoPatch
    {
        public static SpriteRenderer renderer;
        public static Sprite bannerSprite;
        public static Sprite horseBannerSprite;
        public static Sprite banner2Sprite;
        private static PingTracker instance;

        public static GameObject motdObject;
        public static TextMeshPro motdText;

        private static void Postfix(PingTracker __instance)
        {
            var torLogo = new GameObject("bannerLogo_TOR");
            torLogo.transform.SetParent(GameObject.Find("RightPanel").transform, false);
            torLogo.transform.localPosition = new Vector3(-0.4f, 1f, 5f);

            renderer = torLogo.AddComponent<SpriteRenderer>();
            loadSprites();
            renderer.sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);

            instance = __instance;
            loadSprites();
            // renderer.sprite = TORMapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
            renderer.sprite = EventUtility.isEnabled ? banner2Sprite : bannerSprite;
            var credentialObject = new GameObject("credentialsTOR");
            var credentials = credentialObject.AddComponent<TextMeshPro>();
            credentials.SetText(
                $"v{TheOtherRolesPlugin.Version + (TheOtherRolesPlugin.betaDays > 0 ? "-BETA" : "")}\n<size=30f%>\n</size>{mainMenuCredentials}\n<size=30%>\n</size>{contributorsCredentials}");
            credentials.alignment = TextAlignmentOptions.Center;
            credentials.fontSize *= 0.05f;

            credentials.transform.SetParent(torLogo.transform);
            credentials.transform.localPosition = Vector3.down * 1.25f;
            motdObject = new GameObject("torMOTD");
            motdText = motdObject.AddComponent<TextMeshPro>();
            motdText.alignment = TextAlignmentOptions.Center;
            motdText.fontSize *= 0.04f;

            motdText.transform.SetParent(torLogo.transform);
            motdText.enableWordWrapping = true;
            var rect = motdText.gameObject.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(5.2f, 0.25f);

            motdText.transform.localPosition = Vector3.down * 2.25f;
            motdText.color = new Color(1, 53f / 255, 31f / 255);
            var mat = motdText.fontSharedMaterial;
            mat.shaderKeywords = new[] { "OUTLINE_ON" };
            motdText.SetOutlineColor(Color.white);
            motdText.SetOutlineThickness(0.025f);
        }

        public static void loadSprites()
        {
            if (bannerSprite == null)
                bannerSprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner.png", 300f);
            if (banner2Sprite == null)
                banner2Sprite = Helpers.loadSpriteFromResources("TheOtherRoles.Resources.Banner2.png", 300f);
            if (horseBannerSprite == null)
                horseBannerSprite =
                    Helpers.loadSpriteFromResources("TheOtherRoles.Resources.bannerTheHorseRoles.png", 300f);
        }

        public static void updateSprite()
        {
            loadSprites();
            if (renderer != null)
            {
                var fadeDuration = 1f;
                instance.StartCoroutine(Effects.Lerp(fadeDuration, new Action<float>(p =>
                {
                    renderer.color = new Color(1, 1, 1, 1 - p);
                    if (p == 1)
                    {
                        renderer.sprite = MapOptions.enableHorseMode ? horseBannerSprite : bannerSprite;
                        instance.StartCoroutine(Effects.Lerp(fadeDuration,
                            new Action<float>(p => { renderer.color = new Color(1, 1, 1, p); })));
                    }
                })));
            }
        }
    }

    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.LateUpdate))]
    public static class MOTD
    {
        public static List<string> motds = new();
        private static float timer;
        private static readonly float maxTimer = 5f;
        private static int currentIndex;

        public static void Postfix()
        {
            if (motds.Count == 0)
            {
                timer = maxTimer;
                return;
            }

            if (motds.Count > currentIndex && LogoPatch.motdText != null)
                LogoPatch.motdText.SetText(motds[currentIndex]);
            else return;

            // fade in and out:
            var alpha = Mathf.Clamp01(Mathf.Min(new[] { timer, maxTimer - timer }));
            if (motds.Count == 1) alpha = 1;
            LogoPatch.motdText.color = LogoPatch.motdText.color.SetAlpha(alpha);
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = maxTimer;
                currentIndex = (currentIndex + 1) % motds.Count;
            }
        }
    }
}