using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Explicit, one-time art pass. Gameplay roots, colliders, IDs and source FBX stay untouched.
public static class Chapter01GiengTroiRevision
{
    private const string ScenePath = "Assets/_Project/Scenes/02_HangLinhMach.unity";
    private const string ArtPath = "Assets/_Project/Art/Chapter01GiengTroi";
    private const string WorldPath = "World_Geometry/ENV_C1_ExpandedWorld/";
    private const string CampPath = WorldPath + "ENV_01_TraiGiengTroi";
    private const string LinkPath = WorldPath + "LINK_1_01_TraiGiengTroi_to_02_VuonReu";
    private const string CampArtName = "ART_C1_GiengTroi_20261106";
    private const string LinkArtName = "ART_C1_Link1_ThachNhu_20261106";

    [MenuItem("Tools/Son Doong/Chapter 01/Apply Gieng Troi Art")]
    public static void Apply()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (Application.isPlaying || scene.path != ScenePath || scene.isDirty)
            throw new InvalidOperationException("Open the clean 02_HangLinhMach scene in Edit Mode first.");

        var camp = Need(CampPath).transform;
        var link = Need(LinkPath).transform;
        if (camp.Find(CampArtName) || link.Find(LinkArtName))
            throw new InvalidOperationException("Gieng Troi art already exists; this pass must not run twice.");
        var fire = Need("Gameplay/Campfire_Placeholder").GetComponent<CheckpointAltar>();
        if (!fire || fire.CheckpointId != "cp_trai" || !Need("Gameplay/Camp_Interactables/PF_LanternPickup").GetComponent<LanternPickup>() ||
            !Need("Gameplay/Seal_CampExit").GetComponent<SealTarget>())
            throw new InvalidOperationException("Camp gameplay references differ from the surveyed scene.");

        EnsureArtFolder();
        var sourceStone = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Art/Chapter01Expanded/Materials/Limestone_Moss.mat");
        var sourcePath = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Art/Chapter01Expanded/Materials/Worn_Path_Stone.mat");
        if (!sourceStone || !sourcePath || sourceStone.shader.name != "Universal Render Pipeline/Lit" ||
            sourcePath.shader.name != "Universal Render Pipeline/Lit")
            throw new InvalidOperationException("Expected URP Lit source materials are missing.");

        var stoneLight = MaterialCopy(sourceStone, "M_GT_Limestone_Light", new Color(.69f, .75f, .75f));
        var stoneShadow = MaterialCopy(sourceStone, "M_GT_Limestone_Shadow", new Color(.49f, .57f, .57f));
        var pathMaterials = new[]
        {
            MaterialCopy(sourcePath, "M_GT_Path_Warm", new Color(.68f, .66f, .58f)),
            MaterialCopy(sourcePath, "M_GT_Path_Cool", new Color(.57f, .61f, .57f)),
            MaterialCopy(sourcePath, "M_GT_Path_Light", new Color(.75f, .71f, .62f))
        };
        var dripHigh = MeshAsset("Stalactite_High", () => StalactiteMesh(11, 5));
        var dripLow = MeshAsset("Stalactite_Low", () => StalactiteMesh(6, 3));
        var pathMeshes = new[]
        {
            MeshAsset("Flagstone_A", () => FlagstoneMesh(0)),
            MeshAsset("Flagstone_B", () => FlagstoneMesh(1)),
            MeshAsset("Flagstone_C", () => FlagstoneMesh(2))
        };
        var shaftMesh = AssetDatabase.LoadAssetAtPath<Mesh>(
            "Assets/_Project/Art/Chapter01/Meshes/SunShaft_Camp.asset");
        var shaftSource = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Art/Chapter01/Materials/SunShaft.mat");
        if (!shaftMesh || !shaftSource || shaftSource.shader.name != "SonDoong/CaveLightShaft")
            throw new InvalidOperationException("Existing camp sun shaft art is missing.");
        var shaftMaterial = AssetDatabase.LoadAssetAtPath<Material>(ArtPath + "/M_GT_SunRays.mat");
        if (!shaftMaterial)
        {
            shaftMaterial = new Material(shaftSource) { name = "M_GT_SunRays" };
            shaftMaterial.SetColor("_BaseColor", new Color(1, .82f, .53f, .11f));
            AssetDatabase.CreateAsset(shaftMaterial, ArtPath + "/M_GT_SunRays.mat");
        }

        var campArt = NewChild(camp, CampArtName).transform;
        var pathRoot = NewChild(campArt, "Path_HandlaidIrregular").transform;
        var dripRoot = NewChild(campArt, "Roof_Stalactites").transform;
        var foliageRoot = NewChild(campArt, "Rim_Ferns").transform;
        var linkArt = NewChild(link, LinkArtName).transform;
        var linkDrips = NewChild(linkArt, "Roof_Stalactites").transform;
        var shaft = NewChild(campArt, "Skylight_SunRays");
        shaft.transform.localPosition = new Vector3(0, .2f, 0);
        shaft.transform.localScale = new Vector3(1.35f, 1.68f, 1.5f);
        shaft.AddComponent<MeshFilter>().sharedMesh = shaftMesh;
        var shaftRenderer = shaft.AddComponent<MeshRenderer>();
        shaftRenderer.sharedMaterial = shaftMaterial;
        shaftRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        shaftRenderer.receiveShadows = false;

        // Keep the original paving in the scene as inactive history; retain the continuous floor.
        int oldStones = 0;
        foreach (Transform child in camp)
        {
            if (!child.name.StartsWith("Handlaid_Slab_", StringComparison.Ordinal)) continue;
            Undo.RecordObject(child.gameObject, "Replace repeated camp paving visuals");
            child.gameObject.SetActive(false);
            oldStones++;
        }
        var trail = Need(CampPath + "/ChamberTrail_0").GetComponent<MeshRenderer>();
        Undo.RecordObject(trail, "Tint camp trail");
        trail.sharedMaterial = pathMaterials[1];

        var floor = Need(CampPath + "/Walkable_NaturalFloor_0").GetComponent<MeshCollider>();
        for (int row = 0; row < 15; row++)
        {
            float z = -14.3f + row * 2.12f;
            for (int side = 0; side < 2; side++)
            {
                int k = row * 2 + side;
                float x = (side == 0 ? -1.06f : 1.06f) + Mathf.Sin(k * 2.17f) * .19f;
                float stoneZ = z + Mathf.Sin(k * 1.71f) * .23f;
                float groundY = FloorY(floor, x, stoneZ);
                var stone = NewChild(pathRoot, "Flagstone_" + k.ToString("00"));
                stone.transform.position = new Vector3(x, groundY + .015f, stoneZ);
                stone.transform.rotation = Quaternion.Euler(0, (k * 53) % 360, 0);
                stone.transform.localScale = new Vector3(1.1f + (k % 3) * .1f, 1, 1.18f + (k % 4) * .09f);
                stone.layer = LayerMask.NameToLayer("Environment");
                stone.AddComponent<MeshFilter>().sharedMesh = pathMeshes[k % 3];
                stone.AddComponent<MeshRenderer>().sharedMaterial = pathMaterials[k % 3];
            }
        }

        var roof = Need(CampPath + "/CaveVault_Skylight_0").GetComponent<MeshCollider>();
        var campSpots = new[]
        {
            new Vector4(-9, -4, 6.1f, 1.25f), new Vector4(-8.7f, -2.7f, 4.3f, .72f),
            new Vector4(9, 2, 6.7f, 1.3f), new Vector4(10.2f, 3.2f, 4.0f, .75f),
            new Vector4(-9, 14, 6.3f, 1.35f), new Vector4(-10.4f, 13.2f, 4.4f, .75f),
            new Vector4(9, 14, 5.1f, 1.0f), new Vector4(10.4f, 13.1f, 3.4f, .62f),
            new Vector4(-14, 2, 4.5f, .9f), new Vector4(14, -4, 4.8f, .9f)
        };
        for (int i = 0; i < campSpots.Length; i++)
            PlaceDrip(dripRoot, roof, campSpots[i], i, dripHigh, dripLow,
                i % 3 == 0 ? stoneShadow : stoneLight);

        var shell = Need(LinkPath + "/Winding_CaveShell_0").GetComponent<MeshCollider>();
        var linkSpots = new[]
        {
            new Vector4(-3, 23, 2.6f, .65f), new Vector4(3, 23, 2.2f, .57f),
            new Vector4(-3, 27, 2.8f, .72f), new Vector4(3, 37, 1.6f, .44f)
        };
        for (int i = 0; i < linkSpots.Length; i++)
            PlaceDrip(linkDrips, shell, linkSpots[i], i + 20, dripHigh, dripLow,
                i % 2 == 0 ? stoneLight : stoneShadow);

        var fern = Need(CampPath + "/Understory_Fern_0_2").GetComponent<MeshFilter>();
        var fernMaterial = fern.GetComponent<MeshRenderer>().sharedMaterial;
        var fernSpots = new[]
        {
            new Vector2(-11.5f, -7.8f), new Vector2(-12.4f, 4.8f),
            new Vector2(11.7f, -6.5f), new Vector2(12.1f, 8.8f),
            new Vector2(-9.8f, 10.2f), new Vector2(10.3f, 12.4f)
        };
        for (int i = 0; i < fernSpots.Length; i++)
        {
            var spot = fernSpots[i];
            var go = NewChild(foliageRoot, "Fern_Rim_" + i.ToString("00"));
            go.transform.position = new Vector3(spot.x, FloorY(floor, spot.x, spot.y) - .05f, spot.y);
            go.transform.rotation = Quaternion.Euler(0, (i * 73) % 360, 0);
            go.transform.localScale = Vector3.one * (1.05f + .15f * (i % 3));
            go.layer = LayerMask.NameToLayer("Environment");
            go.AddComponent<MeshFilter>().sharedMesh = fern.sharedMesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = fernMaterial;
        }

        // Alternate the already placed wall rocks, keeping their transforms and colliders.
        int[] darkButtresses = { 2, 5, 8, 11, 18, 21, 24, 27 };
        foreach (int index in darkButtresses)
        {
            var buttress = camp.Find("Cliff_Buttress_" + index);
            if (!buttress) continue;
            var renderer = buttress.GetComponentInChildren<MeshRenderer>();
            if (!renderer) continue;
            Undo.RecordObject(renderer, "Vary camp limestone shade");
            renderer.sharedMaterial = stoneShadow;
        }

        EditorSceneManager.MarkSceneDirty(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("Gieng Troi art applied: " + oldStones + " old slabs inactive, 30 irregular slabs, sun rays, " +
                  campSpots.Length + " camp stalactites, " + linkSpots.Length + " link stalactites, 6 rim ferns. Scene not saved.");
    }

    private static GameObject Need(string path)
    {
        var go = GameObject.Find(path);
        if (!go) throw new InvalidOperationException("Missing scene object: " + path);
        return go;
    }

    private static GameObject NewChild(Transform parent, string name)
    {
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Gieng Troi art");
        go.transform.SetParent(parent, false);
        return go;
    }

    private static void EnsureArtFolder()
    {
        if (!AssetDatabase.IsValidFolder(ArtPath))
            AssetDatabase.CreateFolder("Assets/_Project/Art", "Chapter01GiengTroi");
    }

    private static Material MaterialCopy(Material source, string name, Color color)
    {
        string path = ArtPath + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material) return material;
        material = new Material(source) { name = name };
        material.SetColor("_BaseColor", color);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Mesh MeshAsset(string name, Func<Mesh> create)
    {
        string path = ArtPath + "/" + name + ".asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (mesh) return mesh;
        mesh = create();
        mesh.name = name;
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static void PlaceDrip(Transform parent, MeshCollider roof, Vector4 spec, int seed,
        Mesh high, Mesh low, Material material)
    {
        var ray = new Ray(new Vector3(spec.x, 2, spec.y), Vector3.up);
        if (!roof.Raycast(ray, out RaycastHit hit, 32))
            throw new InvalidOperationException("No solid roof at " + spec.x + ", " + spec.y);
        float bottom = hit.point.y - spec.z;
        if (bottom < 4.8f)
            throw new InvalidOperationException("Stalactite would enter camera clearance at " + spec.x + ", " + spec.y);
        var go = NewChild(parent, "Stalactite_" + seed.ToString("00"));
        go.transform.position = hit.point + Vector3.up * .14f;
        go.transform.rotation = Quaternion.Euler(0, seed * 47, 0);
        go.transform.localScale = new Vector3(spec.w, spec.z, spec.w);
        go.layer = LayerMask.NameToLayer("Environment");
        var a = NewChild(go.transform, "LOD0");
        var b = NewChild(go.transform, "LOD1");
        a.layer = b.layer = LayerMask.NameToLayer("Environment");
        a.AddComponent<MeshFilter>().sharedMesh = high;
        b.AddComponent<MeshFilter>().sharedMesh = low;
        var ar = a.AddComponent<MeshRenderer>();
        var br = b.AddComponent<MeshRenderer>();
        ar.sharedMaterial = br.sharedMaterial = material;
        var lod = go.AddComponent<LODGroup>();
        lod.fadeMode = LODFadeMode.CrossFade;
        lod.SetLODs(new[] { new LOD(.22f, new Renderer[] { ar }), new LOD(.045f, new Renderer[] { br }) });
        lod.RecalculateBounds();
    }

    private static float FloorY(MeshCollider floor, float x, float z)
    {
        if (floor.Raycast(new Ray(new Vector3(x, 8, z), Vector3.down), out RaycastHit hit, 16))
            return hit.point.y;
        throw new InvalidOperationException("No walkable floor at " + x + ", " + z);
    }

    private static Mesh StalactiteMesh(int sides, int rings)
    {
        var verts = new Vector3[sides * rings + 1];
        var uv = new Vector2[verts.Length];
        for (int j = 0; j < rings; j++)
        {
            float t = (float)j / (rings - 1);
            float radius = Mathf.Pow(1 - t, 1.2f) * (1 + .07f * Mathf.Sin(j * 3.3f));
            for (int i = 0; i < sides; i++)
            {
                float angle = i * Mathf.PI * 2 / sides;
                float irregular = 1 + .13f * Mathf.Sin(i * 2.7f + j * 1.8f);
                verts[j * sides + i] = new Vector3(Mathf.Cos(angle) * radius * irregular,
                    -t + .025f * Mathf.Sin(i * 2.2f + j), Mathf.Sin(angle) * radius * irregular);
                uv[j * sides + i] = new Vector2((float)i / sides, t);
            }
        }
        verts[verts.Length - 1] = new Vector3(.04f, -1.04f, -.02f);
        uv[uv.Length - 1] = new Vector2(.5f, 1.03f);
        var triangles = new int[(rings - 1) * sides * 6 + sides * 3];
        int p = 0;
        for (int j = 0; j < rings - 1; j++)
            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides, a = j * sides + i, b = j * sides + next;
                int c = (j + 1) * sides + i, d = (j + 1) * sides + next;
                triangles[p++] = a; triangles[p++] = b; triangles[p++] = c;
                triangles[p++] = b; triangles[p++] = d; triangles[p++] = c;
            }
        for (int i = 0; i < sides; i++)
        {
            triangles[p++] = (rings - 1) * sides + i;
            triangles[p++] = (rings - 1) * sides + (i + 1) % sides;
            triangles[p++] = verts.Length - 1;
        }
        var mesh = new Mesh { vertices = verts, uv = uv, triangles = triangles };
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh FlagstoneMesh(int variant)
    {
        const int sides = 8;
        var verts = new Vector3[sides * 2 + 1];
        var uv = new Vector2[verts.Length];
        verts[verts.Length - 1] = new Vector3(0, .085f, 0);
        uv[uv.Length - 1] = new Vector2(.5f, .5f);
        for (int i = 0; i < sides; i++)
        {
            float a = i * Mathf.PI * 2 / sides;
            float r = .8f + .13f * Mathf.Sin(i * 3.4f + variant * 2.1f);
            float x = Mathf.Cos(a) * r, z = Mathf.Sin(a) * r * .82f;
            verts[i] = new Vector3(x * .82f, .085f + .013f * Mathf.Sin(i + variant), z * .82f);
            verts[sides + i] = new Vector3(x, .006f, z);
            uv[i] = new Vector2(.5f + x * .42f, .5f + z * .42f);
            uv[sides + i] = uv[i];
        }
        var triangles = new int[sides * 9];
        int p = 0;
        for (int i = 0; i < sides; i++)
        {
            int next = (i + 1) % sides;
            triangles[p++] = verts.Length - 1; triangles[p++] = next; triangles[p++] = i;
            triangles[p++] = i; triangles[p++] = next; triangles[p++] = sides + i;
            triangles[p++] = next; triangles[p++] = sides + next; triangles[p++] = sides + i;
        }
        var mesh = new Mesh { vertices = verts, uv = uv, triangles = triangles };
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        return mesh;
    }
}
