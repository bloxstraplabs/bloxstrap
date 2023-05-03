/*
 * Roblox Studio Mod Manager (ProjectSrc/Utility/PackageManifest.cs)
 * MIT License
 * Copyright (c) 2015-present MaximumADHD
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Bloxstrap.Models
{
    public class PackageManifest : List<Package>
    {
        private PackageManifest(string data)
        {
            using StringReader reader = new StringReader(data);
            string? version = reader.ReadLine();

            if (version != "v0")
                throw new NotSupportedException($"Unexpected package manifest version: {version} (expected v0!)");

            while (true)
            {
                string? fileName = reader.ReadLine();
                string? signature = reader.ReadLine();

                string? rawPackedSize = reader.ReadLine();
                string? rawSize = reader.ReadLine();

                if (string.IsNullOrEmpty(fileName) ||
                    string.IsNullOrEmpty(signature) ||
                    string.IsNullOrEmpty(rawPackedSize) ||
                    string.IsNullOrEmpty(rawSize))
                    break;

                // ignore launcher
                if (fileName == "RobloxPlayerLauncher.exe")
                    break;

                int packedSize = int.Parse(rawPackedSize);
                int size = int.Parse(rawSize);

                Add(new Package
                {
                    Name = fileName,
                    Signature = signature,
                    PackedSize = packedSize,
                    Size = size
                });
            }
        }

        public static async Task<PackageManifest> Get(string versionGuid)
        {
            string pkgManifestUrl = RobloxDeployment.GetLocation($"/{versionGuid}-rbxPkgManifest.txt");
            var pkgManifestData = await App.HttpClient.GetStringAsync(pkgManifestUrl);

            return new PackageManifest(pkgManifestData);
        }
    }
}
