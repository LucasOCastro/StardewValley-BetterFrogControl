using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Netcode;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Monsters;

namespace StardewBetterFrog.FrogStuffs;

public class BetterFrogCompanion : HungryFrogCompanion
{
    //float monsterEatCheckTimer;
    private static readonly FieldInfo MonsterEatCheckTimeField = AccessTools.Field(typeof(HungryFrogCompanion), "monsterEatCheckTimer");
    
    //GameLocation.onMonsterKilled(Farmer who, Monster monster, Rectangle monsterBox);
    private static readonly MethodInfo LocationMonsterKilledMethod = AccessTools.Method(typeof(GameLocation), "onMonsterKilled");

    private readonly List<SwallowBlacklistPredicate> _blacklistPredicates = new();

    private readonly NetEvent0 _clearFullnessTrigger = new();
    private void OnClearFullnessTrigger() => fullnessTime = 0;  // Coincidentally public field for some reason lol

    private Monster? _monsterInMouth;
    private int _oldDamageToFarmer;
    private bool _oldFarmerPassesThrough;
    
    private static Vector2 MousePos => Game1.getMousePosition().ToVector2() + new Vector2(Game1.viewport.X, Game1.viewport.Y);

    public bool IsBlacklisted(Monster monster) => _blacklistPredicates.Any(p => p.IsBlacklisted(monster));
    public bool IsAllowed(Monster monster) => !IsBlacklisted(monster);
    
    public BetterFrogCompanion() => ModEntry.MonitorSingleton?.Log("Better frog constructed.");
    public BetterFrogCompanion(int variant) : base(variant) => ModEntry.MonitorSingleton?.Log("Better frog constructed.");
    
    #region OVERRIDES
    
    public override void Update(GameTime time, GameLocation location)
    {
        if (!IsLocal)
        {
            base.Update(time, location);
            _clearFullnessTrigger.Poll();
            return;
        }
        
        // Advance predicates timers
        _blacklistPredicates.RemoveAll(p => p.ShouldClearDueToTimer((float)time.ElapsedGameTime.TotalSeconds));
        
        // Will finish digesting the monster
        if (fullnessTime > 0 && fullnessTime <= (float)time.ElapsedGameTime.TotalMilliseconds)
            OnDigestionComplete(location);
        
        base.Update(time, location);
        
        // Ate a new monster
        //if (AttachedMonsterField.GetValue(this) is Monster attachedMonster && attachedMonster != _monsterInMouth)
        //    OnMonsterEaten(attachedMonster);
        // CHANGED TO A HARMONY PREFIX Because I need to cache the monster values before being swallowed.

        HandleInput(location);
    }

    public override void OnOwnerWarp()
    {
        base.OnOwnerWarp();
        _blacklistPredicates.RemoveAll(p => p.ShouldClearDueToWarp());
    }
    
    public override void CleanupCompanion()
    {
        base.CleanupCompanion();
        _monsterInMouth = null;
        _blacklistPredicates.Clear();
    }

    public override void InitNetFields()
    {
        base.InitNetFields();

        NetFields.AddField(_clearFullnessTrigger, nameof(_clearFullnessTrigger));
        _clearFullnessTrigger.onEvent += OnClearFullnessTrigger;
    }

    #endregion
    
    /// <summary>
    /// Called when a monster is brought to the frog via tongue and swallowed.
    /// </summary>
    public void OnMonsterEaten(Monster? monster)
    {
        if (!IsLocal) return;
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
        if (!IsLocal) return;
        if (_monsterInMouth == null) return;
        ModEntry.MonitorSingleton?.Log("Digested monster: " + _monsterInMouth.Name);
        
        //Register that the monster was killed
        if (ModEntry.ConfigSingleton.CountAsPlayerKill)
            LocationMonsterKilledMethod.Invoke(location, new object[]{Owner, _monsterInMouth, new Rectangle(Position.ToPoint(), new(40)), false});   

        _monsterInMouth = null;
    }
    
    /// <summary>
    /// Places the currently swallowed monster back into the location.
    /// </summary>
    private void SpitMonster(GameLocation location)
    {
        ModEntry.MonitorSingleton?.Log("Requested spit monster: " + _monsterInMouth?.Name);
        if (_monsterInMouth == null) return;
        
        _clearFullnessTrigger.Fire();

        Owner.currentLocation.playSound("fishSlap");
        
        //Add monster back to location
        location.addCharacter(_monsterInMouth);
        _monsterInMouth.Position = Position;
        
        //Send the monster away from the player
        var knockBack = Utility.getAwayFromPlayerTrajectory(new(Position.ToPoint(), new(1)), Owner);
        _monsterInMouth.Position += knockBack / 3f;
        _monsterInMouth.setTrajectory(knockBack / 3f);
        
        //Set a delay before eating the next monster
        MonsterEatCheckTimeField.SetValue(this, 0);

        //Revert stats to before swallow
        _monsterInMouth.stunTime.Value = 50;
        _monsterInMouth.position.Paused = false;
        _monsterInMouth.DamageToFarmer = _oldDamageToFarmer;
        _monsterInMouth.farmerPassesThrough = _oldFarmerPassesThrough;
        
        //Set up the blacklist
        _blacklistPredicates.Add(new(_monsterInMouth));
        
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
}