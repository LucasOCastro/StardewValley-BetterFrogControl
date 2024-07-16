using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using StardewBetterFrog.FrogStuffs;
using StardewValley.Companions;

namespace StardewBetterFrog.Patches;

public static class CompanionTrinketEffectPatches
{
    private static readonly ConstructorInfo HungryFrogConstructor = AccessTools.Constructor(typeof(HungryFrogCompanion), new[] {typeof(int)});
    private static readonly ConstructorInfo BetterFrogConstructor = AccessTools.Constructor(typeof(BetterFrogCompanion), new[] {typeof(int)});
    
    public static IEnumerable<CodeInstruction> Apply_UseBetterFrogConstructor_Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (var instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Newobj && HungryFrogConstructor.Equals(instruction.operand))
                instruction.operand = BetterFrogConstructor;
            yield return instruction;
        }
    }
}