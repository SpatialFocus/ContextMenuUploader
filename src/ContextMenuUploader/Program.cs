// <copyright file="Program.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace ContextMenuUploader;

using System.Reflection;

internal class Program
{
	private static void Main(string[] args)
	{
		const string appName = "ContextMenuUploader";
		string appPath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");

		switch (args.Length)
		{
			case > 0 when args[0] == "--register":
				RegistryHelper.AddContextMenu(appName, appPath);
				return;
			case > 0 when args[0] == "--unregister":
				RegistryHelper.RemoveContextMenu(appName);
				return;
			case 0:
				Console.WriteLine("USAGE:");
				Console.WriteLine("  ContextMenuUploader --register      Register the context menu item for files and folders");
				Console.WriteLine("  ContextMenuUploader --unregister    Unregister the context menu item for files and folders");
				return;
		}

		string path = args[0];

		if (File.Exists(path))
		{
			Program.WriteDebugMessage($"Uploading file: {path}");

			// TODO: Handle the actual upload of a single file
			////Uploader uploader = new Uploader();
			////uploader.UploadFile(path).Wait();
		}
		else if (Directory.Exists(path))
		{
			Program.WriteDebugMessage($"Uploading folder: {path}");

			// TODO: Handle the actual upload of all files in the folder
			////foreach (string file in Directory.EnumerateFiles(path))
			////{
			////	Uploader uploader = new Uploader();
			////	uploader.UploadFile(file).Wait();
			////}
		}
		else
		{
			Program.WriteDebugMessage($"File/Folder does not exist: {path}");
		}
	}

	private static void WriteDebugMessage(string message)
	{
		const string debugFilePath = @"C:\temp";
		const string debugFileName = "ContextMenuUploader.log";

		if (!Directory.Exists(debugFilePath))
		{
			return;
		}

		try
		{
			string debugFile = Path.Combine(debugFilePath, debugFileName);
			File.AppendAllLines(debugFile, [$"{DateTime.Now:yyyy-MM-dd HH:mm:ss} -> {message}",]);
		}
		catch (IOException exception)
		{
			Console.WriteLine(exception);
		}
	}
}