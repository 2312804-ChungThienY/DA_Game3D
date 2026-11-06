using System;
using System.Collections.Generic;
using UnityEngine;

// Nguồn sự thật của trạng thái thế giới. Object trong scene chỉ đọc ở đây, không tự đọc file lưu.
public class WorldStateStore : MonoBehaviour
{
    public const string DrumA = "relic_drum_a";
    public const string DrumB = "relic_drum_b";
    public const string DrumC = "relic_drum_c";
    public const string DrumPuzzleId = "pz_trong_01";
    public const string MirrorPuzzleId = "pz_guong_01";

    private readonly Dictionary<StateSet, HashSet<string>> sets = new Dictionary<StateSet, HashSet<string>>();

    public event Action Changed;
    // Replacing a snapshot differs from an unrelated addition during a puzzle attempt.
    public uint RestoreVersion { get; private set; }

    public int RelicCount => Count(StateSet.Fragments);

    public bool Has(StateSet set, string id) => id != null && GetSet(set).Contains(id);

    public int Count(StateSet set) => GetSet(set).Count;

    // false nếu đã có hoặc ID không thuộc danh mục.
    public bool TryAdd(StateSet set, string id)
    {
        if (!WorldIds.IsKnown(set, id))
        {
            Debug.LogError($"ID không có trong danh mục {set}: {id}", this);
            return false;
        }

        if (!GetSet(set).Add(id)) return false;
        Changed?.Invoke();
        return true;
    }

    // Atomic rewards: validate the full batch before changing sets; notification is deferred
    // until inventory and the unique pickup claim have also been committed.
    internal bool TryAddBatch(IList<StateRequirement> additions,out bool changed)
    {
        changed=false;
        foreach(var entry in additions)if(entry==null||!WorldIds.IsKnown(entry.set,entry.id))return false;
        foreach(var entry in additions)changed|=GetSet(entry.set).Add(entry.id);
        return true;
    }
    internal void PublishChange()=>Changed?.Invoke();

    public WorldSnapshot Export()
    {
        WorldSnapshot snapshot = new WorldSnapshot();
        foreach (KeyValuePair<StateSet, HashSet<string>> pair in sets)
        {
            if (pair.Value.Count == 0) continue;
            List<string> ids = new List<string>(pair.Value);
            ids.Sort(StringComparer.Ordinal);
            snapshot.sets.Add(new StringSetDto { set = pair.Key.ToString(), ids = ids });
        }
        return snapshot;
    }

    // Thay toàn bộ, không trộn; phát Changed đúng một lần.
    public void Restore(WorldSnapshot snapshot)
    {
        unchecked { RestoreVersion++; }
        foreach (HashSet<string> set in sets.Values) set.Clear();

        if (snapshot != null && snapshot.sets != null)
        {
            foreach (StringSetDto dto in snapshot.sets)
            {
                if (dto == null || dto.ids == null || !Enum.TryParse(dto.set, out StateSet set)) continue;
                foreach (string id in dto.ids)
                    if (WorldIds.IsKnown(set, id)) GetSet(set).Add(id);
            }
        }

        Changed?.Invoke();
    }

    private HashSet<string> GetSet(StateSet set)
    {
        if (!sets.TryGetValue(set, out HashSet<string> ids))
        {
            ids = new HashSet<string>(StringComparer.Ordinal);
            sets.Add(set, ids);
        }
        return ids;
    }

    // Hàm cũ của chương 1, giữ làm wrapper.
    public static bool IsValidRelicId(string id) => WorldIds.IsKnown(StateSet.Fragments, id);
    public bool HasRelic(string id) => Has(StateSet.Fragments, id);
    public bool TryCollectRelic(string id) => TryAdd(StateSet.Fragments, id);
    public bool HasSolvedPuzzle(string id) => Has(StateSet.Puzzles, id);
    public bool TrySolvePuzzle(string id) => TryAdd(StateSet.Puzzles, id);
}
