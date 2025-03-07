﻿using System;
using System.Linq;
using BDFramework.Core.Tools;
using BDFramework.Editor.BuildPipeline;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace BDFramework.Editor.EditorPipeline.BuildPipeline
{
    public class EditorWindow_BuildPipeline : OdinMenuEditorWindow
    {
        [MenuItem("BDFrameWork工具箱/Odin BuildPipeline")]
        public static void Open()
        {
            var window = GetWindow<EditorWindow_BuildPipeline>("BuildPipeline");
            window.position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 500);
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree(true);
            tree.DefaultMenuStyle.IconSize = 20.00f;

            var setting = BDEditorApplication.BDFrameworkEditorSetting;

            tree.Add("Build", null, EditorIcons.SmartPhone);
            tree.Add($"Build/{BApplication.GetPlatformPath(RuntimePlatform.Android)}", new BuildAndroid(setting.Android, setting.AndroidDebug));
            tree.Add($"Build/{BApplication.GetPlatformPath(RuntimePlatform.IPhonePlayer)}", new BuildIOS(setting.iOS, setting.iOSDebug));
            tree.Add($"Build/{BApplication.GetPlatformPath(RuntimePlatform.WindowsPlayer)}", new BuildWindowsPlayer(setting.WindowsPlayer, setting.WindowsPlayerDebug));
            tree.Add($"Build/{BApplication.GetPlatformPath(RuntimePlatform.OSXPlayer)}(待实现)", new BuildMacOSX());
            // tree.Add($"Build/{BApplication.GetPlatformPath(RuntimePlatform.OSXPlayer)}", new BuildAndroid());
            // tree.Add($"Build/{BApplication.GetPlatformPath(RuntimePlatform.WindowsPlayer)}", new BuildAndroid());

            // tree.Add("Test", EditorWindow.GetWindow<EditorWindow_BDFrameworkConfig>());
            //设置
            tree.Add("Player Settings", Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault());
            //tree.SortMenuItemsByName();
            //默认选择
            var selectMenuitem = tree.MenuItems.Find((m) => m.Name.Equals("Build"));
            if (selectMenuitem != null && selectMenuitem.ChildMenuItems.Count > 0)
            {
                selectMenuitem = selectMenuitem.ChildMenuItems.Find((m) => m.Name.Equals(BApplication.GetPlatformPath(RuntimePlatform.Android)));
            }

            selectMenuitem.Select(true);

            return tree;
        }

        //保存设置
        protected override void OnDestroy()
        {
            BDEditorApplication.BDFrameworkEditorSetting?.Save();
        }
    }
}
