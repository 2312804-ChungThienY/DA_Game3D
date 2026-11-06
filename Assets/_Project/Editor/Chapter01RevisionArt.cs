using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using TMPro;
using static Chapter01RevisionSetup;

public static class Chapter01RevisionArt
{
    static Transform Group(string name,Transform parent)=>Chapter01CompletionArt.Group(name,parent);
    static Material Mat(string name,Color color,float metallic=0,float smooth=.2f,Texture texture=null)
    {
        Chapter01RevisionSetup.Folder(Art+"/Materials");string path=Art+"/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}mat.SetColor("_BaseColor",color);mat.SetFloat("_Metallic",metallic);mat.SetFloat("_Smoothness",smooth);mat.SetFloat("_Cull",0);if(texture)mat.SetTexture("_BaseMap",texture);EditorUtility.SetDirty(mat);return mat;
    }
    static Texture TextureIn(string folder,string contains)
    {
        if(!Directory.Exists(folder))return null;string path=Directory.GetFiles(folder,"*",SearchOption.AllDirectories).FirstOrDefault(p=>!p.EndsWith(".meta")&&Path.GetFileName(p).IndexOf(contains,StringComparison.OrdinalIgnoreCase)>=0);return path!=null?AssetDatabase.LoadAssetAtPath<Texture2D>(path.Replace('\\','/')):null;
    }
    static GameObject Model(string source,string name,Transform parent,Vector3 foot,float height,float yaw=0,Material mat=null)=>Chapter01CompletionArt.Model(source,name,parent,foot,height,yaw,mat);
    static void Width(GameObject model,float x,float z)
    {var fit=model.transform.GetChild(0);var b=CampEnvironmentSetup.BoundsOf(fit.gameObject);var scale=fit.localScale;scale.x*=x/Mathf.Max(.001f,b.size.x);scale.z*=z/Mathf.Max(.001f,b.size.z);fit.localScale=scale;b=CampEnvironmentSetup.BoundsOf(fit.gameObject);fit.position+=new Vector3(model.transform.position.x-b.center.x,model.transform.position.y-b.min.y,model.transform.position.z-b.center.z);}
    static GameObject Box(string name,Transform parent,Vector3 position,Vector3 size,Material material,bool collider=false,int layer=3)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.layer=layer;g.transform.position=position;g.transform.localScale=size;g.transform.SetParent(parent,true);g.GetComponent<Renderer>().sharedMaterial=material;if(!collider)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
    }
    static void Hide(Transform root)
    {foreach(var rr in root.GetComponentsInChildren<Renderer>(true)){rr.enabled=false;if(PrefabUtility.IsPartOfPrefabInstance(rr))PrefabUtility.RecordPrefabInstancePropertyModifications(rr);}}
    public static void Install(Scene scene,Action<string> check)
    {
        Chapter01RevisionSetup.Folder(Art+"/Meshes");var world=All<Transform>(scene).Single(t=>t.name=="World_Geometry");var root=world.Find("ART_C1_Revision")??Group("ART_C1_Revision",world);
        if(!root.Find("Camp_Fire"))Camp(scene,root,check);
        if(!root.Find("Revised_Pedestals"))Pedestals(scene,root,check);
        if(!root.Find("Revised_Drums"))Drums(scene,root,check);
        if(!root.Find("Revised_Mirrors")){Mirrors(scene,root,check);RepairMirrors(scene,check);}
        if(!root.Find("Revised_Portals"))Portals(scene,root,check);
        if(!root.Find("Clue_Papers"))Clues(scene,root,check);
        if(!root.Find("Cave_Mist"))Mist(scene,root,check);
        if(!root.Find("Actors_Revised")){Chapter01RevisionActors.Install(scene,check);Group("Actors_Revised",root);}
        foreach(var t in All<Transform>(scene))if(PrefabUtility.IsPartOfPrefabInstance(t)){PrefabUtility.RecordPrefabInstancePropertyModifications(t);PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);}
        RefineDetails(scene,check);Chapter01CompletionArt.Bake(scene);check("Revision navigation baked from physics; gameplay IDs, checkpoints and source FBX/GLB/OBJ retained.");
    }
    static void Camp(Scene scene,Transform root,Action<string> check)
    {
        foreach(var table in All<CampCraftingTable>(scene))UnityEngine.Object.DestroyImmediate(table.gameObject);
        foreach(var t in All<Transform>(scene).Where(t=>t.name=="VIS_CraftingTable"))t.gameObject.SetActive(false);
        foreach(var t in All<Transform>(scene).Where(t=>t.GetComponent<Renderer>()&&t.gameObject.activeInHierarchy&&(t.position-new Vector3(-4,0,0)).sqrMagnitude<12))
            if(t.name=="Bonfire_Fire"){Hide(t);t.gameObject.SetActive(false);}else if(t.name.StartsWith("Fire_")||t.name=="WoodLog")Hide(t);
        var p=Group("Camp_Fire",root);var wood=Mat("Fire_Wood",new Color(.24f,.10f,.035f));var ash=Mat("Fire_Ash",new Color(.12f,.13f,.12f));
        var bed=GameObject.CreatePrimitive(PrimitiveType.Cylinder);bed.name="Ash_Bed";bed.layer=2;bed.transform.SetParent(p,false);bed.transform.position=new Vector3(-4,.035f,0);bed.transform.localScale=new Vector3(1.45f,.035f,1.45f);bed.GetComponent<Renderer>().sharedMaterial=ash;UnityEngine.Object.DestroyImmediate(bed.GetComponent<Collider>());
        for(int i=0;i<5;i++)Model("Assets/A16 - Survival Pack/FBX/WoodLog.fbx","Hearth_Log_"+i,p,new Vector3(-4,.05f+i%2*.05f,0),.25f,i*72,wood);
        var seats=new[]{new Vector3(-3.8f,0,-2.5f),new Vector3(-6.1f,0,-2.0f),new Vector3(-5.4f,0,-3.8f)};
        for(int i=0;i<seats.Length;i++)
        {var log=Model("Assets/A16 - Survival Pack/FBX/WoodLog.fbx","Camp_Seat_Log_"+i,p,seats[i],.43f,25+i*65,wood);var b=CampEnvironmentSetup.BoundsOf(log);var col=log.AddComponent<BoxCollider>();col.center=log.transform.InverseTransformPoint(b.center);col.size=b.size;}
        var flame=Particle("Fire_Flames",p,new Vector3(-4,.35f,0),TextureIn("Assets/Fog Particles/Bonus","LavaSphere.png"),true);var main=flame.main;main.startLifetime=new ParticleSystem.MinMaxCurve(.6f,1.05f);main.startSpeed=new ParticleSystem.MinMaxCurve(.55f,.9f);main.startSize=new ParticleSystem.MinMaxCurve(.17f,.40f);main.maxParticles=72;main.gravityModifier=-.08f;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startColor=new Color(1,.55f,.17f,.8f);var emission=flame.emission;emission.rateOverTime=35;var shape=flame.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.radius=.31f;shape.angle=7;shape.rotation=new Vector3(-90,0,0);
        var size=flame.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.8f),new Keyframe(.35f,1),new Keyframe(1,0)));
        var color=flame.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(new Color(1,.88f,.35f),0),new GradientColorKey(new Color(1,.34f,.055f),.5f),new GradientColorKey(new Color(.55f,.08f,.01f),1)},new[]{new GradientAlphaKey(.8f,0),new GradientAlphaKey(.5f,.6f),new GradientAlphaKey(0,1)});color.color=gradient;
        var sparks=Particle("Fire_Embers",p,new Vector3(-4,.55f,0),TextureIn("Assets/Fog Particles/Bonus","LavaSphere.png"),true);main=sparks.main;main.startLifetime=1.4f;main.startSpeed=1.1f;main.startSize=.035f;main.maxParticles=12;main.startColor=new Color(1,.54f,.12f);emission=sparks.emission;emission.rateOverTime=4;shape=sparks.shape;shape.radius=.25f;shape.angle=12;shape.rotation=new Vector3(-90,0,0);
        var light=All<Light>(scene).FirstOrDefault(l=>l.name=="Camp_Fire");if(light){light.intensity=8;light.color=new Color(1,.55f,.26f);light.range=8;var flicker=p.gameObject.AddComponent<CampFireAnimation>();BuilderUtil.Set(flicker,"fireLight",light);}
        AddMist(p,new Vector3(-4,1.4f,0),new Vector3(.8f,1,.8f),new Color(.27f,.29f,.29f,.08f),3,.75f,"Fire_Smoke");
        check("Craft station removed; stationary cone/log visuals disabled. Uniform-scale wide log seats and three animated fire/smoke/ember particle systems installed, east checkpoint approach kept clear.");
    }
    static ParticleSystem Particle(string name,Transform parent,Vector3 position,Texture texture,bool additive)
    {
        var g=Group(name,parent).gameObject;g.layer=2;g.transform.position=position;var ps=g.AddComponent<ParticleSystem>();var main=ps.main;main.playOnAwake=true;main.loop=true;main.useUnscaledTime=false;
        string path=Art+"/Materials/"+(additive?"Particle_Fire":"Particle_Mist")+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(!mat){mat=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(mat,path);}mat.SetFloat("_Surface",1);mat.SetFloat("_Blend",additive?2:0);mat.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);mat.SetFloat("_DstBlend",(float)(additive?BlendMode.One:BlendMode.OneMinusSrcAlpha));mat.SetFloat("_ZWrite",0);mat.SetColor("_BaseColor",Color.white);mat.SetFloat("_Cull",0);mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");if(texture)mat.SetTexture("_BaseMap",texture);mat.renderQueue=3000;EditorUtility.SetDirty(mat);var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=mat;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return ps;
    }
    public static GameObject Pedestal(Transform root,string name,Vector3 foot,float height,float width,bool solid=false)
    {
        var mat=Mat("Pedestal_A24",new Color(.69f,.72f,.69f),.1f,.18f,TextureIn("Assets/A24 - Bệ đàn","Albedo"));var model=Model(Art+"/Models/Pedestal_A24.fbx",name,root,foot,height,0,mat);Width(model,width,width);
        if(solid){var col=model.AddComponent<BoxCollider>();col.center=Vector3.up*height*.5f;col.size=new Vector3(width*.8f,height,width*.8f);model.layer=3;}
        return model;
    }
    static void Pedestals(Scene scene,Transform root,Action<string> check)
    {
        var p=Group("Revised_Pedestals",root);
        foreach(var relic in All<RelicPickup>(scene))
        {
            var so=new SerializedObject(relic);string id=so.FindProperty("relicId").stringValue;BuilderUtil.Set(relic,"displayName",id==WorldStateStore.DrumA?"mảnh Mặt Trống":id==WorldStateStore.DrumB?"mảnh Thân Trống":"mảnh Chân Trống");
            var visual=relic.transform.Find("VIS_DrumFragment");if(visual)visual.rotation=Quaternion.Euler(-65,0,0);
            var support=relic.transform.parent;foreach(var t in support.GetComponentsInChildren<Transform>(true).Where(t=>t!=relic.transform&&!t.IsChildOf(relic.transform)&&t.GetComponent<MeshFilter>()&&t.name.IndexOf("Pedestal",StringComparison.OrdinalIgnoreCase)>=0))Hide(t);
            Pedestal(p,"Relic_Pedestal_"+id,new Vector3(relic.transform.position.x,0,relic.transform.position.z),.94f,1.05f);
        }
        foreach(var cp in All<CheckpointAltar>(scene).Where(c=>c.CheckpointId!="cp_trai"))
        {
            foreach(var rr in cp.GetComponentsInChildren<Renderer>(true))if(!rr.GetComponent<TMP_Text>())rr.enabled=false;
            Pedestal(cp.transform,"VIS_Checkpoint_A24",cp.transform.position,.88f,.86f);
            var crystal=Model("Assets/A17 - Ultimate RPG Pack/FBX/Crystal2.fbx","Checkpoint_Heart",cp.transform,cp.transform.position+Vector3.up*.96f,.38f,10,Mat("Checkpoint_Glow",new Color(.18f,.69f,.53f),.25f,.5f));var material=crystal.GetComponentInChildren<Renderer>().sharedMaterial;material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",new Color(.08f,.36f,.25f));EditorUtility.SetDirty(material);
        }
        check("A24 carved pedestals installed for three relics and three checkpoint altars; fragment engraved faces now point toward the south approach. Original pickup/checkpoint collider IDs/spawn refs retained.");
    }
    static void Drums(Scene scene,Transform root,Action<string> check)
    {
        var p=Group("Revised_Drums",root);var bronze=AssetDatabase.LoadAssetAtPath<Material>("Assets/_Project/Art/Chapter01Expanded/Materials/DongSon_Bronze.mat");
        foreach(var drum in All<DrumInteractable>(scene))
        {
            Hide(drum.transform);var note=(DrumNote)new SerializedObject(drum).FindProperty("note").enumValueIndex;
            foreach(var text in drum.GetComponentsInChildren<TMP_Text>(true))text.gameObject.SetActive(false);
            Model("Assets/A20 - Trống đồng/source/filetex.fbx","VIS_DongSon_Playable",drum.transform,new Vector3(drum.transform.position.x,.12f,drum.transform.position.z),1.02f,0,bronze);
            Symbol(p,new Vector3(drum.transform.position.x,.83f,drum.transform.position.z-.68f),note);
        }
        foreach(var text in All<TMP_Text>(scene).Where(t=>t.name=="HintText"&&t.text.Contains("A")))text.text="Có một bản chép quanh Điện Trống.\nTheo dấu Mặt Trời, Cánh Chim và Sóng Nước.";
        check("Three playable A20 Dong Son drums use distinct Sun/Bird/Water emblems and readable names; internal A-C-B sequence/IDs unchanged, no A/B/C player labels.");
    }
    static void Symbol(Transform parent,Vector3 center,DrumNote note)
    {
        var bronze=Mat("Emblem_Bronze",new Color(.53f,.35f,.13f),.6f,.32f);Box("Emblem_Plaque_"+note,parent,center,new Vector3(.72f,.56f,.09f),bronze);
        var gold=Mat("Emblem_Inlay",new Color(.82f,.83f,.57f),.4f,.3f);var g=Group("Symbol_"+note,parent);g.position=center+Vector3.back*.052f;var line=g.gameObject.AddComponent<LineRenderer>();line.sharedMaterial=gold;line.useWorldSpace=false;line.widthMultiplier=.025f;
        var points=new List<Vector3>();
        if(note==DrumNote.A){line.loop=true;for(int i=0;i<24;i++){float a=i*Mathf.PI/12,r=i%2==0?.19f:.11f;points.Add(new Vector3(Mathf.Sin(a)*r,Mathf.Cos(a)*r,0));}}
        else if(note==DrumNote.C)points.AddRange(new[]{new Vector3(-.25f,.08f,0),new Vector3(-.1f,.14f,0),new Vector3(0,-.03f,0),new Vector3(.1f,.14f,0),new Vector3(.25f,.08f,0)});
        else for(int i=0;i<33;i++){float x=-.25f+i*.5f/32;points.Add(new Vector3(x,Mathf.Sin(i*Mathf.PI/8)*.06f,0));}
        line.positionCount=points.Count;line.SetPositions(points.ToArray());var label=new GameObject("Emblem_Name",typeof(TextMeshPro));label.layer=2;label.transform.SetParent(g,false);label.transform.localPosition=new Vector3(0,-.38f,-.015f);var text=label.GetComponent<TextMeshPro>();text.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BuilderUtil.FontPath);text.text=DrumInteractable.NoteName(note);text.fontSize=1.5f;text.alignment=TextAlignmentOptions.Center;text.rectTransform.sizeDelta=new Vector2(1.7f,.36f);text.color=new Color(.88f,.87f,.7f);
    }
    static void Mirrors(Scene scene,Transform root,Action<string> check)
    {
        var p=Group("Revised_Mirrors",root);var frameMat=Mat("Mirror_A23",new Color(.84f,.73f,.46f),.75f,.42f,TextureIn("Assets/A23 - Gương","gltf_embedded_0"));
        foreach(var mirror in All<MirrorRotator>(scene))
        {
            Hide(mirror.transform);var model=Model(Art+"/Models/Mirror_A23.fbx","VIS_A23_Mirror",mirror.transform,mirror.transform.position+new Vector3(0,-.91f,0),1.82f,0,frameMat);Width(model,1.16f,.16f);model.transform.rotation=mirror.transform.rotation;
            var glass=Mat("Mirror_Reflective_Face",new Color(.28f,.57f,.63f),.92f,.94f);var vertices=new List<Vector3>{Vector3.zero};var tri=new List<int>();for(int i=0;i<48;i++){float a=i*Mathf.PI/24;vertices.Add(new Vector3(Mathf.Sin(a)*.4f,Mathf.Cos(a)*.62f,0));tri.AddRange(new[]{0,1+(i+1)%48,1+i});}var face=Chapter01CompletionArt.Surface("Front_Silver_Face",mirror.transform,Chapter01CompletionArt.Mesh("Mirror_Face_Oval",vertices,tri),glass);face.layer=2;face.transform.localPosition=new Vector3(0,.04f,.085f);
            Pedestal(p,"Mirror_Stand_"+mirror.name,new Vector3(mirror.transform.position.x,0,mirror.transform.position.z),.58f,.72f,true);
        }
        check("A23 oval mirror frames and A24 stands fitted around original optical planes; front glass/normal consistent, four rotations and two-mirror ray logic preserved.");
    }
    static void Portals(Scene scene,Transform root,Action<string> check)
    {
        var p=Group("Revised_Portals",root);var stone=Chapter01CompletionArt.Rock;var portalMat=Mat("Portal_A22",new Color(.72f,.76f,.71f),.12f,.2f,TextureIn("Assets/A22 - Cửa phong ấn","BaseColor"));
        var doors=new HashSet<GameObject>();foreach(var seal in All<SealTarget>(scene)){var barrier=Ref<GameObject>(seal,"barrier");if(barrier)doors.Add(barrier);}foreach(var t in All<Transform>(scene).Where(t=>(t.name=="Door"||t.name=="Door_Test"||t.name=="Gate_ArenaLock")&&t.GetComponent<BoxCollider>()))doors.Add(t.gameObject);
        foreach(var door in doors)
        {
            var collider=door.GetComponent<BoxCollider>()??door.GetComponentInChildren<BoxCollider>();if(!collider)continue;var old=collider.bounds;if(old.size==Vector3.zero){old=new Bounds(door.transform.position,new Vector3(4.5f,4.4f,.4f));}float width=old.size.x,height=old.size.y;
            Hide(door.transform);Vector3 foot=new Vector3(old.center.x,0,old.center.z);float crown=height<=3.1f?3.35f:4.4f;
            var model=Model(Art+"/Models/Portal_A22.fbx","VIS_Portal_A22",door.transform,foot,crown+.16f,0,portalMat);Width(model,width,.38f);
            Box("Sealed_Stone_Leaf",door.transform,foot+Vector3.up*crown*.5f,new Vector3(width-.06f,crown,.20f),Mat("Door_Leaf",new Color(.18f,.27f,.24f),.28f,.28f));
            float capBottom=crown-.05f;Box("Door_Crown_Closure_"+door.GetInstanceID(),p,foot+new Vector3(0,(capBottom+6.15f)*.5f,0),new Vector3(width+.35f,6.15f-capBottom,1.65f),stone,true);
            if(height<=3.1f){var size=collider.size;size.y=crown/Mathf.Abs(collider.transform.lossyScale.y);collider.size=size;var center=collider.center;center.y=collider.transform.InverseTransformPoint(foot+Vector3.up*crown*.5f).y;collider.center=center;}
            check("Closed door crown at "+foot+": leaf="+crown.ToString("F2")+"m plus overlapping rock cap; A22 model attached below existing barrier.");
        }
    }
    static void Clues(Scene scene,Transform root,Action<string> check)
    {
        var p=Group("Clue_Papers",root);var world=All<WorldStateStore>(scene).Single();var story=All<StoryCardPanel>(scene).Single();
        for(int i=0;i<2;i++)
        {
            Vector3 foot=i==0?new Vector3(-20.5f,0,136):new Vector3(19,0,203);
            Pedestal(p,"Clue_Reading_Stone_"+i,foot,.72f,.8f,true);var paper=Group(i==0?"Clue_Drum_Note":"Clue_Mirror_Note",p).gameObject;paper.transform.position=foot+new Vector3(0,.78f,0);paper.layer=8;
            var visual=Group("Paper_Visual",paper.transform);Box("Parchment",visual, paper.transform.position,new Vector3(.48f,.025f,.34f),Mat("Paper_Parchment",new Color(.84f,.76f,.54f)),false,2);
            var ink=Mat("Paper_Ink",new Color(.14f,.24f,.21f));for(int row=0;row<4;row++)Box("Handwritten_Line_"+row,visual,paper.transform.position+new Vector3(0,.015f,-.1f+row*.062f),new Vector3(.32f-row%2*.08f,.004f,.012f),ink,false,2);
            var col=paper.AddComponent<BoxCollider>();col.size=new Vector3(.62f,.18f,.48f);var clue=paper.AddComponent<PuzzleCluePickup>();BuilderUtil.Set(clue,"worldState",world);BuilderUtil.Set(clue,"storyPanel",story);BuilderUtil.Set(clue,"clueId",i==0?WorldIds.ClueDrum:WorldIds.ClueMirror);BuilderUtil.Set(clue,"paperVisual",visual.gameObject);BuilderUtil.Set(clue,"interactionCollider",col);
        }
        check("Two discoverable E clue papers installed on side reading stones; Flags IDs catalogued, notes persist in Journal after checkpoint save, no puzzle input prerequisite.");
    }
    public static void RefineDetails(Scene scene,Action<string> check)
    {
        foreach(var label in All<DrumVisualLabel>(scene))UnityEngine.Object.DestroyImmediate(label);
        foreach(var seat in All<Transform>(scene).Where(t=>t.name=="Seat_Left"||t.name=="Seat_Back"))seat.gameObject.SetActive(false);
        foreach(var filter in All<MeshFilter>(scene).Where(f=>f.sharedMesh&&f.sharedMesh.name.Contains("_Beveled_Beveled")))
        {
            string key=filter.sharedMesh.name.Replace("_Beveled_Beveled","_Beveled");var original=AssetDatabase.LoadAssetAtPath<Mesh>(Chapter01CompletionArt.Art+"/Meshes/"+key+".asset");if(!original)throw new InvalidOperationException("Missing undamaged paving: "+key);filter.sharedMesh=original;
        }
        foreach(var drum in All<DrumInteractable>(scene))
        {
            var model=drum.transform.Find("VIS_DongSon_Playable");var collider=drum.GetComponent<BoxCollider>();if(!model||!collider)continue;var b=CampEnvironmentSetup.BoundsOf(model.gameObject);collider.center=drum.transform.InverseTransformPoint(b.center);var scale=drum.transform.lossyScale;collider.size=new Vector3(b.size.x/Mathf.Abs(scale.x),b.size.y/Mathf.Abs(scale.y),b.size.z/Mathf.Abs(scale.z));
        }
        foreach(var text in All<TextMeshPro>(scene).Where(t=>new[]{"A","B","C"}.Contains(t.text.Trim())))text.gameObject.SetActive(false);
        foreach(var receiver in All<EnemyLanternReceiver>(scene))BuilderUtil.Set(receiver,"tintRenderer",(Renderer)null);
        var bronze=Chapter01CompletionArt.Material("Revision_Seal_Bronze",new Color(.57f,.40f,.17f),.25f);
        foreach(var leaf in All<Transform>(scene).Where(t=>t.name=="Sealed_Stone_Leaf"))
        {
            if(leaf.parent.Find("VIS_Seal_Emblem"))continue;var root=new GameObject("VIS_Seal_Emblem").transform;root.position=leaf.position+Vector3.back*(Mathf.Abs(leaf.lossyScale.z)*.5f+.045f);root.SetParent(leaf.parent,true);float radius=Mathf.Min(.85f,Mathf.Abs(leaf.lossyScale.x)*.23f);
            for(int ring=0;ring<3;ring++){var line=Group("Bronze_Inlay_"+ring,root).gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=bronze;line.widthMultiplier=ring==0?.025f:.012f;line.loop=true;line.positionCount=48;for(int i=0;i<48;i++){float a=i*Mathf.PI/24;line.SetPosition(i,new Vector3(Mathf.Sin(a)*radius*(1-ring*.22f),Mathf.Cos(a)*radius*(1-ring*.22f),0));}}
            for(int i=0;i<12;i++){float a=i*Mathf.PI/6;var line=Group("Seal_Ray_"+i,root).gameObject.AddComponent<LineRenderer>();line.useWorldSpace=false;line.sharedMaterial=bronze;line.widthMultiplier=.022f;line.positionCount=2;var direction=new Vector3(Mathf.Sin(a),Mathf.Cos(a),0);line.SetPosition(0,direction*radius*.3f);line.SetPosition(1,direction*radius*.53f);}
        }
        check("Removed legacy label generators, repaired double-beveled paving refs and fitted drum interaction boxes to FBX; bronze seal inlays; weakness emission retains base palette.");
    }
    public static void RepairMirrors(Scene scene,Action<string> check)
    {
        var frame=Mat("Mirror_A23",new Color(.84f,.73f,.46f),.75f,.42f,TextureIn("Assets/A23 - Gương","gltf_embedded_0"));var bronze=Mat("Mirror_Bronze_Back",new Color(.20f,.15f,.075f),.65f,.3f);
        foreach(var mirror in All<MirrorRotator>(scene))
        {
            var old=mirror.transform.Find("VIS_A23_Mirror");if(old)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var model=Model(Art+"/Models/Mirror_A23.fbx","VIS_A23_Mirror",mirror.transform,mirror.transform.position+Vector3.down*.91f,1.82f,0,frame);Width(model,1.16f,.16f);model.transform.rotation=mirror.transform.rotation;
            var front=mirror.transform.Find("Front_Silver_Face");if(front&&!mirror.transform.Find("Back_Bronze_Face")){var back=Chapter01CompletionArt.Surface("Back_Bronze_Face",mirror.transform,front.GetComponent<MeshFilter>().sharedMesh,bronze);back.layer=2;back.transform.localPosition=new Vector3(0,.04f,-.095f);}
            check(mirror.name+": A23 fitted in its local plane before yaw; silver +Z / bronze -Z, original optical collider unchanged.");
        }
    }
    static void Mist(Scene scene,Transform root,Action<string> check)
    {
        var p=Group("Cave_Mist",root);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.12f,.19f,.19f);RenderSettings.fogDensity=.0065f;
        var locations=new[]{new Vector3(-11,.55f,9),new Vector3(30,.45f,76),new Vector3(-28,.45f,153),new Vector3(39,.45f,223),new Vector3(-15,.5f,298)};
        for(int i=0;i<locations.Length;i++)AddMist(p,locations[i],new Vector3(12,.7f,8),new Color(.43f,.58f,.55f,.035f),2,6,"Low_Mist_"+i);
        foreach(var camera in All<Camera>(scene))camera.GetUniversalAdditionalCameraData().requiresDepthOption=CameraOverrideOption.On;
        check("URP exponential base fog + five sparse ground banks from imported Fog Particles; no colliders, hints/beam/main paths kept clear, world pause respected.");
    }
    static void AddMist(Transform parent,Vector3 position,Vector3 box,Color tint,float rate,float size,string name)
    {
        var asset=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Fog Particles/Prefabs/Whitish Fog.prefab");if(!asset)throw new InvalidOperationException("Imported Whitish Fog prefab missing.");var root=(GameObject)PrefabUtility.InstantiatePrefab(asset,parent);root.name=name;root.transform.position=position;
        foreach(var ps in root.GetComponentsInChildren<ParticleSystem>(true))
        {
            ps.gameObject.layer=2;var main=ps.main;main.maxParticles=24;main.startLifetime=10;main.startSize=new ParticleSystem.MinMaxCurve(size*.7f,size*1.2f);main.startColor=tint;main.startSpeed=.05f;main.useUnscaledTime=false;main.simulationSpace=ParticleSystemSimulationSpace.World;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=box;var emission=ps.emission;emission.rateOverTime=rate;
            var renderer=ps.GetComponent<ParticleSystemRenderer>();var source=renderer.sharedMaterial;string path=Art+"/Materials/Fog_URP.mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));material.SetTexture("_BaseMap",source?source.mainTexture:null);material.SetColor("_BaseColor",Color.white);material.SetFloat("_Surface",1);material.SetFloat("_Blend",0);material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);material.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);material.SetFloat("_ZWrite",0);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;AssetDatabase.CreateAsset(material,path);}renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;PrefabUtility.RecordPrefabInstancePropertyModifications(ps);PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }
    }
}
