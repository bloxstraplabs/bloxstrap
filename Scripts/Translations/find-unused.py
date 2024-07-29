import re, glob

directory = input("Enter project path (the one containing Bloxstrap.csproj): ")

existing = []
found = []

with open(f"{directory}\\Resources\\Strings.resx", "r") as file:
	existing = re.findall("name=\"([a-zA-Z0-9.]+)\" xml:space=\"preserve\"", file.read())

for filename in glob.glob(f"{directory}\\**\\*.*", recursive=True):
	if "\\bin\\" in filename or "\\obj\\" in filename or "\\Resources\\" in filename:
		continue

	try:
		with open(filename, "r") as file:
			contents = file.read()

			matches = re.findall("Strings.([a-zA-Z0-9_]+)", contents)
			for match in matches:
				if not '_' in match:
					continue

				ref = match.replace('_', '.')
				if not ref in found:
					found.append(ref)

			matches = re.findall("FromTranslation = \"([a-zA-Z0-9.]+)\"", contents)
			for match in matches:
				if not match in found:
					found.append(match)

	except Exception:
		print(f"Could not open {filename}")
		continue

for entry in existing:
	if not entry in found and not "Enums." in entry:
		print(entry)