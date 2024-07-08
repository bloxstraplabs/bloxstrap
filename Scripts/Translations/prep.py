import glob, shutil, re

exports = input("Path of folder of exported Crowdin files: ")
dest = input("Destination resources folder: ")

for filename in glob.glob(f"{exports}\\**\\*.*", recursive=True):
	print(f"Copying {filename}")

	localeCode = re.search("\\\\([a-zA-Z\\-]+)\\\\Strings.", filename).group(1)

	shutil.copy(filename, dest + f"\\Strings.{localeCode}.resx")