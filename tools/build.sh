#!/usr/bin/env bash
set -euo pipefail
: "${BEPINEX_CORE:?BEPINEX_CORE is not set}"
: "${UNITY_DLL:?UNITY_DLL is not set}"
cd "$(dirname "$0")/.."
mkdir -p build
mcs -target:library -sdk:4.5 -optimize+ -out:build/PPG_Translate.dll \
  -r:"$BEPINEX_CORE/BepInEx.dll" -r:"$BEPINEX_CORE/0Harmony.dll" -r:"$UNITY_DLL" -r:System.dll src/*.cs
mcs -out:build/TranslatorTests.exe tests/TranslatorTests.cs src/Translator.cs
mono build/TranslatorTests.exe translations build/untranslated_test.txt
