using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

public static class Chapter01SoundSetup
{
    public static void Install(Action<string> check)
    {
        Chapter01CompletionArt.Folder("Assets/_Project/Resources/Audio");Chapter01CompletionArt.Folder("Assets/_Project/Audio/C1");
        const string original="Assets/_Project/Audio/Mixers/MainAudioMixer.mixer",dest="Assets/_Project/Resources/Audio/MainAudioMixer.mixer";
        if(AssetDatabase.LoadAssetAtPath<AudioMixer>(original))
        {string before=AssetDatabase.AssetPathToGUID(original),error=AssetDatabase.MoveAsset(original,dest);if(!string.IsNullOrEmpty(error))throw new InvalidOperationException(error);if(before!=AssetDatabase.AssetPathToGUID(dest))throw new InvalidOperationException("Mixer GUID changed.");check("Moved project mixer into Resources with existing GUID/group file IDs retained.");}
        var mixer=AssetDatabase.LoadAssetAtPath<AudioMixer>(dest);if(!mixer)throw new InvalidOperationException("Main mixer missing.");
        var path="Assets/_Project/Resources/C1SoundPalette.asset";var palette=AssetDatabase.LoadAssetAtPath<SoundPalette>(path);if(!palette){palette=ScriptableObject.CreateInstance<SoundPalette>();AssetDatabase.CreateAsset(palette,path);}
        palette.music=mixer.FindMatchingGroups("Music").Single(g=>g.name=="Music");palette.sfx=mixer.FindMatchingGroups("SFX").Single(g=>g.name=="SFX");palette.ui=mixer.FindMatchingGroups("UI").Single(g=>g.name=="UI");
        palette.menuMusic=Copy("Assets/A37 - Dark Cavern Ambient/dark_cavern_ambient_001.ogg","Music_Cavern",true);
        palette.explorationMusic=Copy("Assets/A38 - Deep Humidity/deep-humidity.ogg","Music_Exploration",true);palette.battleMusic=palette.menuMusic;
        palette.ambience=Copy("Assets/A36 - Loopable Dungeon Ambience/dungeon_ambient_1.ogg","Ambience_Dungeon",true);
        palette.click=Copy("Assets/A32 - UI Audio/Audio/click1.ogg","UI_Click");palette.hover=Copy("Assets/A32 - UI Audio/Audio/rollover1.ogg","UI_Hover");
        palette.pickup=Copy("Assets/A32 - UI Audio/Audio/switch4.ogg","Pickup");palette.success=Copy("Assets/A32 - UI Audio/Audio/switch9.ogg","Success");
        palette.hit=Impact("impactGeneric_light_000","Impact_Hit");palette.stone=Impact("impactPlate_heavy_000","Impact_Stone",true);palette.door=Impact("impactWood_heavy_000","Door_Stone");palette.drum=Impact("impactWood_medium_000","Drum_Hit");
        palette.ghost=Copy("Assets/A39 - Ghost Monster Voice Moaning and Growling/wav/qubodup-GhostMoan02.wav","Shadow_Voice");
        EditorUtility.SetDirty(palette);AssetDatabase.SaveAssets();check("Audio palette: Music/SFX/UI mixer routing; three streaming background clips, UI/impact/door/drum/pickup/ghost feedback; original source imports untouched.");
    }
    static AudioClip Impact(string name,string dest,bool fallback=false)
    {
        string source=AssetDatabase.FindAssets(name+" t:AudioClip",new[]{"Assets/A33 - Impact Sounds"}).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
        if(string.IsNullOrEmpty(source)&&fallback)source=AssetDatabase.FindAssets("impactMetal_heavy_000 t:AudioClip",new[]{"Assets/A33 - Impact Sounds"}).Select(AssetDatabase.GUIDToAssetPath).FirstOrDefault();
        if(string.IsNullOrEmpty(source))throw new InvalidOperationException("Missing impact sound: "+name);return Copy(source,dest);
    }
    static AudioClip Copy(string source,string name,bool stream=false)
    {
        string dest="Assets/_Project/Audio/C1/"+name+Path.GetExtension(source);if(!File.Exists(dest)&&!AssetDatabase.CopyAsset(source,dest))throw new InvalidOperationException("Missing source audio: "+source);
        var importer=(AudioImporter)AssetImporter.GetAtPath(dest);var settings=importer.defaultSampleSettings;settings.loadType=stream?AudioClipLoadType.Streaming:AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.65f;importer.defaultSampleSettings=settings;importer.forceToMono=!stream;importer.SaveAndReimport();
        var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(dest);if(!clip||clip.length<=0)throw new InvalidOperationException("Invalid audio: "+dest);return clip;
    }
}
