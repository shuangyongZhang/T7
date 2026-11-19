/****************************************************************************
 * Copyright 2025 YQY (sho)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 ****************************************************************************/

using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Player;
using System.IO;
using System.Linq;
using UnityEditor.Build.Reporting;
using System;


public struct BuildParameters
{
    public BuildTarget Target;
    public string Version;
    public VersionType VersionType;
    public string DateString;
    public bool NeedCompress;

    // --- 添加平台构建设置 ---
    public bool DevelopmentBuild;
    public bool AllowDebugging;
    public bool CompressWithLz4;
    public bool BuildAppBundle; // For Android
}
public enum VersionType
{
    Alpha,
    Beta,
    Release
}
public class BuildTools
{
    public static bool isBuildingWithCustomTool = false;

    #region --- 完整构建流程 ---

    [MenuItem("Tools/Build/Build Player #&b")] // 快捷键: Shift+Alt+B
    public static void ShowBuildPlayerWindow()
    {
        BuildWindow.ShowWindow(DoFullBuild);
    }

    private static void DoFullBuild(BuildParameters parameters)
    {
        Debug.Log($"开始构建: 平台={parameters.Target}, 版本={parameters.Version}, 日期={parameters.DateString}");
        WriteBuildInfoToJson(parameters);

        // 1. 应用设置
        if (EditorUserBuildSettings.SwitchActiveBuildTarget(BuildPipeline.GetBuildTargetGroup(parameters.Target), parameters.Target))
        {
            PlayerSettings.bundleVersion = parameters.Version;
            AssetDatabase.SaveAssets();
            Debug.Log($"已切换平台至 {parameters.Target} 并设置版本号为 {parameters.Version}");
        }
        else
        {
            Debug.LogError("切换平台失败！取消构建。");
            return;
        }

        // 2. 构建输出路径
        string buildRootPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds"));
        string buildFolder = $"{parameters.Target}_{parameters.VersionType}_{parameters.Version}_{parameters.DateString}";
        string fullBuildPath = Path.Combine(buildRootPath, buildFolder);
        string executablePath = Path.Combine(fullBuildPath, GetBuildNameWithExtension(parameters.Target));

        Directory.CreateDirectory(fullBuildPath);

        // 3. 设置构建选项
        EditorUserBuildSettings.development = parameters.DevelopmentBuild;
        EditorUserBuildSettings.allowDebugging = parameters.AllowDebugging;

        if (parameters.Target == BuildTarget.Android)
        {
            EditorUserBuildSettings.buildAppBundle = parameters.BuildAppBundle;
        }

        BuildOptions buildOptions = BuildOptions.None;
        if (parameters.DevelopmentBuild)
        {
            buildOptions |= BuildOptions.Development;
        }
        if (parameters.DevelopmentBuild && parameters.AllowDebugging)
        {
            buildOptions |= BuildOptions.AllowDebugging;
        }
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = executablePath,
            target = parameters.Target,
            options = buildOptions
        };

        isBuildingWithCustomTool = true;
        BuildReport report;
        try
        {
            // 4. 执行构建
            report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        }
        finally
        {
            // 确保无论构建成功、失败或异常，都重置标志位
            isBuildingWithCustomTool = false;
        }

        BuildSummary summary = report.summary;

        // 5.构建成功后的处理逻辑
        if (summary.result == BuildResult.Succeeded)
        {
            string finalOutputPath = fullBuildPath; // 默认输出是文件夹
            string successMessage = $"构建成功！(未压缩)\n\n输出目录: {finalOutputPath}\n总用时: {summary.totalTime.Minutes}分 {summary.totalTime.Seconds}秒";

            // 检查用户是否请求压缩
            if (parameters.NeedCompress)
            {
                // 检查构建结果是否真的需要压缩
                if (IsPackagingRequired(fullBuildPath))
                {
                    string zipFilePath = Path.Combine(buildRootPath, $"{buildFolder}.zip");
                    bool compressionSuccess = CompressDirectory(fullBuildPath, zipFilePath);

                    if (compressionSuccess)
                    {
                        finalOutputPath = zipFilePath;
                        successMessage = $"构建并压缩成功！\n\n输出至: {finalOutputPath}\n总用时: {summary.totalTime.Minutes}分 {summary.totalTime.Seconds}秒";
                    }
                    else
                    {
                        successMessage = $"构建成功，但压缩失败。\n\n构建输出至: {finalOutputPath}\n总用时: {summary.totalTime.Minutes}分 {summary.totalTime.Seconds}秒";
                        Debug.LogError(successMessage);
                    }
                }
                else
                {
                    // 虽然用户请求了压缩，但程序判断无需压缩
                    finalOutputPath = Directory.GetFileSystemEntries(fullBuildPath)[0]; // 获取那个单文件
                    successMessage = $"构建成功！(单个文件无需压缩)\n\n输出文件: {finalOutputPath}\n总用时: {summary.totalTime.Minutes}分 {summary.totalTime.Seconds}秒";
                    Debug.Log("用户请求压缩，但检测到输出为单个文件，自动跳过。");
                }
            }

            Debug.Log(successMessage.Replace("\n\n", " ").Replace("\n", " "));
            if (EditorUtility.DisplayDialog("任务完成", successMessage, "好的，打开位置"))
            {
                EditorUtility.RevealInFinder(finalOutputPath);
            }
        }
        else
        {
            string message = $"构建失败！\n\n总用时: {summary.totalTime.Minutes}分 {summary.totalTime.Seconds}秒\n错误数: {summary.totalErrors}";
            Debug.LogError(message);
            EditorUtility.DisplayDialog("构建失败", message, "好的");
        }
    }

    #endregion

    #region --- 仅编译DLL ---

    [MenuItem("Tools/Build/Compile DLLs Only #&d")]
    public static void ShowCompileDllDialog()
    {
        BuildDllWindow.ShowWindow(DoCompileDll);
    }

    public static bool DoCompileDll(BuildTarget target)
    {
        ScriptCompilationSettings settings = new ScriptCompilationSettings
        {
            group = BuildPipeline.GetBuildTargetGroup(target),
            target = target
        };

        string outDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "DLL"));
        Directory.CreateDirectory(outDir);
        Debug.Log($"输出目录： {outDir}");
        Debug.Log($"为目标平台编译DLL： {target}");

        ScriptCompilationResult scriptCompilationResult = PlayerBuildInterface.CompilePlayerScripts(settings, outDir);
        bool compilationSucceeded = scriptCompilationResult.assemblies != null && scriptCompilationResult.assemblies.Count > 0;

        if (compilationSucceeded)
        {
            string fullPathToAssembly = Path.Combine(outDir, scriptCompilationResult.assemblies[0]);
            if (EditorUtility.DisplayDialog("编译成功", $"DLL已编译至:\n{outDir}", "好的，打开目录"))
                EditorUtility.RevealInFinder(fullPathToAssembly);
        }
        else
        {
            EditorUtility.DisplayDialog("编译失败", "没有程序集被编译。请检查控制台错误。", "好的");
        }
        return compilationSucceeded;
    }

    #endregion

    #region --- 辅助方法 ---

    /// <summary>
    /// 根据平台获取带正确扩展名的可执行文件名
    /// </summary>
    private static string GetBuildNameWithExtension(BuildTarget target)
    {
        string buildName = PlayerSettings.productName;
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return $"{buildName}.exe";
            case BuildTarget.Android:
                return $"{buildName}.apk";
            case BuildTarget.StandaloneOSX:
                return $"{buildName}.app";
            default:
                return buildName;
        }
    }

    private static void WriteBuildInfoToJson(BuildParameters parameters)
    {
        string resourcePath = Path.Combine(Application.dataPath, "Resources");
        Directory.CreateDirectory(resourcePath);

        BuildInfoData data = new BuildInfoData
        {
            version = parameters.Version,
            versionType = parameters.VersionType.ToString(),
            buildDate = parameters.DateString
        };

        string json = JsonUtility.ToJson(data, true);
        string filePath = Path.Combine(resourcePath, "build_info.json.txt");

        File.WriteAllText(filePath, json);
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// 判断构建输出目录是否需要被打包。
    /// </summary>
    /// <param name="buildPath">构建输出的根目录。</param>
    /// <returns>如果需要打包（多文件或单目录）则返回true，否则返回false。</returns>
    private static bool IsPackagingRequired(string buildPath)
    {
        if (!Directory.Exists(buildPath)) return false;

        string[] entries = Directory.GetFileSystemEntries(buildPath);

        // 如果目录为空，或包含多个项目，或只包含一个子目录，则需要打包
        if (entries.Length != 1 || Directory.Exists(entries[0]))
        {
            return true;
        }

        // 如果只包含一个文件，则无需打包
        return false;
    }

    /// <summary>
    /// 目录压缩成zip文件。
    /// </summary>
    private static bool CompressDirectory(string sourceFolderPath, string destinationZipPath)
    {
        try
        {
            if (File.Exists(destinationZipPath))
            {
                File.Delete(destinationZipPath);
            }
#if UNITY_2022_3_OR_NEWER
            // --- 使用 .NET 内置的 ZipFile  ---
            System.IO.Compression.ZipFile.CreateFromDirectory(sourceFolderPath, destinationZipPath, System.IO.Compression.CompressionLevel.Optimal, false);
#else
            // --- 使用 SharpZipLib (旧版Unity兼容) ---
            ICSharpCode.SharpZipLib.Zip.FastZip fastZip = new ICSharpCode.SharpZipLib.Zip.FastZip();
            fastZip.CreateZip(destinationZipPath, sourceFolderPath, true, null);
#endif
            Debug.Log($"压缩成功，输出至: {destinationZipPath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"压缩过程中发生错误: {e.Message}\n{e.StackTrace}");
            return false;
        }
    }
    #endregion
}