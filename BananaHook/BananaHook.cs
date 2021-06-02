using BepInEx;

namespace BananaHook
{
    /* That's me! */
    [BepInPlugin("net.rusjj.gtlib.bananahook", "BananaHook Lib", "1.0.3")]

    public class BananaHook : BaseUnityPlugin
    {
        private static BananaHook m_hInstance;
        internal static bool m_bUseSoundAsRoundEnd = true;
        internal static void Log(string msg) => m_hInstance.Logger.LogMessage(msg);

        void Awake()
        {
            m_hInstance = this;
            Patch.Apply();
            new HookAndPatch.EventListener();
        }
    }
}