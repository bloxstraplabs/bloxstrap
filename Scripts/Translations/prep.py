import glob, shutil, re

exports = input("Path of folder of exported Crowdin files: ")
dest = input("Destination resources folder: ")

icu_codes = {
	"zh-CN": "zh-Hans-CN",
	"zh-HK": "zh-Hant-HK",
	"zh-TW": "zh-Hant-TW"
}

for filename in glob.glob(f"{exports}\\**\\*.*", recursive=True):
	print(f"Copying {filename}")

	localeCode = re.search("\\\\([a-zA-Z\\-]+)\\\\Strings.", filename).group(1)
	localeCode = icu_codes.get(localeCode, localeCode)

	shutil.copy(filename, dest + f"\\Strings.{localeCode}.resx")