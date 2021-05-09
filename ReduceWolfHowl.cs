using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace ReduceWolfHowl
{
    [BepInPlugin("holgero.ReduceWolfHowl", "Reduce Wolf Howl", "0.1.0")]
    public class ReduceWolfHowl : BaseUnityPlugin
    {
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<int> reductionLevel;
        public static AudioClip silence;
        private static ReduceWolfHowl context;
        private static System.Random RandomGen = new System.Random();

        private void Awake()
        {
            context = this;
            modEnabled = Config.Bind<bool>("General", "Enabled", true, "Enable this mod");
            reductionLevel = Config.Bind<int>("General", "Reduction", 90, "Chance in percent to replace the howling with silence. Use values between 0 and 100. A value of 100 replaces all howling with silence.");
            if (!modEnabled.Value)
                return;
            CreateSilence();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }
        public void CreateSilence()
        {
            // 0.1 s worth of zeroes (800 samples at a rate of 8000 per second)
            silence = AudioClip.Create("silence", 800, 1, 8000, false);
            float[] silenceData = new float[800];
            for (int i = 0; i < silenceData.Length; i++)
            {
                silenceData[i] = 0.0f;
            }
            silence.SetData(silenceData, 0);
        }
        [HarmonyPatch(typeof(ZSFX), "Awake")]
        static class ZSFX_Awake_Patch
        {
            static void Postfix(ZSFX __instance)
            {
                string name = GetZSFXName(__instance);
                if (!name.Equals("sfx_wolf_haul"))
                {
                    return;
                }
                if (RandomGen.Next(100) < reductionLevel.Value)
                {
                    for (int i = 0; i < __instance.m_audioClips.Length; i++)
                    {
                        __instance.m_audioClips[i] = silence;
                    }
                }
            }
        }
        public static string GetZSFXName(ZSFX zsfx)
        {
            string name = zsfx.name;
            char[] anyOf = new char[]
            {
            '(',
            ' '
            };
            int num = name.IndexOfAny(anyOf);
            if (num != -1)
            {
                return name.Remove(num);
            }
            return name;
        }

    }
}
