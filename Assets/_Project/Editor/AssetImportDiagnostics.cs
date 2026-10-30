using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

// Editor-only validation. The request file allows a local repair tool to request
// an import check without changing the open scene or entering Play Mode.
[InitializeOnLoad]
public static class AssetImportDiagnostics
{
    private const string RequestPath = "Library/SonDoongImportValidation.request.json";
    private static double nextCheck;
    private static bool running;

    [Serializable] public class Request
    {
        public string phase;
        public bool reimport;
        public string[] paths;
    }

    [Serializable] public class Entry
    {
        public string path;
        public string name;
        public string kind;
        public int vertices;
        public Vector3 boundsCenter;
        public Vector3 boundsSize;
        public string shader;
        public bool supported;
    }

    [Serializable] public class Report
    {
        public string phase;
        public string editorVersion;
        public List<Entry> assets = new List<Entry>();
        public List<string> importMessages = new List<string>();
        public List<string> failures = new List<string>();
    }

    static AssetImportDiagnostics()
    {
        EditorApplication.update += CheckRequest;
    }

    private static void CheckRequest()
    {
        if (running || EditorApplication.timeSinceStartup < nextCheck)
            return;
        nextCheck = EditorApplication.timeSinceStartup + 1;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || !File.Exists(RequestPath))
            return;
        RunRequest();
    }

    public static void RunRequest()
    {
        if (!File.Exists(RequestPath))
            return;
        running = true;
        var report = new Report { editorVersion = Application.unityVersion };
        Application.LogCallback capture = (message, stack, type) =>
        {
            if (type == LogType.Warning || type == LogType.Error || type == LogType.Exception
                || message.Contains("self-intersecting") || message.Contains("uniqueness violation"))
                report.importMessages.Add(type + ": " + message);
        };
        string output = "Logs/AssetImportValidation.json";
        try
        {
            var request = JsonUtility.FromJson<Request>(File.ReadAllText(RequestPath));
            report.phase = request.phase;
            Application.logMessageReceived += capture;
            foreach (string path in request.paths)
            {
                if (!File.Exists(path))
                {
                    report.failures.Add("Missing asset: " + path);
                    continue;
                }
                if (request.reimport)
                    AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
                {
                    if (asset is Mesh mesh)
                    {
                        report.assets.Add(new Entry { path = path, name = mesh.name, kind = "Mesh",
                            vertices = mesh.vertexCount, boundsCenter = mesh.bounds.center, boundsSize = mesh.bounds.size });
                        if (mesh.vertexCount == 0)
                            report.failures.Add("Empty mesh: " + path + "/" + mesh.name);
                    }
                    else if (asset is Material material)
                    {
                        Shader shader = material.shader;
                        report.assets.Add(new Entry { path = path, name = material.name, kind = "Material",
                            shader = shader == null ? "Missing" : shader.name,
                            supported = shader != null && shader.isSupported });
                        if (shader == null || shader.name == "Hidden/InternalErrorShader")
                            report.failures.Add("Missing/error shader: " + path + "/" + material.name);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            report.failures.Add(exception.ToString());
        }
        finally
        {
            Application.logMessageReceived -= capture;
            Directory.CreateDirectory("Logs");
            File.WriteAllText(output, JsonUtility.ToJson(report, true));
            File.Delete(RequestPath);
            running = false;
        }
        Debug.Log("Asset import diagnostics: " + report.phase + ", failures=" + report.failures.Count
            + ", import messages=" + report.importMessages.Count + ". Report: " + output);
    }

    [MenuItem("Tools/Son Doong/Validate Selected Imported Assets")]
    private static void ValidateSelection()
    {
        var paths = new HashSet<string>();
        foreach (UnityEngine.Object selected in Selection.objects)
        {
            string path = AssetDatabase.GetAssetPath(selected);
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                paths.Add(path);
        }
        var request = new Request { phase = "selection", reimport = true, paths = new List<string>(paths).ToArray() };
        Directory.CreateDirectory("Library");
        File.WriteAllText(RequestPath, JsonUtility.ToJson(request));
        RunRequest();
    }
}
