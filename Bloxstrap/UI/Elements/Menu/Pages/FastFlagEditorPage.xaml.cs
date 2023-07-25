using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

using Wpf.Ui.Mvvm.Contracts;

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

        private readonly ObservableCollection<FastFlag> _fastFlagList = new();
        private bool _showPresets = false;

        public FastFlagEditorPage()
        {
            InitializeComponent();
        }

        private void ReloadList()
        {
            var selectedEntry = DataGrid.SelectedItem as FastFlag;

            _fastFlagList.Clear();

            var presetFlags = FastFlagManager.PresetFlags.Values;

            foreach (var pair in App.FastFlags.Prop.OrderBy(x => x.Key))
            {
                if (!_showPresets && presetFlags.Contains(pair.Key))
                    continue;

                var entry = new FastFlag
                {
                    // Enabled = true,
                    Name = pair.Key,
                    Value = pair.Value.ToString()!
                };

                /* if (entry.Name.StartsWith("Disable"))
                {
                    entry.Enabled = false;
                    entry.Name = entry.Name[7..];
                } */

                _fastFlagList.Add(entry);
            }

            if (DataGrid.ItemsSource is null)
                DataGrid.ItemsSource = _fastFlagList;

            if (selectedEntry is null)
                return;

            var newSelectedEntry = _fastFlagList.Where(x => x.Name == selectedEntry.Name).FirstOrDefault();

            if (newSelectedEntry is null)
                return;
            
            DataGrid.SelectedItem = newSelectedEntry;
            DataGrid.ScrollIntoView(newSelectedEntry);
        }

        // refresh list on page load to synchronize with preset page
        private void Page_Loaded(object sender, RoutedEventArgs e) => ReloadList();

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int index = e.Row.GetIndex();
            FastFlag entry = _fastFlagList[index];

            switch (e.Column.Header)
            {
                /* case "Enabled":
                    bool enabled = (bool)((CheckBox)e.EditingElement).IsChecked!;

                    if (enabled)
                    {
                        App.FastFlags.SetValue(entry.Name, entry.Value);
                        App.FastFlags.SetValue($"Disable{entry.Name}", null);
                    }
                    else
                    {
                        App.FastFlags.SetValue(entry.Name, null);
                        App.FastFlags.SetValue($"Disable{entry.Name}", entry.Value);
                    }

                    break; */

                case "Name":
                    string newName = ((TextBox)e.EditingElement).Text;

                    App.FastFlags.SetValue(entry.Name, null);
                    App.FastFlags.SetValue(newName, entry.Value);

                    break;

                case "Value":
                    string newValue = ((TextBox)e.EditingElement).Text;

                    App.FastFlags.SetValue(entry.Name, newValue);

                    break;
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is INavigationWindow window)
                window.Navigate(typeof(FastFlagsPage));
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddFastFlagDialog();
            dialog.ShowDialog();

            if (dialog.Result != MessageBoxResult.OK)
                return;

            string name = dialog.FlagNameTextBox.Text;
            
            FastFlag? entry;

            if (App.FastFlags.GetValue(name) is null)
            {
                entry = new FastFlag
                {
                    // Enabled = true,
                    Name = dialog.FlagNameTextBox.Text,
                    Value = dialog.FlagValueTextBox.Text
                };

                _fastFlagList.Add(entry);

                App.FastFlags.SetValue(entry.Name, entry.Value);
            }
            else
            {
                Controls.ShowMessageBox("An entry for this FastFlag already exists.", MessageBoxImage.Information);

                if (!_showPresets && FastFlagManager.PresetFlags.Values.Contains(dialog.FlagNameTextBox.Text))
                {
                    _showPresets = true;
                    TogglePresetsButton.IsChecked = true;
                    ReloadList();
                }

                entry = _fastFlagList.Where(x => x.Name == name).FirstOrDefault();
            }

            DataGrid.SelectedItem = entry;
            DataGrid.ScrollIntoView(entry);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var tempList = new List<FastFlag>();

            foreach (FastFlag entry in DataGrid.SelectedItems)
                tempList.Add(entry);

            foreach (FastFlag entry in tempList)
            {
                _fastFlagList.Remove(entry);
                App.FastFlags.SetValue(entry.Name, null);
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
