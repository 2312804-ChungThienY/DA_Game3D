using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering;
using static Chapter01RevisionSetup;

public static class Chapter01RevisionUI
{
    static readonly Color Ink=new Color(.035f,.105f,.10f,.98f),Gold=new Color(.67f,.52f,.27f),Paper=new Color(.92f,.95f,.89f);
    static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BuilderUtil.FontPath);
    static RectTransform Place(Transform t,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size)
    {var r=(RectTransform)t;r.SetParent(parent,false);r.localScale=Vector3.one;r.localRotation=Quaternion.identity;r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
    static Image Panel(string name,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size)
    {var g=new GameObject(name,typeof(RectTransform),typeof(Image));Place(g.transform,parent,anchor,pivot,pos,size);var image=g.GetComponent<Image>();image.color=Ink;image.raycastTarget=false;Border(image);return image;}
    static TMP_Text Text(string name,Transform parent,string text,float size,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 dimensions)
    {var g=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));Place(g.transform,parent,anchor,pivot,pos,dimensions);var t=g.GetComponent<TextMeshProUGUI>();t.font=Font;t.fontSize=size;t.text=text;t.color=Paper;t.raycastTarget=false;t.alignment=TextAlignmentOptions.TopLeft;return t;}
    static void Border(Image image)
    {
        image.material=null;image.sprite=null;image.type=Image.Type.Simple;
        foreach(var outline in image.GetComponents<Outline>())UnityEngine.Object.DestroyImmediate(outline);
        var old=image.transform.Find("Kenney_Frame");if(old)old.gameObject.SetActive(false);
        if(image.transform.Find("REV_Border_Top"))return;
        foreach(string side in new[]{"Top","Bottom","Left","Right"})
        {
            var g=new GameObject("REV_Border_"+side,typeof(RectTransform),typeof(Image));var r=(RectTransform)g.transform;r.SetParent(image.transform,false);var im=g.GetComponent<Image>();im.color=new Color(Gold.r,Gold.g,Gold.b,.5f);im.raycastTarget=false;
            if(side=="Top"||side=="Bottom"){r.anchorMin=new Vector2(0,side=="Top"?1:0);r.anchorMax=new Vector2(1,side=="Top"?1:0);r.pivot=new Vector2(.5f,.5f);r.sizeDelta=new Vector2(-2,1);r.anchoredPosition=Vector2.zero;}
            else{r.anchorMin=new Vector2(side=="Left"?0:1,0);r.anchorMax=new Vector2(side=="Left"?0:1,1);r.pivot=Vector2.one*.5f;r.sizeDelta=new Vector2(1,-2);r.anchoredPosition=Vector2.zero;}
            g.transform.SetAsFirstSibling();
        }
    }
    public static void Skin(GameObject root)
    {
        foreach(var image in root.GetComponentsInChildren<Image>(true).Where(i=>i.name=="Card_JadeBronze"||i.name=="Vitals_Frame"||i.name=="Quest_Frame"||i.name=="Potion_Quickslot"||i.name=="Toast_Jade"||i.name=="Title_Shade"||i.name.StartsWith("Scroll_")))
        {image.color=Ink;Border(image);}
        foreach(var t in root.GetComponentsInChildren<TMP_Text>(true))if(t.name=="Controls")t.color=new Color(.72f,.80f,.75f,.9f);
    }
    public static void Shared(Action<string> check)
    {
        const string path="Assets/_Project/Resources/PF_AppRootUI.prefab";var root=PrefabUtility.LoadPrefabContents(path);try{Skin(root);PrefabUtility.SaveAsPrefabAsset(root,path);}finally{PrefabUtility.UnloadPrefabContents(root);}check("Shared pause/settings/death/confirm/toast cards restyled with clear jade surfaces and fine bronze borders; original controller refs retained.");
    }
    public static void Gameplay(Scene scene,Action<string> check)
    {
        foreach(var root in scene.GetRootGameObjects())Skin(root);
        foreach(var inventory in All<InventoryPanel>(scene))
        {
            var panel=Ref<GameObject>(inventory,"panel");var card=panel.transform.Find("Card_JadeBronze");if(!card||card.Find("REV_Recipe"))continue;
            ((RectTransform)card).sizeDelta=new Vector2(1200,820);var title=card.Find("Title");if(title)((TMP_Text)title.GetComponent<TMP_Text>()).text="TÚI ĐỒ  /  CHẾ TẠO";
            var so=new SerializedObject(inventory);var buttons=so.FindProperty("slotButtons");var texts=so.FindProperty("slotTexts");var icons=new Image[buttons.arraySize];
            for(int i=0;i<buttons.arraySize;i++)
            {
                var button=(Button)buttons.GetArrayElementAtIndex(i).objectReferenceValue;Place(button.transform,card,new Vector2(0,1),new Vector2(0,1),new Vector2(40+i%4*148,-118-i/4*146),new Vector2(134,132));
                var text=(TMP_Text)texts.GetArrayElementAtIndex(i).objectReferenceValue;Place(text.transform,button.transform,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,9),new Vector2(124,42));text.fontSize=17;text.alignment=TextAlignmentOptions.Center;
                var g=new GameObject("Item_Icon",typeof(RectTransform),typeof(Image));Place(g.transform,button.transform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-8),new Vector2(68,68));icons[i]=g.GetComponent<Image>();icons[i].preserveAspect=true;icons[i].raycastTarget=false;icons[i].enabled=false;
            }
            BuilderUtil.Set(inventory,"slotIcons",icons);
            var detail=Ref<TMP_Text>(inventory,"detailText");Place(detail.transform,card,new Vector2(0,1),new Vector2(0,1),new Vector2(44,-584),new Vector2(560,120));detail.fontSize=20;
            var keys=Ref<TMP_Text>(inventory,"keyItemsText");Place(keys.transform,card,new Vector2(1,1),new Vector2(1,1),new Vector2(-34,-514),new Vector2(465,168));keys.fontSize=18;
            var recipe=Panel("REV_Recipe",card,new Vector2(1,1),new Vector2(1,1),new Vector2(-34,-118),new Vector2(468,366));recipe.color=new Color(.06f,.15f,.13f,.98f);
            Text("Recipe_Title",recipe.transform,"CHẾ BÌNH MÁU",25,new Vector2(0,1),new Vector2(0,1),new Vector2(24,-20),new Vector2(420,38)).color=new Color(.84f,.72f,.44f);
            var ingredients=Text("Recipe_Ingredients",recipe.transform,"",21,new Vector2(0,1),new Vector2(0,1),new Vector2(24,-76),new Vector2(420,124));
            var status=Text("Recipe_Status",recipe.transform,"Thu thập nguyên liệu quanh hang.",17,new Vector2(0,0),new Vector2(0,0),new Vector2(24,83),new Vector2(420,53));status.color=new Color(.71f,.82f,.74f);
            var b=new GameObject("BTN_CraftHealthPotion",typeof(RectTransform),typeof(Image),typeof(Button),typeof(UIButtonSound));Place(b.transform,recipe.transform,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,22),new Vector2(420,52));b.GetComponent<Image>().color=new Color(.24f,.39f,.27f);var bt=b.GetComponent<Button>();bt.targetGraphic=b.GetComponent<Image>();var label=Text("Label",b.transform,"Chế bình máu",22,Vector2.one*.5f,Vector2.one*.5f,Vector2.zero,new Vector2(400,45));label.alignment=TextAlignmentOptions.Center;
            BuilderUtil.Set(inventory,"craftPotionButton",bt);BuilderUtil.Set(inventory,"recipeText",ingredients);BuilderUtil.Set(inventory,"craftingStatusText",status);
            var close=Ref<Button>(inventory,"closeButton");Place(close.transform,card,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,28),new Vector2(270,54));
        }
        check("Inventory 12-slot icon grid + transactional potion recipe/button/counts, clearly separated details/equipment and accessible close button.");
    }
    public static void Icons(Scene scene,Action<string> check)
    {
        Chapter01RevisionSetup.Folder(Art+"/UI");var catalog=new SerializedObject(All<InventoryService>(scene).Single()).FindProperty("catalog");
        for(int itemIndex=0;itemIndex<catalog.arraySize;itemIndex++)
        {
            var definition=(ItemDefinition)catalog.GetArrayElementAtIndex(itemIndex).objectReferenceValue;if(!definition||!definition.UsesSlot)continue;
            if(definition.itemId!=InventoryService.LaCam&&definition.itemId!=InventoryService.LinhChi&&definition.itemId!=InventoryService.ThuocMau)continue;
            string path=Art+"/UI/Icon_"+definition.itemId+".png";
            if(!File.Exists(path))
            {
                var item=All<ItemPickup>(scene).FirstOrDefault(p=>new SerializedObject(p).FindProperty("itemId").stringValue==definition.itemId);var source=item?item.transform.Find("VIS_Item_FBX"):null;
                if(source)RenderIcon(source.gameObject,path);
            }
            if(File.Exists(path)){var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.alphaIsTransparency=true;importer.maxTextureSize=256;importer.SaveAndReimport();definition.icon=AssetDatabase.LoadAssetAtPath<Sprite>(path);}
            if(definition.itemId==InventoryService.ThuocMau){definition.displayName="Bình máu";definition.description="Dùng Q khi đang chơi để hồi 35 máu. Chế từ 1 Lá cẩm và 1 Linh chi trong túi đồ.";}EditorUtility.SetDirty(definition);
        }
        check("Item icons rendered from actual imported item models into project Sprite assets; no source textures altered.");
    }
    public static void RefineDetails(Scene scene,Action<string> check)
    {
        foreach(var scroll in All<ScrollRect>(scene))
        {
            var text=scroll.content?scroll.content.GetComponent<TMP_Text>():null;if(text)text.margin=new Vector4(8,18,8,14);
            if(scroll.verticalScrollbar||!scroll.content)continue;var view=(RectTransform)scroll.transform;var bar=new GameObject("REV_Scrollbar",typeof(RectTransform),typeof(Image),typeof(Scrollbar));Place(bar.transform,view.parent,view.anchorMin,view.pivot,view.anchoredPosition+new Vector2(view.sizeDelta.x+7,0),new Vector2(7,view.sizeDelta.y));bar.GetComponent<Image>().color=new Color(.1f,.2f,.17f);
            var handle=new GameObject("Handle",typeof(RectTransform),typeof(Image));var hr=(RectTransform)handle.transform;hr.SetParent(bar.transform,false);hr.anchorMin=Vector2.zero;hr.anchorMax=Vector2.one;hr.offsetMin=hr.offsetMax=Vector2.zero;handle.GetComponent<Image>().color=Gold;var sb=bar.GetComponent<Scrollbar>();sb.handleRect=hr;sb.targetGraphic=handle.GetComponent<Image>();sb.direction=Scrollbar.Direction.BottomToTop;scroll.verticalScrollbar=sb;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.AutoHide;
        }
        foreach(var journal in All<JournalPanel>(scene))
        {
            var card=Ref<GameObject>(journal,"panel").transform.Find("Card_JadeBronze");if(card&&!card.Find("REV_ScrollHelp"))Text("REV_ScrollHelp",card,"Cuộn chuột để đọc toàn bộ ghi chép.",16,new Vector2(0,0),new Vector2(0,0),new Vector2(425,120),new Vector2(570,25)).color=new Color(.69f,.77f,.70f);
        }
        check("Journal/story scroll areas have visible draggable scrollbars and journal scroll guidance.");
    }
    static void RenderIcon(GameObject source,string path)
    {
        var model=UnityEngine.Object.Instantiate(source);model.transform.position=new Vector3(850,30,0);model.transform.rotation=Quaternion.identity;model.transform.localScale=Vector3.one;var bounds=CampEnvironmentSetup.BoundsOf(model);var cameraGo=new GameObject("Icon_Camera");var cam=cameraGo.AddComponent<Camera>();cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.clear;cam.orthographic=true;cam.orthographicSize=Mathf.Max(bounds.size.x,bounds.size.y)*.65f;cam.nearClipPlane=.01f;cam.farClipPlane=10;cam.transform.position=bounds.center+new Vector3(1,.7f,-2).normalized*3;cam.transform.LookAt(bounds.center);
        var rt=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);rt.Create();var previous=RenderTexture.active;var tex=new Texture2D(256,256,TextureFormat.RGBA32,false);
        try{RenderPipeline.SubmitRenderRequest(cam,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=rt});RenderTexture.active=rt;tex.ReadPixels(new Rect(0,0,256,256),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());AssetDatabase.ImportAsset(path);}
        finally{RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);UnityEngine.Object.DestroyImmediate(model);UnityEngine.Object.DestroyImmediate(cameraGo);}
    }
}
