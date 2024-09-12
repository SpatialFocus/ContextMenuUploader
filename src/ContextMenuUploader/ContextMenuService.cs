// <copyright file="ContextMenuService.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace ContextMenuUploader;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class ContextMenuService(ILogger<ContextMenuService> logger, IOptions<ToolOptions> options, IServiceProvider serviceProvider)
{
	private const string MutexName = "Global\\ContextMenuUploader";

	private readonly string tempFilePath = Path.Combine(Path.GetTempPath(), "ContextMenuUploader");
	private readonly ToolOptions toolOptions = options.Value;

	public async Task ExecuteAsync(string[] args, CancellationToken stoppingToken = default)
	{
		logger.LogInformation("Started context menu service...");

		// Ensure the temporary directory exists
		Directory.CreateDirectory(this.tempFilePath);

		// Add new paths to a temporary file
		string randomFile = Path.Combine(this.tempFilePath, Path.GetRandomFileName());
		await File.AppendAllLinesAsync(randomFile, args, stoppingToken);

		using Mutex mutex = new(true, ContextMenuService.MutexName, out bool createdNew);

		if (!createdNew)
		{
			// Another instance has the mutex and is already running, so we're done
			return;
		}

		// This instance is the first one to run, collect data and upload the batch
		try
		{
			// Waiting if more files/folders are selected
			await Task.Delay(this.toolOptions.BatchTimeoutMs, stoppingToken);

			int previousCount, nextCount = Directory.GetFiles(this.tempFilePath).Length;

			// Wait until the number stops increasing
			do
			{
				previousCount = nextCount;
				await Task.Delay(this.toolOptions.BatchTimeoutMs, stoppingToken);
				nextCount = Directory.GetFiles(this.tempFilePath).Length;
			}
			while (previousCount != nextCount);

			List<string> allPaths = [];

			// Get all paths from the temporary files
			if (Directory.Exists(this.tempFilePath))
			{
				foreach (string file in Directory.GetFiles(this.tempFilePath))
				{
					allPaths.AddRange(await File.ReadAllLinesAsync(file, stoppingToken));
				}

				Directory.Delete(this.tempFilePath, true);
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
}