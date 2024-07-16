using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Monsters;

namespace StardewBetterFrog;

public class BetterFrogCompanion : HungryFrogCompanion
{
    //float monsterEatCheckTimer;
    private static readonly FieldInfo MonsterEatCheckTimeField = AccessTools.Field(typeof(HungryFrogCompanion), "monsterEatCheckTimer");
    
    //Monster HungryFrogCompanion.attachedMonster
    //private static readonly PropertyInfo AttachedMonsterField = AccessTools.Property(typeof(HungryFrogCompanion), "attachedMonster");
    
    //GameLocation.onMonsterKilled(Farmer who, Monster monster, Rectangle monsterBox);
    private static readonly MethodInfo LocationMonsterKilledMethod = AccessTools.Method(typeof(GameLocation), "onMonsterKilled");

    private Monster? _monsterInMouth;
    private int _oldDamageToFarmer;
    private bool _oldFarmerPassesThrough;
    
    private static Vector2 MousePos => Game1.getMousePosition().ToVector2() + new Vector2(Game1.viewport.X, Game1.viewport.Y);

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
        // Digested a monster
        if (fullnessTime > 0 && fullnessTime <= (float)time.ElapsedGameTime.TotalMilliseconds)
        {
            OnDigestionComplete(location);
        }
        
        base.Update(time, location);
        
        // Ate a new monster
        //if (AttachedMonsterField.GetValue(this) is Monster attachedMonster && attachedMonster != _monsterInMouth)
        //{
        //    OnMonsterEaten(attachedMonster);
        //}
        // CHANGED TO A HARMONY PREFIX
        // Because I need to cache th monster values before being swallowed.

        if (IsLocal)
        {
            HandleInput(location);
        }
    }

    public override void OnOwnerWarp()
    {
        base.OnOwnerWarp();
        _monsterInMouth = null;
    }
    
    /// <summary>
    /// Called when a monster is brought to the frog via tongue and swallowed.
    /// </summary>
    private void OnMonsterEaten(Monster? monster)
    {
        if (monster == null) return;
        
        ModEntry.MonitorSingleton?.Log("Swallowed monster: " + monster.Name);
        _monsterInMouth = monster;
        _oldDamageToFarmer = monster.DamageToFarmer;
        _oldFarmerPassesThrough = monster.farmerPassesThrough;
    }
    
    /// <summary>
    /// Called when the frog finishes digesting what it had in its mouth and is able to swallow another monster.
    /// </summary>
    private void OnDigestionComplete(GameLocation location)
    {
        if (_monsterInMouth == null) return;
        ModEntry.MonitorSingleton?.Log("Digested monster: " + _monsterInMouth.Name);
        
        //Register that the monster was killed
        if (ModEntry.ConfigSingleton.CountAsPlayerKill)
            LocationMonsterKilledMethod.Invoke(location, new object[]{Owner, _monsterInMouth, new Rectangle(Position.ToPoint(), new(40))});   

        _monsterInMouth = null;
    }
    
    /// <summary>
    /// Places the currently swallowed monster back into the location.
    /// </summary>
    private void SpitMonster(GameLocation location)
    {
        ModEntry.MonitorSingleton?.Log("Requested spit monster: " + _monsterInMouth?.Name);
        if (_monsterInMouth == null) return;
        
        fullnessTime = 0; // Coincidentally public field for some reason lol

        Owner.currentLocation.localSound("fishSlap");
        //Add monster back to location
        location.addCharacter(_monsterInMouth);
        _monsterInMouth.Position = Position;
        
        //Send the monster away from the player
        var knockBack = Utility.getAwayFromPlayerTrajectory(new(Position.ToPoint(), new(1)), Owner);
        _monsterInMouth.setTrajectory(knockBack / 3f);
        
        //Set a delay before eating the next monster
        MonsterEatCheckTimeField.SetValue(this, 0);

        //Revert stats to before swallow
        _monsterInMouth.stunTime.Value = 50;
        _monsterInMouth.position.Paused = false;
        _monsterInMouth.DamageToFarmer = _oldDamageToFarmer;
        _monsterInMouth.farmerPassesThrough = _oldFarmerPassesThrough;
        _monsterInMouth = null;
    }

    /// <summary>
    /// Handles local user interaction with the frog.
    /// </summary>
    private void HandleInput(GameLocation location)
    {
        if (!ModEntry.ConfigSingleton.AllowSpittingMonster) return;

        if (_monsterInMouth == null) return;
        if (Game1.input.GetMouseState().RightButton != ButtonState.Pressed) return;
        if (Vector2.Distance(MousePos, Position) > ModEntry.ConfigSingleton.FrogInteractDistance) return;
        
        SpitMonster(location);
    }
    
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// If the frog instance is a <see cref="BetterFrogCompanion"/>,
    /// will cache the monster before it is swallowed.
    /// </summary>
    public static void TongueReachedMonster_Prefix(HungryFrogCompanion __instance, Monster m)
    {
        if (__instance is BetterFrogCompanion betterFrog)
            betterFrog.OnMonsterEaten(m);
    }
}