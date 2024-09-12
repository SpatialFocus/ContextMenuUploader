// <copyright file="RegistryService.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace ContextMenuUploader;

using Microsoft.Win32;

public class RegistryService
{
	public bool AddContextMenu(string appName, string appPath, string contextMenuLabel, int multipleInvokeLimit)
	{
		try
		{
			// Add context menu for files (*\shell)
			string regFilePath = @"*\shell\" + appName;
			CreateRegistryKey(regFilePath, appPath, contextMenuLabel);

			// Add context menu for folders (Directory\shell)
			string regFolderPath = @"Directory\shell\" + appName;
			CreateRegistryKey(regFolderPath, appPath, contextMenuLabel);

			// Set the MultipleInvokePromptMinimum value
			SetMultipleInvokeLimit(multipleInvokeLimit);

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public bool RemoveContextMenu(string appName)
	{
		try
		{
			// Remove context menu for files (*\shell)
			string regFilePath = @"*\shell\" + appName;
			Registry.ClassesRoot.DeleteSubKeyTree(regFilePath, false);

			// Remove context menu for folders (Directory\shell)
			string regFolderPath = @"Directory\shell\" + appName;
			Registry.ClassesRoot.DeleteSubKeyTree(regFolderPath, false);

			// Reset the MultipleInvokePromptMinimum value
			SetMultipleInvokeLimit();

			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public bool VerifyRegistration(string appName)
	{
		// Check if the registry key exists
		string regFilePath = @"*\shell\" + appName;
		string regFolderPath = @"Directory\shell\" + appName;

		return Registry.ClassesRoot.OpenSubKey(regFilePath) != null && Registry.ClassesRoot.OpenSubKey(regFolderPath) != null;
	}

	protected void CreateRegistryKey(string regPath, string appPath, string contextMenuLabel)
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

	protected void SetMultipleInvokeLimit(int? limit = null)
	{
		try
		{
			// Open the registry key
			using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer", true);

			if (limit.HasValue)
			{
				// Set the "MultipleInvokePromptMinimum" value to the provided limit
				key?.SetValue("MultipleInvokePromptMinimum", limit, RegistryValueKind.DWord);
			}
			else
			{
				// Reset the "MultipleInvokePromptMinimum"
				key?.DeleteValue("MultipleInvokePromptMinimum");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Error setting registry value: {ex.Message}");
		}
	}
}