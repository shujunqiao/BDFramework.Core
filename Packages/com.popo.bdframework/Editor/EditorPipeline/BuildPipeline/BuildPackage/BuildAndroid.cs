﻿using System;
using BDFramework.Core.Tools;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace BDFramework.Editor.BuildPipeline
{
    /// <summary>
    /// 构建Android面板
    /// </summary>
    [Serializable]
    public class BuildAndroid
    {
        public BuildAndroid(AndroidSetting releaseSetting, AndroidSetting debugSettingSetting)
        {
            this.AndroidReleaseSetting = releaseSetting;
            this.AndroidDebugSetting = debugSettingSetting;
        }


        #region 打包参数

        
      
        [BoxGroup("Android设置[Release]")]
        [HideLabel]
        [InlineProperty]
        [DisableIf("IsLock")]
        public AndroidSetting AndroidReleaseSetting = new AndroidSetting();
        
       
        [BoxGroup("Android设置[Debug]")]
        [HideLabel]
        [InlineProperty]
        [DisableIf("IsLock")]
        public AndroidSetting AndroidDebugSetting = new AndroidSetting();

        [VerticalGroup("a")]
        [LabelText("锁住配置")]
        public bool IsLock = true;
        #endregion


        #region 标准构建
      
        [HorizontalGroup("a/a1")]
        [Button("构建Debug母包", ButtonSizes.Large)]
        [GUIColor(1, 1, 0)]
        public void Btn_DebugBuild()
        {
            DebugBuild();
        }

        [HorizontalGroup("a/a1")]
        [Button("构建Release母包", ButtonSizes.Large)]
        [GUIColor(0, 1, 0)]
        public void Btn_ReleaseBuild()
        {
            ReleaseBuild();
        }

        #endregion

        #region 自定义构建

        [Space(20)]
        [VerticalGroup("b")]
        [HorizontalGroup("b/a1", LabelWidth = 80)]
        [Title("自定义构建", titleAlignment: TitleAlignments.Left, Bold = true)]
        [LabelText("构建模式")]
        public BuildPackageTools.BuildMode buildMode = BuildPackageTools.BuildMode.UseCurrentConfigDebug;

     
        [HorizontalGroup("b/a2", LabelWidth = 80)]
        [LabelText("构建资源")]
        [InfoBox("重新构建资产", InfoMessageType.Info)]
        public bool isReBuildAssets = false;


        [HorizontalGroup("b/a3", LabelWidth = 80)]
        [LabelText("构建选项")]
       // [EnumToggleButtons]
        public BuildAssetsTools.BuildPackageOption BuildPackageOption = BuildAssetsTools.BuildPackageOption.BuildAll;

       
        [HorizontalGroup("b/a4")]
        [GUIColor(0,1,1)]
        [Button("构建(自定义参数)", ButtonSizes.Large, ButtonStyle.CompactBox)]
        public void Btn_CustomBuild()
        {
            CustomBuild();
        }


        #endregion
        
        /// <summary>
        /// 加载debug配置,debug构建
        /// </summary>
        public static void DebugBuild()
        {
            if (EditorUtility.DisplayDialog("提示", "此操作会重新编译资源,是否继续？", "OK", "Cancel"))
            {
                BuildPackageTools.Build(BuildPackageTools.BuildMode.Debug, true, BApplication.DevOpsPublishPackagePath,BuildTarget.Android);
            }
        }

        /// <summary>
        /// 加载Release配置,Release 构建
        /// </summary>
        public static void ReleaseBuild()
        {
            if (EditorUtility.DisplayDialog("提示", "此操作会重新编译资源,是否继续？", "OK", "Cancel"))
            {
                BuildPackageTools.Build(BuildPackageTools.BuildMode.Release, true, BApplication.DevOpsPublishPackagePath,BuildTarget.Android);
            }
        }

        
        /// <summary>
        /// 自定义构建
        /// </summary>
        public  void CustomBuild()
        {
            BuildPackageTools.Build(buildMode, isReBuildAssets, BApplication.DevOpsPublishPackagePath,BuildTarget.Android,BuildPackageOption);
        }
    }
}
