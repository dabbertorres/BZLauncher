using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace BZLauncher
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private App app;

		private List<Map> displayMaps;

		public MainWindow()
		{
			InitializeComponent();

			app = (App)Application.Current;

			listBox.ItemsSource = displayMaps = app.LoadMaps();
        }

		private void MapSelectedChange(object sender, SelectionChangedEventArgs e)
		{
			Map toLoad = app.GetMapAt(listBox.SelectedIndex);

			mapImage.Source = toLoad.Image();
			mapAuthorOutput.Content = toLoad.author;
			mapWorldOutput.Content = toLoad.world;
			mapSizeOutput.Content = toLoad.size;
			mapPowerupsOutput.Content = toLoad.powerups.ToString();
			mapGeysersOutput.Content = toLoad.geysers.ToString();
			mapScrapOutput.Content = toLoad.scrap.ToString();
			mapTypeOutput.Content = toLoad.type;
			mapVersionOutput.Content = toLoad.version;

			e.Handled = true;
		}

		private void EditChecked(object sender, RoutedEventArgs e)
		{
			checkBoxStartEditOn.IsEnabled = true;

			e.Handled = true;
		}

		private void EditUnchecked(object sender, RoutedEventArgs e)
		{
			checkBoxStartEditOn.IsEnabled = false;

			e.Handled = true;
		}

		private void MapNameSearch(object sender, TextChangedEventArgs e)
		{
			if(mapNameSearch.Text.Length != 0)
			{
				listBox.ItemsSource = displayMaps = app.Maps.FindAll((Map m) => m.filename.Contains(mapNameSearch.Text));
			}
			else
			{
				listBox.ItemsSource = displayMaps = app.Maps;
			}

			e.Handled = true;
		}

		private void MapAuthorSearch(object sender, TextChangedEventArgs e)
		{
			if(mapAuthorSearch.Text.Length != 0)
			{
				listBox.ItemsSource = displayMaps = app.Maps.FindAll((Map m) => m.author.Contains(mapAuthorSearch.Text));
			}
			else
			{
				listBox.ItemsSource = displayMaps = app.Maps;
			}

			e.Handled = true;
		}
	}
}
