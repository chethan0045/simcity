#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Simcity.EditorTools
{
    /// <summary>
    /// Headless build entry points for CI / command line. Living in an `Editor` folder, this
    /// compiles only in the editor and ships with nothing.
    ///
    /// Examples:
    ///   Unity -batchmode -quit -projectPath . -executeMethod Simcity.EditorTools.BuildScript.BuildWindows
    ///   Unity -batchmode -quit -projectPath . -executeMethod Simcity.EditorTools.BuildScript.BuildAndroid
    ///
    /// Set BUILD_VERSION in the environment to stamp the player version. Builds use whatever
    /// scenes are enabled in Build Settings — this project builds its world procedurally at
    /// runtime, so any single (even empty) scene is enough; see PHASE9.md.
    /// </summary>
    public static class BuildScript
    {
        public static void BuildWindows() =>
            Build(BuildTarget.StandaloneWindows64, "Build/Windows/Simcity.exe");

        public static void BuildAndroid() =>
            Build(BuildTarget.Android, "Build/Android/Simcity.apk");

        private static void Build(BuildTarget target, string outputPath)
        {
            string version = Environment.GetEnvironmentVariable("BUILD_VERSION");
            if (!string.IsNullOrEmpty(version)) PlayerSettings.bundleVersion = version;

            string[] scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[Build] No scenes enabled in Build Settings. Add at least one " +
                               "(the world is built procedurally, so an empty scene works). Aborting.");
                EditorApplication.Exit(1);
                return;
            }

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = target,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log($"[Build] {target}: {summary.result} — {summary.totalSize} bytes, " +
                      $"{summary.totalErrors} errors, output: {outputPath}");

            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
#endif
