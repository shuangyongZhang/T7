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
using System.IO;

/// <summary>
/// 这个编辑器脚本会监视新创建的资产。
/// 当检测到一个新的 .asmdef 文件被创建时，
/// 它会自动在同一个目录下生成一个配套的 AssemblyInfo.cs 文件模板。
/// 开发者之后需要手动在这个文件中填入版本号。
/// </summary>
public class AsmdefTemplateGenerator : AssetPostprocessor
{
    // 定义我们自动生成的版本信息文件名
    private const string ASSEMBLY_INFO_FILENAME = "AssemblyInfo.cs";

    /// <summary>
    /// 当任何资产被导入（包括创建）后，这个方法会被调用。
    /// </summary>
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            // 检查被导入的是否是一个 .asmdef 文件
            if (path.EndsWith(".asmdef"))
            {
                // 检查这是否是一个“新创建”的资产。
                CreateAssemblyInfoTemplateIfNeeded(path);
            }
        }
    }

    /// <summary>
    /// 检查并按需创建一个 AssemblyInfo.cs 模板。
    /// </summary>
    private static void CreateAssemblyInfoTemplateIfNeeded(string asmdefPath)
    {
        // 获取 .asmdef 文件所在的目录
        string directory = Path.GetDirectoryName(asmdefPath);
        string assemblyInfoPath = Path.Combine(directory, ASSEMBLY_INFO_FILENAME);

        // 如果 AssemblyInfo.cs 文件已经存在，则什么都不做
        if (File.Exists(assemblyInfoPath))
        {
            return;
        }

        // 如果文件不存在，则创建它
        try
        {
            string templateContent = GenerateAssemblyInfoTemplate();
            File.WriteAllText(assemblyInfoPath, templateContent);

            Debug.Log($"[AsmdefTemplateGenerator] 自动为 '{asmdefPath}' 中的新程序集创建了 '{ASSEMBLY_INFO_FILENAME}'。请填写版本详细信息。");

            // 刷新资产数据库
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[AsmdefTemplateGenerator] 无法为 {asmdefPath} 创建模板文件。错误: {e.Message}");
        }
    }

    /// <summary>
    /// 生成 AssemblyInfo.cs 文件的标准内容模板。
    /// </summary>
    private static string GenerateAssemblyInfoTemplate()
    {
        // 模板
        return @"using System.Reflection;

// 此文件是为新程序集自动生成的。

[assembly: AssemblyInformationalVersion(""1.0.001"")] // <-- TODO: 更新版本号!

";
    }
}