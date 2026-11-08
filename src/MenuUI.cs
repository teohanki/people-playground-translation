using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace PPGTranslate
{
    public sealed class MenuUI
    {
        const string BoostyUrl = "https://boosty.to/teohanki/donate";
        const string LavaUrl = "https://app.lava.top/teohanki";
        const string IssuesUrl = "https://github.com/teohanki/people-playground-translation/issues";

        sealed class Clicker
        {
            readonly Action action;
            public Clicker(Action action) { this.action = action; }
            public void Invoke()
            {
                try { action(); }
                catch (Exception e) { Plugin.LogOnce(e); }
            }
        }

        const string Heart = " <color=#FF5A6E>♥</color>";
        const string LavaWorld = "Visa, Mastercard, Apple Pay, PayPal, SEPA, PIX, iDEAL, Bancontact, MB WAY, Mir, SBP";
        const string BoostyWorld = "Visa, Mastercard, Mir, SBP";
        const string LavaRu = "Visa, Mastercard, Apple Pay, PayPal, SEPA, PIX, iDEAL, Bancontact, MB WAY, МИР, СБП";
        const string BoostyRu = "Visa, Mastercard, МИР, СБП";

        static Type controllerType, buttonType, rawImageType, layoutElementType;
        static PropertyInfo wrapProp, marginProp, rawTextureProp, raycastProp, preferredWidthProp, minWidthProp, layoutPreferredWidthProp;
        static MethodInfo loadImage;
        static readonly Dictionary<string, Texture2D> flags = new Dictionary<string, Texture2D>();
        static FieldInfo mainField, pagesField;
        static PropertyInfo onClickProp;
        static bool resolved, broken;

        object controller;
        Transform menu;
        GameObject template;
        GameObject langButton, supportButton;
        bool mainMenu;
        readonly List<GameObject> listButtons = new List<GameObject>();
        readonly List<GameObject> hidden = new List<GameObject>();
        float nextCheck;

        public void Update()
        {
            if (broken) return;
            if (Time.realtimeSinceStartup < nextCheck) return;
            nextCheck = Time.realtimeSinceStartup + 0.5f;

            if (!Resolve()) return;
            object main = mainField.GetValue(null);
            if (main == null || (main as UnityEngine.Object) == null) return;
            if (ReferenceEquals(main, controller) && menu != null && langButton != null) return;

            controller = main;
            menu = null;
            langButton = null;
            supportButton = null;
            listButtons.Clear();
            hidden.Clear();
            try { Build(main); }
            catch (Exception e)
            {
                broken = true;
                Plugin.Log.LogWarning("Could not add the language button: " + e.Message);
            }
        }

        static bool Resolve()
        {
            if (resolved) return controllerType != null;
            resolved = true;
            controllerType = AccessTools.TypeByName("MenuController");
            buttonType = AccessTools.TypeByName("UnityEngine.UI.Button");
            if (controllerType == null || buttonType == null) return false;
            mainField = AccessTools.Field(controllerType, "main");
            pagesField = AccessTools.Field(controllerType, "pages");
            onClickProp = AccessTools.Property(buttonType, "onClick");
            if (mainField == null || pagesField == null || onClickProp == null)
            {
                controllerType = null;
                return false;
            }

            Type tmp = AccessTools.TypeByName("TMPro.TMP_Text");
            if (tmp != null)
            {
                wrapProp = AccessTools.Property(tmp, "enableWordWrapping");
                marginProp = AccessTools.Property(tmp, "margin");
                preferredWidthProp = AccessTools.Property(tmp, "preferredWidth");
            }
            layoutElementType = AccessTools.TypeByName("UnityEngine.UI.LayoutElement");
            if (layoutElementType != null)
            {
                minWidthProp = AccessTools.Property(layoutElementType, "minWidth");
                layoutPreferredWidthProp = AccessTools.Property(layoutElementType, "preferredWidth");
            }
            rawImageType = AccessTools.TypeByName("UnityEngine.UI.RawImage");
            if (rawImageType != null)
            {
                rawTextureProp = AccessTools.Property(rawImageType, "texture");
                raycastProp = AccessTools.Property(rawImageType, "raycastTarget");
            }
            Type conv = AccessTools.TypeByName("UnityEngine.ImageConversion");
            if (conv != null) loadImage = AccessTools.Method(conv, "LoadImage", new[] { typeof(Texture2D), typeof(byte[]) });
            return true;
        }

        void Build(object main)
        {
            GameObject[] pages = pagesField.GetValue(main) as GameObject[];
            if (pages == null || pages.Length == 0 || pages[0] == null) return;
            if (pages[0].name != "Main") return;
            Transform m = pages[0].transform.Find("Menu");
            if (m == null) return;

            Transform tpl = m.Find("Settings") ?? m.Find("Options");
            if (tpl == null || tpl.GetComponent(buttonType) == null) return;

            menu = m;
            template = tpl.gameObject;
            mainMenu = m.Find("Play") != null;

            int at = SpacerIndex();
            langButton = MakeButton("PPGT language", LanguageLabel(), OpenLanguages, at);
            if (mainMenu) supportButton = MakeButton("PPGT support", Plugin.Current.Support + Heart, OpenSupport, at + 1);

            if (mainMenu && Plugin.FirstRun) OpenLanguages();
        }

        int SpacerIndex()
        {
            for (int i = 0; i < menu.childCount; i++)
                if (string.Equals(menu.GetChild(i).name, "Spacer", StringComparison.OrdinalIgnoreCase)) return i;
            return Math.Max(0, menu.childCount - 1);
        }

        string LanguageLabel()
        {
            Lang l = Plugin.Current;
            return l == Langs.English ? "language: English" : l.Language + ": " + l.Name;
        }

        GameObject MakeButton(string name, string text, Action onClick, int index)
        {
            return MakeButton(name, text, onClick, index, 1f, null);
        }

        GameObject MakeButton(string name, string text, Action onClick, int index, float heightScale, string flag)
        {
            GameObject go = (GameObject)UnityEngine.Object.Instantiate(template, menu, false);
            go.name = name;
            go.SetActive(true);

            foreach (Component c in go.GetComponents<Component>())
            {
                if (c == null) continue;
                if (c.GetType().GetField("pageName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null)
                    UnityEngine.Object.Destroy(c);
            }

            RectTransform rt = go.transform as RectTransform;
            if (rt != null && heightScale != 1f)
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, rt.sizeDelta.y * heightScale);

            Component button = go.GetComponent(buttonType);
            object ev = Activator.CreateInstance(onClickProp.PropertyType);
            onClickProp.SetValue(button, ev, null);
            MethodInfo add = ev.GetType().GetMethod("AddListener");
            Type actionType = add.GetParameters()[0].ParameterType;
            Delegate d = Delegate.CreateDelegate(actionType, new Clicker(onClick), "Invoke");
            add.Invoke(ev, new object[] { d });

            Component label = TextOf(go);
            Plugin.SetOwnText(label, text);
            if (label != null && wrapProp != null) wrapProp.SetValue(label, false, null);
            if (flag != null) AddFlag(go, label, flag);
            FitWidth(go, label);
            go.transform.SetSiblingIndex(Math.Min(index, menu.childCount - 1));
            return go;
        }

        void FitWidth(GameObject go, Component label)
        {
            try
            {
                RectTransform rt = go.transform as RectTransform;
                if (rt == null || label == null || preferredWidthProp == null) return;
                float need = (float)preferredWidthProp.GetValue(label, null);
                if (marginProp != null)
                {
                    Vector4 m = (Vector4)marginProp.GetValue(label, null);
                    need += m.x + m.z;
                }
                need += 16f;
                RectTransform lr = label.transform as RectTransform;
                float pad = lr != null && lr != rt ? Mathf.Max(0f, rt.rect.width - lr.rect.width) : 0f;
                float total = need + pad;
                if (total > rt.rect.width)
                {
                    rt.sizeDelta = new Vector2(rt.sizeDelta.x + total - rt.rect.width, rt.sizeDelta.y);
                    if (layoutElementType != null && minWidthProp != null)
                    {
                        Component le = go.GetComponent(layoutElementType) ?? go.AddComponent(layoutElementType);
                        minWidthProp.SetValue(le, total, null);
                        if (layoutPreferredWidthProp != null) layoutPreferredWidthProp.SetValue(le, total, null);
                    }
                }
                if (lr != null && lr != rt && lr.rect.width < need)
                    lr.offsetMax = new Vector2(lr.offsetMax.x + need - lr.rect.width, lr.offsetMax.y);
            }
            catch (Exception e) { Plugin.LogOnce(e); }
        }

        void AddFlag(GameObject go, Component label, string code)
        {
            try
            {
                Texture2D tex = Flag(code);
                RectTransform parent = go.transform as RectTransform;
                if (tex == null || parent == null || rawImageType == null || label == null || marginProp == null) return;

                float h = Mathf.Max(12f, parent.rect.height * 0.55f);
                float w = h * 4f / 3f;
                GameObject img = new GameObject("PPGT flag", typeof(RectTransform));
                RectTransform rt = (RectTransform)img.transform;
                rt.SetParent(parent, false);
                rt.anchorMin = new Vector2(0f, 0.5f);
                rt.anchorMax = new Vector2(0f, 0.5f);
                rt.pivot = new Vector2(0f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
                rt.sizeDelta = new Vector2(w, h);
                Component raw = img.AddComponent(rawImageType);
                rawTextureProp.SetValue(raw, tex, null);
                if (raycastProp != null) raycastProp.SetValue(raw, false, null);

                Vector4 m = (Vector4)marginProp.GetValue(label, null);
                m.x += w + h * 0.5f;
                marginProp.SetValue(label, m, null);
            }
            catch (Exception e) { Plugin.LogOnce(e); }
        }

        static Texture2D Flag(string code)
        {
            Texture2D tex;
            if (flags.TryGetValue(code, out tex)) return tex;
            flags[code] = null;
            if (loadImage == null || Plugin.Dir == null) return null;
            string path = Path.Combine(Path.Combine(Plugin.Dir, "flags"), code + ".png");
            if (!File.Exists(path)) return null;
            tex = new Texture2D(2, 2);
            if (!(bool)loadImage.Invoke(null, new object[] { tex, File.ReadAllBytes(path) })) return null;
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.hideFlags = HideFlags.DontUnloadUnusedAsset;
            flags[code] = tex;
            return tex;
        }

        static Component TextOf(GameObject go)
        {
            Type t = Plugin.TmpUgui;
            if (t == null) return null;
            Component c = go.GetComponent(t);
            return c ?? go.GetComponentInChildren(t);
        }

        void OpenLanguages()
        {
            OpenList();
            int at = 0;
            foreach (Lang l in Langs.All)
            {
                Lang chosen = l;
                string label = l == Plugin.Current ? "<u>" + l.Name + "</u>" : l.Name;
                listButtons.Add(MakeButton("PPGT lang " + l.Code, label, delegate { Pick(chosen); }, at++, 1.3f, l.Code));
            }
            listButtons.Add(MakeButton("PPGT back", Plugin.Current.Back, CloseList, at, 1.3f, null));
        }

        void OpenSupport()
        {
            OpenList();
            Lang l = Plugin.Current;
            bool ru = l.Code == "ru";
            int at = 0;
            listButtons.Add(MakeText("PPGT pitch", l.Pitch + Heart, at++, 90, 1.05f));
            listButtons.Add(MakeSpacer(0.6f, at++));
            Action boosty = delegate { Application.OpenURL(BoostyUrl); };
            Action lava = delegate { Application.OpenURL(LavaUrl); };
            listButtons.Add(MakeButton("PPGT lava", Link("lava.top") + Small(ru ? LavaRu : LavaWorld), lava, at++, 1.3f, null));
            listButtons.Add(MakeButton("PPGT boosty", Link("Boosty") + Small(ru ? BoostyRu : BoostyWorld), boosty, at++, 1.3f, null));
            listButtons.Add(MakeButton("PPGT issues", "<size=85%>" + Link(l.Issues), delegate { Application.OpenURL(IssuesUrl); }, at++, 1.3f, null));
            listButtons.Add(MakeText("PPGT opens", "<i>" + l.Opens, at++, 75, 0.85f));
            listButtons.Add(MakeSpacer(0.6f, at++));
            listButtons.Add(MakeButton("PPGT back", l.Back, CloseList, at, 1.3f, null));
        }

        GameObject MakeText(string name, string text, int index, int size, float lineHeight)
        {
            int lines = 1;
            foreach (char ch in text) if (ch == '\n') lines++;
            GameObject go = MakeButton(name, "<size=" + size + "%><alpha=#DD>" + text, delegate { }, index, lineHeight * lines, null);
            Component button = go.GetComponent(buttonType);
            if (button != null) UnityEngine.Object.Destroy(button);
            return go;
        }

        GameObject MakeSpacer(float scale, int index)
        {
            GameObject go = new GameObject("PPGT spacer", typeof(RectTransform));
            RectTransform rt = (RectTransform)go.transform;
            rt.SetParent(menu, false);
            RectTransform tpl = template.transform as RectTransform;
            float h = tpl != null ? tpl.rect.height : 20f;
            rt.sizeDelta = new Vector2(0f, h * scale);
            go.transform.SetSiblingIndex(Math.Min(index, menu.childCount - 1));
            return go;
        }

        static string Link(string name)
        {
            return "<u>" + name + "</u> <color=#FF5A6E>→</color> ";
        }

        static string Small(string s)
        {
            return "<size=70%><alpha=#AA>(" + s + ")";
        }

        void OpenList()
        {
            CloseList();
            for (int i = 0; i < menu.childCount; i++)
            {
                GameObject child = menu.GetChild(i).gameObject;
                if (!child.activeSelf) continue;
                child.SetActive(false);
                hidden.Add(child);
            }
        }

        void CloseList()
        {
            foreach (GameObject b in listButtons)
                if (b != null) UnityEngine.Object.Destroy(b);
            listButtons.Clear();
            foreach (GameObject h in hidden)
                if (h != null) h.SetActive(true);
            hidden.Clear();
        }

        void Pick(Lang l)
        {
            CloseList();
            Plugin.ChangeLanguage(l);
            if (langButton != null) Plugin.SetOwnText(TextOf(langButton), LanguageLabel());
            if (supportButton != null) Plugin.SetOwnText(TextOf(supportButton), Plugin.Current.Support + Heart);
        }
    }
}
