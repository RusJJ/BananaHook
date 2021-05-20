using BananaHook.Utils;
using HarmonyLib;
using Photon.Pun;
using System;

namespace BananaHook.Patches
{
    /* Utilla's thing */
    [HarmonyPatch(typeof(PhotonNetworkController))]
    [HarmonyPatch("OnJoinedRoom", MethodType.Normal)]
    internal class OnRoomJoined
    {
        private static void Postfix()
        {
            bool isPrivate = false;
            Room.m_eCurrentLobbyMode = (eRoomQueue)Enum.Parse(typeof(eRoomQueue), UnityEngine.PlayerPrefs.GetString("currentQueue", "DEFAULT"), true);
            Room.m_szRoomCode = PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : null;
            Players.CheckForTheGameEndPre();
            if (PhotonNetwork.CurrentRoom != null)
            {
                var currentRoom = PhotonNetwork.NetworkingClient.CurrentRoom;
                isPrivate = !currentRoom.IsVisible || currentRoom.CustomProperties.ContainsKey("Description");
            }
            if (Events.OnRoomJoined != null)
            {
                RoomJoinedArgs args = new RoomJoinedArgs();
                args.isPrivate = isPrivate;
                args.roomCode = PhotonNetwork.CurrentRoom.Name;
                Events.OnRoomJoined(null, args);
            }
            Players.CheckForTheGameEndPost();
        }
    }

    [HarmonyPatch(typeof(PhotonNetwork))]
    [HarmonyPatch("Disconnect", MethodType.Normal)]
    internal class OnRoomDisconnected
    {
        private static void Prefix()
        {
            Room.m_szRoomCode = null;
            if (!PhotonNetwork.InRoom) return;

            Events.OnRoomDisconnected?.Invoke(null, null);
        }
    }

    [HarmonyPatch(typeof(PhotonHandler))]
    [HarmonyPatch("OnPlayerEnteredRoom", MethodType.Normal)]
    internal class OnPlayerConnected
    {
        private static void Postfix(Photon.Realtime.Player newPlayer)
        {
            if (!PhotonNetwork.InRoom) return;

            if (Events.OnPlayerConnected != null)
            {
                PlayerDisConnectedArgs args = new PlayerDisConnectedArgs();
                args.player = newPlayer;
                Events.OnPlayerConnected(null, args);
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
                Events.OnPlayerDisconnectedPre(null, args);
            }
        }
        private static void Postfix(Photon.Realtime.Player otherPlayer)
        {
            Players.CheckForTheGameEndPre();
            if (Events.OnPlayerDisconnectedPost != null)
            {
                PlayerDisConnectedArgs args = new PlayerDisConnectedArgs();
                args.player = otherPlayer;
                Events.OnPlayerDisconnectedPost(null, args);
            }
            Players.CheckForTheGameEndPost();
        }
    }

    [HarmonyPatch(typeof(GorillaNetworkJoinTrigger))]
    [HarmonyPatch("OnBoxTriggered", MethodType.Normal)]
    internal class OnJoinTriggerTriggered
    {
        private static void Postfix(GorillaNetworkJoinTrigger __instance)
        {
            if (__instance == GorillaComputer.instance.forestMapTrigger) { Room.m_eTriggeredMap = eJoinedMap.Forest; return; }
            if (__instance == GorillaComputer.instance.caveMapTrigger) { Room.m_eTriggeredMap = eJoinedMap.Cave; return; }
            if (__instance == GorillaComputer.instance.canyonMapTrigger) Room.m_eTriggeredMap = eJoinedMap.Canyon;
        }
    }
}