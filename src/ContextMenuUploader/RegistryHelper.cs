// <copyright file="RegistryHelper.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace ContextMenuUploader;

using Microsoft.Win32;

public static class RegistryHelper
{
	public static void AddContextMenu(string appName, string appPath)
	{
		try
		{
			// Add context menu for files (*\shell)
			string regFilePath = @"*\shell\" + appName;
			RegistryHelper.CreateRegistryKey(regFilePath, appPath);

			// Add context menu for folders (Directory\shell)
			string regFolderPath = @"Directory\shell\" + appName;
			RegistryHelper.CreateRegistryKey(regFolderPath, appPath);

			Console.WriteLine("Context menu option added for files and folders.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error adding context menu entry: {ex.Message}");
		}
	}

	public static void RemoveContextMenu(string appName)
	{
		try
		{
			// Remove context menu for files (*\shell)
			string regFilePath = @"*\shell\" + appName;
			Registry.ClassesRoot.DeleteSubKeyTree(regFilePath, false);

			// Remove context menu for folders (Directory\shell)
			string regFolderPath = @"Directory\shell\" + appName;
			Registry.ClassesRoot.DeleteSubKeyTree(regFolderPath, false);

			Console.WriteLine("Context menu option removed for files and folders.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error removing context menu entry: {ex.Message}");
		}
	}

	private static void CreateRegistryKey(string regPath, string appPath)
	{
		using RegistryKey key = Registry.ClassesRoot.CreateSubKey(regPath);

		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (key == null)
		{
			return;
		}

		// The text to display in the context menu
		key.SetValue(string.Empty, "Upload to web service");

		// Add the icon
		key.SetValue("Icon", $"{appPath},0");

		// Create the command key (to set the path to your app)
		using RegistryKey commandKey = key.CreateSubKey("command");

		// ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
		if (commandKey != null)
		{
			// Set the command to run the application with the file/folder path argument
			commandKey.SetValue(string.Empty, $"\"{appPath}\" \"%1\"");
		}
	}
}