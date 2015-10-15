using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace BZLauncher
{
	public partial class MainWindow : Window
	{
		private App app;

		public MainWindow()
		{
			InitializeComponent();

			app = (App)Application.Current;

			app.LoadMaps();
            listBox.ItemsSource = app.Maps;

			Title = "BZLauncher - " + app.DirectoryPath;

			// update the window title to any changed BZ directory
			app.bzonePathChanged += path => Title = "BZLauncher - " + path;
        }

		private void MapSelectedChange(object sender, SelectionChangedEventArgs e)
		{
			if(0 <= listBox.SelectedIndex && listBox.SelectedIndex < app.Maps.Count)
			{
				Map toLoad = listBox.ItemsSource.Cast<Map>().ElementAt(listBox.SelectedIndex);

				mapImage.Source = toLoad.Image();
				mapObjectiveTextBlock.Text = toLoad.Objective();
				mapAuthorOutput.Content = toLoad.author;
				mapWorldOutput.Content = toLoad.world;
				mapSizeOutput.Content = toLoad.size;
				mapPowerupsOutput.Content = toLoad.powerups.ToString();
				mapGeysersOutput.Content = toLoad.geysers.ToString();
				mapScrapOutput.Content = toLoad.scrap.ToString();
				mapTypeOutput.Content = toLoad.type == Map.Type.InstantAction ? "Instant Action" : toLoad.type.ToString();
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
				listBox.ItemsSource = app.Maps.Where(m => m.filename.Contains(mapNameSearch.Text));
			else
				listBox.ItemsSource = app.Maps;

			e.Handled = true;
		}

		private void MapAuthorSearch(object sender, TextChangedEventArgs e)
		{
			if(mapAuthorSearch.Text.Length != 0)
				listBox.ItemsSource = app.Maps.Where(m => m.author.Contains(mapAuthorSearch.Text));
			else
				listBox.ItemsSource = app.Maps;

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
					new MapInstaller(f);
                }

				e.Handled = true;
			}
		}

		private void AppExitClick(object sender, RoutedEventArgs e)
		{
			e.Handled = true;

			Close();
		}

		private void ChangeBzonePathClick(object sender, RoutedEventArgs e)
		{
			string path = FindBzExeDialog.GetPath();

			if(path != null && path.Length != 0)
			{
				app.DirectoryPath = path;
				Properties.Settings.Default.Save();

				RefreshMapListClick(this, e);
			}
			
			e.Handled = true;
        }

		private void AboutClick(object sender, RoutedEventArgs e)
		{
			// TODO

			e.Handled = true;
		}

		private void RefreshMapListClick(object sender, RoutedEventArgs e)
		{
			// clear search boxes
			mapNameSearch.Text = "";
			mapAuthorSearch.Text = "";

			app.LoadMaps();

			// stupid workaround since ListBox doesn't have a sort of "requery ItemsSource" functionality
			listBox.ItemsSource = null;
			listBox.ItemsSource = app.Maps;

			e.Handled = true;
		}
    }
}
