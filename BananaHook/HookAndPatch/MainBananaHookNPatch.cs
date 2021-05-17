using HarmonyLib;
using System.Reflection;

namespace BananaHook.HookAndPatch
{
    public class BananaPatch
    {
        private static Harmony m_hMyInstance = null;
        private static string m_szInstanceId = "net.rusjj.gtlib.bananahook";
        public static bool IsPatched()
        {
            return m_hMyInstance != null;
        }

        internal static void Apply()
        {
            new EventListener();
            if (m_hMyInstance == null)
            {
                m_hMyInstance = new Harmony(m_szInstanceId);
                m_hMyInstance.PatchAll(Assembly.GetExecutingAssembly());
            }
        }

        internal static void Remove()
        {
            if (m_hMyInstance != null)
            {
                m_hMyInstance.UnpatchAll(m_szInstanceId);
            }
        }
    }
}
