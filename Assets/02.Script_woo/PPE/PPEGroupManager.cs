using System.Collections.Generic;

public static class PPEGroupManager
{
    // 현재 그룹별 착용 수
    private static readonly Dictionary<PPEGroup, int> equippedCounts = new Dictionary<PPEGroup, int>();

    public static bool CanEquip(PPEGroup targetGroup)
    {
        if (targetGroup == PPEGroup.None)
            return true;

        foreach (var kv in equippedCounts)
        {
            PPEGroup group = kv.Key;
            int count = kv.Value;

            if (group == PPEGroup.None) continue;
            if (count <= 0) continue;

            // 다른 그룹이 하나라도 입고 있으면 막기
            if (group != targetGroup)
                return false;
        }

        return true;
    }

    public static void Register(PPEGroup group)
    {
        if (group == PPEGroup.None) return;

        if (!equippedCounts.ContainsKey(group))
            equippedCounts[group] = 0;

        equippedCounts[group]++;
    }

    public static void Unregister(PPEGroup group)
    {
        if (group == PPEGroup.None) return;
        if (!equippedCounts.ContainsKey(group)) return;

        equippedCounts[group]--;

        if (equippedCounts[group] <= 0)
            equippedCounts.Remove(group);
    }

    public static bool HasAnyEquippedInGroup(PPEGroup group)
    {
        return equippedCounts.ContainsKey(group) && equippedCounts[group] > 0;
    }
}