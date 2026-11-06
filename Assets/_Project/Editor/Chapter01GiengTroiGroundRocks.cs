using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// One-time visual pass for the approved camp. No gameplay object or collider is changed.
public static class Chapter01GiengTroiGroundRocks
{
    private const string ScenePath = "Assets/_Project/Scenes/02_HangLinhMach.unity";
    private const string ArtPath = "Assets/_Project/Art/Chapter01GiengTroi";
    private const string CampPath = "World_Geometry/ENV_C1_ExpandedWorld/ENV_01_TraiGiengTroi";

    [MenuItem("Tools/Son Doong/Chapter 01/Polish Gieng Troi Ground and Rocks")]
    public static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != ScenePath || scene.isDirty ||
            PrefabStageUtility.GetCurrentPrefabStage() != null)
            throw new InvalidOperationException("Open the clean C1 scene in Edit Mode, outside Prefab Stage.");

        var camp = GameObject.Find(CampPath).transform;
        var art = camp.Find("ART_C1_GiengTroi_20261106");
        if (!art || art.Find("Ground_Rock_Polish"))
            throw new InvalidOperationException("Approved camp art is missing or ground polish already exists.");
        var floor = camp.Find("Walkable_NaturalFloor_0");
        var floorCollider = floor.GetComponent<MeshCollider>();
        var floorRenderer = floor.GetComponent<MeshRenderer>();
        var trail = camp.Find("ChamberTrail_0");
        var pavers = art.Find("Path_HandlaidIrregular");
        if (!floorCollider || !floorRenderer || !trail || !pavers || pavers.childCount != 30 ||
            !GameObject.Find("Gameplay/Campfire_Placeholder") ||
            !GameObject.Find("Gameplay/Camp_Interactables/PF_LanternPickup") ||
            !GameObject.Find("Gameplay/Seal_CampExit"))
            throw new InvalidOperationException("Camp geometry or gameplay anchors differ from the surveyed scene.");

        var soilSource = NeedMaterial("Assets/_Project/Art/Chapter01Expanded/Materials/Cave_Soil.mat");
        var mossSource = NeedMaterial("Assets/_Project/Art/Chapter01Expanded/Materials/Moss_Soil.mat");
        var stoneSource = NeedMaterial("Assets/_Project/Art/Chapter01Expanded/Materials/Limestone_Moss.mat");
        var pathSource = NeedMaterial("Assets/_Project/Art/Chapter01Expanded/Materials/Worn_Path_Stone.mat");
        var soil = CopyMaterial(soilSource, "M_GT_Ground_Fine", new Color(.64f, .62f, .53f), new Vector2(8, 8));
        var beaten = CopyMaterial(soilSource, "M_GT_Trail_Beaten", new Color(.49f, .50f, .44f), Vector2.one);
        var moss = CopyMaterial(mossSource, "M_GT_Ground_MossIslands", new Color(.37f, .48f, .28f), Vector2.one);
        var stoneMids = new[]
        {
            CopyMaterial(stoneSource, "M_GT_Rock_Gray", new Color(.62f, .68f, .69f), new Vector2(1.8f, 1.8f)),
            CopyMaterial(stoneSource, "M_GT_Rock_Warm", new Color(.72f, .70f, .63f), new Vector2(2.0f, 2.0f)),
            CopyMaterial(stoneSource, "M_GT_Rock_Moss", new Color(.53f, .63f, .56f), new Vector2(1.65f, 1.65f))
        };
        var paverMats = new[]
        {
            CopyMaterial(pathSource, "M_GT_Paver_Gray", new Color(.54f, .57f, .54f), new Vector2(1.9f, 1.9f)),
            CopyMaterial(pathSource, "M_GT_Paver_Warm", new Color(.61f, .59f, .53f), new Vector2(2.0f, 2.0f)),
            CopyMaterial(pathSource, "M_GT_Paver_Moss", new Color(.46f, .55f, .47f), new Vector2(1.8f, 1.8f))
        };

        var trailMesh = SaveMesh("NaturalTrail_Tiled", BuildTrail(floorCollider));
        var mossMesh = SaveMesh("GroundMoss_Islands", BuildMossIslands(floorCollider));
        var root = NewChild(art, "Ground_Rock_Polish");
        var groundRoot = NewChild(root, "Ground_Detail");
        var rockRoot = NewChild(root, "Rocks_FootAndRim");

        Undo.RecordObject(floorRenderer, "Tile and tint camp floor");
        floorRenderer.sharedMaterial = soil;
        Undo.RecordObject(trail.gameObject, "Retire stretched trail visual");
        trail.gameObject.SetActive(false);
        AddMesh(groundRoot, "Trail_NaturalSoil", trailMesh, beaten);
        AddMesh(groundRoot, "Moss_Islands", mossMesh, moss);

        for (int i = 0; i < pavers.childCount; i++)
        {
            var renderer = pavers.GetChild(i).GetComponent<MeshRenderer>();
            Undo.RecordObject(renderer, "Vary camp stone color");
            renderer.sharedMaterial = paverMats[(i * 7 + i / 4) % paverMats.Length];
        }

        // The imported cave rock silhouettes already exist at the chamber edge.
        // Small copies add scale variation; all stay visual-only and away from the route.
        var rockA = camp.Find("Cliff_Buttress_2").GetComponentInChildren<MeshFilter>().sharedMesh;
        var rockB = camp.Find("Cliff_Buttress_3").GetComponentInChildren<MeshFilter>().sharedMesh;
        for (int i = 0; i < 24; i++)
        {
            bool left = i % 2 == 0;
            float z = (left ? -9.8f + (i / 2) * 2f : -14.4f + (i / 2) * 2.4f)
                + .24f * Mathf.Sin(i * 1.73f);
            float x = (left ? -1 : 1) * (7.2f + (i % 4) * .88f);
            float s = .10f + (i % 4) * .023f;
            var mesh = i % 3 == 0 ? rockA : rockB;
            var rock = AddMesh(rockRoot, "Limestone_Foot_" + i.ToString("00"), mesh,
                stoneMids[(i * 5 + i / 3) % stoneMids.Length]);
            rock.transform.localScale = new Vector3(s * (1.0f + .25f * Mathf.Sin(i)),
                s * (.55f + .12f * (i % 3)), s * (1.1f + .18f * (i % 4)));
            float y = FloorY(floorCollider, x, z) - mesh.bounds.min.y * rock.transform.localScale.y - .11f;
            rock.transform.position = new Vector3(x, y, z);
            rock.transform.rotation = Quaternion.Euler(0, (i * 67) % 360, 0);
        }

        int[] varied = { 1, 3, 4, 6, 7, 9, 10, 12, 13, 17, 19, 20, 22, 23, 25, 26, 28, 29, 30 };
        for (int i = 0; i < varied.Length; i++)
        {
            var buttress = camp.Find("Cliff_Buttress_" + varied[i]);
            if (!buttress) continue;
            var renderer = buttress.GetComponentInChildren<MeshRenderer>();
            Undo.RecordObject(renderer, "Vary camp cliff texture scale and color");
            renderer.sharedMaterial = stoneMids[(i + i / 3) % stoneMids.Length];
        }

        RefineUvInternal(camp, art, floor, pavers);
        RetireOldRoomPavingInternal();

        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Gieng Troi ground polish: tiled soil, natural trail, moss islands, 30 pavers, 24 visual rock accents, 19 cliff variants. Scene not saved.");
    }

    [MenuItem("Tools/Son Doong/Chapter 01/Refine Gieng Troi Ground UV")]
    public static void RefineUv()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != ScenePath || PrefabStageUtility.GetCurrentPrefabStage() != null)
            throw new InvalidOperationException("Open the C1 scene in Edit Mode, outside Prefab Stage.");
        var camp = GameObject.Find(CampPath).transform;
        var art = camp.Find("ART_C1_GiengTroi_20261106");
        var polish = art ? art.Find("Ground_Rock_Polish") : null;
        if (!polish) throw new InvalidOperationException("Ground polish must be applied first.");
        RefineUvInternal(camp, art, camp.Find("Walkable_NaturalFloor_0"),
            art.Find("Path_HandlaidIrregular"));
        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Gieng Troi visual UV refined; original floor collider and source meshes unchanged. Scene not saved.");
    }

    [MenuItem("Tools/Son Doong/Chapter 01/Retire Old Gieng Troi Paving")]
    public static void RetireOldRoomPaving()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != ScenePath || PrefabStageUtility.GetCurrentPrefabStage() != null)
            throw new InvalidOperationException("Open the C1 scene in Edit Mode, outside Prefab Stage.");
        RetireOldRoomPavingInternal();
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Four old camp-only visual paving meshes inactive; collider and NavMesh unchanged. Scene not saved.");
    }

    private static void RetireOldRoomPavingInternal()
    {
        var root = GameObject.Find("World_Geometry/ENV_C1_ExpandedWorld/Handlaid_Paths");
        if (!root) throw new InvalidOperationException("Handlaid_Paths is missing.");
        for (int i = 0; i < 4; i++)
        {
            var t = root.transform.Find("Flagstones_Room_0_" + i);
            if (!t || t.GetComponent<Collider>() || t.childCount != 0)
                throw new InvalidOperationException("Old room 0 paving differs from the surveyed visual-only meshes.");
            Undo.RecordObject(t.gameObject, "Retire repeated camp paving mesh");
            t.gameObject.SetActive(false);
        }
    }

    private static void RefineUvInternal(Transform camp, Transform art, Transform floor, Transform pavers)
    {
        var polish = art.Find("Ground_Rock_Polish");
        if (polish.Find("UV_Refined"))
            throw new InvalidOperationException("Gieng Troi visual UV already refined.");
        var original = floor.GetComponent<MeshFilter>().sharedMesh;
        var floorMesh = AssetDatabase.LoadAssetAtPath<Mesh>(ArtPath + "/Ground_PlanarUV.asset");
        if (!floorMesh)
        {
            floorMesh = UnityEngine.Object.Instantiate(original);
            floorMesh.name = "Ground_PlanarUV";
            var vertices = floorMesh.vertices;
            var uv = new Vector2[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
                uv[i] = new Vector2(vertices[i].x / 36f + .5f, vertices[i].z / 40f + .5f);
            floorMesh.uv = uv;
            AssetDatabase.CreateAsset(floorMesh, ArtPath + "/Ground_PlanarUV.asset");
        }
        Undo.RecordObject(floor.GetComponent<MeshFilter>(), "Use planar UV on camp floor visual");
        floor.GetComponent<MeshFilter>().sharedMesh = floorMesh;

        var centers = new[] { new Vector2(.39f, .41f), new Vector2(.61f, .38f), new Vector2(.45f, .65f) };
        var meshes = new Mesh[3];
        for (int i = 0; i < 3; i++)
            meshes[i] = SaveMesh("Flagstone_Refined_" + (char)('A' + i), BuildPaver(i, centers[i]));
        for (int i = 0; i < pavers.childCount; i++)
        {
            var filter = pavers.GetChild(i).GetComponent<MeshFilter>();
            Undo.RecordObject(filter, "Use corrected UV on camp paver");
            filter.sharedMesh = meshes[i % 3];
        }
        var paverTints = new[]
        {
            new Color(.71f, .74f, .71f), new Color(.77f, .72f, .65f),
            new Color(.64f, .71f, .65f)
        };
        var names = new[] { "M_GT_Paver_Gray", "M_GT_Paver_Warm", "M_GT_Paver_Moss" };
        for (int i = 0; i < names.Length; i++)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(ArtPath + "/" + names[i] + ".mat");
            mat.SetTextureScale("_BaseMap", Vector2.one);
            mat.SetColor("_BaseColor", paverTints[i]);
            EditorUtility.SetDirty(mat);
        }
        NewChild(polish, "UV_Refined");
    }

    private static Mesh BuildPaver(int variant, Vector2 uvCenter)
    {
        const int sides = 9;
        var vertices = new Vector3[1 + sides * 3];
        var uv = new Vector2[vertices.Length];
        vertices[0] = new Vector3(0, .09f, 0);
        uv[0] = uvCenter;
        for (int j = 0; j < sides; j++)
        {
            float a = j * Mathf.PI * 2f / sides;
            float r = .78f + .10f * Mathf.Sin(j * 3.4f + variant * 2.2f);
            float x = Mathf.Cos(a) * r, z = Mathf.Sin(a) * r * .82f;
            vertices[1 + j] = new Vector3(x * .85f, .09f + .008f * Mathf.Sin(j + variant), z * .85f);
            vertices[1 + sides + j] = vertices[1 + j];
            vertices[1 + sides * 2 + j] = new Vector3(x, .005f, z);
            var sample = uvCenter + new Vector2(x, z) * .08f;
            uv[1 + j] = sample;
            uv[1 + sides + j] = sample;
            uv[1 + sides * 2 + j] = sample + new Vector2(.012f, .008f);
        }
        var triangles = new int[sides * 9];
        int p = 0;
        for (int j = 0; j < sides; j++)
        {
            int next = (j + 1) % sides;
            triangles[p++] = 0; triangles[p++] = 1 + next; triangles[p++] = 1 + j;
            int a = 1 + sides + j, b = 1 + sides + next;
            int c = 1 + sides * 2 + j, d = 1 + sides * 2 + next;
            triangles[p++] = a; triangles[p++] = b; triangles[p++] = c;
            triangles[p++] = b; triangles[p++] = d; triangles[p++] = c;
        }
        var mesh = new Mesh { vertices = vertices, uv = uv, triangles = triangles };
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }

    private static Material NeedMaterial(string path)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!mat || mat.shader.name != "Universal Render Pipeline/Lit")
            throw new InvalidOperationException("Missing URP Lit material: " + path);
        return mat;
    }

    private static Material CopyMaterial(Material source, string name, Color tint, Vector2 tiling)
    {
        string path = ArtPath + "/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat) return mat;
        mat = new Material(source) { name = name, enableInstancing = true };
        mat.SetColor("_BaseColor", tint);
        mat.SetTextureScale("_BaseMap", tiling);
        mat.SetFloat("_Smoothness", .08f);
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    private static Mesh SaveMesh(string name, Mesh mesh)
    {
        string path = ArtPath + "/" + name + ".asset";
        var prior = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (prior) return prior;
        mesh.name = name;
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static Transform NewChild(Transform parent, string name)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Polish Gieng Troi ground");
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    private static GameObject AddMesh(Transform parent, string name, Mesh mesh, Material mat)
    {
        var go = NewChild(parent, name).gameObject;
        go.layer = LayerMask.NameToLayer("Environment");
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = mat;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return go;
    }

    private static float FloorY(MeshCollider floor, float x, float z)
    {
        if (!floor.Raycast(new Ray(new Vector3(x, 4, z), Vector3.down), out RaycastHit hit, 9))
            throw new InvalidOperationException("No camp floor at " + x + ", " + z);
        return hit.point.y;
    }

    private static Mesh BuildTrail(MeshCollider floor)
    {
        const int rows = 53, columns = 5;
        var vertices = new Vector3[rows * columns];
        var uv = new Vector2[vertices.Length];
        for (int row = 0; row < rows; row++)
        {
            float z = -14f + row * 28.5f / (rows - 1);
            float drift = .13f * Mathf.Sin(row * .52f);
            float width = 2.34f + .14f * Mathf.Sin(row * 1.13f);
            for (int col = 0; col < columns; col++)
            {
                float x = drift + width * (col - 2) / 2f;
                int k = row * columns + col;
                vertices[k] = new Vector3(x, FloorY(floor, x, z) + .016f, z);
                uv[k] = new Vector2(x / 3.2f, z / 3.2f);
            }
        }
        var triangles = new int[(rows - 1) * (columns - 1) * 6];
        int p = 0;
        for (int row = 0; row < rows - 1; row++)
            for (int col = 0; col < columns - 1; col++)
            {
                int a = row * columns + col, b = a + 1, c = a + columns, d = c + 1;
                triangles[p++] = a; triangles[p++] = c; triangles[p++] = b;
                triangles[p++] = b; triangles[p++] = c; triangles[p++] = d;
            }
        var mesh = new Mesh { vertices = vertices, uv = uv, triangles = triangles };
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh BuildMossIslands(MeshCollider floor)
    {
        var centers = new[]
        {
            new Vector3(-9,0,-9), new Vector3(-10.1f,0,-2), new Vector3(-9.2f,0,5),
            new Vector3(-8.2f,0,10), new Vector3(9.3f,0,-12), new Vector3(10.2f,0,-5),
            new Vector3(9.4f,0,3), new Vector3(8.7f,0,11)
        };
        const int sides = 16;
        var vertices = new Vector3[centers.Length * (sides + 1)];
        var uv = new Vector2[vertices.Length];
        var triangles = new int[centers.Length * sides * 3];
        int p = 0;
        for (int n = 0; n < centers.Length; n++)
        {
            var c = centers[n];
            int start = n * (sides + 1);
            vertices[start] = new Vector3(c.x, FloorY(floor, c.x, c.z) + .022f, c.z);
            uv[start] = new Vector2(c.x / 3.1f, c.z / 3.1f);
            for (int j = 0; j < sides; j++)
            {
                float a = j * Mathf.PI * 2f / sides;
                float r = 2.15f + .34f * Mathf.Sin(j * 2.8f + n * 1.6f);
                float x = c.x + Mathf.Cos(a) * r * (1.05f + .16f * (n % 3));
                float z = c.z + Mathf.Sin(a) * r * (.72f + .10f * (n % 2));
                int k = start + 1 + j;
                vertices[k] = new Vector3(x, FloorY(floor, x, z) + .022f, z);
                uv[k] = new Vector2(x / 3.1f, z / 3.1f);
                triangles[p++] = start; triangles[p++] = start + 1 + (j + 1) % sides;
                triangles[p++] = start + 1 + j;
            }
        }
        var mesh = new Mesh { vertices = vertices, uv = uv, triangles = triangles };
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }
}
