using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Monsters;
// ReSharper disable InconsistentNaming

namespace StardewBetterFrog;

public static class HungryFrogCompanionPatches
{
    private static readonly MethodInfo FindClosestMonsterMethod = AccessTools.Method(typeof(Utility), nameof(Utility.findClosestMonsterWithinRange), new[] { typeof(GameLocation), typeof(Vector2), typeof(int), typeof(bool) });
    private static readonly MethodInfo FindClosestMonsterBetterFrogMethod = AccessTools.Method(typeof(HungryFrogCompanionPatches), nameof(FindClosestMonster_BetterFrog));

    /// <summary>
    /// If frog is not BetterFrog, use the default <see cref="Utility.findClosestMonsterWithinRange"/>.
    /// If is BetterFrog, use custom logic to apply the current blacklist.
    /// </summary>
    private static Monster FindClosestMonster_BetterFrog(GameLocation location, Vector2 originPoint, int range, bool ignoreUntargetables,
        HungryFrogCompanion instance)
    {
        if (instance is not BetterFrogCompanion frog)
            return Utility.findClosestMonsterWithinRange(location, originPoint, range);
        
        return location.characters
            .OfType<Monster>()
            .Where(m => (!ignoreUntargetables || m is not Spiker) && !m.IsInvisible && !frog.IsBlacklisted(m))
            .Select(m => (Monster: m, Dist: Vector2.Distance(originPoint, m.getStandingPosition())))
            .Where(p => p.Dist <= range)
            .DefaultIfEmpty()
            .MinBy(p => p.Dist).Monster;
    }
    
    
    /// <summary>
    /// Changes the call to the find monster method to use my own custom method which applies the blacklist.
    /// </summary>
    public static IEnumerable<CodeInstruction> Update_UseCustomTargetFunction_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.Calls(FindClosestMonsterMethod))
            {
                yield return new(OpCodes.Ldarg_0);
                instruction.operand = FindClosestMonsterBetterFrogMethod;
            }

            yield return instruction;
        }
    }
    
    /// <summary>
    /// If the frog instance is a <see cref="BetterFrogCompanion"/>, will cache the monster before it is swallowed.
    /// </summary>
    public static void TongueReachedMonster_CallOnMonsterEaten_Prefix(HungryFrogCompanion __instance, Monster m)
    {
        if (__instance is BetterFrogCompanion betterFrog)
            betterFrog.OnMonsterEaten(m);
    }
}