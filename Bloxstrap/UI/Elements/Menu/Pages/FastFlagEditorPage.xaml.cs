using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using Bloxstrap.UI.Elements.Dialogs;

namespace Bloxstrap.UI.Elements.Menu.Pages
{
    /// <summary>
    /// Interaction logic for FastFlagEditorPage.xaml
    /// </summary>
    public partial class FastFlagEditorPage
    {
        // believe me when i say there is absolutely zero point to using mvvm for this
        // using a datagrid is a codebehind thing only and thats it theres literally no way around it

        private readonly ObservableCollection<KeyValuePair<string, string>> _fastFlagList = new();
        private bool _showPresets = false;

        public FastFlagEditorPage()
        {
            InitializeComponent();
        }

        private void ReloadList()
        {
            _fastFlagList.Clear();

            var presetFlags = FastFlagManager.PresetFlags.Values;

            foreach (var entry in App.FastFlags.Prop)
            {
                if (!_showPresets && presetFlags.Contains(entry.Key))
                    continue;

                _fastFlagList.Add(entry);
            }

            DataGrid.ItemsSource = _fastFlagList;
        }

        // refresh list on page load to synchronize with preset page
        private void Page_Loaded(object sender, RoutedEventArgs e) => ReloadList();

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int index = e.Row.GetIndex();
            var entry = _fastFlagList[index];

            switch (e.Column.Header)
            {
                case "Name":
                    string newName = ((TextBox)e.EditingElement).Text;

                    App.FastFlags.SetValue(entry.Key, null);
                    App.FastFlags.SetValue(newName, entry.Value);

                    break;

                case "Value":
                    string newValue = ((TextBox)e.EditingElement).Text;

                    App.FastFlags.SetValue(entry.Key, newValue);

                    break;
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddFastFlagDialog();
            dialog.ShowDialog();

            if (dialog.Result != MessageBoxResult.OK)
                return;

            var entry = new KeyValuePair<string, string>(dialog.FlagNameTextBox.Text, dialog.FlagValueTextBox.Text);

            _fastFlagList.Add(entry);

            App.FastFlags.SetValue(entry.Key, entry.Value);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var tempList = new List<KeyValuePair<string, string>>();

            foreach (KeyValuePair<string, string> entry in DataGrid.SelectedItems)
                tempList.Add(entry);

            foreach (var entry in tempList)
            {
                _fastFlagList.Remove(entry);
                App.FastFlags.SetValue(entry.Key, null);
            }
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton button)
                return;

            _showPresets = button.IsChecked ?? false;
            ReloadList();
        }
    }
}
