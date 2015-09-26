using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace BZLauncher
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private const string BZ_REG_KEY = "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall\\{B3B61934-313A-44A2-B589-700FDAA6C758}_is1";

		private string bzDirectoryPath;
		private string bzExePath;
		private string bzAddonPath;

		private List<Map> maps;

		public List<Map> Maps
		{
			get { return maps; }
		}

		public App()
		{
			// should ask for write access just in case we need to set it (if it doesn't already exist and we get a path from the user)
			// another option would be to create our own key that doesn't require admin access to write to (in CurrentUser), and only ever
			// read from the BZ uninstall key if our key does not exist
			using(RegistryKey bzkey = Registry.LocalMachine.OpenSubKey(BZ_REG_KEY))
			{
				if(bzkey != null)
				{
					bzDirectoryPath = bzkey.GetValue("InstallLocation") as string;
					bzExePath = bzDirectoryPath + "bzone.exe";
					bzAddonPath = bzDirectoryPath + "addon";

					if(!File.Exists(bzExePath))
					{
						// pop up window asking user to give the path to BZ
					}
				}
				else
				{
					// pop up window asking user to give the path to BZ
				}
			}
		}

		public Map GetMapAt(int idx)
		{
			return maps[idx];
		}

		public List<Map> LoadMaps()
		{
			maps = FindMapsInDir(bzAddonPath);

			// alphabetical order
			maps.Sort((Map one, Map two) => one.CompareTo(two));

			return maps;
		}

		public void PromptForPath()
		{
			
		}

		private List<Map> FindMapsInDir(string path)
		{
			List<Map> ret = new List<Map>();

			var dirs = Directory.EnumerateDirectories(path);
			
			foreach(string d in dirs)
			{
				ret.AddRange(FindMapsInDir(d));
			}

			var bzns = Directory.EnumerateFiles(path, "*.bzn");

			foreach(string b in bzns)
			{
				ret.Add(LoadMap(b.Substring(0, b.LastIndexOf('.'))));
			}

			return ret;
		}

		private Map LoadMap(string path)
		{
			string desFile = path + ".des";
			string trnFile = path + ".trn";

			Map ret = new Map();
			ret.bznPath = path.Substring(0, path.LastIndexOfAny(new char[] {'/', '\\'}));
			ret.filename = Path.GetFileNameWithoutExtension(path);

			// if we already have this Map loaded, don't load it, and return null
			//if(maps.Contains(ret))
			//	return null;
			
			if(File.Exists(desFile))
			{
				using(StreamReader sr = new StreamReader(desFile))
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
									ret.world = value;
									break;

								case "SIZE":
									ret.size = value;
									break;

								case "POWERUPS":
									ret.powerups = value == "Yes";
									break;

								case "GEYSERS":
									ret.geysers = uint.Parse(value);
									break;

								case "SCRAP":
									ret.scrap = uint.Parse(value);
									break;

								case "AUTHOR":
									ret.author = value;
									break;

								case "VERSION":
									ret.version = value;
									break;

								default:
									break;
							}
						}
					}
				}
			}
			else if(File.Exists(trnFile))
			{
				// do nothing for now
			}

			return ret;
		}

		// functions to read des, trn, bzn, etc files
	}
}
