using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BZLauncher
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private App app;

		private List<Map> displayMaps;

		private object loadingMaps = new object();

		public MainWindow()
		{
			InitializeComponent();

			app = (App)Application.Current;

			listBox.ItemsSource = displayMaps = app.LoadMaps();
        }

		private void MapSelectedChange(object sender, SelectionChangedEventArgs e)
		{
			if(0 <= listBox.SelectedIndex && listBox.SelectedIndex < app.Maps.Count)
			{
				Map toLoad = displayMaps.ElementAt(listBox.SelectedIndex);

				mapImage.Source = toLoad.Image();
				mapObjectiveTextBlock.Text = toLoad.Objective();
				mapAuthorOutput.Content = toLoad.author;
				mapWorldOutput.Content = toLoad.world;
				mapSizeOutput.Content = toLoad.size;
				mapPowerupsOutput.Content = toLoad.powerups.ToString();
				mapGeysersOutput.Content = toLoad.geysers.ToString();
				mapScrapOutput.Content = toLoad.scrap.ToString();
				mapTypeOutput.Content = toLoad.type;
				mapVersionOutput.Content = toLoad.version;
			}

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

		private void ListBoxDrop(object sender, DragEventArgs e)
		{
			if(e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

				// get zip file, make new folder with zip file name, and extract contents to new folder
				foreach(string f in files)
				{
					new MapInstaller(f).loadMapsSignal += () =>
					{
						lock (loadingMaps)
						{
							Dispatcher.Invoke(() => listBox.ItemsSource = displayMaps = app.LoadMaps());
						}
					};
				}

				e.Handled = true;
			}
		}
	}
}
