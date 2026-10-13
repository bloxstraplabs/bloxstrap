from pathlib import Path
import shutil

exports = Path(input("Path of folder of exported Crowdin files: "))
dest = Path(input("Destination resources folder: "))

icu_codes = {
    "zh-CN": "zh-Hans-CN",
    "zh-HK": "zh-Hant-HK",
    "zh-TW": "zh-Hant-TW",
}

for file in exports.rglob("Strings.*"):
    locale = icu_codes.get(file.parent.name, file.parent.name)
    print(f"Copying {file}")
    shutil.copy(file, dest / f"Strings.{locale}.resx")
