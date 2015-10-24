﻿using Microsoft.Win32;

namespace Directories
{
	public static class User
	{
		public class Directory
		{
			public readonly string guid;
			
			public static Directory Contacts = new Directory("{56784854-C6CB-462B-8169-88E350ACB882}");
			public static Directory Desktop = new Directory("{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}");
			public static Directory Documents = new Directory("{FDD39AD0-238F-46AF-ADB4-6C85480369C7}");
			public static Directory Downloads = new Directory("{374DE290-123F-4565-9164-39C4925E467B}");
			public static Directory Favorites = new Directory("{1777F761-68AD-4D8A-87BD-30B759FA33DD}");
			public static Directory Links = new Directory("{BFB9D5E0-C6A9-404C-B2B2-AE6DB6AF4968}");
			public static Directory Music = new Directory("{4BD8D571-6D19-48D3-BE97-422220080E43}");
			public static Directory Pictures = new Directory("{33E28130-4E1E-4676-835A-98395C3BC3BB}");
			public static Directory SavedGaems = new Directory("{4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4}");
			public static Directory SavedSearches = new Directory("{7D1D3A04-DEBB-4115-95CF-2F29DA2920DA}");
			public static Directory Videos = new Directory("{18989B1D-99B5-455B-841C-AB7C74E4DDFC}");

			private Directory(string guid)
			{
				this.guid = guid;
			}
		}

		public static string get(Directory dir)
		{
			return (Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Shell Folders", dir.guid, null) as string) + '\\';
		}
	}
}
