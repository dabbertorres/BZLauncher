using Microsoft.Win32;

namespace BZLauncher
{
	public static class FindBzExeDialog
	{
		private static OpenFileDialog dialog = new OpenFileDialog();

		static FindBzExeDialog()
		{
			dialog.DefaultExt = "bzone.exe";
			dialog.CheckPathExists = true;
			dialog.DereferenceLinks = true;
			dialog.Filter = "BZ Executable|bzone.exe";
			dialog.Title = "bzone.exe location";
		}

		public static string GetPath()
		{
			bool? result = dialog.ShowDialog();

			if(result == true)
				return dialog.FileName;
			else
				return null;
		}
	}
}
