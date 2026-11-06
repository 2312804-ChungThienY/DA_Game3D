using UnityEngine;
using UnityEngine.Audio;

public static class RoutedAudio
{
    private static bool missingGroupReported;

    public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume, AudioMixerGroup output)
    {
        if (clip == null) return;
        if (output == null)
        {
            if (!missingGroupReported)
            {
                Debug.LogError("RoutedAudio: assign SfxOutput on ThirdPersonController to the SFX mixer group.");
                missingGroupReported = true;
            }
            return;
        }

        GameObject instance = new GameObject("OneShot_SFX");
        instance.transform.position = position;
        AudioSource source = instance.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.outputAudioMixerGroup = output;
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.spatialBlend = 1f;
        source.minDistance = 1f;
        source.maxDistance = 20f;
        source.Play();
        Object.Destroy(instance, clip.length + 0.2f);
    }
}
