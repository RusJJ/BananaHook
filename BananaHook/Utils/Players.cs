using System.Collections.Generic;
using Photon.Realtime;
using Photon.Pun;

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
        public static bool IsTagger(Player playerToCheck)
        {
            object nMatIndex;
            return (playerToCheck.CustomProperties.TryGetValue("matIndex", out nMatIndex) && (int)nMatIndex == 1);
        }
        public static Player GetCurrentTagger()
        {
            object obj;
            return PhotonNetwork.NetworkingClient.CurrentRoom.CustomProperties.TryGetValue("currentIt", out obj) ? (Player)obj : null;
        }
    }
}
