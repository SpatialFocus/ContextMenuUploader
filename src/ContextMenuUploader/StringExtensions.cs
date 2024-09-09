// <copyright file="StringExtensions.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace ContextMenuUploader;

public static class StringExtensions
{
	/// <summary>
	///     Writes to a file in a thread-safe manner using a Mutex lock.
	///     Overwrites files by default.
	/// </summary>
	/// <remarks>Inspired by https://stackoverflow.com/a/229567 .</remarks>
	/// <param name="output">Input string to write to the file.</param>
	/// <param name="filePath">Path of file to write to.</param>
	/// <param name="overwrite">Whether to overwrite pre-existing files.</param>
	public static void SafelyWriteLinesToFile(this string[] output, string filePath, bool overwrite = true)
	{
		// Unique name for global mutex - Global prefix means it is global to the machine
		// We use filePath to ensure the mutex is only held for the particular file
		string mutexName = $"Global\\{{{Path.GetFileNameWithoutExtension(filePath)}}}";

		// We create/query the Mutex
		using Mutex mutex = new(false, mutexName);

		bool hasHandle = false;

		try
		{
			// We wait for lock to release
			hasHandle = mutex.WaitOne(Timeout.Infinite, false);

			// Write to file
			if (overwrite)
			{
				File.WriteAllLines(filePath, output);
			}
			else
			{
				File.AppendAllLines(filePath, output);
			}
		}
		finally
		{
			// If we have the Mutex, we release it
			if (hasHandle)
			{
				mutex.ReleaseMutex();
			}
		}
	}
}