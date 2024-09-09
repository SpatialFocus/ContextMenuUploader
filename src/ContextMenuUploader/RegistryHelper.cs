// <copyright file="RegistryHelper.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace ContextMenuUploader;

using Microsoft.Win32;

public static class RegistryHelper
{
	public static void AddContextMenu(string appName, string appPath, string contextMenuLabel)
	{
		try
		{
			// Add context menu for files (*\shell)
			string regFilePath = @"*\shell\" + appName;
			RegistryHelper.CreateRegistryKey(regFilePath, appPath, contextMenuLabel);

			// Add context menu for folders (Directory\shell)
			string regFolderPath = @"Directory\shell\" + appName;
			RegistryHelper.CreateRegistryKey(regFolderPath, appPath, contextMenuLabel);

			// Set the MultipleInvokePromptMinimum value to 256
			RegistryHelper.SetMultipleInvokeLimit(256);

			Console.WriteLine("Context menu option added for up to 256 files and folders.");
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

			// Reset the MultipleInvokePromptMinimum value to 15
			RegistryHelper.SetMultipleInvokeLimit(15);

			Console.WriteLine("Context menu option removed for files and folders.");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error removing context menu entry: {ex.Message}");
		}
	}

	public static void SetMultipleInvokeLimit(int limit)
	{
		try
		{
			// Open the registry key
			using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer", true);

			// Set the "MultipleInvokePromptMinimum" value to the provided limit
			key?.SetValue("MultipleInvokePromptMinimum", limit, RegistryValueKind.DWord);

			Console.WriteLine($"Successfully set MultipleInvokePromptMinimum to {limit}");
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error setting registry value: {ex.Message}");
		}
	}

	private static void CreateRegistryKey(string regPath, string appPath, string contextMenuLabel)
	{
		// Create or update the registry key
		using RegistryKey key = Registry.ClassesRoot.OpenSubKey(regPath, true) ?? Registry.ClassesRoot.CreateSubKey(regPath);

		// Set the context menu label
		key.SetValue(string.Empty, contextMenuLabel);

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