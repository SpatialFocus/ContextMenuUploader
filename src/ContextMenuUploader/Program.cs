// <copyright file="Program.cs" company="Spatial Focus GmbH">
// Copyright (c) Spatial Focus GmbH. All rights reserved.
// </copyright>

namespace ContextMenuUploader;

using System.Diagnostics;
using System.Security.Principal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

public class Program
{
	private const string AppName = "ContextMenuUploader";
	private const string ContextMenuLabel = "Upload to web service";

	[STAThread]
	public static async Task Main(string[] args)
	{
		Application.SetHighDpiMode(HighDpiMode.SystemAware);
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(false);

		IHost host = Host.CreateDefaultBuilder(args)
			.ConfigureServices((services, config) =>
			{
				config.Configure<ToolOptions>(services.Configuration.GetSection("Options"));
				config.AddTransient<ContextMenuService>();
				config.AddSingleton<RegistryService>();
			})
			.Build();

		ToolOptions toolOptions = host.Services.GetRequiredService<IOptions<ToolOptions>>().Value;
		RegistryService registry = host.Services.GetRequiredService<RegistryService>();

		if (args.Length == 0)
		{
			if (registry.VerifyRegistration(Program.AppName))
			{
				if (MessageBox.Show("Do you want to remove the context menu entry?", "Context Menu Uploader", MessageBoxButtons.YesNo) ==
					DialogResult.Yes)
				{
					Program.RegisterOrUnregister(registry, "--unregister");
				}
			}
			else
			{
				if (MessageBox.Show("Do you want to register the context menu entry?", "Context Menu Uploader", MessageBoxButtons.YesNo) ==
					DialogResult.Yes)
				{
					Program.RegisterOrUnregister(registry, "--register", toolOptions.MultipleInvokeLimit);
				}
			}
		}
		else if (args[0] == "--register" || args[0] == "--unregister")
		{
			Program.RegisterOrUnregister(registry, args[0], toolOptions.MultipleInvokeLimit);
		}
		else
		{
			ContextMenuService service = host.Services.GetRequiredService<ContextMenuService>();
			await service.ExecuteAsync(args);
		}
	}

	private static bool IsUserAdministrator()
	{
		using WindowsIdentity identity = WindowsIdentity.GetCurrent();

		WindowsPrincipal principal = new(identity);
		return principal.IsInRole(WindowsBuiltInRole.Administrator);
	}

	private static void RegisterOrUnregister(RegistryService registry, string parameter, int multipleInvokeLimit = 0)
	{
		if (!Program.IsUserAdministrator())
		{
			// Restart the application with administrative rights
			ProcessStartInfo processInfo = new()
			{
				UseShellExecute = true, FileName = Application.ExecutablePath, Verb = "runas", Arguments = parameter,
			};

			try
			{
				Process.Start(processInfo);
			}
			catch (Exception)
			{
				MessageBox.Show("Admin privileges are required to modify the registry.", "Context Menu Uploader", MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			}

			return;
		}

		if (parameter == "--register")
		{
			bool result = registry.AddContextMenu(Program.AppName, Application.ExecutablePath, Program.ContextMenuLabel,
				multipleInvokeLimit);

			if (result)
			{
				MessageBox.Show("Context menu entry was added.", "Context Menu Uploader", MessageBoxButtons.OK, MessageBoxIcon.Information);
			}
		}
		else
		{
			bool result = registry.RemoveContextMenu(Program.AppName);

			if (result)
			{
				MessageBox.Show("Context menu entry was removed.", "Context Menu Uploader", MessageBoxButtons.OK,
					MessageBoxIcon.Information);
			}
		}
	}
}