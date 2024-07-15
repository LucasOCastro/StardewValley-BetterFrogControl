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
    private static bool CanCurrentlyEat(GameLocation location)
    {
        return location is not SlimeHutch;
    }
    
    private const int TongueCenterOffset = 32;
    private const int TongueCollisionSize = 40;
    private const float Speed = 12f;
    private const int TongueCollisionOffset = TongueCenterOffset + TongueCollisionSize / 2;

    private Rectangle TongueCollisionRectangle => new(
        (int)tonguePosition.X + TongueCollisionOffset,
        (int)tonguePosition.Y + TongueCollisionOffset,
        TongueCollisionSize, TongueCollisionSize);

    private Vector2 TongueOrigin => Position
                                    - new Vector2(TongueCenterOffset, TongueCenterOffset)
                                    + new Vector2(direction.Value == 3 ? 0.0f : 28f, -20f);

    private Monster? GetTargetMonster(GameLocation location)
    {
        var monsterWithinRange = Utility.findClosestMonsterWithinRange(location, Position, RANGE);
        switch (monsterWithinRange)
        {
            case null:
                return null;
            case Bat when monsterWithinRange.Age == 789: // Mayor basement bat, 789 = shorts id
                return null;
            case GreenSlime {prismatic.Value: true }:
                return null;
            default:
                if (monsterWithinRange.Name.Equals("Truffle Crab"))
                    return null;
                break;
        }

        return monsterWithinRange;
    }

    private void SnapAttachedMonsterToTongue()
    {
        attachedMonster.Position = tonguePosition.Value;
        attachedMonster.xVelocity = 0.0f;
        attachedMonster.yVelocity = 0.0f;
    }
    
    public override void Update(GameTime time, GameLocation location)
    {
        if (!tongueOut.Value)
            base.Update(time, location);
        if (!Game1.shouldTimePass())
            return;

        //Advance timers
        float deltaTime = (float)time.ElapsedGameTime.TotalMilliseconds;
        if (fullnessTime > 0.0)
            fullnessTime -= deltaTime;
        lastHopTimer += deltaTime;
        if (initialEquipDelay > 0.0)
        {
            initialEquipDelay -= deltaTime;
            return; // in delay
        }

        //If not local, simply move the attached monster to follow
        if (!IsLocal)
        {
            if (tongueOut.Value && attachedMonster != null)
            {
                attachedMonster.position.Paused = true;
                SnapAttachedMonsterToTongue();
            }
            fullnessTrigger.Poll();
            return;
        }

        monsterEatCheckTimer += deltaTime;
        bool wantsToEat = monsterEatCheckTimer >= 2000.0 && fullnessTime <= 0.0; 
        if (wantsToEat && !tongueOut.Value)
        {
            monsterEatCheckTimer = 0;
            if (CanCurrentlyEat(location))
            {
                var targetMonster = GetTargetMonster(location);
                if (targetMonster == null)
                {
                    //TODO in the base code, it only did this if the closes monster existed but wasn't valid, such as truffle crab.
                    //Should verify if it is ok to return even when there is no monster nearby, but I feel like it is.
                    monsterEatCheckTimer = 0;
                    return;
                }

                ShootTongue(location, targetMonster);
            }
            tongueOutTimer = 0;
        }

        if (tongueOut.Value)
        {
            UpdateTongue(deltaTime, location);
        }
    }

    private void ShootTongue(GameLocation location, Monster targetMonster)
    {
        height = 0;
        tongueOut.Value = true;
        tongueReturn.Value = false;
        tonguePosition.Value = TongueOrigin;
        tongueVelocity.Value = Utility.getVelocityTowardPoint(Position, targetMonster.getStandingPosition(), Speed);
        direction.Value = targetMonster.Position.X < Position.X ? 3 : 1;
        location.playSound("croak");
    }


    private void UpdateTongue(float deltaTime, GameLocation location)
    {
        tongueOutTimer += deltaTime * (tongueReturn ? -1 : 1);
        tonguePosition.Value += tongueVelocity.Value;
        
        if (attachedMonster == null)
        {
            if (Vector2.Distance(Position, tonguePosition.Value) >= RANGE)
                tongueReachedMonster(null);
            else if (Owner.currentLocation.doesPositionCollideWithCharacter(TongueCollisionRectangle) is Monster m)
                tongueReachedMonster(m);
        }
        
        if (attachedMonster != null)
        {
            SnapAttachedMonsterToTongue();
        }

        //Tongue is returning to frog
        if (tongueReturn)
        {
            Vector2 returnDirection = TongueOrigin - tonguePosition.Value;
            returnDirection.Normalize();
            tongueVelocity.Value = returnDirection * Speed;
        }

        //Tongue returned to frog 
        if (tongueReturn && Vector2.Distance(Position, tonguePosition.Value) <= 48.0 || tongueOutTimer <= 0.0)
        {
            if (attachedMonster != null)
            {
                EatMonster(location);
            }
            tongueOut.Value = false;
            tongueReturn.Value = false;
        }
    }

    private void EatMonster(GameLocation location)
    {
        if (attachedMonster is HotHead hotHead && hotHead.timeUntilExplode.Value > 0.0)
            hotHead.currentLocation?.netAudio.StopPlaying("fuse");
        if (attachedMonster.currentLocation != null)
            attachedMonster.currentLocation.characters.Remove(attachedMonster);
        else
            location.characters.Remove(attachedMonster);
        fullnessTrigger.Fire();
        attachedMonster = null;
    }
}