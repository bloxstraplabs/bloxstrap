using System.ComponentModel;

namespace Bloxstrap
{
    static class Utilities
    {
        public static void ShellExecute(string website)
        {
            try
            {
                Process.Start(new ProcessStartInfo 
                { 
                    FileName = website, 
                    UseShellExecute = true 
                });
            }
            catch (Win32Exception ex)
            {
                // lmfao

                if (ex.NativeErrorCode != (int)ErrorCode.CO_E_APPNOTFOUND)
                    throw;

                Process.Start(new ProcessStartInfo
                {
                    FileName = "rundll32.exe",
                    Arguments = $"shell32,OpenAs_RunDLL {website}"
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="versionStr1"></param>
        /// <param name="versionStr2"></param>
        /// <returns>
        /// Result of System.Version.CompareTo <br />
        /// -1: version1 &lt; version2 <br />
        ///  0: version1 == version2 <br />
        ///  1: version1 &gt; version2
        /// </returns>
        public static int CompareVersions(string versionStr1, string versionStr2)
        {
            var version1 = new Version(versionStr1.Replace("v", ""));
            var version2 = new Version(versionStr2.Replace("v", ""));

            return version1.CompareTo(version2);
        }
    }
}
