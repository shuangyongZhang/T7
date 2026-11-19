using System;
using System.IO;
using UnityEngine;
#if UNITY_2018_4_OR_NEWER && !NATIVE_FILE_PICKER_DISABLE_ASYNC_FUNCTIONS
using System.Threading.Tasks;
#endif
#if UNITY_ANDROID || UNITY_IOS
using NativeFilePickerNamespace;
#endif
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
using System.Windows.Forms;
#endif

public static class NativeFilePicker
{
	public delegate void PermissionCallback(Permission permission);
	public delegate void FilePickedCallback(string path);
	public delegate void MultipleFilesPickedCallback(string[] paths);
	public delegate void FilesExportedCallback(bool success);

	public enum Permission { Denied = 0, Granted = 1, ShouldAsk = 2 };

	#region Platform Specific Elements
#if !UNITY_EDITOR && UNITY_ANDROID
	private static AndroidJavaClass m_ajc = null;
	private static AndroidJavaClass AJC
	{
		get
		{
			if( m_ajc == null )
				m_ajc = new AndroidJavaClass( "com.yasirkula.unity.NativeFilePicker" );

			return m_ajc;
		}
	}

	private static AndroidJavaObject m_context = null;
	private static AndroidJavaObject Context
	{
		get
		{
			if( m_context == null )
			{
				using( AndroidJavaObject unityClass = new AndroidJavaClass( "com.unity3d.player.UnityPlayer" ) )
				{
					m_context = unityClass.GetStatic<AndroidJavaObject>( "currentActivity" );
				}
			}

			return m_context;
		}
	}
#elif !UNITY_EDITOR && UNITY_IOS
	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern int _NativeFilePicker_CanPickMultipleFiles();

	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern string _NativeFilePicker_ConvertExtensionToUTI( string extension );

	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern void _NativeFilePicker_PickFile( string[] UTIs, int UTIsCount );

	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern void _NativeFilePicker_PickMultipleFiles( string[] UTIs, int UTIsCount );

	[System.Runtime.InteropServices.DllImport( "__Internal" )]
	private static extern void _NativeFilePicker_ExportFiles( string[] files, int filesCount );
#endif
	#endregion

#if !UNITY_EDITOR && UNITY_ANDROID
	private static string m_selectedFilePath = null;
	private static string SelectedFilePath
	{
		get
		{
			if( m_selectedFilePath == null )
			{
				m_selectedFilePath = Path.Combine( Application.temporaryCachePath, "pickedFile" );
				Directory.CreateDirectory( Application.temporaryCachePath );
			}

			return m_selectedFilePath;
		}
	}
#endif

	#region Runtime Permissions
	public static Permission CheckPermission(bool readPermissionOnly = false)
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		Permission result = (Permission) AJC.CallStatic<int>( "CheckPermission", Context, readPermissionOnly );
		if( result == Permission.Denied && (Permission) PlayerPrefs.GetInt( "NativeFilePickerPermission", (int) Permission.ShouldAsk ) == Permission.ShouldAsk )
			result = Permission.ShouldAsk;

		return result;
#else
		return Permission.Granted;
#endif
	}

	public static Permission RequestPermission(bool readPermissionOnly = false)
	{
		// Don't block the main thread if the permission is already granted
		if (CheckPermission(readPermissionOnly) == Permission.Granted)
			return Permission.Granted;

#if !UNITY_EDITOR && UNITY_ANDROID
		object threadLock = new object();
		lock( threadLock )
		{
			FPPermissionCallbackAndroid nativeCallback = new FPPermissionCallbackAndroid( threadLock );

			AJC.CallStatic( "RequestPermission", Context, nativeCallback, readPermissionOnly, (int) Permission.ShouldAsk );

			if( nativeCallback.Result == -1 )
				System.Threading.Monitor.Wait( threadLock );

			if( (Permission) nativeCallback.Result != Permission.ShouldAsk && PlayerPrefs.GetInt( "NativeFilePickerPermission", -1 ) != nativeCallback.Result )
			{
				PlayerPrefs.SetInt( "NativeFilePickerPermission", nativeCallback.Result );
				PlayerPrefs.Save();
			}

			return (Permission) nativeCallback.Result;
		}
#else
		return Permission.Granted;
#endif
	}

	public static void RequestPermissionAsync(PermissionCallback callback, bool readPermissionOnly = false)
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		FPPermissionCallbackAsyncAndroid nativeCallback = new FPPermissionCallbackAsyncAndroid( callback );
		AJC.CallStatic( "RequestPermission", Context, nativeCallback, readPermissionOnly, (int) Permission.ShouldAsk );
#else
		callback(Permission.Granted);
#endif
	}

#if UNITY_2018_4_OR_NEWER && !NATIVE_FILE_PICKER_DISABLE_ASYNC_FUNCTIONS
	public static Task<Permission> RequestPermissionAsync(bool readPermissionOnly = false)
	{
		TaskCompletionSource<Permission> tcs = new TaskCompletionSource<Permission>();
		RequestPermissionAsync((permission) => tcs.SetResult(permission), readPermissionOnly);
		return tcs.Task;
	}
#endif

	public static void OpenSettings()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		AJC.CallStatic( "OpenSettings", Context );
#endif
	}
	#endregion

	#region Helper Functions
	public static bool CanPickMultipleFiles()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		return AJC.CallStatic<bool>( "CanPickMultipleFiles" );
#elif !UNITY_EDITOR && UNITY_IOS
		return _NativeFilePicker_CanPickMultipleFiles() == 1;
#else
		return false;
#endif
	}

	public static bool CanExportFiles()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return true;
#elif UNITY_ANDROID
		return AJC.CallStatic<bool>( "CanExportFiles" );
#elif UNITY_IOS
		return true;
#else
		return false;
#endif
	}

	public static bool CanExportMultipleFiles()
	{
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
		return true;
#elif UNITY_ANDROID
		return AJC.CallStatic<bool>( "CanExportMultipleFiles" );
#elif UNITY_IOS
		return _NativeFilePicker_CanPickMultipleFiles() == 1;
#else
		return false;
#endif
	}

	public static bool IsFilePickerBusy()
	{
#if !UNITY_EDITOR && UNITY_IOS
		return FPResultCallbackiOS.IsBusy;
#else
		return false;
#endif
	}

	public static string ConvertExtensionToFileType(string extension)
	{
		if (string.IsNullOrEmpty(extension))
			return null;

		if (extension.IndexOf('*') >= 0)
		{
			// So many users try to do this that it's now necessary to throw an exception for this particular scenario
			throw new ArgumentException("See: https://github.com/yasirkula/UnityNativeFilePicker#faq");
		}

#if !UNITY_EDITOR && UNITY_ANDROID
		return AJC.CallStatic<string>( "GetMimeTypeFromExtension", extension.ToLowerInvariant() );
#elif !UNITY_EDITOR && UNITY_IOS
		return _NativeFilePicker_ConvertExtensionToUTI( extension.ToLowerInvariant() );
#else
		return extension;
#endif
	}

#if UNITY_STANDALONE_WIN
	private static string BuildWinFilter(string[] allowedFileTypes)
	{
		if (allowedFileTypes == null || allowedFileTypes.Length == 0)
			return "All files|*.*";

		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		for (int i = 0; i < allowedFileTypes.Length; i++)
		{
			string type = allowedFileTypes[i];
			if (type.IndexOf('*') >= 0)
			{
				if (type == "image/*")
				{
					sb.Append("Image files|*.png;*.jpg;*.jpeg|");
				}
				else if (type == "video/*")
				{
					sb.Append("Video files|*.mp4;*.mov;*.webm;*.avi|");
				}
				else if (type == "audio/*")
				{
					sb.Append("Audio files|*.mp3;*.wav;*.aac;*.flac|");
				}
				else
				{
					sb.Append("All files|*.*|");
				}
			}
			else
			{
				string ext = type;
				if (type.IndexOf('/') >= 0)
					ext = type.Substring(type.IndexOf('/') + 1);
				else if (type.StartsWith("public."))
					ext = type.Substring(7);
				else if (type.IndexOf('.') == 0)
					ext = type.Substring(1);

				sb.Append(ext.ToUpper() + " files|*." + ext + "|");
			}
		}
		sb.Append("All files|*.*");
		return sb.ToString();
	}
#endif
	#endregion

	#region Import Functions
	public static Permission PickFile(FilePickedCallback callback, params string[] allowedFileTypes)
	{
		// If no file type is specified, allow all file types
		if (allowedFileTypes == null || allowedFileTypes.Length == 0)
		{
#if UNITY_ANDROID
			allowedFileTypes = new string[] { "*/*" };
#else
			allowedFileTypes = new string[] { "public.item", "public.content" };
#endif
		}

		Permission result = RequestPermission(true);
		if (result == Permission.Granted && !IsFilePickerBusy())
		{
#if UNITY_EDITOR
			// Accept Android and iOS UTIs when possible, for user's convenience
			string[] editorFilters = new string[allowedFileTypes.Length * 2];
			for (int i = 0; i < allowedFileTypes.Length; i++)
			{
				if (allowedFileTypes[i].IndexOf('*') >= 0)
				{
					if (allowedFileTypes[i] == "image/*")
					{
						editorFilters[i * 2] = "Image files";
						editorFilters[i * 2 + 1] = "png,jpg,jpeg";
					}
					else if (allowedFileTypes[i] == "video/*")
					{
						editorFilters[i * 2] = "Video files";
						editorFilters[i * 2 + 1] = "mp4,mov,webm,avi";
					}
					else if (allowedFileTypes[i] == "audio/*")
					{
						editorFilters[i * 2] = "Audio files";
						editorFilters[i * 2 + 1] = "mp3,wav,aac,flac";
					}
					else
					{
						editorFilters[i * 2] = "All files";
						editorFilters[i * 2 + 1] = "*";
					}
				}
				else
				{
					editorFilters[i * 2] = allowedFileTypes[i];

					if (allowedFileTypes[i].IndexOf('/') >= 0) // Android UTIs like 'image/png'
						editorFilters[i * 2 + 1] = allowedFileTypes[i].Substring(allowedFileTypes[i].IndexOf('/') + 1);
					else if (allowedFileTypes[i].StartsWith("public.")) // iOS UTIs like 'public.png'
						editorFilters[i * 2 + 1] = allowedFileTypes[i].Substring(7);
					else if (allowedFileTypes[i].IndexOf('.') == 0) // Extensions starting with period like '.png'
						editorFilters[i * 2 + 1] = allowedFileTypes[i].Substring(1);
					else
						editorFilters[i * 2 + 1] = allowedFileTypes[i];
				}
			}

			string pickedFile = UnityEditor.EditorUtility.OpenFilePanelWithFilters("Select file", "", editorFilters);

			if (callback != null)
				callback(pickedFile != "" ? pickedFile : null);
#elif UNITY_STANDALONE_WIN
			using (OpenFileDialog dialog = new OpenFileDialog())
			{
				dialog.Title = "Select file";
				dialog.Filter = BuildWinFilter(allowedFileTypes);
				if (dialog.ShowDialog() == DialogResult.OK && callback != null)
					callback(dialog.FileName);
				else if (callback != null)
					callback(null);
			}
#elif UNITY_ANDROID
			AJC.CallStatic( "PickFiles", Context, new FPResultCallbackAndroid( callback, null, null ), false, SelectedFilePath, allowedFileTypes, "" );
#elif UNITY_IOS
			FPResultCallbackiOS.Initialize( callback, null, null );
			_NativeFilePicker_PickFile( allowedFileTypes, allowedFileTypes.Length );
#else
			if( callback != null )
				callback( null );
#endif
		}

		return result;
	}

	public static Permission PickMultipleFiles(MultipleFilesPickedCallback callback, params string[] allowedFileTypes)
	{
		// If no file type is specified, allow all file types
		if (allowedFileTypes == null || allowedFileTypes.Length == 0)
		{
#if UNITY_ANDROID
			allowedFileTypes = new string[] { "*/*" };
#else
			allowedFileTypes = new string[] { "public.item", "public.content" };
#endif
		}

		Permission result = RequestPermission(true);
		if (result == Permission.Granted && !IsFilePickerBusy())
		{
			if (CanPickMultipleFiles())
			{
#if !UNITY_EDITOR && UNITY_ANDROID
				AJC.CallStatic( "PickFiles", Context, new FPResultCallbackAndroid( null, callback, null ), true, SelectedFilePath, allowedFileTypes, "" );
#elif !UNITY_EDITOR && UNITY_IOS
				FPResultCallbackiOS.Initialize( null, callback, null );
				_NativeFilePicker_PickMultipleFiles( allowedFileTypes, allowedFileTypes.Length );
#endif
			}
			else if (callback != null)
				callback(null);
		}

		return result;
	}
	#endregion

	#region Export Functions
	public static Permission ExportFile(string filePath, FilesExportedCallback callback = null)
	{
		if (string.IsNullOrEmpty(filePath))
			throw new ArgumentException("Parameter 'filePath' is null or empty!");

		Permission result = RequestPermission(false);
		if (result == Permission.Granted && !IsFilePickerBusy())
		{
			if (CanExportFiles())
			{
#if UNITY_EDITOR
				string extension = Path.GetExtension(filePath);
				if (extension == null)
					extension = "";
				else if (extension.IndexOf('.') == 0)
					extension = extension.Substring(1);

				string destination = UnityEditor.EditorUtility.SaveFilePanel("Select destination", Path.GetDirectoryName(filePath), Path.GetFileName(filePath), extension);
				if (string.IsNullOrEmpty(destination))
				{
					if (callback != null)
						callback(false);
				}
				else
				{
					if (File.Exists(destination))
					{
						if (IsFileInUse(destination))
						{
							if (callback != null)
								callback(false);
						}
						else
						{
							if (callback != null)
								callback(true);
						}
					}
					else
					{
						try
						{
							File.Copy(filePath, destination, true);

							if (callback != null)
								callback(true);
						}
						catch (Exception e)
						{
							Debug.LogException(e);

							if (callback != null)
								callback(false);
						}
					}

				}
#elif UNITY_STANDALONE_WIN
				string winExt = Path.GetExtension(filePath);
				if (winExt == null) winExt = "";
				else if (winExt.IndexOf('.') == 0) winExt = winExt.Substring(1);

				using (SaveFileDialog sfd = new SaveFileDialog())
				{
					sfd.Title = "选择目录";
					sfd.InitialDirectory = Path.GetDirectoryName(filePath);
					sfd.FileName = Path.GetFileName(filePath);
					sfd.Filter = winExt.ToUpper() + " files|*." + winExt + "|All files|*.*";
					if (sfd.ShowDialog() == DialogResult.OK)
					{
						try
						{
							if (File.Exists(sfd.FileName))
							{
								if (IsFileInUse(sfd.FileName))
								{
									if (callback != null)
									 	callback(false);
								}
								else if (callback != null) callback(true);
							}
							else
							{
								File.Copy(filePath, sfd.FileName, true);
								if (callback != null) callback(true);
							}
						}
						catch (Exception e)
						{
							Debug.LogException(e);
							if (callback != null) callback(false);
						}
					}
					else
					{
						if (callback != null) callback(false);
					}
				}
#elif UNITY_ANDROID
				AJC.CallStatic( "ExportFiles", Context, new FPResultCallbackAndroid( null, null, callback ), new string[1] { filePath }, 1 );
#elif UNITY_IOS
				FPResultCallbackiOS.Initialize( null, null, callback );
				_NativeFilePicker_ExportFiles( new string[1] { filePath }, 1 );
#endif
			}
			else if (callback != null)
				callback(false);
		}

		return result;
	}
	public static bool IsFileInUse(string filePath)
	{
		FileStream fs = null;
		try
		{
			fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
			return false; // 文件没有被占用
		}
		catch
		{
			return true; // 文件被占用
		}
		finally
		{
			if (fs != null)
			{
				fs.Close();
			}
		}
	}
	public static Permission ExportMultipleFiles(string[] filePaths, FilesExportedCallback callback = null)
	{
		if (filePaths == null || filePaths.Length == 0)
			throw new ArgumentException("Parameter 'filePaths' is null or empty!");

		Permission result = RequestPermission(false);
		if (result == Permission.Granted && !IsFilePickerBusy())
		{
			if (CanExportMultipleFiles())
			{
#if UNITY_EDITOR
				string destination = UnityEditor.EditorUtility.OpenFolderPanel("Select destination", Path.GetDirectoryName(filePaths[0]), "");
				if (string.IsNullOrEmpty(destination))
				{
					if (callback != null)
						callback(false);
				}
				else
				{
					try
					{
						for (int i = 0; i < filePaths.Length; i++)
							File.Copy(filePaths[i], Path.Combine(destination, Path.GetFileName(filePaths[i])), true);

						if (callback != null)
							callback(true);
					}
					catch (Exception e)
					{
						Debug.LogException(e);

						if (callback != null)
							callback(false);
					}
				}
#elif UNITY_STANDALONE_WIN
				using (FolderBrowserDialog fbd = new FolderBrowserDialog())
				{
					fbd.Description = "Select destination folder";
					if (fbd.ShowDialog() == DialogResult.OK)
					{
						try
						{
							for (int i = 0; i < filePaths.Length; i++)
								File.Copy(filePaths[i], Path.Combine(fbd.SelectedPath, Path.GetFileName(filePaths[i])), true);

							if (callback != null) callback(true);
						}
						catch (Exception e)
						{
							Debug.LogException(e);
							if (callback != null) callback(false);
						}
					}
					else
					{
						if (callback != null) callback(false);
					}
				}
#elif UNITY_ANDROID
				AJC.CallStatic( "ExportFiles", Context, new FPResultCallbackAndroid( null, null, callback ), filePaths, filePaths.Length );
#elif UNITY_IOS
				FPResultCallbackiOS.Initialize( null, null, callback );
				_NativeFilePicker_ExportFiles( filePaths, filePaths.Length );
#endif
			}
			else if (callback != null)
				callback(false);
		}

		return result;
	}
	#endregion
}