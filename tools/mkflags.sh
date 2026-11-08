#!/usr/bin/env bash
set -euo pipefail
: "${FLAGS_DIR:?FLAGS_DIR is not set}"
out="${1:-build/flags}"
mkdir -p "$out"
while read -r lang country; do
  rsvg-convert -w 48 -h 36 "$FLAGS_DIR/flags/4x3/$country.svg" -o "$out/$lang.png"
done <<'LIST'
en gb
zh-Hans cn
es es
fr fr
pt-BR br
ru ru
de de
ja jp
tr tr
ko kr
pl pl
LIST
ls -la "$out"
