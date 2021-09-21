using Photon.Pun;
using System;
using System.Threading;

namespace BananaHook.Utils
{
    public enum eRoomQueue : byte
    {
        Custom = 0,
        Default = 1,
        Casual = 2,
        Competitive = 3,
    };
    public enum eJoinedMap : byte
    {
        Unknown = 0,
        GorillaShop = 1,
        Forest = 2,
        Cave = 3,
        Canyon = 4,
    };
    public class Room
    {
        public static eRoomQueue m_eCurrentLobbyMode { get; internal set; } = 0;
        public static eJoinedMap m_eTriggeredMap { get; internal set; } = eJoinedMap.Unknown;
        public static string m_szRoomCode { get; internal set; } = null; // Just a room code
        public static bool m_bIsGameEnded { get; internal set; } = false; // True - the game is in process of ending
        public static bool m_bIsTagging { get; internal set; } = true; // True - Rock-Monk, False - Lava predators!

        public static bool IsInRoom() => PhotonNetwork.InRoom;
        public static string GetRoomCode() => PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : null;
        public static int GetMaxPlayers() => PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.MaxPlayers : 1;
        public static int GetPlayers() => PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 1;
        public static bool IsTagging()
        {
            if (!PhotonNetwork.InRoom) return false;
            object obj;
            return (PhotonNetwork.NetworkingClient.CurrentRoom.CustomProperties.TryGetValue("isCurrentlyTag", out obj) && (bool)obj);
        }

        private static bool m_bThreadStarted = false;
        internal static Thread m_hCheckerThread = null;
        internal static int m_nTagged = 0, m_nTotal = 0;
        /* Should work. Be aware of glitch "the game is ended but 1 guy is untagged" */
        internal static void CheckForTheGameEndPre()
        {
            if (m_bThreadStarted || m_bIsTagging) return;
            m_nTagged = Players.CountInfectedPlayers();
            if (m_nTagged < 4) return;
            m_nTotal = Players.CountValidPlayers();
            if (m_nTotal > 0 && m_nTagged == m_nTotal)
            {
                m_bIsGameEnded = true;
                try
                { 
                    Events.OnRoundEndPre?.Invoke(null, null);
                }
                catch (Exception e) { BananaHook.Log("OnRoundEndPre Exception: " + e.StackTrace); }
            }
        }
        internal static void CheckForTheGameEndPost()
        {
            if (BananaHook.m_bUseSoundAsRoundStart || m_bIsTagging) return;
            if (!m_bThreadStarted && m_bIsGameEnded && m_nTotal > 0 && m_nTagged == m_nTotal)
            {
                m_bThreadStarted = true;
                try
                { 
                    Events.OnRoundEndPost?.Invoke(null, null);
                }
                catch (Exception e) { BananaHook.Log("OnRoundEndPost Exception: " + e.StackTrace); }
                if (Events.OnRoundStart != null)
                {
                    // Sadly i need it currently.
                    // Because the MasterClient is not sending any RPC for this.
                    m_hCheckerThread = new Thread(Thread_CheckForGameToStart);
                    m_hCheckerThread.Start();
                }
            }
        }
        internal static void Thread_CheckForGameToStart()
        {
            // Enough for 3000 * 100 = 5 min (i hope its enough because i know about the rare bug)
            // Pretty sure its not enough.
            int nAntiInfinity = 3000;
            while (--nAntiInfinity > 0 && Players.CountInfectedPlayers() != 1) Thread.Sleep(100);
            m_bThreadStarted = false;
            m_bIsGameEnded = false;
            OnRoundStartArgs args = new OnRoundStartArgs();
            args.player = Players.GetFirstGuyInfected();
            try
            { 
                Events.OnRoundStart(null, args);
            }
            catch (Exception e) { BananaHook.Log("OnRoundStart Exception: " + e.StackTrace); }
        }
    }
}