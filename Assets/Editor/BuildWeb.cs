using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Hushwake.Editor
{
    public static class BuildWeb
    {
        [MenuItem("Hushwake/Build Web")]
        public static void Run()
        {
            Audit.Run();
            Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Runtime bootstrap creates the world after scene load, in both Editor and Web players.
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Hushwake.unity");
            PlayerSettings.companyName = "Rakeem Chapman";
            PlayerSettings.productName = "Hushwake";
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.defaultScreenWidth = 1280; PlayerSettings.defaultScreenHeight = 720;
            PlayerSettings.runInBackground = false;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.template = "PROJECT:Hushwake";
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
            PlayerSettings.WebGL.dataCaching = true;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            var input = settings.FindProperty("activeInputHandler"); if (input != null) { input.intValue = 0; settings.ApplyModifiedPropertiesWithoutUndo(); }
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Build/Web");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = new[] { "Assets/Scenes/Hushwake.unity" }, locationPathName = "Build/Web", target = BuildTarget.WebGL, options = BuildOptions.None });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("Unity Web build failed: " + report.summary.result);
            File.Copy("vercel.json", "Build/Web/vercel.json", true);
            File.WriteAllText("Build/Web/build-info.json", "{\"engine\":\"Unity\",\"editor\":\"" + Application.unityVersion + "\",\"version\":\"0.1.0\",\"builtUtc\":\"" + DateTime.UtcNow.ToString("O") + "\"}");
            Debug.Log("HUSHWAKE_WEB_BUILD_SUCCEEDED bytes=" + report.summary.totalSize);
        }
    }
}
