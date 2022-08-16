using Bloxstrap.Helpers;

namespace Bloxstrap
{
    partial class Bootstrapper
    {
        private async Task ModifyDeathSound()
        {
            string fileContentName = "ouch.ogg";
            string fileContentLocation = "content\\sounds\\ouch.ogg";
            string fileLocation = Path.Combine(VersionFolder, fileContentLocation);

            string officialHash = VersionFileManifest[fileContentLocation];
            string currentHash = Utilities.CalculateMD5(fileLocation);

            if (Program.Settings.UseOldDeathSound && currentHash == officialHash)
            {
                // let's get the old one!

                var response = await Client.GetAsync($"{Program.BaseUrlApplication}/mods/{fileContentLocation}");

                if (File.Exists(fileLocation))
                    File.Delete(fileLocation);

                using (var fileStream = new FileStream(fileLocation, FileMode.CreateNew))
                {
                    await response.Content.CopyToAsync(fileStream);
                }
            }
            else if (!Program.Settings.UseOldDeathSound && currentHash != officialHash)
            {
                // who's lame enough to ever do this?
                // well, we need to re-extract the one that's in the content-sounds.zip package

                string[] files = { fileContentName };
                ExtractFilesFromPackage("content-sounds.zip", files);
            }
        }

        private async Task ModifyMouseCursor()
        {
            string baseFolder = Path.Combine(VersionFolder, "content\\textures\\");
            
            string arrowCursor = "Cursors\\KeyboardMouse\\ArrowCursor.png";
            string arrowFarCursor = "Cursors\\KeyboardMouse\\ArrowFarCursor.png";

            string officialHash = VersionFileManifest["content\\textures\\Cursors\\KeyboardMouse\\ArrowCursor.png"];
            string currentHash = Utilities.CalculateMD5(Path.Combine(baseFolder, arrowCursor));

            if (Program.Settings.UseOldMouseCursor && currentHash == officialHash)
            {
                // the old cursors are actually still in the content\textures\ folder, so we can just get them from there

                File.Copy(Path.Combine(baseFolder, "ArrowCursor.png"), Path.Combine(baseFolder, arrowCursor), true);
                File.Copy(Path.Combine(baseFolder, "ArrowFarCursor.png"), Path.Combine(baseFolder, arrowFarCursor), true);
            }
            else if (!Program.Settings.UseOldMouseCursor && currentHash != officialHash)
            {
                string[] files = { arrowCursor, arrowFarCursor };
                ExtractFilesFromPackage("content-textures2.zip", files);
            }
        }
    }
}
