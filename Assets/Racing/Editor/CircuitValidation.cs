using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Run via -batchmode -executeMethod CircuitValidation.Run in a disposable project copy.
public static class CircuitValidation
{
    [InitializeOnLoadMethod]
    static void ResumePlayValidation()
    {
        if (SessionState.GetBool("CircuitValidation.Pending", false))
            EditorApplication.update += ValidatePlayMode;
    }

    static void ValidatePlayMode()
    {
        if (!EditorApplication.isPlaying || Time.time < 1f) return;
        try
        {
            var session = UnityEngine.Object.FindFirstObjectByType<CircuitSession>();
            var vehicle = UnityEngine.Object.FindFirstObjectByType<Movement>();
            Check(session != null && session.Progress != null, "runtime session initialized");
            Check(vehicle != null && vehicle.enabled, "runtime vehicle enabled");
            var body = vehicle.GetComponent<Rigidbody>();
            if (!SessionState.GetBool("CircuitValidation.Recovery", false))
            {
                var road = GameObject.Find("Road");
                Check(road != null && road.GetComponent<MeshCollider>().sharedMesh != null, "road collider generated");
                Check(Vector3.Dot(road.GetComponent<MeshFilter>().sharedMesh.normals[0], Vector3.up) > 0.99f, "road faces upward");
                Check(GameObject.Find("Gate 7") != null, "eight gates generated");
                Check(body.position.y > -1f && Vector3.Distance(body.position, new Vector3(65, 1, 6)) < 5f, "spawn on road");
                string[] arguments = Environment.GetCommandLineArgs();
                int screenshotOption = Array.IndexOf(arguments, "-circuitScreenshot");
                if (screenshotOption >= 0 && screenshotOption + 1 < arguments.Length)
                    CapturePreview(arguments[screenshotOption + 1]);
                body.position = new Vector3(0, 3, 0);
                body.linearVelocity = Vector3.right * 20f;
                body.angularVelocity = Vector3.one;
                session.RequestRecovery();
                SessionState.SetBool("CircuitValidation.Recovery", true);
                return;
            }
            if (Time.time < 2f) return;
            Check(Vector3.Distance(body.position, new Vector3(65, 1, 6)) < 5f, "recovery pose restored");
            Check(body.linearVelocity.magnitude < 5f, "recovery clears momentum");
            Check(session.Progress.NextGate == 1 && session.Progress.CompletedLaps == 0, "recovery awards no progress");
            SessionState.SetBool("CircuitValidation.Pending", false);
            Debug.Log("CIRCUIT PLAY VALIDATION PASSED: generated road, upward normals, gates, vehicle spawn and recovery.");
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            SessionState.SetBool("CircuitValidation.Pending", false);
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    static void CapturePreview(string path)
    {
        var camera = UnityEngine.Object.FindFirstObjectByType<CameraMotion>().GetComponent<Camera>();
        var target = RenderTexture.GetTemporary(1280, 720, 24);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        var image = new Texture2D(1280, 720, TextureFormat.RGB24, false);
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            image.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            image.Apply();
            System.IO.File.WriteAllBytes(path, image.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(target);
            UnityEngine.Object.DestroyImmediate(image);
        }
    }

    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("Circuit validation failed: " + message);
    }

    public static void Run()
    {
        try
        {
            var progress = new CircuitProgress(8);
            Check(progress.NextGate == 1 && progress.LastGate == 0, "initial spawn");
            Check(!progress.TryPass(0) && !progress.TryPass(3), "finish and skipped gate rejected");
            for (int lap = 1; lap <= 2; lap++)
            {
                for (int gate = 1; gate < 8; gate++)
                {
                    Check(progress.TryPass(gate), "ordered checkpoint");
                    Check(!progress.TryPass(gate), "duplicate checkpoint rejected");
                }
                Check(progress.CompletedLaps == lap - 1, "no premature lap");
                Check(progress.TryPass(0) && progress.CompletedLaps == lap, "full lap");
            }
            Check(!progress.TryPass(-1) && !progress.TryPass(8), "invalid gate IDs rejected");
            Check(CircuitProgress.CrossedGate(new Vector3(0, 0, -100), new Vector3(0, 0, 100), 8, 3), "fast forward sweep");
            Check(!CircuitProgress.CrossedGate(Vector3.forward, Vector3.back, 8, 3), "backward rejected");
            Check(!CircuitProgress.CrossedGate(new Vector3(9, 0, -1), new Vector3(9, 0, 1), 8, 3), "outside width rejected");
            Check(!CircuitProgress.CrossedGate(new Vector3(0, 4, -1), new Vector3(0, 4, 1), 8, 3), "above gate rejected");
            Check(!CircuitProgress.CrossedGate(Vector3.zero, Vector3.zero, 8, 3), "stationary rejected");
            Check(CircuitProgress.CrossedGate(new Vector3(-20, 0, -1), new Vector3(20, 0, 1), 8, 3), "intersection rather than endpoint");
            EditorSceneManager.OpenScene("Assets/Scenes/MVP Circuit.unity");
            var builder = UnityEngine.Object.FindFirstObjectByType<CircuitBuilder>();
            Check(builder != null, "scene builder");
            var vehicle = new SerializedObject(builder).FindProperty("vehicle").objectReferenceValue as Movement;
            Check(vehicle != null && vehicle.tires.Length == 4, "existing vehicle reference");
            foreach (var tire in vehicle.tires) Check(tire != null, "wheel reference");
            Debug.Log("CIRCUIT VALIDATION PASSED: progress, directional sweep, scene and vehicle references.");
            SessionState.SetBool("CircuitValidation.Pending", true);
            SessionState.SetBool("CircuitValidation.Recovery", false);
            EditorApplication.EnterPlaymode();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }
}
