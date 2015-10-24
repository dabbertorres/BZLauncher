using System.Diagnostics;
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

					try
					{
						DirectoryInfo di = Directory.CreateDirectory(mapDir);

						// map directory already exists!
						if(di.EnumerateFiles().Count() != 0)
							throw new IOException("A directory for this map already exists.");

						extractZip(zip, mapDir, mapFilesInFolder);
					}
					catch(UnauthorizedAccessException)
					{
						string newDest = Directories.User.get(Directories.User.Directory.Downloads) + bznName;
						Directory.CreateDirectory(newDest);
						extractZip(zip, newDest, mapFilesInFolder);

						// since we don't have access to BZ folder as current user, use cmd's move command as admin to
						// put the map in the right spot
						using(Process move = new Process())
						{
							move.StartInfo.FileName = "cmd";
							move.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
							move.StartInfo.CreateNoWindow = true;
							move.StartInfo.UseShellExecute = true;
							move.StartInfo.Verb = "runas";
							move.StartInfo.Arguments = "move /Y " + newDest + ' ' + mapDir;

							move.Start();
							move.WaitForExit();
						}
					}
					catch(IOException ioe)
					{
						MessageBox.Show(ioe.Message, "Error");
						return;
					}

					loadMapsSignal.Invoke(mapDir);
				}
			}
			catch(Exception)
			{
				MessageBox.Show("Could not open \"" + zipFile + "\" as a map archive.", "Error");
			}
		}

		private static void extractZip(ZipArchive zip, string destination, bool mapFilesInFolder)
		{
			if(mapFilesInFolder)
			{
				foreach(ZipArchiveEntry e in zip.Entries)
				{
					e.ExtractToFile(destination + "/" + e.Name);
				}
			}
			else
			{
				zip.ExtractToDirectory(destination);
			}
		}
	}
}
