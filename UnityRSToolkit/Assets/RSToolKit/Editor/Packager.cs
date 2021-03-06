﻿using UnityEditor;
using UnityEngine;
using System;
namespace RSToolkit{
    public class Packager
    {

        #region TOOLS
        private enum PATH_TYPE{
            SCRIPTS,
            PREFABS
        }
        const string RS_PATH = "Assets/RSToolKit";
        static readonly string rsPathScripts = $"{RS_PATH}/Scripts";
        static readonly string rsPathPrefabs = $"{RS_PATH}/Prefabs";
        const string RS_SUBPATH_GENERAL = "General"; 
        const string RS_SUBPATH_SPACE2D = "Space2D"; 
        const string RS_SUBPATH_SPACE3D = "Space3D"; 
        const string RS_SUBPATH_CHARACTER = "Character"; 
        const string RS_SUBPATH_UI = "UI"; 
        private static string GetPath(PATH_TYPE pathType, string subPath = ""){
            string basePath = string.Empty;
            switch(pathType){
                case PATH_TYPE.SCRIPTS:
                basePath = rsPathScripts;
                break;
                case PATH_TYPE.PREFABS:
                basePath = rsPathPrefabs;
                break;
            }

            return $"{basePath}/{subPath}";
        }

        private static string GetPackageName(string name){
            return $"rstoolkit-{name}.unitypackage";
        }
        private static void DebugLogExportStart(string packageName){
            Debug.Log($"Exporting {packageName}");
        } 
        private static void DebugLogExportEnd(string packageName){
            Debug.Log($"Exported {packageName}");
        } 

        #endregion TOOLS

        [UnityEditor.MenuItem("Tools/RSToolkit/Export Package/Spawner")]
        public static void ExportSpawner()
        {
            DebugLogExportStart("Spawner");
            var toExportPaths = new string[]{
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_GENERAL)}/Controls/Spawner",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_GENERAL)}/RSMonoBehaviour.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_GENERAL)}/Helpers/TransformHelpers.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_GENERAL)}/Helpers/RandomHelpers.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_GENERAL)}/Helpers/DebugHelpers.cs"
            };
            AssetDatabase.ExportPackage(
                    toExportPaths,
                    GetPackageName("spawner"),
                    ExportPackageOptions.Recurse);
            DebugLogExportEnd("Spawner");
        }
    
        [UnityEditor.MenuItem("Tools/RSToolkit/Export Package/UI/Paging")]
        public static void ExportUIPaging(){

            DebugLogExportStart("UI Paging");
            var toExportPaths  = new string[]{
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_UI) }/Paging",
                $"{GetPath(PATH_TYPE.PREFABS, RS_SUBPATH_UI)}/Paging",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_GENERAL)}/Collections/SizedStack.cs"
            };
            AssetDatabase.ExportPackage(
                    toExportPaths,
                    GetPackageName("uipaging"),
                    ExportPackageOptions.Recurse);
            DebugLogExportEnd("UI Paging");
        }
    
        [UnityEditor.MenuItem("Tools/RSToolkit/Export Package/UI/Controls/Popup")]
        public static void ExportUIControlsPopup(){

            DebugLogExportStart("UI Controls Popup");
            var toExportPaths = new string[]{
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_UI)}/Controls/UIPopup.cs",
                $"{GetPath(PATH_TYPE.PREFABS, RS_SUBPATH_UI)}/Controls/UI Popup.prefab"
            };
            AssetDatabase.ExportPackage(
                    toExportPaths,
                    GetPackageName("uipopup"),
                    ExportPackageOptions.Recurse);
            DebugLogExportEnd("UI Controls Popup");
        }
    
        [UnityEditor.MenuItem("Tools/RSToolkit/Export Package/AI/Bot")]
        public static void ExportBotAI(){

            DebugLogExportStart("Bot AI");
            var toExportPaths = new string[]{
                $"Assets/RSToolKit/Scripts/AI",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_GENERAL)}/Helpers/DebugHelpers.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_GENERAL)}/Helpers/PhysicsHelpers.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_GENERAL)}/Helpers/RandomHelpers.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_GENERAL)}/Helpers/TransformHelpers.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_SPACE3D)}/Draggable3DObject.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_SPACE3D)}/Flying3DObject.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_SPACE3D)}/GameObjectNavSpawnerMarker.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_SPACE3D)}/LightNavSpawnerMarker.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_SPACE3D)}/NavSpawnerMarker.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_SPACE3D)}/SpawnerMarker.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_SPACE3D)}/ProximityChecker.cs"
            };
            AssetDatabase.ExportPackage(
                    toExportPaths,
                    GetPackageName("botai"),
                    ExportPackageOptions.Recurse);
            DebugLogExportEnd("Bot AI");
        }

        [UnityEditor.MenuItem("Tools/RSToolkit/Export Package/UI/Adjust Layout Tools")]
        public static void ExportAdjustLayoutTools(){
            DebugLogExportStart("Adjust Layout Tools");
            var toExportPaths = new string[]{
                "Assets/RSToolKit/Scripts/UI/Layout",
            };
            AssetDatabase.ExportPackage(
                    toExportPaths,
                    GetPackageName("adjustlayouttools"),
                    ExportPackageOptions.Recurse);
            DebugLogExportEnd("Adjust Layout Tools");
        }

        [UnityEditor.MenuItem("Tools/RSToolkit/Export Package/Character/2D")]
        public static void ExportCharacter2D(){

            DebugLogExportStart("Character 2D");
            var toExportPaths = new string[]{
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_CHARACTER)}/.",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_SPACE2D)}/CharacterController2D.cs",
                $"{GetPath(PATH_TYPE.SCRIPTS, RS_SUBPATH_SPACE2D)}/PlayerMovement2D.cs"
            };
            AssetDatabase.ExportPackage(
                    toExportPaths,
                    GetPackageName("character2d"),
                    ExportPackageOptions.Recurse);
            DebugLogExportEnd("Character 2D");
        }

    }
}