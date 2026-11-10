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
        public string Repo;

        public Lang(string code, string name, string font, string[] system, string language, string support, string back, string pitch, string opens, string repo)
        {
            Repo = repo;
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
        public static readonly Lang English = new Lang("en", "English", null, new[] { "English" }, "language", "support the author", "back", "<b>Hi, I'm Teo Hanki!</b>\nI make PPG Translate on my own, in my free time.\nThe mod is free and will always stay free.\nEvery donation helps me keep the translation up to date\nand add new languages. With each update I put\nsupporters' names in the credits. Thank you!", "opens in your browser", "new versions, bug reports, ideas");

        public static readonly Lang[] All =
        {
            English,
            new Lang("zh-Hans", "简体中文", "zh", new[] { "ChineseSimplified", "Chinese" }, "语言", "支持作者", "返回", "<b>你好，我是 Teo Hanki！</b>\nPPG Translate 是我一个人利用业余时间制作的。\n这个模组免费，并且永远免费。\n每一笔捐助都能帮我跟上游戏更新、维护翻译\n并加入新的语言。每次更新时，我都会把\n支持者的名字写进致谢名单。谢谢你！", "在浏览器中打开", "新版本、错误反馈、建议"),
            new Lang("es", "Español", null, new[] { "Spanish" }, "idioma", "apoyar al autor", "atrás", "<b>¡Hola, soy Teo Hanki!</b>\nHago PPG Translate yo solo, en mi tiempo libre.\nEl mod es gratis y siempre lo será.\nCada donación me ayuda a mantener la traducción al día\ny añadir idiomas. En cada actualización pongo\nlos nombres de quienes me apoyan en los créditos. ¡Gracias!", "se abre en el navegador", "versiones nuevas, errores e ideas"),
            new Lang("fr", "Français", null, new[] { "French" }, "langue", "soutenir l'auteur", "retour", "<b>Salut, moi c'est Teo Hanki !</b>\nJe fais PPG Translate seul, sur mon temps libre.\nLe mod est gratuit et le restera toujours.\nChaque don m'aide à garder la traduction à jour\net à ajouter des langues. À chaque mise à jour,\nj'ajoute le nom des donateurs aux crédits. Merci !", "s'ouvre dans le navigateur", "nouvelles versions, bugs et idées"),
            new Lang("pt-BR", "Português (Brasil)", null, new[] { "Portuguese" }, "idioma", "apoiar o autor", "voltar", "<b>Oi, eu sou o Teo Hanki!</b>\nFaço o PPG Translate sozinho, no meu tempo livre.\nO mod é gratuito e sempre vai ser.\nCada doação me ajuda a manter a tradução em dia\ne a adicionar idiomas. A cada atualização, coloco\no nome de quem apoia nos créditos. Valeu!", "abre no navegador", "versões novas, bugs e ideias"),
            new Lang("ru", "Русский", null, new[] { "Russian" }, "язык", "поддержать автора", "назад", "<b>Привет, я Teo Hanki!</b>\nДелаю PPG Translate один, в свободное время.\nМод бесплатный и таким останется.\nКаждый донат помогает обновлять перевод вместе с игрой\nи добавлять языки. С каждым обновлением я вписываю\nимена поддержавших в раздел благодарностей. Спасибо!", "откроется в браузере", "новые версии, баги и идеи"),
            new Lang("de", "Deutsch", null, new[] { "German" }, "sprache", "autor unterstützen", "zurück", "<b>Hi, ich bin Teo Hanki!</b>\nIch mache PPG Translate allein, in meiner Freizeit.\nDer Mod ist kostenlos und bleibt es auch.\nJede Spende hilft mir, die Übersetzung aktuell zu halten\nund Sprachen hinzuzufügen. Mit jedem Update trage ich\ndie Namen der Unterstützer in die Credits ein. Danke!", "öffnet sich im Browser", "neue Versionen, Fehler und Ideen"),
            new Lang("ja", "日本語", "ja", new[] { "Japanese" }, "言語", "作者を支援する", "戻る", "<b>こんにちは、Teo Hanki です！</b>\nPPG Translate は私が一人で、空き時間に作っています。\nこの MOD は無料で、これからもずっと無料です。\n支援はゲームの更新に合わせた翻訳や\n新しい言語の追加に役立ちます。更新のたびに\n支援者のお名前をクレジットに載せます。ありがとうございます！", "ブラウザで開きます", "新バージョン、バグ報告、ご提案"),
            new Lang("tr", "Türkçe", null, new[] { "Turkish" }, "dil", "yapımcıyı destekle", "geri", "<b>Selam, ben Teo Hanki!</b>\nPPG Translate'i boş zamanlarımda tek başıma yapıyorum.\nMod ücretsiz ve hep ücretsiz kalacak.\nHer bağış çeviriyi güncel tutmama ve yeni diller\neklememe yardım ediyor. Her güncellemede\ndestekçilerin adını teşekkürler bölümüne yazıyorum. Teşekkürler!", "tarayıcıda açılır", "yeni sürümler, hatalar ve fikirler"),
            new Lang("ko", "한국어", "ko", new[] { "Korean" }, "언어", "제작자 후원하기", "뒤로", "<b>안녕하세요, Teo Hanki입니다!</b>\nPPG Translate는 제가 혼자 여가 시간에 만들고 있어요.\n이 모드는 무료이고 앞으로도 계속 무료입니다.\n후원은 게임 업데이트에 맞춰 번역을 고치고\n새 언어를 추가하는 데 큰 힘이 됩니다. 업데이트할 때마다\n후원자 이름을 크레딧에 올립니다. 감사합니다!", "브라우저에서 열립니다", "새 버전, 버그 신고, 아이디어"),
            new Lang("pl", "Polski", null, new[] { "Polish" }, "język", "wesprzyj autora", "wstecz", "<b>Cześć, jestem Teo Hanki!</b>\nRobię PPG Translate sam, w wolnym czasie.\nMod jest darmowy i zawsze taki zostanie.\nKażda wpłata pomaga mi aktualizować tłumaczenie\ni dodawać języki. Przy każdej aktualizacji wpisuję\nimiona wspierających do podziękowań. Dzięki!", "otworzy się w przeglądarce", "nowe wersje, błędy i pomysły"),
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
