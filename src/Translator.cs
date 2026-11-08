using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace PPGTranslate
{
    public sealed class Translator
    {
        const int MaxTextLength = 4000;
        const int MaxCaseInsensitiveLen = 80;
        const int CacheLimit = 20000;
        const int DumpLimit = 3000;
        const string NumRx = "(-?[0-9]+(?:[.,][0-9]+)*)";

        static readonly Regex TagRx = new Regex("<[^<>]*>");
        static readonly Regex DigitsRx = new Regex("[0-9]+");

        readonly Dictionary<string, string> exact = new Dictionary<string, string>(StringComparer.Ordinal);
        readonly Dictionary<string, string> lower = new Dictionary<string, string>(StringComparer.Ordinal);
        readonly Dictionary<string, string> plainKeys = new Dictionary<string, string>(StringComparer.Ordinal);
        readonly HashSet<string> outputs = new HashSet<string>(StringComparer.Ordinal);
        readonly List<Template> templates = new List<Template>();
        readonly Dictionary<string, int> templateIndex = new Dictionary<string, int>(StringComparer.Ordinal);
        readonly Dictionary<string, string> cache = new Dictionary<string, string>(StringComparer.Ordinal);

        readonly HashSet<string> missed = new HashSet<string>(StringComparer.Ordinal);
        readonly List<string> pending = new List<string>();

        public bool DumpEnabled;
        public string DumpPath;
        public int Count { get { return exact.Count + templates.Count; } }
        public int TemplateCount { get { return templates.Count; } }

        sealed class Template
        {
            public Regex Rx;
            public string Value;
            public string[] Names;
            public string Hint;
        }

        public int LoadFile(string path)
        {
            int n = 0;
            foreach (string raw in File.ReadAllLines(path, Encoding.UTF8))
            {
                string line = raw.TrimEnd('\r');
                if (line.Length == 0 || line[0] == '#') continue;
                int tab = line.IndexOf('\t');
                if (tab <= 0) continue;
                string key = Unescape(line.Substring(0, tab));
                string val = Unescape(line.Substring(tab + 1));
                if (key.Trim().Length == 0 || val.Trim().Length == 0) continue;
                Add(key, val);
                n++;
            }
            cache.Clear();
            return n;
        }

        public void Add(string key, string val)
        {
            if (HasPlaceholder(key))
            {
                AddTemplate(key, val);
                return;
            }
            string t = key.Trim();
            exact[key] = val;
            outputs.Add(val.Trim());
            if (t.Length > 0 && t.Length <= MaxCaseInsensitiveLen)
                lower[t.ToLowerInvariant()] = val;
            if (key.IndexOf('<') < 0) plainKeys[t] = val;
        }

        void AddTemplate(string key, string val)
        {
            List<string> names = new List<string>();
            StringBuilder rx = new StringBuilder("^");
            StringBuilder lit = new StringBuilder();
            string hint = "";
            int i = 0;
            while (i < key.Length)
            {
                string ph = null;
                if (key[i] == '{' && i + 2 < key.Length && key[i + 2] == '}')
                {
                    char c = key[i + 1];
                    if (IsPlaceholderChar(c)) ph = "{" + c + "}";
                }
                if (ph != null)
                {
                    if (lit.Length > hint.Length) hint = lit.ToString();
                    lit.Length = 0;
                    rx.Append(ph == "{s}" || ph == "{t}" ? "(.+?)" : NumRx);
                    names.Add(ph);
                    i += 3;
                }
                else
                {
                    rx.Append(Regex.Escape(key[i].ToString()));
                    lit.Append(key[i]);
                    i++;
                }
            }
            rx.Append('$');
            if (lit.Length > hint.Length) hint = lit.ToString();

            Template t = new Template();
            t.Rx = new Regex(rx.ToString(), RegexOptions.CultureInvariant);
            t.Value = val;
            t.Names = names.ToArray();
            t.Hint = hint.Trim();

            int idx;
            if (templateIndex.TryGetValue(key, out idx)) templates[idx] = t;
            else
            {
                templateIndex[key] = templates.Count;
                templates.Add(t);
            }
        }

        public void LoadDumpHistory()
        {
            if (string.IsNullOrEmpty(DumpPath) || !File.Exists(DumpPath)) return;
            foreach (string raw in File.ReadAllLines(DumpPath, Encoding.UTF8))
            {
                string line = raw.TrimEnd('\r');
                if (line.Length == 0 || line[0] == '#') continue;
                int tab = line.IndexOf('\t');
                missed.Add(Norm(Unescape(tab >= 0 ? line.Substring(0, tab) : line)));
            }
        }

        public string Translate(string s)
        {
            if (string.IsNullOrEmpty(s) || s.Length > MaxTextLength) return s;
            string r;
            if (cache.TryGetValue(s, out r)) return r;

            r = TranslateCore(s);

            if (cache.Count >= CacheLimit) cache.Clear();
            cache[s] = r;
            return r;
        }

        string TranslateCore(string s)
        {
            if (!HasLatinLetter(s)) return s;

            string r = Line(s);
            if (r != null) return r;

            if (s.IndexOf('\n') >= 0)
            {
                string[] lines = s.Split('\n');
                bool any = false;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!HasLatinLetter(lines[i])) continue;
                    string t = Line(lines[i]);
                    if (t != null) { lines[i] = t; any = true; }
                    else NoteMiss(lines[i].Trim());
                }
                return any ? string.Join("\n", lines) : s;
            }

            NoteMiss(s.Trim());
            return s;
        }

        string Line(string s)
        {
            string r = Whole(s);
            if (r != null) return r;
            if (s.IndexOf('<') < 0) return null;

            r = Segments(s);
            if (r != null) return r;

            string plain = TagRx.Replace(s, "");
            if (plain.Length == s.Length || !HasLatinLetter(plain)) return null;
            string v;
            return plainKeys.TryGetValue(plain.Trim(), out v) ? v : null;
        }

        string Whole(string s)
        {
            string v;
            if (exact.TryGetValue(s, out v)) return v;

            string t = s.Trim();
            if (t.Length == 0) return null;
            string lead = s.Substring(0, s.Length - s.TrimStart().Length);
            string trail = s.Substring(s.TrimEnd().Length);

            v = Core(t);
            if (v == null && !EndsWithPunct(t) && exact.TryGetValue(t + ".", out v) && v.Length > 1 && v[v.Length - 1] == '.')
                v = v.Substring(0, v.Length - 1);
            if (v == null && t.Length > 1)
            {
                char last = t[t.Length - 1];
                if (last == '.' || last == ':' || last == '!')
                {
                    v = Core(t.Substring(0, t.Length - 1).TrimEnd());
                    if (v != null && !EndsWithPunct(v)) v += last;
                }
            }
            return v == null ? null : lead + v + trail;
        }

        string Core(string t)
        {
            string v;
            if (exact.TryGetValue(t, out v)) return v;
            if (t.Length <= MaxCaseInsensitiveLen && lower.TryGetValue(t.ToLowerInvariant(), out v))
                return MatchCase(t, v);
            for (int i = 0; i < templates.Count; i++)
            {
                Template tp = templates[i];
                if (tp.Hint.Length > 0 && t.IndexOf(tp.Hint, StringComparison.Ordinal) < 0) continue;
                Match m = tp.Rx.Match(t);
                if (!m.Success) continue;
                string res = tp.Value;
                for (int g = 0; g < tp.Names.Length; g++)
                    res = res.Replace(tp.Names[g], m.Groups[g + 1].Value);
                return res;
            }
            return null;
        }

        string Segments(string s)
        {
            MatchCollection tags = TagRx.Matches(s);
            if (tags.Count == 0) return null;

            StringBuilder sb = new StringBuilder(s.Length + 16);
            int pos = 0, done = 0;
            foreach (Match m in tags)
            {
                if (!AppendSegment(sb, s.Substring(pos, m.Index - pos), ref done)) return null;
                sb.Append(m.Value);
                pos = m.Index + m.Length;
            }
            if (!AppendSegment(sb, s.Substring(pos), ref done)) return null;
            return done > 0 ? sb.ToString() : null;
        }

        bool AppendSegment(StringBuilder sb, string seg, ref int done)
        {
            if (seg.Length == 0 || !HasLatinLetter(seg)) { sb.Append(seg); return true; }
            string t = Whole(seg);
            if (t == null) return false;
            sb.Append(t);
            done++;
            return true;
        }

        void NoteMiss(string s)
        {
            if (!DumpEnabled || missed.Count >= DumpLimit) return;
            if (outputs.Contains(s) || HasForeignScript(s) || CountLatin(s) < 3) return;
            if (missed.Add(Norm(s))) pending.Add(s);
        }

        public void FlushDump()
        {
            if (pending.Count == 0 || string.IsNullOrEmpty(DumpPath)) return;
            StringBuilder sb = new StringBuilder();
            foreach (string s in pending) sb.Append(Escape(s)).Append('\t').Append('\n');
            pending.Clear();
            File.AppendAllText(DumpPath, sb.ToString(), new UTF8Encoding(false));
        }

        static string Norm(string s)
        {
            return DigitsRx.Replace(s, "#");
        }

        static string MatchCase(string src, string v)
        {
            if (v.Length == 0) return v;
            bool hasLetter = false, allUpper = true;
            foreach (char c in src)
            {
                if (!char.IsLetter(c)) continue;
                hasLetter = true;
                if (!char.IsUpper(c)) { allUpper = false; break; }
            }
            if (hasLetter && allUpper && src.Length > 1) return v.ToUpperInvariant();
            if (char.IsLower(src[0]) && char.IsUpper(v[0])) return char.ToLowerInvariant(v[0]) + v.Substring(1);
            if (char.IsUpper(src[0]) && char.IsLower(v[0])) return char.ToUpperInvariant(v[0]) + v.Substring(1);
            return v;
        }

        static bool EndsWithPunct(string v)
        {
            char c = v[v.Length - 1];
            return c == '.' || c == ':' || c == '!' || c == '?';
        }

        static bool HasLatinLetter(string s)
        {
            foreach (char c in s) if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) return true;
            return false;
        }

        static int CountLatin(string s)
        {
            int n = 0;
            foreach (char c in s) if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z')) n++;
            return n;
        }

        static bool HasForeignScript(string s)
        {
            foreach (char c in s)
            {
                if (c >= '\u0400' && c <= '\u04FF') return true;
                if (c >= '\u3040' && c <= '\u30FF') return true;
                if (c >= '\u4E00' && c <= '\u9FFF') return true;
                if (c >= '\uAC00' && c <= '\uD7AF') return true;
            }
            return false;
        }

        static bool IsPlaceholderChar(char c)
        {
            return c == '0' || c == '1' || c == '2' || c == 's' || c == 't';
        }

        static bool HasPlaceholder(string key)
        {
            for (int i = 0; i + 2 < key.Length; i++)
                if (key[i] == '{' && key[i + 2] == '}' && IsPlaceholderChar(key[i + 1])) return true;
            return false;
        }

        static string Unescape(string s)
        {
            if (s.IndexOf('\\') < 0) return s;
            StringBuilder sb = new StringBuilder(s.Length);
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c == '\\' && i + 1 < s.Length)
                {
                    char n = s[i + 1];
                    if (n == 'n') { sb.Append('\n'); i++; continue; }
                    if (n == 't') { sb.Append('\t'); i++; continue; }
                    if (n == '\\') { sb.Append('\\'); i++; continue; }
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        static string Escape(string s)
        {
            return s.Replace("\\", "\\\\").Replace("\t", "\\t").Replace("\r", "").Replace("\n", "\\n");
        }
    }
}
