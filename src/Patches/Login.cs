using System;
using BepInEx;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Il2CppSystem.IO;
using Server;
using ServerConfig;
using SimpleJSON;
using UnhollowerRuntimeLib;
using UnityEngine;
using Utils;

namespace Lethe.Patches;

public class Login : Il2CppSystem.Object
{

    public static System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>> StaticData = new();
    
    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Login>();
        harmony.PatchAll(typeof(Login));
    }

    [HarmonyPatch(typeof(LoginSceneManager), nameof(LoginSceneManager.SetLoginInfo))]
    [HarmonyPostfix]
    private static void SetLoginInfo(LoginSceneManager __instance)
    {
        __instance.tmp_loginAccount.text = "Lethe v" + LetheMain.VERSION;
    }
   
    [HarmonyPatch(typeof(StaticDataManager), nameof(StaticDataManager.LoadStaticDataFromJsonFile))]
    [HarmonyPrefix]
    private static void PreLoadStaticDataFromJsonFile(StaticDataManager __instance, string dataClass,
        ref Il2CppSystem.Collections.Generic.List<JSONNode> nodeList)
    {
        LetheHooks.LOG.LogInfo($"Saving {dataClass}");
        StaticData[dataClass] = new System.Collections.Generic.List<string>();
        foreach (var jsonNode in nodeList)
        {
            StaticData[dataClass].Add(jsonNode.ToString(2));
        }

        var customDataList = new JSONArray();


        var templatePath = Directory.CreateDirectory(Path.Combine(LetheMain.templatePath.FullPath, "custom_limbus_data", dataClass));
        foreach (var modPath in Directory.GetDirectories(LetheMain.modsPath.FullPath))
        {
            var expectedPath = Path.Combine(modPath, "custom_limbus_data", dataClass);
            if (!Directory.Exists(expectedPath)) continue;

            foreach(var customData in Directory.GetFiles(expectedPath, "*.json", SearchOption.AllDirectories))
            {
                LetheHooks.LOG.LogInfo($"loading file from {customData.Substring(LetheMain.modsPath.FullPath.Length)}");
                try
                {
                    var node = JSONNode.Parse(File.ReadAllText(customData));
                    customDataList.Add(node);
                    nodeList.Insert(0, node);
                }
                catch (Exception ex)
                {
                    LetheHooks.LOG.LogError($"ERROR PARSING FILE {customData}: {ex.GetType()} {ex.Message}");
                }
            }

        }


            try
            {
            var url = Singleton<ServerSelector>.Instance.GetServerURL() + "/custom/upload/" + dataClass;
            var auth = SingletonBehavior<LoginInfoManager>.Instance.UserAuth.ToServerUserAuthFormat();
            var body = new JSONObject();
            var subNode = new JSONObject();
            subNode.Add("list", customDataList);
            body.Add("parameters", subNode);
            body.Add("userAuth", JSONNode.Parse(JsonUtility.ToJson(auth)));
            var schema = new HttpApiSchema(url, body.ToString(2), new Action<string>(_ => { }), "", false);
            HttpApiRequester.Instance.SendRequest(schema, true);
        }
        catch (Exception ex)
        {
            LetheHooks.LOG.LogError($"Error uploading {dataClass}: {ex.GetType()} {ex.Message}");
        }
    }

    //stub for some silly error
    [HarmonyPatch(typeof(PassiveDetail), nameof(PassiveDetail.OnPartBreaked))]
    [HarmonyPrefix]
    private static void sigma() { }

}