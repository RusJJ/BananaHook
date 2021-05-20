using ExitGames.Client.Photon;
using Photon.Pun;

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
        public void OnEvent(EventData photonEvent)
        {
            switch(photonEvent.Code)
            {
                case GorillaTagManager.ReportInfectionTagEvent:
                    OnPlayerTaggedByPlayerHook.OnEvent(photonEvent);
                    break;
            }
        }
    }
}
