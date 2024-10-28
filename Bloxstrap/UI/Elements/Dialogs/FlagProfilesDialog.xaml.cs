using Microsoft.Win32;
using System.Windows;
using Bloxstrap.Resources;
using Bloxstrap.Enums.FlagPresets;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Bloxstrap.UI.Elements.Dialogs
{
    /// <summary>
    /// Interaction logic for FlagProfilesDialog.xaml
    /// </summary>
    public partial class FlagProfilesDialog
    {
        public MessageBoxResult Result = MessageBoxResult.Cancel;

        public FlagProfilesDialog()
        {
            InitializeComponent();
            LoadProfiles();
        }

        private void LoadProfiles()
        {
            LoadProfile.Items.Clear();

            string profilesDirectory = Path.Combine(Paths.Base, Paths.SavedFlagProfiles);

            if (!Directory.Exists(profilesDirectory))
                Directory.CreateDirectory(profilesDirectory);

            string[] Profiles = Directory.GetFiles(profilesDirectory);

            foreach (string rawProfileName in Profiles)
            {
                string ProfileName = Path.GetFileName(rawProfileName);
                LoadProfile.Items.Add(ProfileName);
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string profilesDirectory = Path.Combine(Paths.Base, Paths.SavedFlagProfiles);

            if (!Directory.Exists(profilesDirectory))
                Directory.CreateDirectory(profilesDirectory);

            if (LoadProfile.SelectedItem == null)
                return;
            
            var SelectedItem = LoadProfile.SelectedItem.ToString();

            if (String.IsNullOrEmpty(SelectedItem))
                return;

            File.Delete(Path.Combine(profilesDirectory,SelectedItem));

            LoadProfiles();
        }
    }
}
