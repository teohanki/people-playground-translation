using System;

namespace PPGTranslate
{
    public sealed class Lang
    {
        public string Code;
        public string Name;
        public string Font;
        public string[] System;
        public string Language;
        public string Support;
        public string Back;
        public string Pitch;
        public string Opens;

        public Lang(string code, string name, string font, string[] system, string language, string support, string back, string pitch, string opens)
        {
            Pitch = pitch;
            Opens = opens;
            Code = code;
            Name = name;
            Font = font;
            System = system;
            Language = language;
            Support = support;
            Back = back;
        }
    }

    public static class Langs
    {
        public static readonly Lang English = new Lang("en", "English", null, new[] { "English" }, "language", "support the author", "back", "<b>Hi, I'm Teo Hanki!</b>\nI make PPG Translate on my own, in my free time.\nEvery donation, even a small one, helps me keep\nthe translation up to date and add new languages.\nThank you for being here!", "opens in your browser");

        public static readonly Lang[] All =
        {
            English,
            new Lang("zh-Hans", "简体中文", "zh", new[] { "ChineseSimplified", "Chinese" }, "语言", "支持作者", "返回", "<b>你好，我是 Teo Hanki！</b>\nPPG Translate 是我一个人利用业余时间制作的。\n每一笔捐助，哪怕很少，都能帮我跟上游戏更新、\n维护翻译并加入新的语言。\n谢谢你的支持！", "在浏览器中打开"),
            new Lang("es", "Español", null, new[] { "Spanish" }, "idioma", "apoyar al autor", "atrás", "<b>¡Hola, soy Teo Hanki!</b>\nHago PPG Translate yo solo, en mi tiempo libre.\nCada donación, por pequeña que sea, me ayuda a\nmantener la traducción al día y añadir idiomas.\n¡Gracias por estar aquí!", "se abre en el navegador"),
            new Lang("fr", "Français", null, new[] { "French" }, "langue", "soutenir l'auteur", "retour", "<b>Salut, moi c'est Teo Hanki !</b>\nJe fais PPG Translate seul, sur mon temps libre.\nChaque don, même petit, m'aide à garder\nla traduction à jour et à ajouter des langues.\nMerci d'être là !", "s'ouvre dans le navigateur"),
            new Lang("pt-BR", "Português (Brasil)", null, new[] { "Portuguese" }, "idioma", "apoiar o autor", "voltar", "<b>Oi, eu sou o Teo Hanki!</b>\nFaço o PPG Translate sozinho, no meu tempo livre.\nCada doação, mesmo pequena, me ajuda a manter\na tradução em dia e a adicionar novos idiomas.\nValeu por estar aqui!", "abre no navegador"),
            new Lang("ru", "Русский", null, new[] { "Russian" }, "язык", "поддержать автора", "назад", "<b>Привет, я Teo Hanki!</b>\nДелаю PPG Translate один, в свободное время.\nКаждый донат, даже небольшой, помогает мне\nобновлять перевод вместе с игрой и добавлять языки.\nСпасибо, что ты здесь!", "откроется в браузере"),
            new Lang("de", "Deutsch", null, new[] { "German" }, "sprache", "autor unterstützen", "zurück", "<b>Hi, ich bin Teo Hanki!</b>\nIch mache PPG Translate allein, in meiner Freizeit.\nJede Spende, auch eine kleine, hilft mir,\ndie Übersetzung aktuell zu halten und Sprachen hinzuzufügen.\nDanke, dass du da bist!", "öffnet sich im Browser"),
            new Lang("ja", "日本語", "ja", new[] { "Japanese" }, "言語", "作者を支援する", "戻る", "<b>こんにちは、Teo Hanki です！</b>\nPPG Translate は私が一人で、空き時間に作っています。\nどんなに少額の支援でも、ゲームの更新に合わせた翻訳や\n新しい言語の追加に役立ちます。\nいつもありがとうございます！", "ブラウザで開きます"),
            new Lang("tr", "Türkçe", null, new[] { "Turkish" }, "dil", "yapımcıyı destekle", "geri", "<b>Selam, ben Teo Hanki!</b>\nPPG Translate'i boş zamanlarımda tek başıma yapıyorum.\nKüçük de olsa her bağış, çeviriyi güncel tutmama\nve yeni diller eklememe yardım ediyor.\nBurada olduğun için teşekkürler!", "tarayıcıda açılır"),
            new Lang("ko", "한국어", "ko", new[] { "Korean" }, "언어", "제작자 후원하기", "뒤로", "<b>안녕하세요, Teo Hanki입니다!</b>\nPPG Translate는 제가 혼자 여가 시간에 만들고 있어요.\n작은 후원이라도 게임 업데이트에 맞춰 번역을 고치고\n새 언어를 추가하는 데 큰 힘이 됩니다.\n함께해 주셔서 감사합니다!", "브라우저에서 열립니다"),
            new Lang("pl", "Polski", null, new[] { "Polish" }, "język", "wesprzyj autora", "wstecz", "<b>Cześć, jestem Teo Hanki!</b>\nRobię PPG Translate sam, w wolnym czasie.\nKażda wpłata, nawet mała, pomaga mi aktualizować\ntłumaczenie i dodawać nowe języki.\nDzięki, że jesteś!", "otworzy się w przeglądarce"),
        };

        public static Lang Find(string code)
        {
            if (string.IsNullOrEmpty(code)) return null;
            foreach (Lang l in All)
                if (string.Equals(l.Code, code.Trim(), StringComparison.OrdinalIgnoreCase)) return l;
            return null;
        }

        public static Lang FromSystem(string systemLanguage)
        {
            foreach (Lang l in All)
                foreach (string s in l.System)
                    if (s == systemLanguage) return l;
            return English;
        }
    }
}
