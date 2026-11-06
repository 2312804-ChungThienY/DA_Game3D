using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName="Son Doong/Sound Palette")]
public sealed class SoundPalette:ScriptableObject
{
    public AudioMixerGroup music,sfx,ui;
    public AudioClip menuMusic,explorationMusic,battleMusic,ambience;
    public AudioClip click,hover,pickup,success,hit,stone,door,drum,ghost;
}
