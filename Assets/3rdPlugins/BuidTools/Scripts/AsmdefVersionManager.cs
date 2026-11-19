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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

/// <summary>
/// 编辑器模式下实时分析，只显示含有版本信息的自定义程序集。
/// 运行时模式依赖生成的 JSON 注册表
/// </summary>
public static class AsmdefVersionManager
{
    [Serializable]
    private class RegistryEntry
    {
        public string name;
        public string version;
    }
    [Serializable]
    private class AssemblyRegistryData
    {
        public List<RegistryEntry> entries = new List<RegistryEntry>();
    }

    private static List<AsmdefInfo> _cachedInfos;
    private const string REGISTRY_JSON_NAME = "ProjectAssemblyRegistry";

#if UNITY_EDITOR
    private static readonly string RegistryJsonFullPath = $"Assets/Resources/{REGISTRY_JSON_NAME}.json";
#endif

    public static List<AsmdefInfo> GetAllVersionedAsmdefs()
    {
        _cachedInfos = new List<AsmdefInfo>();
        FindVersions();
        _cachedInfos.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
        return _cachedInfos;
    }

    private static void FindVersions()
    {
        Assembly[] allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

#if UNITY_EDITOR
        foreach (var assembly in allAssemblies)
        {
            // 过滤条件 1: 必须是用户在 Assets 目录下定义的程序集
            if (IsProjectAssembly(assembly))
            {
                // 过滤条件 2: 该程序集必须含有版本信息特性
                var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (versionAttribute != null && !string.IsNullOrEmpty(versionAttribute.InformationalVersion))
                {
                    _cachedInfos.Add(new AsmdefInfo
                    {
                        name = assembly.GetName().Name,
                        version = versionAttribute.InformationalVersion,
                        Path = GetAssemblyInfoFilePath(assembly)
                    });
                }
            }
        }
        // 更新注册表
        UpdateRegistry(_cachedInfos);
#else
        Dictionary<string, string> validVersions;
        if (!GetRegistryVersDict(out validVersions)) return;

        foreach (var assembly in allAssemblies)
        {
            string assemblyName = assembly.GetName().Name;

            // 过滤条件 1: 程序集名称必须在我们的注册表里
            if (validVersions.ContainsKey(assemblyName))
            {
                // 过滤条件 2: 该程序集必须含有版本信息特性
                var versionAttribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
                if (versionAttribute != null && !string.IsNullOrEmpty(versionAttribute.InformationalVersion))
                {
                    _cachedInfos.Add(new AsmdefInfo
                    {
                        name = assemblyName,
                        version = versionAttribute.InformationalVersion,
                        Path = "N/A (Runtime)"
                    });
                }
            }
        }
#endif
    }

    public static bool GetRegistryVersDict(out Dictionary<string, string> validVersions)
    {
        validVersions = new Dictionary<string, string>();
        var jsonAsset = Resources.Load<TextAsset>(REGISTRY_JSON_NAME);
        if (jsonAsset == null)
        {
            Debug.LogError($"[AsmdefVersionManager] 注册表文件 '{REGISTRY_JSON_NAME}.json' 未找到。");
            return false;
        }

        var registryData = JsonUtility.FromJson<AssemblyRegistryData>(jsonAsset.text);
        validVersions = registryData.entries.ToDictionary(entry => entry.name, entry => entry.version);
        return true;
    }

#if UNITY_EDITOR
    /// <summary>
    /// 这个工具的目的是为“运行时”准备数据。
    /// </summary>

    public static void UpdateRegistry(List<AsmdefInfo> infos)
    {
        string resourcesFolder = "Assets/Resources";
        if (!Directory.Exists(resourcesFolder))
        {
            Directory.CreateDirectory(resourcesFolder);
        }
        var registryData = new AssemblyRegistryData();
        foreach (var info in infos)
        {
            registryData.entries.Add(new RegistryEntry
            {
                name = info.name,
                version = info.version
            });
        }
        string json = JsonUtility.ToJson(registryData, true);
        File.WriteAllText(RegistryJsonFullPath, json);
        AssetDatabase.ImportAsset(RegistryJsonFullPath);
    }


    private static bool IsProjectAssembly(Assembly assembly)
    {
        try
        {
            if (string.IsNullOrEmpty(assembly.Location)) return false;

            string asmdefPath = UnityEditor.Compilation.CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.GetName().Name);

            if (string.IsNullOrEmpty(asmdefPath))
            {
                string assemblyName = assembly.GetName().Name;
                return assemblyName == "Assembly-CSharp" || assemblyName == "Assembly-CSharp-firstpass";
            }

            return asmdefPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase);
        }
        catch { return false; }
    }

    private static string GetAssemblyInfoFilePath(Assembly assembly)
    {
        string asmdefPath = UnityEditor.Compilation.CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(assembly.GetName().Name);

        if (string.IsNullOrEmpty(asmdefPath))
        {
            if (assembly.GetName().Name.StartsWith("Assembly-CSharp"))
            {
                string potentialPath = "Assets/AssemblyInfo.cs";
                return File.Exists(potentialPath) ? potentialPath : "Assets (Default Assembly)";
            }
            return "N/A (No .asmdef)";
        }

        string directory = Path.GetDirectoryName(asmdefPath);
        return Path.Combine(directory, "AssemblyInfo.cs").Replace('\\', '/');
    }
#endif
}

/// <summary>
/// 用于显示版本信息的数据结构。
/// </summary>
public class AsmdefInfo
{
    public string name;
    public string version;
    public string Path;
}