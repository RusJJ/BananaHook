using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;

namespace BananaHook.HookAndPatch
{
    class EventListener
    {
        public const byte OnGorillaHandStepEvent = 226;
        public const byte UnknownEvent1 = 200;
        public const byte UnknownEvent2 = 201;
        public const byte UnknownEvent3 = 202; // Room players update?
        public const byte UnknownEvent4 = 206;
        public EventListener()
        {
            PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
        }
        public void OnEvent(EventData photonEvent) // Hunt doesnt send any events
        {
            try
            {
                switch (photonEvent.Code)
                {
                    case GorillaTagManager.ReportTagEvent:
                    case GorillaTagManager.ReportInfectionTagEvent:
                        object[] tagObj = (object[])photonEvent.Parameters.TryGetObject(245);
                        string taggerUserId = (string)tagObj[0], victimUserId = (string)tagObj[1];
                        Player tagger = null, victim = null;
                        foreach (var p in PhotonNetwork.PlayerList)
                        {
                            if (p.UserId == taggerUserId) tagger = p;
                            if (p.UserId == victimUserId) victim = p;
                        }
                        OnPlayerTaggedByPlayerHook.OnEvent(tagger, victim);
                        break;
                }
            }
            catch (Exception e) { BananaHook.Log("OnEvent Exception: " + e.Message + "\n" + e.StackTrace); }
        }
    }
}
