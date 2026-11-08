#!/usr/bin/env bash
set -euo pipefail
: "${ARCH:?ARCH is not set}"
: "${VERSION:?VERSION is not set}"
: "${BEPINEX_ZIP:?BEPINEX_ZIP is not set}"
: "${THIRD_PARTY_LICENSES:?THIRD_PARTY_LICENSES is not set}"
cd "$(dirname "$0")/.."
root="$PWD"
name="PPG_Translate"
stage="$root/build/stage_$ARCH"
out="$root/dist/${name}_${VERSION}_${ARCH}.zip"
rm -rf "$stage"
mkdir -p "$stage" "$root/dist"
unzip -q "$BEPINEX_ZIP" -d "$stage"
plug="$stage/BepInEx/plugins/$name"
mkdir -p "$plug/source" "$plug/translations" "$plug/fonts" "$plug/flags" "$stage/LICENSES"
cp "build/$name.dll" "$plug/"
cp src/*.cs "$plug/source/"
for d in translations/*/; do
  mkdir -p "$plug/$d"
  cp "$d"*.txt "$plug/$d"
done
for f in NotoSansJP-PPG.ttf NotoSansKR-PPG.ttf NotoSansSC-PPG.ttf; do
  test -s "build/fonts/$f"
  cp "build/fonts/$f" "$plug/fonts/"
done
cp build/flags/*.png "$plug/flags/"
cp INSTALL.txt INSTALL_RU.txt "$stage/"
cp LICENSE "$stage/LICENSES/$name.txt"
cp fonts/OFL.txt "$stage/LICENSES/NotoSansCJK-OFL.txt"
cp "$THIRD_PARTY_LICENSES"/*.txt "$stage/LICENSES/"
cd "$stage"
{
  echo "SHA-256. BepInEx original: https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.5"
  echo "$(basename "$BEPINEX_ZIP"): $(sha256sum "$BEPINEX_ZIP" | cut -d' ' -f1)"
  find . -type f ! -name 'SHA256SUMS.txt' ! -path './LICENSES/*' ! -path "./BepInEx/plugins/$name/source/*" ! -path "./BepInEx/plugins/$name/translations/*" | LC_ALL=C sort | xargs -d '\n' sha256sum
} > ../SHA256SUMS.tmp
mv ../SHA256SUMS.tmp SHA256SUMS.txt
rm -f "$out"
zip -qrX "$out" .
echo "$out"
