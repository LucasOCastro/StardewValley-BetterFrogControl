using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Companions;
using StardewValley.Monsters;
using StardewValley.Network;

namespace StardewBetterFrog;

public class BetterFrogCompanion : HungryFrogCompanion
{
    public override void Update(GameTime time, GameLocation location)
    {
        if (!tongueOut.Value)
            base.Update(time, location);
        if (!Game1.shouldTimePass())
            return;

        //Advance timers
        if (fullnessTime > 0.0)
            fullnessTime -= (float)time.ElapsedGameTime.TotalMilliseconds;
        lastHopTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
        if (initialEquipDelay > 0.0)
        {
            initialEquipDelay -= (float)time.ElapsedGameTime.TotalMilliseconds;
            return; // in delay
        }

        //If not local, simply move the attached monster to follow
        if (!IsLocal)
        {
            if (tongueOut.Value && attachedMonster != null)
            {
                attachedMonster.Position = tonguePosition.Value;
                attachedMonster.position.Paused = true;
                attachedMonster.xVelocity = 0.0f;
                attachedMonster.yVelocity = 0.0f;
            }

            fullnessTrigger.Poll();
            return;
        }

        monsterEatCheckTimer += (float)time.ElapsedGameTime.TotalMilliseconds;
        if (monsterEatCheckTimer >= 2000.0 && fullnessTime <= 0.0 && !tongueOut.Value)
        {
            monsterEatCheckTimer = 0.0f;
            if (location is not SlimeHutch)
            {
                var monsterWithinRange = Utility.findClosestMonsterWithinRange(location, Position, 300);
                if (monsterWithinRange != null)
                {
                    if (monsterWithinRange is Bat && monsterWithinRange.Age == 789)
                    {
                        monsterEatCheckTimer = 0.0f;
                        return;
                    }

                    if (monsterWithinRange.Name.Equals("Truffle Crab"))
                    {
                        monsterEatCheckTimer = 0.0f;
                        return;
                    }

                    if (monsterWithinRange is GreenSlime greenSlime && greenSlime.prismatic.Value)
                    {
                        monsterEatCheckTimer = 0.0f;
                        return;
                    }

                    height = 0.0f;
                    Vector2 velocityTowardPoint =
                        Utility.getVelocityTowardPoint(Position, monsterWithinRange.getStandingPosition(), 12f);
                    tongueOut.Value = true;
                    tongueReturn.Value = false;
                    tonguePosition.Value = Position + new Vector2(-32f, -32f) +
                                           new Vector2(direction.Value != 3 ? 28f : 0.0f, -20f);
                    tongueVelocity.Value = velocityTowardPoint;
                    location.playSound("croak");
                    direction.Value = monsterWithinRange.Position.X < (double)Position.X ? 3 : 1;
                }
            }

            tongueOutTimer = 0.0f;
        }

        if (tongueOut.Value)
        {
            tongueOutTimer +=
                (float)(time.ElapsedGameTime.TotalMilliseconds * (tongueReturn ? -1.0 : 1.0));
            NetPosition tonguePosition = this.tonguePosition;
            tonguePosition.Value += tongueVelocity.Value;
            if (attachedMonster == null)
            {
                if ((double)Vector2.Distance(Position, tonguePosition.Value) >= 300.0)
                {
                    tongueReachedMonster(null);
                }
                else
                {
                    int num = 40;
                    if (Owner.currentLocation.doesPositionCollideWithCharacter(new Rectangle(
                            (int)tonguePosition.X + 32 - num / 2, (int)tonguePosition.Y + 32 - num / 2, num,
                            num)) is Monster m)
                        tongueReachedMonster(m);
                }
            }

            if (attachedMonster != null)
            {
                attachedMonster.Position = tonguePosition.Value;
                attachedMonster.xVelocity = 0.0f;
                attachedMonster.yVelocity = 0.0f;
            }

            if (tongueReturn.Value)
            {
                Vector2 vector2 =
                    Vector2.Subtract(
                        Position + new Vector2(-32f, -32f) + new Vector2(direction.Value != 3 ? 28f : 0.0f, -20f),
                        tonguePosition.Value);
                vector2.Normalize();
                tongueVelocity.Value = vector2 * 12f;
            }

            if (tongueReturn.Value && (double)Vector2.Distance(Position, tonguePosition.Value) <= 48.0 ||
                (double)tongueOutTimer <= 0.0)
            {
                if (attachedMonster != null)
                {
                    if (this.attachedMonster is HotHead attachedMonster &&
                        attachedMonster.timeUntilExplode.Value > 0.0)
                        attachedMonster.currentLocation?.netAudio.StopPlaying("fuse");
                    if (this.attachedMonster.currentLocation != null)
                        this.attachedMonster.currentLocation.characters.Remove(this.attachedMonster);
                    else
                        location.characters.Remove(this.attachedMonster);
                    fullnessTrigger.Fire();
                    this.attachedMonster = null;
                }

                double num = Vector2.Distance(Position, tonguePosition.Value);
                tongueOut.Value = false;
                tongueReturn.Value = false;
            }
        }
    }
}