using System.Collections.Generic;
using System.IO;
using System.Linq;
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

		public string DirectoryPath
		{
			get { return bzDirectoryPath; }
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
				Map test = new Map();
				test.filename = Path.GetFileNameWithoutExtension(b);

				// if we've already loaded the map, don't load it again
				if(maps == null || (maps != null && !maps.Contains(test)))
				{
					Map m = LoadMap(b.Substring(0, b.LastIndexOf('.')));

					if(m != null)
					{
						ret.Add(m);
					}
				}
			}

			return ret;
		}

		private Map LoadMap(string path)
		{
			string desFile = path + ".des";
			string trnFile = path + ".trn";
			string bznFile = path + ".bzn";

			Map ret = new Map();
			ret.bznPath = path.Substring(0, path.LastIndexOfAny(new char[] {'/', '\\'}));
			ret.filename = Path.GetFileNameWithoutExtension(path);

			// if we already have this Map loaded, don't load it, and return null
			//if(maps.Contains(ret))
			//	return null;
			
			if(File.Exists(desFile))
			{
				ParseDes(desFile, ref ret);
			}
			else if(File.Exists(trnFile))
			{
				ParseTrn(trnFile, ref ret);
			}

			ParseBzn(bznFile, ref ret);

			if(ret.type != Map.Type.InstantAction)
			{
				return null;
			}
			else
			{
				return ret;
			}
		}

		// functions to read des, trn, bzn, etc files

		private void ParseDes(string file, ref Map map)
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
						{
							continue;
						}

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

		private void ParseTrn(string file, ref Map map)
		{
			
		}

		private void ParseBzn(string file, ref Map map)
		{
			byte[] mission = System.Text.Encoding.ASCII.GetBytes("Mission");

			using(FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read))
			{
				using(BinaryReader br = new BinaryReader(fs))
				{
					List<byte> bytes = br.ReadBytes((int)fs.Length).ToList();
					
					// don't check the last 6 bytes. "Mission" is 7 bytes
					for(int i = 0; i < bytes.Count - (mission.Length - 1); ++i)
					{
						if(bytes[i] == mission[0])
						{
							bool foundMission = true;

							for(int j = 1; j < mission.Length; ++j)
							{
								if(bytes[i + j] != mission[j])
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
	}
}
