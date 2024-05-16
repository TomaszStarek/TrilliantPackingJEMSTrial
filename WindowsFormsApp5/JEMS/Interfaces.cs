using System.Collections.Generic;

namespace WindowsFormsApp5
{
	interface ILogInformation
    {
		string FileExtension
		{
			get ;
			set;
		}
	}

	interface IReadLog
	{
		LogStructur ReadFile(string logPath);
	}

	interface IReadMultipleLog
	{
		List<LogStructur> ReadFile(string logPath);
	}

	interface IWriteLog
	{
		void SaveFile(string path);
	}
}
