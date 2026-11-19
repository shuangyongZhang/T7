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

using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildInterceptor : IPreprocessBuildWithReport
{
    // 这个数字决定了回调的执行顺序，数字越小越先执行
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        // 如果是，则直接返回，不进行任何拦截，让构建继续。
        if (BuildTools.isBuildingWithCustomTool)
        {
            return;
        }

        // 如果不是我们的工具发起的（即用户点击了Build Settings里的Build），则弹出对话框。
        int option = EditorUtility.DisplayDialogComplex(
            "构建方式警告",
            "您正在使用Unity的标准构建流程。\n\n推荐使用自定义的编译工具来代替。\n\n您想怎么做?",
            "使用自定义工具", // 选项 0
            "取消构建",       // 选项 1
            "仍然继续标准构建"  // 选项 2
        );

        switch (option)
        {
            // 选项 0: 使用自定义工具
            case 0:
                // 使用delayCall确保弹窗在当前构建流程安全退出后再执行
                EditorApplication.delayCall += BuildTools.ShowBuildPlayerWindow;
                throw new BuildFailedException("构建已取消，切换至自定义编译工具。");

            // 选项 1: 取消构建
            case 1:
                throw new BuildFailedException("构建已被用户取消。");

            // 选项 2: 仍然继续标准构建
            case 2:
                Debug.LogWarning("用户选择继续执行标准构建流程。");
                break;
        }
    }
}