using System;
using HarmonyLib;
using Il2CppSystem.IO;
using MainUI;
using SimpleJSON;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CustomEncounter.Patches;

public class Data : Il2CppSystem.Object
{
   
    private static bool _localizeDataLoaded;

    public static void Setup(Harmony harmony)
    {
        ClassInjector.RegisterTypeInIl2Cpp<Data>();
        harmony.PatchAll(typeof(Data));
    }
    
    [HarmonyPatch(typeof(StageStaticDataList), nameof(StageStaticDataList.GetStage))]
    [HarmonyPrefix]
    private static bool PreGetStage(ref int id, ref StageStaticData __result)
    {
        switch (id)
        {
            case 1 when CustomEncounterHook.Encounter != null:
                __result = CustomEncounterHook.Encounter;
                return false;
            case -1:
                id = 1;
                break;
        }

        return true;
    }


    public static void LoadCustomLocale<T>(DirectoryInfo root, string name, JsonDataList<T> list)
        where T : LocalizeTextData, new()
    {
        CustomEncounterHook.LOG.LogInfo("Checking for custom locale: " + name);
        root = Directory.CreateDirectory(Path.Combine(root.FullName, name));
        foreach (var file in Directory.GetFiles(root.FullName, "*.json"))
        {
            var localeJson = JSONNode.Parse(File.ReadAllText(file));
            CustomEncounterHook.LOG.LogInfo("Loading custom locale: " + file);
            foreach (var keyValuePair in localeJson)
            {
                var valueJson = keyValuePair.value.ToString(2);

                try
                {
                    var value = JsonUtility.FromJson<T>(valueJson);
                    if (value == null) throw new NullReferenceException("json parse result is null");
                    list._dic[keyValuePair.key] = value;
                    CustomEncounterHook.LOG.LogInfo("Loaded custom locale for " + keyValuePair.key);
                }
                catch (Exception ex)
                {
                    CustomEncounterHook.LOG.LogError("Cannot load custom locale for " + keyValuePair.key + ", reason: " + ex);
                    CustomEncounterHook.LOG.LogError(valueJson);
                }
            }
        }
    }

    private static void LoadCustomLocale(TextDataManager __instance, LOCALIZE_LANGUAGE lang)
    {
        var root = Directory.CreateDirectory(Path.Combine(CustomEncounterHook.CustomLocaleDir.FullName, lang.ToString()));
        LoadCustomLocale(root, "uiList", __instance._uiList);
        LoadCustomLocale(root, "characterList", __instance._characterList);
        LoadCustomLocale(root, "personalityList", __instance._personalityList);
        LoadCustomLocale(root, "enemyList", __instance._enemyList);
        LoadCustomLocale(root, "egoList", __instance._egoList);
        LoadCustomLocale(root, "skillList", __instance._skillList);
        LoadCustomLocale(root, "passiveList", __instance._passiveList);
        LoadCustomLocale(root, "bufList", __instance._bufList);
        LoadCustomLocale(root, "itemList", __instance._itemList);
        LoadCustomLocale(root, "keywordList", __instance._keywordList);
        LoadCustomLocale(root, "skillTagList", __instance._skillTagList);
        LoadCustomLocale(root, "abnormalityEventList", __instance._abnormalityEventList);
        LoadCustomLocale(root, "attributeList", __instance._attributeList);
        LoadCustomLocale(root, "abnormalityCotentData", __instance._abnormalityCotentData);
        LoadCustomLocale(root, "keywordDictionary", __instance._keywordDictionary);
        LoadCustomLocale(root, "actionEvents", __instance._actionEvents);
        LoadCustomLocale(root, "egoGiftData", __instance._egoGiftData);
        LoadCustomLocale(root, "stageChapter", __instance._stageChapter);
        LoadCustomLocale(root, "stagePart", __instance._stagePart);
        LoadCustomLocale(root, "stageNodeText", __instance._stageNodeText);
        LoadCustomLocale(root, "dungeonNodeText", __instance._dungeonNodeText);
        LoadCustomLocale(root, "storyDungeonNodeText", __instance._storyDungeonNodeText);
        LoadCustomLocale(root, "quest", __instance._quest);
        LoadCustomLocale(root, "dungeonArea", __instance._dungeonArea);
        LoadCustomLocale(root, "battlePass", __instance._battlePass);
        LoadCustomLocale(root, "storyTheater", __instance._storyTheater);
        LoadCustomLocale(root, "announcer", __instance._announcer);
        LoadCustomLocale(root, "normalBattleResultHint", __instance._normalBattleResultHint);
        LoadCustomLocale(root, "abBattleResultHint", __instance._abBattleResultHint);
        LoadCustomLocale(root, "tutorialDesc", __instance._tutorialDesc);
        LoadCustomLocale(root, "iapProductText", __instance._iapProductText);
        LoadCustomLocale(root, "illustGetConditionText", __instance._illustGetConditionText);
        LoadCustomLocale(root, "choiceEventResultDesc", __instance._choiceEventResultDesc);
        LoadCustomLocale(root, "battlePassMission", __instance._battlePassMission);
        LoadCustomLocale(root, "gachaTitle", __instance._gachaTitle);
        LoadCustomLocale(root, "introduceCharacter", __instance._introduceCharacter);
        LoadCustomLocale(root, "userBanner", __instance._userBanner);
    }

    [HarmonyPatch(typeof(LobbyUIPresenter), nameof(LobbyUIPresenter.Initialize))]
    [HarmonyPostfix]
    private static void PostMainUILoad()
    {
        if (!_localizeDataLoaded)
        {
            LoadCustomLocale(Singleton<TextDataManager>.Instance, GlobalGameManager.Instance.Lang);
            _localizeDataLoaded = true;
        }
    }


}