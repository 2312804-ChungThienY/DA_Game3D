using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Chapter01UISetup
{
    static readonly Color Ink=new Color(.035f,.075f,.073f,.96f),Gold=new Color(.68f,.55f,.30f),Paper=new Color(.94f,.92f,.83f);
    static TMP_FontAsset Font=>AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BuilderUtil.FontPath);
    static Sprite frame;
    static T Ref<T>(UnityEngine.Object obj,string field)where T:UnityEngine.Object{var p=new SerializedObject(obj).FindProperty(field);return p!=null?p.objectReferenceValue as T:null;}
    static T[] All<T>(Scene scene)where T:Component=>scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<T>(true)).ToArray();
    static RectTransform Rect(Transform t,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size)
    {var r=(RectTransform)t;r.SetParent(parent,false);r.localScale=Vector3.one;r.localRotation=Quaternion.identity;r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
    static void Stretch(RectTransform r,Transform parent,float padding=0){r.SetParent(parent,false);r.localScale=Vector3.one;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.one*padding;r.offsetMax=-Vector2.one*padding;}
    static Image Panel(string name,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size,bool block=false)
    {
        var g=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));var r=Rect(g.transform,parent,anchor,pivot,pos,size);var image=g.GetComponent<Image>();image.color=Ink;image.raycastTarget=block;
        var outline=g.AddComponent<Outline>();outline.effectColor=new Color(Gold.r,Gold.g,Gold.b,.65f);outline.effectDistance=new Vector2(1,-1);
        if(frame){var edge=new GameObject("Kenney_Frame",typeof(RectTransform),typeof(Image));Stretch((RectTransform)edge.transform,r);var im=edge.GetComponent<Image>();im.sprite=frame;im.type=Image.Type.Sliced;im.fillCenter=false;im.color=new Color(Gold.r,Gold.g,Gold.b,.55f);im.raycastTarget=false;}
        return image;
    }
    static TMP_Text Text(string name,Transform parent,string value,float size,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 dimensions,TextAlignmentOptions align=TextAlignmentOptions.Left)
    {
        var g=new GameObject(name,typeof(RectTransform),typeof(TextMeshProUGUI));Rect(g.transform,parent,anchor,pivot,pos,dimensions);var t=g.GetComponent<TextMeshProUGUI>();t.font=Font;t.fontSize=size;t.text=value;t.color=Paper;t.alignment=align;t.raycastTarget=false;t.textWrappingMode=TextWrappingModes.Normal;return t;
    }
    static void StyleText(TMP_Text text,float size=22){if(!text)return;text.font=Font;text.fontSize=size;text.color=Paper;text.enableAutoSizing=false;text.raycastTarget=false;}
    static void ShowGraphics(GameObject g){foreach(var graphic in g.GetComponentsInChildren<Graphic>(true)){graphic.enabled=true;if(graphic is TMP_Text t)StyleText(t);}}
    static void Button(Button button,Transform parent,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size,string label=null)
    {
        Rect(button.transform,parent,anchor,pivot,pos,size);ShowGraphics(button.gameObject);var image=button.GetComponent<Image>();if(image){image.color=new Color(.10f,.20f,.18f,1);button.targetGraphic=image;var outline=button.GetComponent<Outline>()??button.gameObject.AddComponent<Outline>();outline.effectColor=Gold;outline.effectDistance=new Vector2(1,-1);}
        var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(.70f,.94f,.78f);colors.pressedColor=new Color(.54f,.72f,.61f);colors.selectedColor=colors.highlightedColor;colors.disabledColor=new Color(.40f,.43f,.40f,.65f);colors.fadeDuration=.12f;button.colors=colors;
        var text=button.GetComponentInChildren<TMP_Text>(true);if(text){Stretch(text.rectTransform,button.transform,8);StyleText(text,22);text.alignment=TextAlignmentOptions.Center;if(label!=null)text.text=label;}
        if(!button.GetComponent<UIButtonSound>())button.gameObject.AddComponent<UIButtonSound>();
    }
    static RectTransform Modal(GameObject panel,Vector2 size,string title)
    {
        foreach(var g in panel.GetComponentsInChildren<Graphic>(true))g.enabled=false;
        Stretch((RectTransform)panel.transform,panel.transform.parent);var bg=panel.GetComponent<Image>()??panel.AddComponent<Image>();bg.enabled=true;bg.color=new Color(.015f,.03f,.03f,.84f);bg.raycastTarget=true;
        var card=Panel("Card_JadeBronze",panel.transform,Vector2.one*.5f,Vector2.one*.5f,Vector2.zero,size,true);
        var heading=Text("Title",card.transform,title,32,new Vector2(0,1),new Vector2(0,1),new Vector2(40,-32),new Vector2(size.x-80,50));heading.fontStyle=FontStyles.Bold;
        return (RectTransform)card.transform;
    }
    public static void Prepare()
    {
        Chapter01CompletionArt.Folder("Assets/_Project/Art/Chapter01Completion/UI");const string src="Assets/A28 - Fantasy UI Borders/PNG/Double/Transparent center/panel-transparent-center-001.png",dest="Assets/_Project/Art/Chapter01Completion/UI/Panel_Frame.png";
        if(!File.Exists(dest)&&File.Exists(src))AssetDatabase.CopyAsset(src,dest);
        if(File.Exists(dest)){var importer=(TextureImporter)AssetImporter.GetAtPath(dest);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spriteBorder=new Vector4(16,16,16,16);importer.SaveAndReimport();frame=AssetDatabase.LoadAssetAtPath<Sprite>(dest);}
    }
    static void Slider(Slider slider,Transform card,string title,float y,bool decimals=false)
    {
        Rect(slider.transform,card,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(110,y-24),new Vector2(390,30));ShowGraphics(slider.gameObject);
        foreach(var image in slider.GetComponentsInChildren<Image>(true))image.color=image.transform==slider.handleRect?Gold:image.transform==slider.fillRect?new Color(.38f,.67f,.52f):new Color(.13f,.24f,.22f);
        Text("Label_"+title,card,title,21,new Vector2(0,1),new Vector2(0,1),new Vector2(40,y-23),new Vector2(240,34));
        var value=Text("Value_"+title,card,"",18,new Vector2(1,1),new Vector2(1,1),new Vector2(-40,y+7),new Vector2(95,28),TextAlignmentOptions.Right);
        var presenter=value.gameObject.AddComponent<SliderValueLabel>();BuilderUtil.Set(presenter,"slider",slider);BuilderUtil.Set(presenter,"decimals",decimals);
    }
    public static void SharedUI(Action<string> check)
    {
        Prepare();const string path="Assets/_Project/Resources/PF_AppRootUI.prefab";var root=PrefabUtility.LoadPrefabContents(path);
        try
        {
            var ui=root.GetComponent<AppRootUI>()??root.GetComponentInChildren<AppRootUI>(true);if(root.transform.Find("C1_UI_Theme")){check("Shared UI theme retained from completed prefab step.");return;}new GameObject("C1_UI_Theme").transform.SetParent(root.transform,false);
            var settings=Ref<GameObject>(ui,"settingsPanel");var card=Modal(settings,new Vector2(760,700),"CÀI ĐẶT");var volume=Ref<Slider>(ui,"volumeSlider");
            var music=UnityEngine.Object.Instantiate(volume.gameObject,volume.transform.parent).GetComponent<Slider>();music.name="SLD_Music";var sfx=UnityEngine.Object.Instantiate(volume.gameObject,volume.transform.parent).GetComponent<Slider>();sfx.name="SLD_SFX";
            Slider(volume,card,"Âm lượng tổng",-102);Slider(music,card,"Nhạc",-180);Slider(sfx,card,"Hiệu ứng",-258);Slider(Ref<Slider>(ui,"sensitivitySlider"),card,"Độ nhạy chuột",-336,true);
            BuilderUtil.Set(ui,"musicSlider",music);BuilderUtil.Set(ui,"sfxSlider",sfx);
            var toggle=Ref<Toggle>(ui,"fullscreenToggle");Rect(toggle.transform,card,new Vector2(0,1),new Vector2(0,1),new Vector2(40,-508),new Vector2(620,42));ShowGraphics(toggle.gameObject);var toggleLabel=toggle.GetComponentInChildren<TMP_Text>();if(toggleLabel)toggleLabel.text="Toàn màn hình";
            Text("Quality_Label",card,"Chất lượng",21,new Vector2(0,1),new Vector2(0,1),new Vector2(40,-432),new Vector2(240,40));
            var dropdown=TMP_DefaultControls.CreateDropdown(new TMP_DefaultControls.Resources()).GetComponent<TMP_Dropdown>();dropdown.name="DD_Quality";Rect(dropdown.transform,card,new Vector2(1,1),new Vector2(1,1),new Vector2(-40,-429),new Vector2(390,44));dropdown.ClearOptions();dropdown.AddOptions(new System.Collections.Generic.List<string>{"Thấp","Vừa"});foreach(var t in dropdown.GetComponentsInChildren<TMP_Text>(true))StyleText(t,20);BuilderUtil.Set(ui,"qualityDropdown",dropdown);
            Button(Ref<Button>(ui,"settingsBackButton"),card,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,36),new Vector2(250,56),"Trở lại");settings.SetActive(false);
            var pause=Ref<GameObject>(ui,"pausePanel");card=Modal(pause,new Vector2(620,460),"TẠM DỪNG");Button(Ref<Button>(ui,"resumeButton"),card,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-120),new Vector2(420,62),"Tiếp tục hành trình");Button(Ref<Button>(ui,"pauseSettingsButton"),card,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-205),new Vector2(420,62),"Cài đặt");Button(Ref<Button>(ui,"pauseMenuButton"),card,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-290),new Vector2(420,62),"Về menu chính");pause.SetActive(false);
            var death=Ref<GameObject>(ui,"deathPanel");card=Modal(death,new Vector2(700,420),"LINH MẠCH CHƯA TẮT");Text("Respawn_Hint",card,"Trở lại mốc đã lưu để tiếp tục hành trình.",22,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,12),new Vector2(600,80),TextAlignmentOptions.Center);Button(Ref<Button>(ui,"respawnButton"),card,new Vector2(.5f,0),new Vector2(1,0),new Vector2(-14,55),new Vector2(270,60),"Hồi sinh");Button(Ref<Button>(ui,"deathMenuButton"),card,new Vector2(.5f,0),new Vector2(0,0),new Vector2(14,55),new Vector2(270,60),"Về menu");death.SetActive(false);
            var confirm=Ref<GameObject>(ui,"confirmPanel");card=Modal(confirm,new Vector2(720,410),"XÁC NHẬN");var message=Ref<TMP_Text>(ui,"confirmText");Rect(message.transform,card,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(0,12),new Vector2(620,120));message.enabled=true;StyleText(message,23);message.alignment=TextAlignmentOptions.Center;Button(Ref<Button>(ui,"confirmYesButton"),card,new Vector2(.5f,0),new Vector2(1,0),new Vector2(-14,42),new Vector2(250,58),"Đồng ý");Button(Ref<Button>(ui,"confirmNoButton"),card,new Vector2(.5f,0),new Vector2(0,0),new Vector2(14,42),new Vector2(250,58),"Quay lại");confirm.SetActive(false);
            var toast=Ref<TMP_Text>(ui,"toastText");var parent=toast.canvas.transform;var toastGroup=Panel("Toast_Jade",parent,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,115),new Vector2(900,62));Stretch(toast.rectTransform,toastGroup.transform,14);StyleText(toast,22);toast.alignment=TextAlignmentOptions.Center;BuilderUtil.Set(ui,"toastGroup",toastGroup.gameObject);toastGroup.gameObject.SetActive(false);
            foreach(var scaler in root.GetComponentsInChildren<CanvasScaler>(true)){scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;}
            PrefabUtility.SaveAsPrefabAsset(root,path);check("Shared UI: three audio sliders, sensitivity/fullscreen/quality, styled pause/death/confirm/toast; original buttons and EventSystem retained.");
        }
        finally{PrefabUtility.UnloadPrefabContents(root);}
    }
    public static void Menu(Scene scene,Action<string> check)
    {
        Prepare();var menu=All<MainMenuController>(scene).Single();var canvas=All<Canvas>(scene).First();if(canvas.transform.Find("C1_MainMenu_Theme")){check("Main menu theme retained from completed scene step.");return;}
        foreach(var graphic in All<Graphic>(scene))graphic.enabled=false;var theme=Panel("C1_MainMenu_Theme",canvas.transform,Vector2.one*.5f,Vector2.one*.5f,Vector2.zero,Vector2.zero);Stretch((RectTransform)theme.transform,canvas.transform);theme.transform.SetAsFirstSibling();theme.color=new Color(.035f,.07f,.07f);
        string dest="Assets/_Project/Art/Chapter01Completion/UI/Menu_Cave.png";if(!File.Exists(dest)){File.Copy("docs/images/c1-expanded-main-camp-2026-11-05.png",dest);AssetDatabase.ImportAsset(dest);}
        var importer=(TextureImporter)AssetImporter.GetAtPath(dest);importer.textureType=TextureImporterType.Sprite;importer.maxTextureSize=2048;importer.SaveAndReimport();var picture=new GameObject("Cave_Backdrop",typeof(RectTransform),typeof(Image));Stretch((RectTransform)picture.transform,theme.transform);var image=picture.GetComponent<Image>();image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(dest);image.color=new Color(.46f,.56f,.54f);image.raycastTarget=false;
        var shade=Panel("Title_Shade",theme.transform,new Vector2(0,.5f),new Vector2(0,.5f),Vector2.zero,new Vector2(840,1300));shade.color=new Color(.025f,.055f,.052f,.94f);
        var title=Text("Title_SonDoong",canvas.transform,"SƠN ĐOÒNG",78,new Vector2(0,1),new Vector2(0,1),new Vector2(118,-134),new Vector2(900,120));title.fontStyle=FontStyles.Bold;
        Text("Subtitle",canvas.transform,"CÕI MỘNG KỲ THẦN",27,new Vector2(0,1),new Vector2(0,1),new Vector2(124,-264),new Vector2(700,60)).color=Gold;
        Text("Chapter",canvas.transform,"CHƯƠNG I  /  LINH MẠCH KIM QUY",18,new Vector2(0,1),new Vector2(0,1),new Vector2(124,-326),new Vector2(720,40)).color=new Color(.69f,.77f,.70f);
        string[] fields={"continueButton","newGameButton","settingsButton","quitButton"},labels={"Tiếp tục","Chơi mới","Cài đặt","Thoát"};for(int i=0;i<4;i++)Button(Ref<Button>(menu,fields[i]),canvas.transform,new Vector2(0,1),new Vector2(0,1),new Vector2(126,-427-i*84),new Vector2(470,62),labels[i]);
        Text("Journey",canvas.transform,"Khám phá lòng hang. Đánh thức linh mạch.",19,new Vector2(0,0),new Vector2(0,0),new Vector2(126,85),new Vector2(780,42)).color=new Color(.72f,.79f,.73f);
        foreach(var scaler in All<CanvasScaler>(scene)){scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;}
        check("Main menu: cave backdrop, jade/bronze type and four original functional buttons; Continue availability stays save-driven.");
    }
    public static void Gameplay(Scene scene,Action<string> check)
    {
        Prepare();var hud=All<HudBars>(scene).Single();var canvas=hud.GetComponentInParent<Canvas>()??All<Canvas>(scene).First(c=>c.GetComponentInChildren<HudBars>(true));if(canvas.transform.Find("C1_HUD_Theme"))throw new InvalidOperationException("HUD already installed.");
        var root=new GameObject("C1_HUD_Theme",typeof(RectTransform));Stretch((RectTransform)root.transform,canvas.transform);
        var vitals=Panel("Vitals_Frame",root.transform,new Vector2(0,1),new Vector2(0,1),new Vector2(42,-38),new Vector2(348,176));
        var oldLantern=Ref<GameObject>(hud,"lanternGroup");
        string[] fills={"healthFill","staminaFill","lanternFill"},labels={"healthLabel","staminaLabel","lanternLabel"};Color[] colors={new Color(.76f,.20f,.22f),new Color(.28f,.65f,.46f),new Color(.91f,.64f,.22f)};
        for(int i=0;i<3;i++)
        {
            var fill=Ref<RectTransform>(hud,fills[i]);var label=Ref<TMP_Text>(hud,labels[i]);var oldParent=fill.parent;
            var row=Panel("Resource_"+i,vitals.transform,new Vector2(0,1),new Vector2(0,1),new Vector2(18,-10-i*52),new Vector2(312,48));row.color=Color.clear;if(row.GetComponent<Outline>())UnityEngine.Object.DestroyImmediate(row.GetComponent<Outline>());foreach(Transform t in row.transform)if(t.name=="Kenney_Frame")t.gameObject.SetActive(false);
            Rect(label.transform,row.transform,new Vector2(0,1),new Vector2(0,1),Vector2.zero,new Vector2(312,25));label.enabled=true;StyleText(label,18);label.alignment=TextAlignmentOptions.Left;
            var track=Panel("Track",row.transform,new Vector2(0,0),new Vector2(0,0),new Vector2(0,4),new Vector2(312,10));track.color=new Color(.12f,.19f,.18f);
            Stretch(fill,track.transform);fill.anchorMax=new Vector2(1,1);var graphic=fill.GetComponent<Image>();if(graphic){graphic.enabled=true;graphic.color=colors[i];}
            if(oldParent&&oldParent!=canvas.transform&&oldParent!=hud.transform)oldParent.gameObject.SetActive(false);
            if(i==2)BuilderUtil.Set(hud,"lanternGroup",row.gameObject);
        }
        if(oldLantern)oldLantern.SetActive(false);
        var potion=Ref<TMP_Text>(hud,"potionText");var pbox=Panel("Potion_Quickslot",root.transform,Vector2.zero,Vector2.zero,new Vector2(42,42),new Vector2(286,56));Stretch(potion.rectTransform,pbox.transform,13);StyleText(potion,20);
        var tracker=All<QuestTracker>(scene).Single();var objective=Ref<TMP_Text>(tracker,"objectiveText");var oldObjectiveParent=objective.transform.parent;
        var quest=Panel("Quest_Frame",root.transform,Vector2.one,Vector2.one,new Vector2(-42,-38),new Vector2(480,186));Text("Quest_Header",quest.transform,"LINH MẠCH KIM QUY",18,new Vector2(0,1),new Vector2(0,1),new Vector2(22,-16),new Vector2(435,28)).color=Gold;
        Rect(objective.transform,quest.transform,new Vector2(0,1),new Vector2(0,1),new Vector2(22,-57),new Vector2(435,65));StyleText(objective,21);objective.enabled=true;objective.alignment=TextAlignmentOptions.Left;
        var progress=Text("Quest_Progress",quest.transform,"Mảnh trống  0/3     •     Câu đố  0/2",18,new Vector2(0,0),new Vector2(0,0),new Vector2(22,16),new Vector2(435,30));progress.color=new Color(.69f,.83f,.73f);BuilderUtil.Set(tracker,"progressText",progress);
        if(oldObjectiveParent!=canvas.transform&&oldObjectiveParent!=hud.transform&&oldObjectiveParent!=root.transform)oldObjectiveParent.gameObject.SetActive(false);
        var prompt=Ref<TMP_Text>(All<InteractionScanner>(scene).Single(),"promptText");if(prompt){Rect(prompt.transform,root.transform,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,154),new Vector2(950,60));StyleText(prompt,24);prompt.alignment=TextAlignmentOptions.Center;}
        var lantern=All<PlayerMotor>(scene).Single().GetComponent<LanternController>();var energy=Ref<TMP_Text>(lantern,"energyText");if(energy&&energy!=Ref<TMP_Text>(hud,"lanternLabel")){Rect(energy.transform,root.transform,new Vector2(1,0),new Vector2(1,0),new Vector2(-42,94),new Vector2(465,38));energy.enabled=true;energy.gameObject.SetActive(true);StyleText(energy,18);energy.alignment=TextAlignmentOptions.Right;energy.color=Gold;}
        Text("Controls",root.transform,"WASD  Di chuyển     E  Tương tác     I  Túi đồ     J  Nhật ký     Esc  Tạm dừng",17,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(90,42),new Vector2(1020,32),TextAlignmentOptions.Center).color=new Color(.80f,.84f,.77f);
        var area=Text("Area_Title",root.transform,"",22,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-44),new Vector2(550,45),TextAlignmentOptions.Center);area.fontStyle=FontStyles.Bold;var group=area.gameObject.AddComponent<CanvasGroup>();group.blocksRaycasts=false;var banner=area.gameObject.AddComponent<ChapterAreaBanner>();BuilderUtil.Set(banner,"target",All<PlayerMotor>(scene).Single());BuilderUtil.Set(banner,"label",area);BuilderUtil.Set(banner,"group",group);
        Panels(scene);foreach(var scaler in All<CanvasScaler>(scene)){scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;}
        check("C1 HUD: compact vitals, potion quickslot, contextual E, area banner, Moss Garden objective and separate 3-fragment/2-puzzle progress; inventory/journal/story restyled.");
    }
    static void Panels(Scene scene)
    {
        foreach(var inventory in All<InventoryPanel>(scene))
        {
            var panel=Ref<GameObject>(inventory,"panel");var card=Modal(panel,new Vector2(1040,670),"TÚI ĐỒ");var so=new SerializedObject(inventory);var buttons=so.FindProperty("slotButtons");
            for(int i=0;i<buttons.arraySize;i++)Button((Button)buttons.GetArrayElementAtIndex(i).objectReferenceValue,card,new Vector2(0,1),new Vector2(0,1),new Vector2(38+i%4*138,-117-i/4*118),new Vector2(124,104));
            var detail=Ref<TMP_Text>(inventory,"detailText");Rect(detail.transform,card,new Vector2(1,1),new Vector2(1,1),new Vector2(-35,-115),new Vector2(350,220));detail.enabled=true;StyleText(detail,21);
            var keys=Ref<TMP_Text>(inventory,"keyItemsText");Rect(keys.transform,card,new Vector2(1,1),new Vector2(1,1),new Vector2(-35,-365),new Vector2(350,165));keys.enabled=true;StyleText(keys,18);
            Button(Ref<Button>(inventory,"closeButton"),card,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,26),new Vector2(240,54),"Đóng túi");BuilderUtil.Set(inventory,"normalColor",new Color(.10f,.20f,.18f));BuilderUtil.Set(inventory,"selectedColor",new Color(.36f,.38f,.20f));panel.SetActive(false);
        }
        foreach(var journal in All<JournalPanel>(scene))
        {
            var panel=Ref<GameObject>(journal,"panel");var card=Modal(panel,new Vector2(1040,700),"NHẬT KÝ HÀNH TRÌNH");
            var objective=Ref<TMP_Text>(journal,"objectiveText");Rect(objective.transform,card,new Vector2(0,1),new Vector2(0,1),new Vector2(40,-120),new Vector2(350,260));objective.enabled=true;StyleText(objective,22);
            var points=Ref<TMP_Text>(journal,"pointsText");Rect(points.transform,card,new Vector2(0,0),new Vector2(0,0),new Vector2(40,130),new Vector2(350,70));points.enabled=true;StyleText(points,21);
            var knowledge=Ref<TMP_Text>(journal,"knowledgeText");ScrollText(knowledge,card,new Vector2(425,-115),new Vector2(570,430));
            Button(Ref<Button>(journal,"closeButton"),card,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,30),new Vector2(250,54),"Đóng nhật ký");panel.SetActive(false);
        }
        foreach(var story in All<StoryCardPanel>(scene))
        {
            var panel=Ref<GameObject>(story,"panel");var card=Modal(panel,new Vector2(1060,760),"");var title=Ref<TMP_Text>(story,"titleText");Rect(title.transform,card,new Vector2(0,1),new Vector2(0,1),new Vector2(42,-34),new Vector2(975,88));title.enabled=true;StyleText(title,30);title.fontStyle=FontStyles.Bold;
            ScrollText(Ref<TMP_Text>(story,"bodyText"),card,new Vector2(42,-148),new Vector2(975,460));Button(Ref<Button>(story,"continueButton"),card,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,36),new Vector2(360,58));panel.SetActive(false);
        }
        foreach(var boss in All<StoneGuardian>(scene))
        {
            var bar=Ref<GameObject>(boss,"bossBar");if(!bar)continue;Rect(bar.transform,bar.transform.parent,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-112),new Vector2(560,60));
            var label=Ref<TMP_Text>(boss,"bossBarLabel");if(label)StyleText(label,21);
        }
    }
    static void ScrollText(TMP_Text text,Transform parent,Vector2 pos,Vector2 size)
    {
        var view=Panel("Scroll_"+text.name,parent,new Vector2(0,1),new Vector2(0,1),pos,size,true);view.color=new Color(.025f,.06f,.057f,.5f);view.gameObject.AddComponent<RectMask2D>();var scroll=view.gameObject.AddComponent<ScrollRect>();scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=32;scroll.viewport=(RectTransform)view.transform;
        Rect(text.transform,view.transform,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-8),new Vector2(size.x-24,size.y-16));text.enabled=true;StyleText(text,21);text.alignment=TextAlignmentOptions.TopLeft;text.overflowMode=TextOverflowModes.Overflow;var fit=text.GetComponent<ContentSizeFitter>()??text.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.content=text.rectTransform;
    }
    public static void Refine(GameObject root)
    {
        var hud=root.GetComponentsInChildren<Transform>(true).FirstOrDefault(t=>t.name=="C1_HUD_Theme");
        if(hud)hud.SetAsFirstSibling();
        foreach(var toast in root.GetComponentsInChildren<RectTransform>(true).Where(t=>t.name=="Toast_Jade"))toast.anchoredPosition=new Vector2(0,52);
        if(hud)foreach(var text in root.GetComponentsInChildren<TMP_Text>(true).Where(t=>t.name=="TXT_SealProgress"||t.name=="TXT_DrumPuzzleStatus"||t.name=="TXT_MirrorStatus"))
        {Rect(text.transform,hud,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,226),new Vector2(900,38));StyleText(text,21);text.alignment=TextAlignmentOptions.Center;text.color=Paper;}
        foreach(var image in root.GetComponentsInChildren<Image>(true))if(image.name=="Kenney_Frame")image.fillCenter=false;
        foreach(var dropdown in root.GetComponentsInChildren<TMP_Dropdown>(true))
        {
            foreach(var image in dropdown.GetComponentsInChildren<Image>(true))image.color=image.name=="Arrow"||image.name.Contains("Checkmark")?Gold:new Color(.10f,.20f,.18f);
            foreach(var text in dropdown.GetComponentsInChildren<TMP_Text>(true))StyleText(text,20);
        }
        foreach(var image in root.GetComponentsInChildren<Image>(true).Where(i=>i.name=="Cave_Backdrop"))
        {
            const string path="Assets/_Project/Art/Chapter01Completion/UI/Menu_Cave.png";var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.spriteImportMode=SpriteImportMode.Single;importer.SaveAndReimport();image.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);image.color=new Color(.75f,.82f,.77f);image.preserveAspect=false;
        }
        foreach(var boss in root.GetComponentsInChildren<StoneGuardian>(true))
        {
            var fill=Ref<RectTransform>(boss,"bossBarFill");if(fill&&fill.GetComponent<Image>())fill.GetComponent<Image>().color=new Color(.56f,.22f,.19f);
            var bar=Ref<GameObject>(boss,"bossBar");if(bar&&bar.GetComponent<Image>())bar.GetComponent<Image>().color=Ink;
        }
    }
    public static void RefreshMenuPicture(Scene scene,Action<string> check)
    {
        var player=All<PlayerMotor>(scene).Single();bool wasActive=player.gameObject.activeSelf;var original=All<Camera>(scene).First(c=>c.CompareTag("MainCamera"));
        var go=new GameObject("MenuPictureCapture");var cam=go.AddComponent<Camera>();cam.CopyFrom(original);cam.enabled=false;cam.transform.position=new Vector3(1.5f,2.7f,-10.8f);cam.transform.LookAt(new Vector3(-1,1.3f,7));
        var target=new RenderTexture(1920,1080,24);target.Create();var previous=RenderTexture.active;var texture=new Texture2D(1920,1080,TextureFormat.RGB24,false);
        try
        {
            player.gameObject.SetActive(false);UnityEngine.Rendering.RenderPipeline.SubmitRenderRequest(cam,new UnityEngine.Rendering.Universal.UniversalRenderPipeline.SingleCameraRequest{destination=target});RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,1920,1080),0,0);texture.Apply();
            const string dest="Assets/_Project/Art/Chapter01Completion/UI/Menu_Cave.png";File.WriteAllBytes(dest,texture.EncodeToPNG());AssetDatabase.ImportAsset(dest);check("Updated menu backdrop rendered from current camp without the placeholder An model; existing Sprite GUID retained.");
        }
        finally{player.gameObject.SetActive(wasActive);RenderTexture.active=previous;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(texture);UnityEngine.Object.DestroyImmediate(go);}
    }
}
