using BananaHook.Utils;
using ExitGames.Client.Photon;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace BananaHook.HookAndPatch
{
    public class OnPlayerTaggedByPlayerHook
    {
        static public void OnEvent(EventData photonEvent)
        {
            object[] tagObj = (object[])photonEvent.Parameters.TryGetObject(245);
            string taggerUserId = (string)tagObj[0], victimUserId = (string)tagObj[1];
            Player tagger = null, victim = null;
            bool isTag = Utils.Room.IsTagging();
            if(!BananaHook.m_bUseSoundAsRoundEnd) Utils.Room.CheckForTheGameEndPre();
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if (p.UserId == taggerUserId) tagger = p;
                if (p.UserId == victimUserId) victim = p;
            }
            if (Events.OnPlayerTagPlayer != null)
            {
                PlayerTaggedPlayerArgs args = new PlayerTaggedPlayerArgs();
                args.tagger = tagger;
                args.victim = victim;
                args.isTagging = isTag;
                Events.OnPlayerTagPlayer(null, args);
            }
            if(victim == PhotonNetwork.LocalPlayer && Events.OnLocalPlayerTag != null)
            {
                PlayerTaggedPlayerArgs args = new PlayerTaggedPlayerArgs();
                args.tagger = tagger;
                args.victim = victim;
                args.isTagging = isTag;
                Events.OnLocalPlayerTag(null, args);
            }
            if (!BananaHook.m_bUseSoundAsRoundEnd) Utils.Room.CheckForTheGameEndPost();
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
                    Events.OnLocalNicknameChange(null, args);
                }
            }
        }
    }
}
