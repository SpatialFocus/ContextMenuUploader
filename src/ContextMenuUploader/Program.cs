// <copyright file="Program.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace ContextMenuUploader;

using System.Reflection;

public class Program
{
	private const string AppName = "ContextMenuUploader";
	private const int BatchTimeoutMs = 1000; // Timeout in milliseconds to wait for more files to be selected
	private const string ContextMenuLabel = "Upload to web service";
	private const string MutexName = "Global\\ContextMenuUploader";
	private static readonly string TempFilePath = Path.Combine(Path.GetTempPath(), "ContextMenuUploader.txt");

	[STAThread]
	public static async Task Main(string[] args)
	{
		Application.SetHighDpiMode(HighDpiMode.SystemAware);
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		string appPath = Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");

		switch (args.Length)
		{
			case > 0 when args[0] == "--register":
				RegistryHelper.AddContextMenu(Program.AppName, appPath, Program.ContextMenuLabel);
				return;
			case > 0 when args[0] == "--unregister":
				RegistryHelper.RemoveContextMenu(Program.AppName);
				return;
			case 0:
				string helpText = "This is a context menu tool.\n\n" + "Register the context menu item for files and folders:\n" +
					"  ContextMenuUploader --register\n\n" + "Unregister the context menu item for files and folders:\n" +
					"  ContextMenuUploader --unregister";
				MessageBox.Show(helpText, "Context Menu Uploader", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
		}

		await Program.HandleFileActionAsync(args);
	}

	private static async Task HandleFileActionAsync(string[] newPaths)
	{
		// Add new paths to the temporary file
		newPaths.SafelyWriteLinesToFile(Program.TempFilePath, false);

		using Mutex mutex = new(true, Program.MutexName, out bool createdNew);

		if (createdNew)
		{
			// This instance is the first one to run
			try
			{
				// Waiting if more files/folders are selected
				await Task.Delay(Program.BatchTimeoutMs);

				int previousCount, nextCount = (await File.ReadAllLinesAsync(Program.TempFilePath)).Length;

				// Wait until the number stops increasing
				do
				{
					previousCount = nextCount;
					await Task.Delay(Program.BatchTimeoutMs);
					nextCount = (await File.ReadAllLinesAsync(Program.TempFilePath)).Length;
				}
				while (previousCount != nextCount);

				List<string> allPaths = new();

				// Get all paths from the temporary file
				if (File.Exists(Program.TempFilePath))
				{
					allPaths = (await File.ReadAllLinesAsync(Program.TempFilePath)).ToList();
					File.Delete(Program.TempFilePath);
				}

				int count = allPaths.Count;

				if (count > 0)
				{
					string message = $"Sending {count} files/folders to web service.";

					if (count <= 10)
					{
						message += "\n\n" + string.Join("\n", allPaths);
					}

					MessageBox.Show(message, "Context Menu Uploader", MessageBoxButtons.OK, MessageBoxIcon.Information);

					// TODO: Handle the actual upload of all files/folders selected
					////await SendFilePathsToApiAsync(allPaths);
				}
			}
			finally
			{
				mutex.ReleaseMutex();
			}
		}

		// Another instance has the mutex and is already running, so we're done
	}
}