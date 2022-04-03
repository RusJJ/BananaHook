using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

namespace BananaHook.HookAndPatch
{
    public class OnPlayerTaggedByPlayerHook
    {
        static public void OnEvent(Player tagger, Player victim)
        {
            bool isTagging = Utils.Room.IsTagging();
            if (Events.OnPlayerTagPlayer != null)
            {
                PlayerTaggedPlayerArgs args = new PlayerTaggedPlayerArgs();
                args.tagger = tagger;
                args.victim = victim;
                args.isTagging = isTagging;
                object[] obja = { null, args };
                foreach (var del in Events.OnPlayerTagPlayer.GetInvocationList())
                {
                    try
                    {
                        del.DynamicInvoke(obja);
                    }
                    catch (Exception e)
                    {
                        BananaHook.Log("OnPlayerTagPlayer Exception: " + e.Message + "\n" + e.StackTrace + "\nTagger: " + (tagger == null ? "null" : tagger.NickName) + "\nVictim: " + (victim == null ? "null" : victim.NickName));
                        BananaHook.Log(e.StackTrace);
                    }
                }
            }
            if (victim == PhotonNetwork.LocalPlayer && Events.OnLocalPlayerTag != null)
            {
                PlayerTaggedPlayerArgs args = new PlayerTaggedPlayerArgs();
                args.tagger = tagger;
                args.victim = victim;
                args.isTagging = isTagging;
                object[] obja = { null, args };
                foreach (var del in Events.OnLocalPlayerTag.GetInvocationList())
                {
                    try
                    {
                        del.DynamicInvoke(obja);
                    }
                    catch (Exception e) { BananaHook.Log("OnLocalPlayerTag Exception: " + e.Message + "\n" + e.StackTrace); }
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPrefs))]
    [HarmonyPatch("SetString", MethodType.Normal)]
    public class OnNicknameChange
    {
        private static void Postfix(string key, string value)
        {
            if (Events.OnLocalNicknameChange != null && key == "playerName")
            {
                PlayerNicknameArgs args = new PlayerNicknameArgs();
                args.oldNickName = PhotonNetwork.LocalPlayer.NickName;
                args.newNickName = value;
                object[] obja = { null, args };
                foreach (var del in Events.OnLocalNicknameChange.GetInvocationList())
                {
                    try
                    {
                        del.DynamicInvoke(obja);
                    }
                    catch (Exception e) { BananaHook.Log("OnLocalNicknameChange Exception: " + e.Message + "\n" + e.StackTrace); }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GorillaNetworking.GorillaComputer))]
    [HarmonyPatch("InitializeNameState", MethodType.Normal)]
    public class OnComputerGotOurName
    {
        private static void Postfix(GorillaNetworking.GorillaComputer __instance)
        {
            if (Events.OnLocalNicknameChange != null)
            {
                PlayerNicknameArgs args = new PlayerNicknameArgs();
                args.oldNickName = "gorilla";
                args.newNickName = __instance.currentName;
                object[] obja = { null, args };
                foreach (var del in Events.OnLocalNicknameChange.GetInvocationList())
                {
                    try
                    {
                        del.DynamicInvoke(obja);
                    }
                    catch (Exception e) { BananaHook.Log("OnLocalNicknameChange Exception: " + e.Message + "\n" + e.StackTrace); }
                }
            }
        }
    }

    /* Stuff for MasterClient is below */
    /* Stuff for MasterClient is below */
    /* Stuff for MasterClient is below */

    [HarmonyPatch(typeof(GorillaTagManager))]
    [HarmonyPatch("ReportTag", MethodType.Normal)]
    internal class OnPlayerTagged_MasterClient
    {
        internal static void Prefix(GorillaTagManager __instance, Player taggedPlayer, Player taggingPlayer)
        {
            if(__instance.photonView.IsMine && __instance.IsGameModeTag())
            {
                if(__instance.isCurrentlyTag)
                {
                    if (taggingPlayer == __instance.currentIt && taggingPlayer != taggedPlayer && Time.time > __instance.lastTag + __instance.tagCoolDown) goto TAGEVENT;
                }
                else if (__instance.currentInfected.Contains(taggingPlayer) && !__instance.currentInfected.Contains(taggedPlayer) && Time.time > __instance.lastTag + __instance.tagCoolDown) goto TAGEVENT;
            }
            return;
        TAGEVENT:
            try
            {
                OnPlayerTaggedByPlayerHook.OnEvent(taggingPlayer, taggedPlayer);
            }
            catch (Exception e) { BananaHook.Log("ReportTag Exception: " + e.Message + "\n" + e.StackTrace); }
        }
    }

    [HarmonyPatch(typeof(GorillaHuntManager))]
    [HarmonyPatch("ReportTag", MethodType.Normal)]
    internal class OnPlayerHunted_MasterClient
    {
        internal static void Prefix(GorillaHuntManager __instance, Player taggedPlayer, Player taggingPlayer)
        {
            if ((__instance.currentHunted.Contains(taggingPlayer) || !__instance.currentTarget.Contains(taggingPlayer)) && !__instance.currentHunted.Contains(taggedPlayer) && __instance.currentTarget.Contains(taggedPlayer))
            {
                goto TAGEVENT;
            }
            else
            {
                if (__instance.IsTargetOf(taggingPlayer, taggedPlayer)) goto TAGEVENT;
            }
            return;
        TAGEVENT:
            try
            {
                OnPlayerTaggedByPlayerHook.OnEvent(taggingPlayer, taggedPlayer);
            }
            catch (Exception e) { BananaHook.Log("ReportTag Exception: " + e.Message + "\n" + e.StackTrace); }
        }
    }
}