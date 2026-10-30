using System;
using System.Collections.Generic;
using UnityEngine;

public class WorldStateStore : MonoBehaviour
{
    public const string DrumA = "relic_drum_a";
    public const string DrumB = "relic_drum_b";
    public const string DrumC = "relic_drum_c";

    private readonly HashSet<string> collectedRelics =
        new HashSet<string>(StringComparer.Ordinal);

    public event Action Changed;

    public int RelicCount => collectedRelics.Count;

    public static bool IsValidRelicId(string id)
    {
        return id == DrumA || id == DrumB || id == DrumC;
    }

    public bool HasRelic(string id)
    {
        return collectedRelics.Contains(id);
    }

    public bool TryCollectRelic(string id)
    {
        if (!IsValidRelicId(id))
        {
            Debug.LogError($"ID mảnh trống không hợp lệ: {id}", this);
            return false;
        }

        if (!collectedRelics.Add(id))
        {
            return false;
        }

        Changed?.Invoke();
        return true;
    }

    public List<string> ExportRelics()
    {
        return new List<string>(collectedRelics);
    }

    public void RestoreRelics(List<string> ids)
    {
        collectedRelics.Clear();

        if (ids != null)
        {
            foreach (string id in ids)
            {
                if (IsValidRelicId(id))
                {
                    collectedRelics.Add(id);
                }
            }
        }

        Changed?.Invoke();
    }
}
