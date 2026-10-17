using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class CreateScene
{
    const string ScenePath = "Assets/Scenes/Main.unity";

    public static void Run()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var app = new GameObject("CameraApp");
        app.AddComponent<CameraApp>();
        app.AddComponent<ObjectDetector>();
        
        Directory.CreateDirectory("Assets/Scenes");
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[] { new EditorBuildSettingsScene(ScenePath, true) };
        Debug.Log("Scene written to " + ScenePath);
    }
}
