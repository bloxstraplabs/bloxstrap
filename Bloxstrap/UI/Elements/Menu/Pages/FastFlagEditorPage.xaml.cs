using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        public FastFlagEditorPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // refresh list on page load to synchronize with preset page

            _fastFlagList.Clear();

            foreach (var pair in App.FastFlags.Prop)
            {
                var entry = new FastFlag
                {
                    Enabled = true,
                    Name = pair.Key,
                    Value = pair.Value
                };

                if (entry.Name.StartsWith("Disable"))
                {
                    entry.Enabled = false;
                    entry.Name = entry.Name[7..];
                }

                _fastFlagList.Add(entry);
            }

            DataGrid.ItemsSource = _fastFlagList;
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            int index = e.Row.GetIndex();
            FastFlag entry = _fastFlagList[index];

            switch (e.Column.Header)
            {
                case "Enabled":
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

                    break;

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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddFastFlagDialog();
            dialog.ShowDialog();

            if (dialog.Result != MessageBoxResult.OK)
                return;

            var entry = new FastFlag
            {
                Enabled = true,
                Name = dialog.FlagNameTextBox.Text,
                Value = dialog.FlagValueTextBox.Text
            };

            _fastFlagList.Add(entry);

            App.FastFlags.SetValue(entry.Name, entry.Value);
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
    }
}
