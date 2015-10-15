using System.Linq;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System;

namespace BZLauncher
{
	public class MapInstaller
	{
		public delegate void LoadMapsSignal(string path);
		public static event LoadMapsSignal loadMapsSignal;
		
		public MapInstaller(string zipFile)
		{
			string addonPath = ((App)Application.Current).AddonPath;

			FileAttributes attribs = File.GetAttributes(zipFile);

			// is this an existing zip file?
			if(File.Exists(zipFile) && (attribs == FileAttributes.Compressed || attribs == FileAttributes.Archive))
				ThreadPool.QueueUserWorkItem(o => InstallMap(zipFile, addonPath));
		}

		private static void InstallMap(string zipFile, string dest)
		{
			try
			{
				using(ZipArchive zip = ZipFile.OpenRead(zipFile))
				{
					bool mapFilesInFolder = false;
					string bznName = null;

					foreach(ZipArchiveEntry e in zip.Entries)
					{
						// Are the map files already in a folder in the zip?
						if(e.FullName.LastIndexOfAny(new char[] { '/', '\\' }) != -1)
							mapFilesInFolder = true;

						// does it have a BZ map file?
						if(e.Name.Contains("bzn"))
							bznName = Path.GetFileNameWithoutExtension(e.Name);
					}

					if(bznName == null)
					{
						// couldn't find a bzn in the zip!
						MessageBox.Show(zipFile + " does not contain a .bzn!", "Error");
						return;
					}

					string mapDir = dest + "/" + bznName;
					DirectoryInfo di = Directory.CreateDirectory(mapDir);

					if(!di.Exists)
					{
						// couldn't create directory for map!
						MessageBox.Show("Could not create directory for the map, is your BZ directory in Program Files? Try running as administrator.", "Error");
						return;
					}

					if(di.EnumerateFiles().Count() != 0)
					{
						// map directory already exists!
						MessageBox.Show("A directory for this map already exists!", "Error");
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

					loadMapsSignal.Invoke(mapDir);
				}
			}
			catch(Exception)
			{
				MessageBox.Show("Could not open \"" + zipFile + "\" as a map archive.", "Error");
			}
        }
	}
}
