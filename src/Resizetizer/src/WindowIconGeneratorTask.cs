﻿using System;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Uno.Resizetizer;

public class WindowIconGeneratorTask_V0 : Task
{
	private const string FileName = "Uno.Resizetizer.WindowIconExtensions.g.cs";

	public ITaskItem[] UnoIcons { get; set; }

	[Required]
	public string IntermediateOutputDirectory { get; set; }

	public string WindowTitle { get; set; }

	[Output]
	public ITaskItem[] GeneratedClass { get; private set; } = Array.Empty<ITaskItem>();

	public override bool Execute()
	{
		if (UnoIcons is null || UnoIcons.Length == 0)
		{
			return true;
		}

		if(string.IsNullOrEmpty(IntermediateOutputDirectory))
		{
			Log.LogError("The IntermediateOutputDirectory (typically the obj directory) is a required parameter but was null or empty.");
			return false;
		}

		var iconPath = UnoIcons[0].ItemSpec;
		var iconName = Path.GetFileNameWithoutExtension(iconPath);

		var code = @$"//------------------------------------------------------------------------------
// <auto-generated>
//  This code was auto-generated.
//
//  Changes to this file may cause incorrect behavior and will be lost if
//  the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Uno.Resizetizer
{{
	/// <summary>
	/// Extension methods for the <see cref=""global::Microsoft.UI.Xaml.Window"" /> class.
	/// </summary>
	public static class WindowExtensions
	{{
		/// <summary>
		/// This will set the Window Icon for the given <see cref=""global::Microsoft.UI.Xaml.Window"" /> using
		/// the provided UnoIcon.
		/// </summary>
		public static void SetWindowIcon(this global::Microsoft.UI.Xaml.Window window)
		{{
#if WINDOWS && !HAS_UNO
			var hWnd =
			global::WinRT.Interop.WindowNative.GetWindowHandle(window);

			// Retrieve the WindowId that corresponds to hWnd.
			global::Microsoft.UI.WindowId windowId =
			global::Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);

			// Lastly, retrieve the AppWindow for the current (XAML) WinUI 3 window.
			global::Microsoft.UI.Windowing.AppWindow appWindow =
				global::Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
			appWindow.SetIcon(""{iconName}.ico"");

			// Set the Window Title Only if it has the Default WinUI Desktop value and we are running Unpackaged
			if (!IsPackaged() && appWindow.Title == ""WinUI Desktop"")
			{{
				appWindow.Title = ""{WindowTitle}"";
			}}

			static bool IsPackaged()
			{{
				try
				{{
					if (global::Windows.ApplicationModel.Package.Current != null)
						return true;
				}}
				catch
				{{
					// no-op
				}}

				return false;
			}}
#endif
		}}
	}}
}}";

		if(!Directory.Exists(IntermediateOutputDirectory))
		{
			Directory.CreateDirectory(IntermediateOutputDirectory);
		}

		var item = new TaskItem(Path.Combine(IntermediateOutputDirectory, FileName));
		File.WriteAllText(item.ItemSpec, code);
		GeneratedClass = new [] { item };
		return true;
    }
}
