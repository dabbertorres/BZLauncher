using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BZLauncher
{
	public class Map : IComparable
	{
		public enum Type
		{
			Strategy,
			Deathmatch,
			InstantAction,
			Unknown
		}
		
		// path to folder containing map files
		public string bznPath;

		// name part of the file name (no file extension)
		public string filename;

		public string author;
		public string version;
		public string type;

		public string world;
		public string size;
		public bool powerups;
		public uint geysers;
		public uint scrap;

		public Map()
		{
			bznPath = null;
			filename = null;
			author = "Unknown";
			version = "1.0";
			type = "Unknown";
			world = "Unknown";
			size = "Unknown";
			powerups = false;
			geysers = 0;
			scrap = 0;
		}

		public ImageSource Image()
		{
			string imgPath = bznPath + "/" + filename + ".bmp";

			if(File.Exists(imgPath))
				return new BitmapImage(new Uri(imgPath, UriKind.Absolute));
			else
				return null;
		}

		public string Objective()
		{
			string objFilePath = bznPath + "/" + filename + ".otf";

			if(File.Exists(objFilePath))
			{
				string objective = null;

				using(StreamReader sr = new StreamReader(objFilePath))
				{
					objective = sr.ReadToEnd();
				}

				return objective;
			}
			else
			{
				return null;
			}
		}

		public int CompareTo(object obj)
		{
			Map other = (Map)obj;
			return CompareTo(other);
		}

		public int CompareTo(Map other)
		{
			return filename.CompareTo(other.filename);
		}

		public override string ToString()
		{
			return filename;
		}
	}
}