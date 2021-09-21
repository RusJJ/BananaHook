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
            bool isTag = Utils.Room.IsTagging();
            if (!BananaHook.m_bUseSoundAsRoundEnd) Utils.Room.CheckForTheGameEndPre();
            if (Events.OnPlayerTagPlayer != null)
            {
                PlayerTaggedPlayerArgs args = new PlayerTaggedPlayerArgs();
                args.tagger = tagger;
                args.victim = victim;
                args.isTagging = isTag;
                try
                {
                    Events.OnPlayerTagPlayer(null, args);
                }
                catch (Exception e)
                {
                    BananaHook.Log("OnPlayerTagPlayer Exception: " + e.StackTrace);
                    BananaHook.Log(e.StackTrace);
                    BananaHook.Log("Tagger: " + (tagger==null?"null":tagger.NickName));
                    BananaHook.Log("Victim: " + (victim==null?"null":victim.NickName));
                }
            }
            if (victim == PhotonNetwork.LocalPlayer && Events.OnLocalPlayerTag != null)
            {
                PlayerTaggedPlayerArgs args = new PlayerTaggedPlayerArgs();
                args.tagger = tagger;
                args.victim = victim;
                args.isTagging = isTag;
                try
                { 
                    Events.OnLocalPlayerTag(null, args);
                }
                catch (Exception e) { BananaHook.Log("OnLocalPlayerTag Exception: " + e.StackTrace); }
            }
            if (!BananaHook.m_bUseSoundAsRoundEnd) Utils.Room.CheckForTheGameEndPost();
        }
    }

    /* Because MasterClient is NOT listening for events */
    [HarmonyPatch(typeof(GorillaTagManager))]
    [HarmonyPatch("ReportTag", MethodType.Normal)]
    internal class OnPlayerTagged_MasterClient
    {
        internal static void Prefix(GorillaTagManager __instance, Player taggedPlayer, Player taggingPlayer)
        {
            if (__instance.isCurrentlyTag)
            {
                if (taggingPlayer == __instance.currentIt && taggingPlayer != taggedPlayer && (double)Time.time > __instance.lastTag + (double)__instance.tagCoolDown)
                {
                    //__instance.ChangeCurrentIt(taggedPlayer);
                    goto TAGEVENT;
                }
            }
            else if (__instance.currentInfected.Contains(taggingPlayer) && !__instance.currentInfected.Contains(taggedPlayer) && (double)Time.time > __instance.lastTag + (double)__instance.tagCoolDown)
            {
                //__instance.AddInfectedPlayer(taggedPlayer);
                goto TAGEVENT;
            }
            return;
        TAGEVENT:
            try
            {
                OnPlayerTaggedByPlayerHook.OnEvent(taggingPlayer, taggedPlayer);
            }
            catch (Exception e)
            {
                BananaHook.Log("ReportTag Exception: " + e.StackTrace);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerPrefs))]
    [HarmonyPatch("SetString", MethodType.Normal)]
    public class OnNicknameChange
    {
        private static void Prefix(string key, string value)
        {
            if(key == "playerName")
            {
                if (Events.OnLocalNicknameChange != null)
                {
                    PlayerNicknameArgs args = new PlayerNicknameArgs();
                    args.oldNickName = PhotonNetwork.LocalPlayer.NickName;
                    args.newNickName = value;
                    try
                    { 
                        Events.OnLocalNicknameChange(null, args);
                    }
                    catch (Exception e) { BananaHook.Log("OnLocalNicknameChange Exception: " + e.StackTrace); }
                }
            }
        }
    }
}