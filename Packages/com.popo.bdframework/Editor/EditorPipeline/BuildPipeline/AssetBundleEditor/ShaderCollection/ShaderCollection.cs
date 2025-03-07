﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using BDFramework.Core.Tools;
using BDFramework.ResourceMgr;
using LitJson;

namespace BDFramework.Editor.AssetBundle
{
    public class ShaderCollection
    {
        private ShaderVariantCollection svc;
        static string toolsSVCpath = "Assets/Resource/Shaders/Tools.shadervariants";
        static ShaderVariantCollection ToolSVC = null;
        static List<string> allShaderNameList = new List<string>();

        /// <summary>
        /// 简单收集
        /// </summary>
        public static void CollectShaderVariant()
        {
            //先搜集所有keyword到工具类SVC
            ToolSVC = new ShaderVariantCollection();
            var shaders = AssetDatabase.FindAssets("t:Shader", new string[] {"Assets", "Packages"}).ToList();
            foreach (var shader in shaders)
            {
                ShaderVariantCollection.ShaderVariant sv = new ShaderVariantCollection.ShaderVariant();
                var shaderPath = AssetDatabase.GUIDToAssetPath(shader);
                sv.shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
                ToolSVC.Add(sv);
                //
                allShaderNameList.Add(shaderPath);
            }


            //防空
            var dirt = Path.GetDirectoryName(toolsSVCpath);
            if (!Directory.Exists(dirt))
            {
                Directory.CreateDirectory(dirt);
            }

            AssetDatabase.CreateAsset(ToolSVC, toolsSVCpath);


            var paths = BApplication.GetAllRuntimeDirects().ToArray();
            //搜索所有runtime中所有可能挂载mat的地方
            var scriptObjectAssets = AssetDatabase.FindAssets("t:ScriptableObject", paths).ToList(); //自定义序列化脚本中也有可能有依赖
            var prefabAssets = AssetDatabase.FindAssets("t:Prefab", paths).ToList();
            var matAssets = AssetDatabase.FindAssets("t:Material", paths).ToList();


            //ERROR: 添加runtime中独立的shader,没有mat 视为其没有Shader_Featrue变体
            var shadertAssets = AssetDatabase.FindAssets("t:Shader", paths); //自定义序列化脚本中也有可能有依赖
            foreach (var guid in shadertAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                Debug.LogError("不建议将shader放在runtime中,没有mat信息 无法搜集变体！ " + path);
            }

            //搜索mat
            var guidList = new List<string>();
            guidList.AddRange(prefabAssets);
            guidList.AddRange(matAssets);
            guidList.AddRange(scriptObjectAssets);
            List<string> allMatPaths = new List<string>();
            //GUID to assetPath
            for (int i = 0; i < guidList.Count; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guidList[i]);
                //获取依赖中的mat
                var dependenciesPath = AssetDatabase.GetDependencies(path, true);
                foreach (var dp in dependenciesPath)
                {
                    if (Path.GetExtension(dp).Equals(".mat", StringComparison.OrdinalIgnoreCase))
                    {
                        allMatPaths.Add(dp);
                    }
                    else if (Path.GetExtension(dp).Equals(".asset", StringComparison.OrdinalIgnoreCase)) //依赖的ScripttableObject,会
                    {
                        scriptObjectAssets.Add(AssetDatabase.AssetPathToGUID(dp));
                    }
                }
            }

            //ScripttableObject 里面有可能存mat信息
            foreach (var asset in scriptObjectAssets)
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat != null)
                {
                    allMatPaths.Add(path);
                }
            }

            //开始收集ShaderVaraint
            allMatPaths = allMatPaths.Distinct().ToList();
            ShaderVariantCollection allShaderVaraint = null;
            var tools = new ShaderVariantsCollectionTools();
            allShaderVaraint = tools.CollectionKeywords(allMatPaths.ToArray(), ToolSVC);


            //输出SVC文件
            var targetDir = Path.GetDirectoryName(BResources.ALL_SHADER_VARAINT_ASSET_PATH);
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            AssetDatabase.DeleteAsset(BResources.ALL_SHADER_VARAINT_ASSET_PATH);
            AssetDatabase.CreateAsset(allShaderVaraint, BResources.ALL_SHADER_VARAINT_ASSET_PATH);
            AssetDatabase.Refresh();

            Debug.Log("<color=red>shader_features收集完毕,multi_compiles默认全打包需要继承IPreprocessShaders.OnProcessShader自行剔除!</color>");
            // var dependencies = AssetDatabase.GetDependencies(BResources.ALL_SHADER_VARAINT_ASSET_PATH);
            // foreach (var guid in dependencies )
            // {
            //     Debug.Log("依赖shader:" + guid);
            // }
        }


        /// <summary>
        /// 打包ShaderOnly
        /// </summary>
        public static void BuildShadersAssetBundle()
        {
            AssetDatabase.StartAssetEditing();
            {
                CollectShaderVariant();
                var guid = AssetDatabase.AssetPathToGUID(BResources.ALL_SHADER_VARAINT_ASSET_PATH);

                List<AssetImporter> list = new List<AssetImporter>();
                //依赖信息
                var dependice = AssetDatabase.GetDependencies(BResources.ALL_SHADER_VARAINT_ASSET_PATH);
                foreach (var depend in dependice)
                {
                    var type = AssetDatabase.GetMainAssetTypeAtPath(depend);
                    if (type == typeof(Material) || type == typeof(ShaderVariantCollection))
                    {
                        var ai = AssetImporter.GetAtPath(depend);
                        ai.SetAssetBundleNameAndVariant(guid, null);
                        Debug.Log("打包:" +depend);
                        list.Add(ai);
                    }
                }

                //开始编译
                var outpath = IPath.Combine(BApplication.Library, "BDBuildTest", BApplication.GetRuntimePlatformPath());
                if (Directory.Exists(outpath))
                {
                    Directory.Delete(outpath, true);
                }

                Directory.CreateDirectory(outpath);
                //
                var buildtarget = BApplication.GetBuildTarget(BApplication.RuntimePlatform);
                UnityEditor.BuildPipeline.BuildAssetBundles(outpath, BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression, buildtarget);
                //
                foreach (var ai in list)
                {
                    ai.SetAssetBundleNameAndVariant(null, null);
                }
                Debug.Log("测试AB已经输出:" + outpath);
            }
            AssetDatabase.StopAssetEditing();
        }
    }
}
