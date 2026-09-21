#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class HoanKiemStreetUpgradeRunner
{
    public static void Run()
    {
        string statusFile = Path.Combine(Application.dataPath, "upgrade_runner_status.txt");
        File.WriteAllText(statusFile, "Starting runner...\n");

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/HoanKiem.unity", OpenSceneMode.Single);
        File.AppendAllText(statusFile, $"Opened scene: {scene.name}\n");

        HoanKiemAirVR.Editor.HoanKiemStreetEnvironmentSetup.UpgradeAllScene();
        File.AppendAllText(statusFile, "UpgradeAllScene finished!\n");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        File.AppendAllText(statusFile, "Scene saved successfully!\n");

        if (Application.isBatchMode)
        {
            EditorApplication.Exit(0);
        }
    }
}
#endif
