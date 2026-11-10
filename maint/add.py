p = 'README.md'
lines = open(p, encoding='utf-8').read().split('\n')
add = [
    'New versions are published in [Releases](../../releases). To get an email about each one: **Watch → Custom → Releases** (a GitHub account is needed). In the game, the GitHub button in the support menu opens this page.',
    '新版本会发布在 [Releases](../../releases)。想收到邮件通知：**Watch → Custom → Releases**（需要 GitHub 账号）。游戏内“支持作者”菜单里的 GitHub 按钮会打开本页。',
    'Las versiones nuevas se publican en [Releases](../../releases). Para recibir un correo con cada una: **Watch → Custom → Releases** (hace falta una cuenta de GitHub). En el juego, el botón GitHub del menú «apoyar al autor» abre esta página.',
    'Les nouvelles versions sortent dans [Releases](../../releases). Pour recevoir un e-mail à chaque fois : **Watch → Custom → Releases** (compte GitHub requis). Dans le jeu, le bouton GitHub du menu « soutenir l\'auteur » ouvre cette page.',
    'As versões novas saem em [Releases](../../releases). Para receber um e-mail a cada uma: **Watch → Custom → Releases** (precisa de conta no GitHub). No jogo, o botão GitHub do menu «apoiar o autor» abre esta página.',
    'Новые версии выходят в [Releases](../../releases). Чтобы получать письмо о каждой: **Watch → Custom → Releases** (нужен аккаунт GitHub). В игре сюда ведёт кнопка GitHub в меню «поддержать автора».',
    'Neue Versionen erscheinen unter [Releases](../../releases). Für eine E-Mail bei jeder: **Watch → Custom → Releases** (GitHub-Konto nötig). Im Spiel öffnet der GitHub-Button im Menü «Autor unterstützen» diese Seite.',
    '新バージョンは [Releases](../../releases) で公開します。公開のたびにメールを受け取るには **Watch → Custom → Releases**（GitHub アカウントが必要）。ゲーム内の「作者を支援する」メニューにある GitHub ボタンからもこのページを開けます。',
    'Yeni sürümler [Releases](../../releases) sayfasında yayımlanır. Her sürümde e-posta almak için: **Watch → Custom → Releases** (GitHub hesabı gerekir). Oyunda "yapımcıyı destekle" menüsündeki GitHub düğmesi bu sayfayı açar.',
    '새 버전은 [Releases](../../releases)에 올라옵니다. 새 버전마다 메일을 받으려면 **Watch → Custom → Releases** (GitHub 계정 필요). 게임 안 "제작자 후원하기" 메뉴의 GitHub 버튼으로도 이 페이지가 열립니다.',
    'Nowe wersje pojawiają się w [Releases](../../releases). Żeby dostawać e-mail o każdej: **Watch → Custom → Releases** (potrzebne konto GitHub). W grze przycisk GitHub w menu «wesprzyj autora» otwiera tę stronę.',
]
idx = [i for i, l in enumerate(lines) if '](../../issues)' in l]
assert len(idx) == 11, idx
for i, text in reversed(list(zip(idx, add))):
    lines[i + 1:i + 1] = ['', text]
open(p, 'w', encoding='utf-8', newline='\n').write('\n'.join(lines))
