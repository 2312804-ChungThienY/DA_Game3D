using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Chapter01RevisionSetup;

public static class Chapter01GuidePedestalSetup
{
    public static Bounds GeometryBounds(GameObject root)
    {
        Bounds result=default;bool any=false;
        foreach(var mf in root.GetComponentsInChildren<MeshFilter>(true))
        {
            var rr=mf.GetComponent<Renderer>();if(!mf.sharedMesh||!rr||!rr.enabled)continue;var b=mf.sharedMesh.bounds;
            for(int i=0;i<8;i++){var q=mf.transform.TransformPoint(b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1)));if(!any){result=new Bounds(q,Vector3.zero);any=true;}else result.Encapsulate(q);}
        }
        if(!any)throw new InvalidOperationException("No visible mesh geometry: "+root.name);return result;
    }
    static void FitBox(BoxCollider box,Bounds bounds)
    {
        if(!box)return;box.center=box.transform.InverseTransformPoint(bounds.center);var scale=box.transform.lossyScale;box.size=new Vector3(bounds.size.x/Mathf.Abs(scale.x),bounds.size.y/Mathf.Abs(scale.y),bounds.size.z/Mathf.Abs(scale.z));
    }
    static Bounds SitOn(Transform visual,Bounds pedestal,float gap=.025f)
    {
        var b=GeometryBounds(visual.gameObject);visual.position+=new Vector3(pedestal.center.x-b.center.x,pedestal.max.y+gap-b.min.y,pedestal.center.z-b.center.z);return GeometryBounds(visual.gameObject);
    }
    static GameObject Stone(Transform parent,string name,Vector3 foot,float height,float width,bool solid=false)=>Chapter01RevisionArt.Pedestal(parent,name,foot,height,width,solid);
    static void VisualCube(string name,Transform parent,Vector3 position,Vector3 size,Material material)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,true);g.transform.position=position;g.transform.localScale=size;g.layer=2;g.GetComponent<Renderer>().sharedMaterial=material;UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());
    }
    public static void AddGuide(Scene scene,Transform parent,string id,string name,Vector3 foot)
    {
        if(All<PuzzleCluePickup>(scene).Any(c=>c.ClueId==id))return;
        var stone=Stone(parent,"Guide_Reading_Stone_"+name,foot,.72f,.8f,true);var paper=Chapter01CompletionArt.Group(name,parent).gameObject;paper.layer=8;paper.transform.position=foot+Vector3.up*.745f;
        var visual=Chapter01CompletionArt.Group("Paper_Visual",paper.transform);var parchment=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/Paper_Parchment.mat");var ink=AssetDatabase.LoadAssetAtPath<Material>(Art+"/Materials/Paper_Ink.mat");
        VisualCube("Parchment",visual,paper.transform.position,new Vector3(.48f,.025f,.34f),parchment);
        for(int row=0;row<4;row++)VisualCube("Handwritten_Line_"+row,visual,paper.transform.position+new Vector3(0,.015f,-.1f+row*.062f),new Vector3(.32f-row%2*.08f,.004f,.012f),ink);
        var col=paper.AddComponent<BoxCollider>();col.size=new Vector3(.62f,.18f,.48f);var clue=paper.AddComponent<PuzzleCluePickup>();BuilderUtil.Set(clue,"worldState",All<WorldStateStore>(scene).Single());BuilderUtil.Set(clue,"storyPanel",All<StoryCardPanel>(scene).Single());BuilderUtil.Set(clue,"clueId",id);BuilderUtil.Set(clue,"paperVisual",visual.gameObject);BuilderUtil.Set(clue,"interactionCollider",col);
    }
    public static void Apply(Scene scene,Action<string> check)
    {
        var world=All<Transform>(scene).Single(t=>t.name=="World_Geometry");var art=world.Find("ART_C1_Revision");var guides=art.Find("Camp_And_KimQuy_Guides")??Chapter01CompletionArt.Group("Camp_And_KimQuy_Guides",art);
        var lanternBase=All<Transform>(scene).Single(t=>t.name=="LanternPedestal");lanternBase.gameObject.SetActive(true);if(lanternBase.GetComponent<Renderer>())lanternBase.GetComponent<Renderer>().enabled=false;
        var lampStone=lanternBase.Find("VIS_LanternPedestal_A24")?.gameObject??Stone(lanternBase,"VIS_LanternPedestal_A24",new Vector3(lanternBase.position.x,0,lanternBase.position.z),.8f,.86f);
        foreach(var reset in All<DrumPuzzleReset>(scene))if(!reset.transform.Find("VIS_ResetPedestal_A24"))Stone(reset.transform,"VIS_ResetPedestal_A24",new Vector3(reset.transform.position.x,0,reset.transform.position.z),1f,.72f);
        AddGuide(scene,guides,WorldIds.GuideCampIntro,"Guide_Camp_Introduction",new Vector3(2.6f,0,-3));
        AddGuide(scene,guides,WorldIds.GuideCampControls,"Guide_Camp_Controls",new Vector3(2.6f,0,.5f));
        AddGuide(scene,guides,WorldIds.GuideKimQuy,"Guide_Before_KimQuy",new Vector3(4.2f,0,262.5f));
        foreach(var plate in All<Transform>(scene).Where(t=>t.name=="Top_Bronze_Rim").ToArray())UnityEngine.Object.DestroyImmediate(plate.gameObject);
        int recentered=0;
        foreach(var fit in All<Transform>(scene).Where(t=>t.name=="Fit"&&t.GetComponentsInChildren<MeshFilter>(true).Any(m=>m.sharedMesh&&AssetDatabase.GetAssetPath(m.sharedMesh)==Art+"/Models/Pedestal_A24.fbx")))
        {
            var b=GeometryBounds(fit.gameObject);var outer=fit.parent;fit.position+=new Vector3(outer.position.x-b.center.x,outer.position.y-b.min.y,outer.position.z-b.center.z);FitBox(outer.GetComponent<BoxCollider>(),GeometryBounds(fit.gameObject));recentered++;
        }
        foreach(var relic in All<RelicPickup>(scene))
        {
            var id=new SerializedObject(relic).FindProperty("relicId").stringValue;var stone=art.Find("Revised_Pedestals/Relic_Pedestal_"+id);var visual=relic.transform.Find("VIS_DrumFragment");if(!stone||!visual)continue;var b=SitOn(visual,GeometryBounds(stone.gameObject));b.Expand(.08f);FitBox(relic.GetComponent<BoxCollider>(),b);check("Centered relic without moving gameplay root: "+id);
        }
        foreach(var cp in All<CheckpointAltar>(scene).Where(c=>c.CheckpointId!="cp_trai"))
        {
            var stone=cp.transform.Find("VIS_Checkpoint_A24");var heart=cp.transform.Find("Checkpoint_Heart");if(!stone||!heart)continue;var b=GeometryBounds(stone.gameObject);var item=SitOn(heart,b,.02f);b.Encapsulate(item);FitBox(cp.GetComponent<BoxCollider>(),b);
        }
        var lamp=All<LanternPickup>(scene).Single();var lampVisual=lamp.transform.Find("VIS_LotusLantern");if(!lampVisual)throw new InvalidOperationException("Lantern visual missing");var lampBounds=SitOn(lampVisual,GeometryBounds(lampStone));lampBounds.Expand(.08f);FitBox(lamp.GetComponent<BoxCollider>(),lampBounds);FitBox(lanternBase.GetComponent<BoxCollider>(),GeometryBounds(lampStone));
        foreach(var clue in All<PuzzleCluePickup>(scene))
        {
            string standName=clue.ClueId==WorldIds.ClueDrum?"Clue_Reading_Stone_0":clue.ClueId==WorldIds.ClueMirror?"Clue_Reading_Stone_1":"Guide_Reading_Stone_"+clue.name;var stand=clue.transform.parent.Find(standName);if(stand)clue.transform.position=new Vector3(stand.position.x,GeometryBounds(stand.gameObject).max.y+.025f,stand.position.z);
        }
        foreach(var t in All<Transform>(scene))if(PrefabUtility.IsPartOfPrefabInstance(t)){PrefabUtility.RecordPrefabInstancePropertyModifications(t);PrefabUtility.RecordPrefabInstancePropertyModifications(t.gameObject);}
        foreach(var col in All<BoxCollider>(scene))if(PrefabUtility.IsPartOfPrefabInstance(col))PrefabUtility.RecordPrefabInstancePropertyModifications(col);
        Chapter01CompletionArt.Bake(scene);check("Recentered "+recentered+" A24 meshes, removed square add-on plates; permanent lantern pedestal and aligned rewards/checkpoint crystals/papers; three optional guide notes, NavMesh rebaked.");
    }
    static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
    public static void Inspect(Scene scene,Action<string> check)
    {
        foreach(var t in All<Transform>(scene).Where(t=>t.name.Contains("Pedestal")||t.name=="Top_Bronze_Rim"||t.name.StartsWith("Relic_Pedestal_")||t.name.StartsWith("Mirror_Stand_")||t.name=="Checkpoint_Heart"||t.GetComponent<LanternPickup>()||t.GetComponent<RelicPickup>()))
        {
            check(PathOf(t)+": active="+t.gameObject.activeInHierarchy+", pos="+t.position+", scale="+t.lossyScale);
            foreach(var rr in t.GetComponentsInChildren<Renderer>(true).Where(r=>!(r is ParticleSystemRenderer)&&!(r is LineRenderer)).Take(12))check("  "+rr.name+": enabled="+rr.enabled+", active="+rr.gameObject.activeInHierarchy+", bounds="+rr.bounds);
        }
    }
    public static void Validate(Scene scene,Action<string> check)
    {
        void Require(bool pass,string label){if(!pass)throw new InvalidOperationException(scene.name+": "+label);check(scene.name+": "+label);}
        var clues=All<PuzzleCluePickup>(scene);Require(clues.Length==PuzzleCluePickup.Chapter1ClueIds.Count&&clues.Select(c=>c.ClueId).Distinct().Count()==clues.Length,"All five optional paper IDs are unique");
        Require(!All<Transform>(scene).Any(t=>t.name=="Top_Bronze_Rim"),"No square add-on plates remain");
        foreach(var fit in All<Transform>(scene).Where(t=>t.name=="Fit"&&t.GetComponentsInChildren<MeshFilter>(true).Any(m=>m.sharedMesh&&AssetDatabase.GetAssetPath(m.sharedMesh)==Art+"/Models/Pedestal_A24.fbx")))
        {var b=GeometryBounds(fit.gameObject);var foot=fit.parent.position;Require(Mathf.Abs(b.center.x-foot.x)<.005f&&Mathf.Abs(b.center.z-foot.z)<.005f&&Mathf.Abs(b.min.y-foot.y)<.005f,"Centered A24 mesh: "+fit.parent.name);}
        var lamp=All<LanternPickup>(scene).Single();var stand=All<Transform>(scene).Single(t=>t.name=="VIS_LanternPedestal_A24");Require(stand.gameObject.activeInHierarchy&&!stand.IsChildOf(lamp.transform),"Visible lantern stand remains separate from pickup");
        var art=All<Transform>(scene).Single(t=>t.name=="ART_C1_Revision");
        foreach(var relic in All<RelicPickup>(scene))
        {
            var id=new SerializedObject(relic).FindProperty("relicId").stringValue;var b=GeometryBounds(art.Find("Revised_Pedestals/Relic_Pedestal_"+id).gameObject);var item=GeometryBounds(relic.transform.Find("VIS_DrumFragment").gameObject);
            Require(Mathf.Abs(item.center.x-b.center.x)<.005f&&Mathf.Abs(item.center.z-b.center.z)<.005f&&Mathf.Abs(item.min.y-b.max.y-.025f)<.005f,"Relic rests centered on pedestal: "+id);
        }
        foreach(var clue in clues)
        {
            bool accessible=false;
            foreach(var direction in new[]{Vector3.back,Vector3.right,Vector3.left,Vector3.forward})
            {
                var approach=clue.transform.position+direction*1.6f;if(!Physics.Raycast(new Vector3(approach.x,2,approach.z),Vector3.down,out var floor,3,1<<3,QueryTriggerInteraction.Ignore)||floor.normal.y<.7f)continue;
                var p=floor.point;if(Physics.CheckCapsule(p+Vector3.up*.35f,p+Vector3.up*1.52f,.28f,1<<3,QueryTriggerInteraction.Ignore))continue;
                var eye=p+Vector3.up*1.3f;var offset=clue.GetComponent<Collider>().bounds.center-eye;
                if(Physics.Raycast(eye,offset.normalized,out var hit,3,LayerMask.GetMask("Environment","Interactable"),QueryTriggerInteraction.Ignore)&&hit.collider.GetComponentInParent<PuzzleCluePickup>()==clue){accessible=true;break;}
            }
            Require(accessible,"Walkable capsule clearance and unobstructed interaction ray at paper: "+clue.ClueId);
        }
    }
}
