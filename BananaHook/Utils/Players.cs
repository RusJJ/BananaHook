using System.Collections.Generic;
using Photon.Realtime;
using Photon.Pun;
using System.Threading;

namespace BananaHook.Utils
{
    public class Players
    {
        /* Valid players - completed a tutorial */
        public static int CountValidPlayers()
        {
            int nCount = 0;
            object bDidTutorial;
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if(p.CustomProperties.TryGetValue("didTutorial", out bDidTutorial) && (bool)bDidTutorial) ++nCount;
            }
            return nCount;
        }
        // Reversed GorillaTagManager::CurrentInfectionPlayers() (before i downloaded a decompiler, lmao)
        // P.S. Returning a List instead of Array[]. We dont really need a dumb copying.
        public static List<Player> CurrentValidPlayers()
        {
            var pArray = new List<Player>();
            object bDidTutorial;
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if(p.CustomProperties.TryGetValue("didTutorial", out bDidTutorial) && (bool)bDidTutorial) pArray.Add(p);
            }
            return pArray;
        }
        public static int CountInfectedPlayers()
        {
            int nCount = 0;
            object bInfected;
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if (p.CustomProperties.TryGetValue("isInfected", out bInfected) && (bool)bInfected) ++nCount;
            }
            return nCount;
        }
        public static List<Player> CurrentInfectedPlayers()
        {
            var pArray = new List<Player>();
            object bInfected;
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if(p.CustomProperties.TryGetValue("isInfected", out bInfected) && (bool)bInfected) pArray.Add(p);
            }
            return pArray;
        }
        /* Just a first guy in a loop that is infected */
        public static Player GetFirstGuyInfected()
        {
            object bInfected;
            foreach (var p in PhotonNetwork.PlayerList)
            {
                if (p.CustomProperties.TryGetValue("isInfected", out bInfected) && (bool)bInfected) return p;
            }
            return null;
        }
        public static bool IsInfected(Player playerToCheck)
        {
            object bInfected;
            return (playerToCheck.CustomProperties.TryGetValue("isInfected", out bInfected) && (bool)bInfected);
        }

        /* It checks for infected players, so it's located in Players... */

        private static Thread m_hCheckerThread = null;
        private static bool m_bThreadStarted = false;
        internal static int m_nTagged = 0, m_nTotal = 0;
        internal static void CheckForTheGameEndPre()
        {
            if (Room.m_bIsGameEnded) return;
            m_nTagged = CountInfectedPlayers();
            m_nTotal = CountValidPlayers();
            if (m_nTotal > 0 && m_nTagged == m_nTotal)
            {
                Room.m_bIsGameEnded = true;
                Events.OnRoundEndPost?.Invoke(null, null);
            }
        }
        internal static void CheckForTheGameEndPost()
        {
            if (!m_bThreadStarted && Room.m_bIsGameEnded && m_nTotal > 0 && m_nTagged == m_nTotal)
            {
                m_bThreadStarted = true;
                Events.OnRoundEndPost?.Invoke(null, null);
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
            // Enough for 1200 * 100 = 2 min (i hope its enough because i know about the rare bug)
            int nAntiInfinity = 1200;
            while (--nAntiInfinity > 0 && CountInfectedPlayers() != 1) Thread.Sleep(100);
            m_bThreadStarted = false;
            Room.m_bIsGameEnded = false;
            OnRoundStartArgs args = new OnRoundStartArgs();
            args.player = GetFirstGuyInfected();
            Events.OnRoundStart(null, args);
        }
    }
}
