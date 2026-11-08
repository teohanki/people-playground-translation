using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PPGTranslate
{
    public static class Fonts
    {
        static readonly Dictionary<string, string> Files = new Dictionary<string, string>
        {
            { "ja", "NotoSansJP-PPG.ttf" },
            { "ko", "NotoSansKR-PPG.ttf" },
            { "zh", "NotoSansSC-PPG.ttf" },
        };

        static readonly Dictionary<string, object> assets = new Dictionary<string, object>();
        static readonly List<object> added = new List<object>();
        static object latin;
        static bool latinTried;
        static MethodInfo create;
        static Type assetType;
        static object renderMode;
        static object populationMode;
        static string fontDir;
        static bool broken;

        public static void Init(string pluginDir)
        {
            fontDir = Path.Combine(pluginDir, "fonts");
            try
            {
                assetType = AccessTools.TypeByName("TMPro.TMP_FontAsset");
                if (assetType == null) { broken = true; return; }
                foreach (MethodInfo m in assetType.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (m.Name != "CreateFontAsset") continue;
                    ParameterInfo[] p = m.GetParameters();
                    if (p.Length == 8 && p[0].ParameterType == typeof(Font))
                    {
                        create = m;
                        renderMode = Enum.Parse(p[3].ParameterType, "SDFAA");
                        populationMode = Enum.Parse(p[6].ParameterType, "Dynamic");
                    }
                }
                if (create == null) broken = true;
            }
            catch (Exception e)
            {
                broken = true;
                Plugin.Log.LogWarning("Fonts unavailable: " + e.Message);
            }
        }

        public static void Apply(Lang lang)
        {
            if (broken) return;
            try
            {
                IList list = GlobalFallbacks();
                if (list == null) return;
                foreach (object a in added) list.Remove(a);
                added.Clear();

                List<object> order = new List<object>();
                object l = Latin();
                if (l != null) order.Add(l);
                if (lang.Font != null)
                {
                    object own = Get(lang.Font);
                    if (own != null) order.Add(own);
                }
                foreach (Lang other in Langs.All)
                {
                    if (other.Font == null || other.Font == lang.Font) continue;
                    object a = Get(other.Font);
                    if (a != null && !order.Contains(a)) order.Add(a);
                }

                for (int i = 0; i < order.Count; i++)
                {
                    list.Insert(i, order[i]);
                    added.Add(order[i]);
                }
            }
            catch (Exception e)
            {
                broken = true;
                Plugin.Log.LogWarning("Could not set fallback fonts: " + e.Message);
            }
        }

        public static void Preload(Lang lang)
        {
            if (broken) return;
            Latin();
            if (lang.Font != null) Get(lang.Font);
        }

        static IList GlobalFallbacks()
        {
            Type settings = AccessTools.TypeByName("TMPro.TMP_Settings");
            if (settings == null) return null;
            PropertyInfo p = AccessTools.Property(settings, "fallbackFontAssets");
            return p == null ? null : p.GetValue(null, null) as IList;
        }

        static object Latin()
        {
            if (latinTried) return latin;
            latinTried = true;
            string path = Path.Combine(Application.streamingAssetsPath, "arial.ttf");
            if (!File.Exists(path)) path = Path.Combine(fontDir, "NotoSans-PPG.ttf");
            latin = Load(path, 64, 6);
            return latin;
        }

        static object Get(string key)
        {
            object a;
            if (assets.TryGetValue(key, out a)) return a;
            string file;
            a = Files.TryGetValue(key, out file) ? Load(Path.Combine(fontDir, file), 56, 6) : null;
            assets[key] = a;
            return a;
        }

        static object Load(string path, int size, int padding)
        {
            if (!File.Exists(path))
            {
                Plugin.Log.LogWarning("Font not found: " + path);
                return null;
            }
            try
            {
                ConstructorInfo ctor = typeof(Font).GetConstructor(new[] { typeof(string) });
                Font font = (Font)ctor.Invoke(new object[] { path });
                font.name = Path.GetFileNameWithoutExtension(path);
                object asset = create.Invoke(null, new object[] { font, size, padding, renderMode, 2048, 2048, populationMode, true });
                if (asset == null)
                {
                    Plugin.Log.LogWarning("Font failed to load: " + path);
                    return null;
                }
                UnityEngine.Object o = asset as UnityEngine.Object;
                if (o != null)
                {
                    o.name = font.name + " SDF";
                    o.hideFlags = HideFlags.DontUnloadUnusedAsset;
                }
                font.hideFlags = HideFlags.DontUnloadUnusedAsset;
                Plugin.Log.LogInfo("Font: " + Path.GetFileName(path));
                return asset;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Font " + path + " failed to load: " + e.Message);
                return null;
            }
        }
    }
}
