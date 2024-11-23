using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewBetterFrog.FrogStuffs;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Monsters;

// ReSharper disable InconsistentNaming

namespace StardewBetterFrog.Patches;

public static class HungryFrogCompanionPatches
{
    private static readonly MethodInfo FindClosestMonsterMethod = AccessTools.Method(typeof(Utility), nameof(Utility.findClosestMonsterWithinRange), new[] { typeof(GameLocation), typeof(Vector2), typeof(int), typeof(bool), typeof(Func<Monster, bool>) });
    
    private static Func<Monster, bool>? GetMatchPredicate(HungryFrogCompanion instance)
    {
        if (instance is not BetterFrogCompanion frog)
            return null;
        
        return frog.IsAllowed;
    }
    
    
    /// <summary>
    /// Changes the call to the find monster method to use my own custom method which applies the blacklist.
    /// </summary>
    public static IEnumerable<CodeInstruction> Update_UseCustomTargetFunction_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var instructionsList = instructions.ToList();
        for (int i = 0; i < instructionsList.Count; i++)
        {
            bool nextCalls = i < instructionsList.Count - 1 && instructionsList[i + 1].Calls(FindClosestMonsterMethod);
            if (!nextCalls)
            {
                yield return instructionsList[i];
                continue;
            }
            
            // The last instruction before calling the method loads a null for the match predicate.
            // Instead, I'll load the current blacklist's predicate.
            yield return new(OpCodes.Ldarg_0);
            yield return CodeInstruction.Call(
                typeof(HungryFrogCompanionPatches),
                nameof(GetMatchPredicate),
                new[] { typeof(HungryFrogCompanion) }
            );
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