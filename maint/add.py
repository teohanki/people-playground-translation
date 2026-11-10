import re

p = 'src/MenuUI.cs'
s = open(p, encoding='utf-8').read()
old = 'return "<u>" + name + "</u> <color=#FF5A6E>→</color> ";'
new = 'return "<color=#FF5A6E><u>" + name + "</u></color>";'
assert s.count(old) == 1
open(p, 'w', encoding='utf-8', newline='\n').write(s.replace(old, new))

p = 'src/Plugin.cs'
s = open(p, encoding='utf-8').read()
assert s.count('2.3.3') == 3
open(p, 'w', encoding='utf-8', newline='\n').write(s.replace('2.3.3', '2.3.4'))

extras = {
    'Keywords:': 'PPG translation mod, People Playground language pack, People Playground non-English, People Playground translate to Russian, People Playground translate to Spanish, People Playground translate to Chinese, People Playground translate to Japanese, People Playground Steam language mod, PPG language mod',
    '关键词：': 'PPG 汉化, PPG 中文, People Playground 汉化补丁, 人类游乐场 汉化补丁, 人类游乐场 怎么设置中文, People Playground 简体中文, 人类游乐场 中文补丁',
    'Palabras clave:': 'People Playground en castellano, People Playground traducido, People Playground español latino, PPG español, PPG traducción, People Playground idioma español, People Playground parche de traducción',
    'Mots-clés :': 'People Playground VF, People Playground FR, PPG en français, PPG traduction, People Playground traduction FR, People Playground version française',
    'Palavras-chave:': 'People Playground PT-BR, People Playground PTBR, People Playground em português BR, PPG tradução, PPG português, People Playground patch de tradução, People Playground tradução Brasil',
    'Ключевые слова:': 'пипл плейграунд, пипл плейграунд русификатор, пипл плейграунд на русском, пипл плэйграунд, русификатор пипл плейграунд, русик People Playground, ппг русификатор, ППГ перевод, перевод ППГ на русский, People Playground локализация, People Playground русская версия, People Playground русский язык Steam, People Playground перевод интерфейса',
    'Stichwörter:': 'People Playground Deutsch Mod, People Playground deutsche Version, PPG Deutsch, PPG Übersetzung, People Playground Deutsch Patch download, People Playground Sprache ändern',
    'キーワード：': 'ピープルプレイグラウンド 日本語, PPG 日本語化, People Playground 日本語化パッチ, People Playground 日本語化 方法, ピープルプレイグラウンド 日本語化 MOD, People Playground 日本語 設定',
    'Anahtar kelimeler:': 'PPG Türkçe yama, People Playground Türkçe yama indir, People Playground Türkçe nasıl yapılır, People Playground Türkçe dil paketi, People Playground Türkçe çeviri',
    '키워드:': 'PPG 한글패치, 피플 플레이그라운드 한국어, 피플플레이그라운드 한글화, People Playground 한글화, People Playground 한글패치 설치, 피플 플레이그라운드 한글 모드',
    'Słowa kluczowe:': 'People Playground PL, PPG spolszczenie, PPG po polsku, People Playground polska wersja, People Playground polskie tłumaczenie, People Playground spolszczenie Steam',
}

p = 'README.md'
lines = open(p, encoding='utf-8').read().split('\n')
done = 0
for i, l in enumerate(lines):
    if not l.startswith('<sub>'):
        continue
    body = l[len('<sub>'):]
    for key, add in extras.items():
        if body.startswith(key):
            assert l.endswith('</sub>')
            l = l[:-len('</sub>')] + ', ' + add + '</sub>'
            if key == 'Anahtar kelimeler:':
                assert ', People Playground Türkçe yapma' in l
                l = l.replace(', People Playground Türkçe yapma', '')
            lines[i] = l
            done += 1
            break
assert done == 11, done
open(p, 'w', encoding='utf-8', newline='\n').write('\n'.join(lines))
