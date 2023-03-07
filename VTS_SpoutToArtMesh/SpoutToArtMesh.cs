using System;
using BepInEx;
using System.IO;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace VTS_SpoutToArtMesh
{
    [BepInPlugin(GUID, PluginName, VERSION)]
    public class SpoutToArtMesh : BaseUnityPlugin
    {
        public const string GUID = "me.xiaoye97.plugin.VTubeStudio.SpoutToArtMesh";
        public const string PluginName = "SpoutToArtMesh";
        public const string VERSION = "1.0.0";

        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(SpoutToArtMesh));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(VTubeStudioModel), "Start")]
        public static void VTubeStudioModel_Start_Patch(VTubeStudioModel __instance)
        {
            FileInfo modelJSonFile = new FileInfo(__instance.ModelJSON.FilePath);
            var files = modelJSonFile.Directory.GetFiles("*.SpoutToArtMesh.json");
            foreach (var file in files)
            {
                if (file.Name.StartsWith(__instance.ModelJSON.Name))
                {
                    ReadConfigFile(__instance, file);
                }
            }
        }

        /// <summary>
        /// 读取配置文件
        /// </summary>
        public static void ReadConfigFile(VTubeStudioModel model, FileInfo file)
        {
            string json = File.ReadAllText(file.FullName);
            try
            {
                var config = JsonConvert.DeserializeObject<SpoutToArtMeshConfig>(json);
                if (config.ArtMeshNames == null || config.ArtMeshNames.Count == 0)
                {
                    Debug.LogWarning($"SpoutToArtMesh 模型:{model.ModelJSON.Name} 的SpoutToArtMesh配置文件中没有目标ArtMesh，不进行Spout接收");
                    return;
                }
                List<Renderer> renderers = new List<Renderer>();
                foreach (var d in model.Live2DModel.Drawables)
                {
                    // ID相同，找到目标
                    if (config.ArtMeshNames.Contains(d.Id))
                    {
                        var renderer = d.GetComponent<Renderer>();
                        renderers.Add(renderer);
                    }
                }
                if (renderers.Count > 0)
                {
                    var sp = model.gameObject.AddComponent<XYSpoutReceiver>();
                    sp.sourceName = config.SpoutName;
                    sp.HFlip = config.HorizontalFlip;
                    sp.VFlip = config.VerticalFlip;
                    sp.targetMaterialProperty = "_MainTex";
                    sp.targetRenderers = renderers;
                    Debug.Log($"SpoutToArtMesh在模型:{model.ModelJSON.Name} 的{renderers.Count}/{config.ArtMeshNames.Count}个ArtMesh上开启了Spout接收");
                }
                else
                {
                    Debug.Log($"SpoutToArtMesh 模型:{model.ModelJSON.Name} 上没有目标的ArtMesh，不进行Spout接收");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"SpoutToArtMesh解析配置文件异常 {ex}");
            }
        }
    }
}