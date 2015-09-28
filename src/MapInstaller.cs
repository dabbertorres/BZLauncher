using System.Linq;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BZLauncher
{
	public class MapInstaller
	{
		public delegate void LoadMapsSignal();
		public event LoadMapsSignal loadMapsSignal;
		
		public MapInstaller(string zipFile)
		{
			string addonPath = ((App)Application.Current).AddonPath;

			FileAttributes attribs = File.GetAttributes(zipFile);

			// is this an existing zip file?
			if(File.Exists(zipFile) && attribs == FileAttributes.Compressed || attribs == FileAttributes.Archive)
			{
				ThreadPool.QueueUserWorkItem((o) => InstallMap(zipFile, addonPath, loadMapsSignal));
			}
		}

		private static void InstallMap(string zipFile, string dest, LoadMapsSignal signal)
		{
			using(ZipArchive zip = ZipFile.OpenRead(zipFile))
			{
				bool mapFilesInFolder = false;
				string bznName = null;
				
				foreach(ZipArchiveEntry e in zip.Entries)
				{
					// Are the map files already in a folder in the zip?
					if(e.FullName.LastIndexOfAny(new char[] {'/', '\\'}) != -1)
					{
						mapFilesInFolder = true;
                    }

					// does it have a BZ map file?
					if(e.Name.Contains("bzn"))
					{
						bznName = Path.GetFileNameWithoutExtension(e.Name);
					}
				}

				if(bznName == null)
				{
					// couldn't find a bzn in the zip!
					/*Popup pop = new Popup();
					Button b = new Button();
					b.Content = "Zip file, " + zipFile + " does not contain a .bzn!";
					pop.Child = b;
					pop.IsOpen = true;*/
					
					return;
				}

				string mapDir = dest + "/" + bznName;
				DirectoryInfo di = Directory.CreateDirectory(mapDir);
				
				if(!di.Exists)
				{
					// couldn't create directory for map!
					return;
				}

				if(di.EnumerateFiles().Count() != 0)
				{
					// map directory already exists!
					return;
				}

				if(mapFilesInFolder)
				{
					foreach(ZipArchiveEntry e in zip.Entries)
					{
						e.ExtractToFile(mapDir + "/" + e.Name);
					}
                }
				else
				{
					zip.ExtractToDirectory(mapDir);
				}
			}

			signal.Invoke();
        }
	}
}
