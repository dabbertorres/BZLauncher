using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace BZLauncher
{
	public sealed partial class App : Application, IDisposable
	{
		// notify listeners about BZ Path change
		public delegate void BzonePathChanged(string path);
		public event BzonePathChanged bzonePathChanged;
		
		private const string BZ_REG_KEY = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{B3B61934-313A-44A2-B589-700FDAA6C758}_is1";

		private readonly byte[] MISSION_BYTES = System.Text.Encoding.ASCII.GetBytes("Mission");

		private Process bzoneProcess;

		private string bzDirectoryPath;
		private string bzExePath;
		private string bzAddonPath;

		private List<Map> maps;
		private readonly object MAPS_LOCK = new object();

		public string DirectoryPath
		{
			get { return bzDirectoryPath; }

			set
			{
				bzDirectoryPath = value;
				bzExePath = bzDirectoryPath + "/bzone.exe";
				bzAddonPath = bzDirectoryPath + "/addon";

				if(bzonePathChanged != null)
					bzonePathChanged.Invoke(bzDirectoryPath);
            }
		}

		public string BzoneExe
		{
			get { return bzExePath; }
		}

		public string AddonPath
		{
			get { return bzAddonPath; }
		}

		public List<Map> Maps
		{
			get { return maps; }
		}

		public App()
		{
			// listen for our exit event to save any changed settings
			Exit += (s, args) => BZLauncher.Properties.Settings.Default.Save();
			
			// listen for newly installed maps
			MapInstaller.loadMapsSignal += LoadMaps;

			bool promptForPath = true;

			// if our Settings' BzonePath string does not exist, check the registry for a BZ install location
			if(BZLauncher.Properties.Settings.Default.BzonePath == null || BZLauncher.Properties.Settings.Default.BzonePath.Length == 0)
			{
				using(RegistryKey bzkey = Registry.LocalMachine.OpenSubKey(BZ_REG_KEY))
				{
					if(bzkey != null)
					{
						string installLocation = bzkey.GetValue("InstallLocation") as string;

						if(installLocation != null && installLocation.Length != 0)
						{
							DirectoryPath = installLocation;

							promptForPath = false;
						}
					}
				}
			}
			// otherwise, it does exist, so use it
			else
			{
				if(BZLauncher.Properties.Settings.Default.BzonePath.LastIndexOf('.') >= BZLauncher.Properties.Settings.Default.BzonePath.Length - 5)
					BZLauncher.Properties.Settings.Default.BzonePath = BZLauncher.Properties.Settings.Default.BzonePath.Substring(0, BZLauncher.Properties.Settings.Default.BzonePath.LastIndexOfAny(new char[] { '/', '\\' }));

                DirectoryPath = BZLauncher.Properties.Settings.Default.BzonePath;

				promptForPath = false;
			}

			// if we didn't have a BzonePath setting, and we couldn't find a path in the registry, ask the user for a path
			if(promptForPath)
			{
				string path = FindBzExeDialog.GetPath();

				if(path == null)
				{
					MessageBox.Show("No path selected or found, exiting.");
					Shutdown();
				}

				DirectoryPath = path;
			}

			maps = new List<Map>();

			bzoneProcess = new Process();
			bzoneProcess.Exited += (s, args) =>
            {
				BZLauncher.Properties.Settings.Default.TimePlayed += bzoneProcess.ExitTime - bzoneProcess.StartTime;

				int totalTime = BZLauncher.Properties.Settings.Default.TimePlayed.Days * 24 + BZLauncher.Properties.Settings.Default.TimePlayed.Hours;

				MessageBox.Show("Time played: " + totalTime + " hours", "Time Played", MessageBoxButton.OK);
			};
		}

		public void LaunchBzone(string args)
		{
			bzoneProcess.StartInfo.FileName = bzExePath;
			bzoneProcess.StartInfo.UseShellExecute = false;
			bzoneProcess.StartInfo.WorkingDirectory = bzDirectoryPath;
			bzoneProcess.EnableRaisingEvents = true;

			bzoneProcess.StartInfo.Arguments = args;

			bzoneProcess.Start();
		}

		public void LoadMaps(string path = null)
		{
			// reloading from addon path
			if(path == null)
				maps.Clear();
			
			// block until all Threads finish
			using(CountdownEvent cntDwn = new CountdownEvent(1))
			{
				FindMapsInDir(path ?? bzAddonPath, cntDwn);
				cntDwn.Wait();
			}

			// alphabetical order
			maps.Sort((one, two) => one.CompareTo(two));
		}

		private void FindMapsInDir(string path, CountdownEvent cntDwn)
		{
			var dirs = Directory.EnumerateDirectories(path);
			
			foreach(string d in dirs)
			{
				cntDwn.AddCount();
				ThreadPool.QueueUserWorkItem(o => FindMapsInDir(d, cntDwn));
			}

			var bzns = Directory.EnumerateFiles(path, "*.bzn");

			foreach(string b in bzns)
			{
				Map m = LoadMap(b.Substring(0, b.LastIndexOf('.')));

				lock(MAPS_LOCK)
				{
					if(m != null)
						maps.Add(m);
				}
			}

			cntDwn.Signal();
		}

		private Map LoadMap(string path)
		{
			string desFile = path + ".des";
			string trnFile = path + ".trn";
			string bznFile = path + ".bzn";

			Map ret = new Map();
			ret.bznPath = path.Substring(0, path.LastIndexOfAny(new char[] {'/', '\\'}));
			ret.filename = Path.GetFileNameWithoutExtension(path);
			
			if(File.Exists(desFile))
				ParseDes(desFile, ret);
			else if(File.Exists(trnFile))
				ParseTrn(trnFile, ret);

			ParseBzn(bznFile, ret);

			return ret.type == Map.Type.InstantAction ? ret : null;
		}

		// functions to read des, trn, bzn, etc files

		private void ParseDes(string file, Map map)
		{
			using(StreamReader sr = new StreamReader(file))
			{
				string line = null;

				while((line = sr.ReadLine()) != null)
				{
					int colonIdx = line.IndexOf(':');

					if(colonIdx != -1 && line.Length > colonIdx)
					{
						string key = line.Substring(0, colonIdx).Trim();
						string value = line.Substring(colonIdx + 1).Trim();

						if(value.Length == 0)
							continue;

						switch(key)
						{
							case "WORLD":
								map.world = value;
								break;

							case "SIZE":
								map.size = value;
								break;

							case "POWERUPS":
								map.powerups = value == "Yes";
								break;

							case "GEYSERS":
								map.geysers = uint.Parse(value);
								break;

							case "SCRAP":
								map.scrap = uint.Parse(value);
								break;

							case "AUTHOR":
								map.author = value;
								break;

							case "VERSION":
								map.version = value;
								break;

							default:
								break;
						}
					}
				}
			}
		}

		private void ParseTrn(string file, Map map)
		{
			
		}

		private void ParseBzn(string file, Map map)
		{
			using(FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				using(BinaryReader br = new BinaryReader(fs))
				{
					List<byte> bytes = br.ReadBytes((int)fs.Length).ToList();
					
					// don't check the last 6 bytes. "Mission" is 7 bytes
					for(int i = 0; i < bytes.Count - (MISSION_BYTES.Length - 1); ++i)
					{
						if(bytes[i] == MISSION_BYTES[0])
						{
							bool foundMission = true;

							for(int j = 1; j < MISSION_BYTES.Length; ++j)
							{
								if(bytes[i + j] != MISSION_BYTES[j])
								{
									foundMission = false;
									break;
								}
							}

							if(foundMission)
							{
								string prefix = System.Text.Encoding.ASCII.GetString(bytes.GetRange(i - 6, 6).ToArray());

								switch(prefix)
								{
									case "MultST":
										map.type = Map.Type.Strategy;
										break;

									case "MultDM":
										map.type = Map.Type.Deathmatch;
										break;

									case "Inst4X":
										map.type = Map.Type.InstantAction;
										break;

									default:
										if(prefix.Substring(1) == "Empty")
										{
											map.type = Map.Type.InstantAction;
										}
										else if(prefix.Substring(3) == "Lua")
										{
											map.type = Map.Type.InstantAction;
										}
										else
										{
											map.type = Map.Type.Unknown;
										}
										break;
								}

								break;
							}
						}
					}
				}
			}
		}

		public void Dispose()
		{
			((IDisposable)bzoneProcess).Dispose();
		}
	}
}
