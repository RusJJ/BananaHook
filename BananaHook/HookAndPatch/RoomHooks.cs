using BananaHook.Utils;
using HarmonyLib;
using Photon.Pun;
using System;
using System.Threading;

namespace BananaHook.Patches
{
    /* Utilla's thing */
    [HarmonyPatch(typeof(PhotonNetworkController))]
    [HarmonyPatch("OnJoinedRoom", MethodType.Normal)]
    internal class OnRoomJoined
    {
        private static bool m_bIsPrivateLobby = false;
        private static void Postfix()
        {
            Room.m_bIsTagging = Room.IsTagging();
            Room.m_eCurrentLobbyMode = (eRoomQueue)Enum.Parse(typeof(eRoomQueue), UnityEngine.PlayerPrefs.GetString("currentQueue", "DEFAULT"), true);
            Room.m_szRoomCode = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : null;
            Room.CheckForTheGameEndPre();
            if (PhotonNetwork.CurrentRoom != null)
            {
                var currentRoom = PhotonNetwork.NetworkingClient.CurrentRoom;
                m_bIsPrivateLobby = !currentRoom.IsVisible || currentRoom.CustomProperties.ContainsKey("Description");
            }
            if (Events.OnRoomJoined != null)
            {
                RoomJoinedArgs args = new RoomJoinedArgs();
                args.isPrivate = m_bIsPrivateLobby;
                args.roomCode = PhotonNetwork.CurrentRoom.Name;
                try
                { 
                    Events.OnRoomJoined(null, args);
                }
                catch (Exception e) { BananaHook.Log("OnRoundStart Exception: " + e.StackTrace); }
            }
            /*if (Events.OnRoomJoinedPost != null)
            {
                BananaHook.Log("Ping before: " + PhotonNetwork.GetPing());
                BananaHook.Log("Region: " + PhotonNetwork.CloudRegion);
                Thread thread = new Thread(Thread_Late);
                thread.Start();
            }*/
            Room.CheckForTheGameEndPost();
        }
        // Looks dumb so currently it's "removed".
        // Lets hope there wont be any "join untagged" glitches
        /*private static void Thread_Late()
        {
            // I can't determine a MasterClient's ping... So, we should use THIS. Sorry. (max. required is 350)
            Thread.Sleep(350);
            //Thread.Sleep(PhotonNetwork.GetPing());
            if (Events.OnRoomJoinedPost != null)
            {
                RoomJoinedArgs args = new RoomJoinedArgs();
                args.isPrivate = m_bIsPrivateLobby;
                args.roomCode = PhotonNetwork.CurrentRoom.Name;
                Events.OnRoomJoinedPost(null, args);
            }
            BananaHook.Log("Tagged: " + (Players.IsInfected(PhotonNetwork.LocalPlayer) ? "tagged" : "not tagged"));
        }*/
    }

    [HarmonyPatch(typeof(PhotonNetwork))]
    [HarmonyPatch("Disconnect", MethodType.Normal)]
    internal class OnRoomDisconnected
    {
        private static void Prefix()
        {
            Room.m_bIsGameEnded = false;
            Room.m_szRoomCode = null;
            if (!PhotonNetwork.InRoom) return;
            try
            {
                Events.OnRoomDisconnected?.Invoke(null, null);
            }
            catch (Exception e) { BananaHook.Log("OnRoomDisconnected Exception: " + e.StackTrace); }
            // Player is still infected at the join moment?
            // That thing is not curing but i dont want to change game's mechanics
            /*Hashtable hash = new Hashtable(); hash.Add("isInfected", false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);*/
        }
    }

    [HarmonyPatch(typeof(PhotonHandler))]
    [HarmonyPatch("OnPlayerEnteredRoom", MethodType.Normal)]
    internal class OnPlayerConnected
    {
        private static void Postfix(Photon.Realtime.Player newPlayer)
        {
            if (!PhotonNetwork.InRoom) return;
            if (Room.m_eCurrentLobbyMode != eRoomQueue.Casual)
            {
                bool isTaggingNow = Room.IsTagging();
                if(Room.m_bIsTagging != isTaggingNow && Events.OnTagOrInfectChange != null)
                {
                    IsTagOrInfectArgs args = new IsTagOrInfectArgs();
                    args.isTagging = isTaggingNow;
                    try
                    { 
                        Events.OnTagOrInfectChange(null, args);
                    }
                    catch (Exception e) { BananaHook.Log("OnTagOrInfectChange Exception: " + e.StackTrace); }
                }
                Room.m_bIsTagging = isTaggingNow;
            }
            if (Events.OnPlayerConnected != null)
            {
                PlayerDisConnectedArgs args = new PlayerDisConnectedArgs();
                args.player = newPlayer;
                try
                { 
                    Events.OnPlayerConnected(null, args);
                }
                catch (Exception e) { BananaHook.Log("OnPlayerConnected Exception: " + e.StackTrace); }
            }
        }
    }

    [HarmonyPatch(typeof(VRRig))]
    [HarmonyPatch("PlayTagSound", MethodType.Normal)]
    internal class OnPlayedTagSound
    {
        private static void Postfix(int soundIndex)
        {
            // Can be risky. What if "2" become changed?
            // Also it's really useful since there's a glitch
            // where someone is NOT tagged, but the game ended.
            switch(soundIndex)
            {
                case 0: // Player Tagged
                    if(BananaHook.m_bUseSoundAsRoundStart && Room.m_bIsGameEnded && !Room.m_bIsTagging && Players.CountInfectedPlayers() == 1)
                    {
                        Room.m_bIsGameEnded = false;
                        OnRoundStartArgs args = new OnRoundStartArgs();
                        args.player = Players.GetFirstGuyInfected();
                        try
                        {
                            Events.OnRoundStart(null, args);
                        }
                        catch (Exception e) { BananaHook.Log("OnRoundStart (PlayTagSound) Exception: " + e.StackTrace); }
                    }
                    break;

                //case 1: break; // Player joined? Tag/Untag?

                case 2: // End of Infection Game
                    if (!BananaHook.m_bUseSoundAsRoundEnd) return;
                    Room.m_bIsGameEnded = true;
                    try
                    {
                        Events.OnRoundEndPost?.Invoke(null, null);
                    }
                    catch (Exception e) { BananaHook.Log("OnRoundEndPost Exception: " + e.StackTrace); }
                    if (!BananaHook.m_bUseSoundAsRoundStart && Events.OnRoundStart != null)
                    {
                        // Sadly i need it currently.
                        // Because the MasterClient is not sending any RPC for this.
                        Room.m_hCheckerThread = new Thread(Room.Thread_CheckForGameToStart);
                        Room.m_hCheckerThread.Start();
                    }
                    break;

                //case 3: break; // Flag taken
            }
        }
    }

    [HarmonyPatch(typeof(PhotonHandler))]
    [HarmonyPatch("OnPlayerLeftRoom", MethodType.Normal)]
    internal class OnPlayerDisconnected
    {
        private static void Prefix(Photon.Realtime.Player otherPlayer)
        {
            if (Events.OnPlayerDisconnectedPre != null)
            {
                PlayerDisConnectedArgs args = new PlayerDisConnectedArgs();
                args.player = otherPlayer;
                try
                { 
                    Events.OnPlayerDisconnectedPre(null, args);
                }
                catch (Exception e) { BananaHook.Log("OnPlayerDisconnectedPre Exception: " + e.StackTrace); }
            }
        }
        private static void Postfix(Photon.Realtime.Player otherPlayer)
        {
            if (!BananaHook.m_bUseSoundAsRoundEnd) Room.CheckForTheGameEndPre();
            if (Events.OnPlayerDisconnectedPost != null)
            {
                PlayerDisConnectedArgs args = new PlayerDisConnectedArgs();
                args.player = otherPlayer;
                try
                { 
                    Events.OnPlayerDisconnectedPost(null, args);
                }
                catch (Exception e) { BananaHook.Log("OnPlayerDisconnectedPost Exception: " + e.StackTrace); }
            }
            if (Room.m_eCurrentLobbyMode != eRoomQueue.Casual)
            {
                bool isTaggingNow = Room.IsTagging();
                if (Room.m_bIsTagging != isTaggingNow && Events.OnTagOrInfectChange != null)
                {
                    IsTagOrInfectArgs args = new IsTagOrInfectArgs();
                    args.isTagging = isTaggingNow;
                    try
                    { 
                        Events.OnTagOrInfectChange(null, args);
                    }
                    catch (Exception e) { BananaHook.Log("OnTagOrInfectChange Exception: " + e.StackTrace); }
                }
                Room.m_bIsTagging = isTaggingNow;
            }
            if (!BananaHook.m_bUseSoundAsRoundEnd) Room.CheckForTheGameEndPost();
        }
    }

    [HarmonyPatch(typeof(GorillaNetworkJoinTrigger))]
    [HarmonyPatch("OnBoxTriggered", MethodType.Normal)]
    internal class OnJoinTriggerTriggered
    {
        private static void Postfix(GorillaNetworkJoinTrigger __instance)
        {
            if (__instance == GorillaComputer.instance.cityMapTrigger) { Room.m_eTriggeredMap = eJoinedMap.GorillaShop; return; }
            if (__instance == GorillaComputer.instance.forestMapTrigger) { Room.m_eTriggeredMap = eJoinedMap.Forest; return; }
            if (__instance == GorillaComputer.instance.caveMapTrigger) { Room.m_eTriggeredMap = eJoinedMap.Cave; return; }
            if (__instance == GorillaComputer.instance.canyonMapTrigger) { Room.m_eTriggeredMap = eJoinedMap.Canyon; return; }

            Room.m_eTriggeredMap = eJoinedMap.Unknown;
        }
    }
}