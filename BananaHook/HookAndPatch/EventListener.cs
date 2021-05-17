using ExitGames.Client.Photon;
using Photon.Pun;

namespace BananaHook.HookAndPatch
{
    class EventListener
    {
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
