using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

[assembly: AssemblyTitle("PPG Translate")]
[assembly: AssemblyDescription("People Playground in 11 languages")]
[assembly: AssemblyCompany("Teo Hanki")]
[assembly: AssemblyProduct("PPG Translate")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Teo Hanki")]
[assembly: AssemblyVersion("2.3.0.0")]
[assembly: AssemblyFileVersion("2.3.0.0")]

namespace PPGTranslate
{
    [BepInPlugin(Guid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "ppgrussian";
        public const string PluginName = "PPG Translate";
        public const string PluginVersion = "2.3.0";

        internal static ManualLogSource Log;
        internal static bool FirstRun;
        internal static Lang Current = Langs.English;

        static Translator tr;
        static int generation;
        static bool active;
        static bool translateWorld;
        static bool dumpEnabled;
        static bool busy;
        static int errors;
        static string dir;
        internal static string Dir { get { return dir; } }
        static ConfigEntry<string> cLang;

        static Type tmpText, tmpUgui, tmpInput, uiText, uiInput;
        static PropertyInfo tmpTextProp, uiTextProp, tmpInputTc, uiInputTc;
        static PropertyInfo autoSizeProp, sizeProp, sizeMinProp, sizeMaxProp, wrapProp, overflowProp;
        static readonly List<Type> userTextOwners = new List<Type>();

        static readonly Dictionary<int, bool> skip = new Dictionary<int, bool>();
        static readonly HashSet<int> own = new HashSet<int>();
        static readonly HashSet<int> fitted = new HashSet<int>();
        static readonly Dictionary<int, Entry> entries = new Dictionary<int, Entry>();

        sealed class Entry
        {
            public string Orig;
            public string Shown;
            public int Gen;
        }

        float sweepInterval;
        float nextSweep, nextFlush;
        bool fontsReady;
        MenuUI menu;

        void Awake()
        {
            Log = Logger;
            try
            {
                bool upgrade = IsUpgradeFromRussian();
                ConfigEntry<bool> cEnabled = Config.Bind("General", "Enabled", true, "Turn the translation on or off");
                cLang = Config.Bind("General", "Language", "",
                    "Language code: en, ru, es, pt-BR, de, ja, fr, pl, ko, tr, zh-Hans. Empty means pick on first launch");
                ConfigEntry<bool> cWorld = Config.Bind("General", "TranslateWorldText", true,
                    "Translate labels on items and maps. Text typed by the player is never translated");
                ConfigEntry<float> cSweep = Config.Bind("General", "SweepIntervalSeconds", 2f,
                    "How often to scan the screen for missed text, in seconds. 0 turns it off");
                ConfigEntry<bool> cDump = Config.Bind("Debug", "DumpUntranslated", true,
                    "Write untranslated strings to untranslated_<language>.txt");

                if (upgrade)
                {
                    cLang.Value = "ru";
                    cWorld.Value = true;
                    Log.LogInfo("Settings from PPG Russian found, keeping Russian");
                }

                active = cEnabled.Value;
                translateWorld = cWorld.Value;
                dumpEnabled = cDump.Value;
                sweepInterval = Mathf.Max(0f, cSweep.Value);
                if (!active)
                {
                    Log.LogInfo("Translation is disabled in the config");
                    return;
                }

                dir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
                if (string.IsNullOrEmpty(dir)) dir = Path.Combine(Paths.PluginPath, "PPG_Translate");

                LogGameVersion();
                ResolveTypes();
                Fonts.Init(dir);

                Lang chosen = Langs.Find(cLang.Value);
                if (chosen == null)
                {
                    FirstRun = true;
                    chosen = Langs.FromSystem(Application.systemLanguage.ToString());
                }
                LoadLanguage(chosen);
                ApplyPatches();
                menu = new MenuUI();
            }
            catch (Exception e)
            {
                active = false;
                Log.LogError("Translation failed to start, the game stays in English: " + e);
            }
        }

        bool IsUpgradeFromRussian()
        {
            try
            {
                string path = Config.ConfigFilePath;
                if (!File.Exists(path)) return false;
                string text = File.ReadAllText(path);
                return text.IndexOf("TranslateWorldText", StringComparison.Ordinal) >= 0
                    && text.IndexOf("Language =", StringComparison.Ordinal) < 0;
            }
            catch { return false; }
        }

        internal static void ChangeLanguage(Lang l)
        {
            if (l == null) return;
            FirstRun = false;
            try
            {
                cLang.Value = l.Code;
                if (l == Current && tr != null) return;
                Flush();
                LoadLanguage(l);
                Fonts.Apply(l);
                RetranslateAll();
            }
            catch (Exception e) { LogOnce(e); }
        }

        static void LoadLanguage(Lang l)
        {
            Current = l;
            generation++;
            skip.Clear();
            if (l == Langs.English)
            {
                tr = null;
                Log.LogInfo("Language: English");
                return;
            }

            Translator t = new Translator();
            t.DumpEnabled = dumpEnabled;
            t.DumpPath = Path.Combine(dir, "untranslated_" + l.Code + ".txt");
            string root = Path.Combine(dir, "translations");
            LoadFolder(t, Path.Combine(root, "_all"));
            LoadFolder(t, Path.Combine(root, l.Code));
            t.LoadDumpHistory();
            tr = t;
            Log.LogInfo("Language: " + l.Name + " (" + l.Code + "), entries: " + t.Count + ", templates: " + t.TemplateCount);
        }

        static void LoadFolder(Translator t, string folder)
        {
            if (!Directory.Exists(folder))
            {
                Log.LogWarning("Missing folder: " + folder);
                return;
            }
            string[] files = Directory.GetFiles(folder, "*.txt");
            Array.Sort(files, StringComparer.OrdinalIgnoreCase);
            foreach (string f in files)
            {
                try { t.LoadFile(f); }
                catch (Exception e) { Log.LogWarning("Could not read " + f + ": " + e.Message); }
            }
        }

        static string Tr(string s)
        {
            return tr == null ? s : tr.Translate(s);
        }

        static void LogGameVersion()
        {
            try
            {
                string v = Application.version;
                if (string.IsNullOrEmpty(v)) return;
                Log.LogInfo("Game version: " + v);
                string[] p = v.Split('.');
                int a, b, c = 0;
                if (p.Length >= 2 && int.TryParse(p[0], out a) && int.TryParse(DigitsOnly(p[1]), out b))
                {
                    if (p.Length >= 3) int.TryParse(DigitsOnly(p[2]), out c);
                    if (a == 1 && (b < 24 || (b == 24 && c < 5)))
                        Log.LogWarning("The game is older than 1.24.5, its fonts have no Cyrillic");
                }
            }
            catch (Exception e) { Log.LogWarning("Could not read the game version: " + e.Message); }
        }

        static string DigitsOnly(string s)
        {
            int n = 0;
            while (n < s.Length && char.IsDigit(s[n])) n++;
            return s.Substring(0, n);
        }

        static void ResolveTypes()
        {
            tmpText = AccessTools.TypeByName("TMPro.TMP_Text");
            tmpUgui = AccessTools.TypeByName("TMPro.TextMeshProUGUI");
            tmpInput = AccessTools.TypeByName("TMPro.TMP_InputField");
            uiText = AccessTools.TypeByName("UnityEngine.UI.Text");
            uiInput = AccessTools.TypeByName("UnityEngine.UI.InputField");

            if (tmpText != null)
            {
                tmpTextProp = AccessTools.Property(tmpText, "text");
                autoSizeProp = AccessTools.Property(tmpText, "enableAutoSizing");
                sizeProp = AccessTools.Property(tmpText, "fontSize");
                sizeMinProp = AccessTools.Property(tmpText, "fontSizeMin");
                sizeMaxProp = AccessTools.Property(tmpText, "fontSizeMax");
                wrapProp = AccessTools.Property(tmpText, "enableWordWrapping");
                overflowProp = AccessTools.Property(tmpText, "overflowMode");
            }
            if (uiText != null) uiTextProp = AccessTools.Property(uiText, "text");
            if (tmpInput != null) tmpInputTc = AccessTools.Property(tmpInput, "textComponent");
            if (uiInput != null) uiInputTc = AccessTools.Property(uiInput, "textComponent");

            foreach (string n in new[] { "DisplayBehaviour", "TransmitterBehaviour" })
            {
                Type t = AccessTools.TypeByName(n);
                if (t != null) userTextOwners.Add(t);
            }

            Log.LogInfo("TextMeshPro: " + (tmpText != null) + ", Unity UI: " + (uiText != null));
        }

        static void ApplyPatches()
        {
            Harmony h = new Harmony(Guid);
            HarmonyMethod pre = new HarmonyMethod(typeof(Plugin), "SetTextPrefix");
            HarmonyMethod post = new HarmonyMethod(typeof(Plugin), "OnEnablePostfix");

            if (tmpTextProp != null) TryPatch(h, tmpTextProp.GetSetMethod(true), pre, null, "TMP_Text.text");
            if (tmpText != null)
            {
                foreach (MethodInfo m in tmpText.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (m.Name != "SetText") continue;
                    ParameterInfo[] p = m.GetParameters();
                    if (p.Length >= 1 && p[0].ParameterType == typeof(string) && (p.Length == 1 || p[1].ParameterType == typeof(bool)))
                        TryPatch(h, m, pre, null, "TMP_Text.SetText(" + p.Length + ")");
                }
            }
            if (uiTextProp != null) TryPatch(h, uiTextProp.GetSetMethod(true), pre, null, "UI.Text.text");

            Type world = translateWorld ? AccessTools.TypeByName("TMPro.TextMeshPro") : null;
            foreach (Type t in new[] { tmpUgui, world, uiText })
            {
                if (t == null) continue;
                MethodInfo m = AccessTools.DeclaredMethod(t, "OnEnable");
                if (m != null) TryPatch(h, m, null, post, t.Name + ".OnEnable");
            }
        }

        static void TryPatch(Harmony h, MethodBase m, HarmonyMethod pre, HarmonyMethod post, string name)
        {
            if (m == null) return;
            try
            {
                h.Patch(m, pre, post);
                Log.LogInfo("Hook: " + name);
            }
            catch (Exception e) { Log.LogWarning("Hook " + name + " failed: " + e.Message); }
        }

        static void SetTextPrefix(object __instance, ref string __0)
        {
            if (!active || busy || __0 == null) return;
            try
            {
                Component c = __instance as Component;
                if (c == null || IsSkipped(c)) return;
                int id = c.GetInstanceID();
                string res = Tr(__0);
                if (res == __0)
                {
                    entries.Remove(id);
                    return;
                }
                Remember(id, __0, res);
                string before = __0;
                __0 = res;
                FitWorld(c, before, res);
            }
            catch (Exception e) { LogOnce(e); }
        }

        static void OnEnablePostfix(object __instance)
        {
            if (!active || busy) return;
            try { TranslateComponent(__instance as Component); }
            catch (Exception e) { LogOnce(e); }
        }

        static void Remember(int id, string orig, string shown)
        {
            if (entries.Count > 60000) entries.Clear();
            Entry e;
            if (!entries.TryGetValue(id, out e))
            {
                e = new Entry();
                entries[id] = e;
            }
            e.Orig = orig;
            e.Shown = shown;
            e.Gen = generation;
        }

        static PropertyInfo TextProp(Component c)
        {
            if (tmpText != null && tmpText.IsInstanceOfType(c)) return tmpTextProp;
            if (uiText != null && uiText.IsInstanceOfType(c)) return uiTextProp;
            return null;
        }

        static void TranslateComponent(Component c)
        {
            if (c == null || IsSkipped(c)) return;
            PropertyInfo p = TextProp(c);
            if (p == null) return;

            string cur = p.GetValue(c, null) as string;
            if (string.IsNullOrEmpty(cur)) return;

            int id = c.GetInstanceID();
            Entry e;
            string orig = cur;
            if (entries.TryGetValue(id, out e) && e.Shown == cur)
            {
                if (e.Gen == generation) return;
                orig = e.Orig;
            }

            string t = Tr(orig);
            if (t == orig) entries.Remove(id);
            else Remember(id, orig, t);
            if (t == cur) return;

            busy = true;
            try { p.SetValue(c, t, null); }
            finally { busy = false; }
            FitWorld(c, orig, t);
        }

        static void FitWorld(Component c, string orig, string shown)
        {
            if (autoSizeProp == null || tmpUgui == null || tmpUgui.IsInstanceOfType(c)) return;
            if (tmpText == null || !tmpText.IsInstanceOfType(c)) return;
            if (VisualLength(shown) <= VisualLength(orig)) return;
            int id = c.GetInstanceID();
            if (fitted.Contains(id)) return;
            fitted.Add(id);
            if (fitted.Count > 20000) fitted.Clear();

            if ((bool)autoSizeProp.GetValue(c, null)) return;
            bool wrap = (bool)wrapProp.GetValue(c, null);
            int overflow = Convert.ToInt32(overflowProp.GetValue(c, null));
            if (!wrap && overflow == 0) return;

            float size = (float)sizeProp.GetValue(c, null);
            if (size <= 0f) return;
            sizeMaxProp.SetValue(c, size, null);
            sizeMinProp.SetValue(c, size * 0.55f, null);
            autoSizeProp.SetValue(c, true, null);
        }

        static int VisualLength(string s)
        {
            int n = 0;
            bool tag = false;
            foreach (char ch in s)
            {
                if (ch == '<') { tag = true; continue; }
                if (tag) { if (ch == '>') tag = false; continue; }
                n += ch >= 'ᄀ' ? 2 : 1;
            }
            return n;
        }

        static bool IsSkipped(Component c)
        {
            int id = c.GetInstanceID();
            if (own.Contains(id)) return true;
            bool r;
            if (skip.TryGetValue(id, out r)) return r;

            r = false;
            if (tmpText != null && tmpText.IsInstanceOfType(c))
            {
                bool world = tmpUgui != null && !tmpUgui.IsInstanceOfType(c);
                if (world && !translateWorld) r = true;
                else if (world && OwnedByUserText(c)) r = true;
                else r = IsInputText(c, tmpInput, tmpInputTc);
            }
            else if (uiText != null && uiText.IsInstanceOfType(c))
            {
                r = IsInputText(c, uiInput, uiInputTc);
            }

            if (skip.Count > 50000) skip.Clear();
            skip[id] = r;
            return r;
        }

        static bool OwnedByUserText(Component c)
        {
            foreach (Type t in userTextOwners)
                if (c.GetComponentInParent(t) != null) return true;
            return false;
        }

        static bool IsInputText(Component c, Type inputType, PropertyInfo tcProp)
        {
            if (inputType == null || tcProp == null) return false;
            Component input = c.GetComponentInParent(inputType);
            if (input == null) return false;
            return ReferenceEquals(tcProp.GetValue(input, null), c);
        }

        internal static void SetOwnText(Component c, string text)
        {
            if (c == null) return;
            own.Add(c.GetInstanceID());
            PropertyInfo p = TextProp(c);
            if (p == null) return;
            busy = true;
            try { p.SetValue(c, text, null); }
            finally { busy = false; }
        }

        internal static Type TmpUgui { get { return tmpUgui; } }

        static void RetranslateAll()
        {
            Sweep(tmpText);
            Sweep(uiText);
        }

        void Update()
        {
            if (!active) return;
            try
            {
                if (!fontsReady)
                {
                    fontsReady = true;
                    Fonts.Apply(Current);
                }
                menu.Update();
            }
            catch (Exception e) { LogOnce(e); }

            float now = Time.realtimeSinceStartup;
            if (sweepInterval > 0f && now >= nextSweep)
            {
                nextSweep = now + sweepInterval;
                Sweep(translateWorld ? tmpText : (tmpUgui ?? tmpText));
                Sweep(uiText);
            }
            if (now >= nextFlush)
            {
                nextFlush = now + 15f;
                Flush();
            }
        }

        static void Sweep(Type t)
        {
            if (t == null) return;
            try
            {
                UnityEngine.Object[] all = UnityEngine.Object.FindObjectsOfType(t);
                for (int i = 0; i < all.Length; i++)
                {
                    try { TranslateComponent(all[i] as Component); }
                    catch (Exception e) { LogOnce(e); }
                }
            }
            catch (Exception e) { LogOnce(e); }
        }

        static void Flush()
        {
            try { if (tr != null) tr.FlushDump(); }
            catch (Exception e) { LogOnce(e); }
        }

        void OnApplicationQuit()
        {
            Flush();
        }

        internal static void LogOnce(Exception e)
        {
            if (errors++ < 5) Log.LogWarning("Translation error, the game keeps running: " + e);
        }
    }
}
