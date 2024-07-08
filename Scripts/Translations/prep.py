import glob, shutil

exports = input("Path of folder of exported Crowdin files: ")
dest = input("Destination resources folder: ")

for filename in glob.glob(f"{exports}\\**\\*.*", recursive=True):
	print(f"Copying {filename}")

	suffix = ""

	if filename.endswith("Strings.bs-BA.resx"):
		suffix = "\\Strings.bs.resx"
	
	shutil.copy(filename, dest + suffix)