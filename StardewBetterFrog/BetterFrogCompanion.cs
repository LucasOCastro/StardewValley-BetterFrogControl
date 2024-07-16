using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Monsters;

namespace StardewBetterFrog;

//StardewValley.Stats.monsterKilled
//StardewValley.GameLocation.onMonsterKilled
//StardewValley.Monsters.Monster.takeDamage for knockback

public class BetterFrogCompanion : HungryFrogCompanion
{
    private static readonly PropertyInfo AttachedMonsterField = AccessTools.Property(typeof(HungryFrogCompanion), "attachedMonster");
    private static readonly MethodInfo LocationMonsterKilledMethod = AccessTools.Method(typeof(GameLocation), "onMonsterKilled");

    private Monster? _monsterInMouth;

    public BetterFrogCompanion()
    {
        ModEntry.MonitorSingleton?.Log("Better frog constructed.");
    }

    public BetterFrogCompanion(int variant) : base(variant)
    {
        ModEntry.MonitorSingleton?.Log("Better frog constructed.");
    }
    
    public override void Update(GameTime time, GameLocation location)
    {
        //Digested a monster
        if (fullnessTime > 0 && fullnessTime <= (float)time.ElapsedGameTime.TotalMilliseconds)
        {
            OnDigestionComplete(location);
        }
        
        base.Update(time, location);
        
        //Ate a new monster
        if (AttachedMonsterField.GetValue(this) is Monster attachedMonster && attachedMonster != _monsterInMouth)
        {
            OnMonsterEaten(attachedMonster);
        }
    }

    public override void OnOwnerWarp()
    {
        base.OnOwnerWarp();
        _monsterInMouth = null;
    }

    /// <summary>
    /// Called when the frog finishes digesting what it had in its mouth and is able to swallow another monster.
    /// </summary>
    private void OnDigestionComplete(GameLocation location)
    {
        if (_monsterInMouth == null) return;

        if (ModEntry.ConfigSingleton.CountAsPlayerKill)
        {
            //location.onMonsterKilled(Farmer who, Monster monster, Rectangle monsterBox);
            LocationMonsterKilledMethod.Invoke(location, new object[]{Owner, _monsterInMouth, new Rectangle(Position.ToPoint(), new(40))});   
        }
        _monsterInMouth = null;
    }

    /// <summary>
    /// Called when a monster is brought to the frog via tongue and swallowed.
    /// </summary>
    private void OnMonsterEaten(Monster monster)
    {
        _monsterInMouth = monster;
    }
}