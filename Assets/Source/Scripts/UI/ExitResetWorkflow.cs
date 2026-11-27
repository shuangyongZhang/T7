using UnityEngine;
using Cysharp.Threading.Tasks;
using HT.Framework;

namespace LB
{
    public static class ExitResetWorkflow
    {
        private static bool quitApproved;

        public static void RequestExit()
        {
            quitApproved = false;
            if (Main.m_UI.GetOpenedUI<UIExitResetPanel>() == null)
                Main.m_UI.OpenUI<UIExitResetPanel>().Forget();
        }

        public static bool OnWantsToQuit()
        {
            if (quitApproved) return true;
            RequestExit();
            return false;
        }

        public static void ConfirmExit()
        {
            quitApproved = true;
            Application.Quit();
        }
    }
}
