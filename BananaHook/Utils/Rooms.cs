using Photon.Pun;

namespace BananaHook.Utils
{
    public enum eRoomQueue
    {
        Default = 0,
        Casual = 1,
        Competitive = 2,
    };
    public enum eJoinedMap
    {
        Forest = 0,
        Cave = 1,
        Canyon = 2,
    };
    public class Room
    {
        public static eRoomQueue m_eCurrentLobbyMode { get; internal set; } = 0;
        public static eJoinedMap m_eTriggeredMap { get; internal set; } = eJoinedMap.Forest;
        public static string m_szRoomCode { get; internal set; } = null;
        public static string GetRoomCode()
        {
            return PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.Name : null;
        }
        public static int GetMaxPlayers()
        {
            return PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.MaxPlayers : 1;
        }
        public static int GetPlayers()
        {
            return PhotonNetwork.InRoom ? PhotonNetwork.CurrentRoom.PlayerCount : 1;
        }
    }
}