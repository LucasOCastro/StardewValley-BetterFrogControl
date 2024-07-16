using StardewValley.Monsters;

namespace StardewBetterFrog.FrogStuffs;

public class SwallowBlacklistPredicate
{
    private readonly Monster _source;
    private float _secondsSinceCreation;

    private readonly BlacklistType _blacklistType;
    private readonly BlacklistDuration _blacklistDuration;
    
    public SwallowBlacklistPredicate(Monster source)
    {
        _source = source;

        var config = ModEntry.ConfigSingleton;
        _blacklistType = config.BlacklistType;
        _blacklistDuration = config.BlacklistDuration;
    }

    public bool IsBlacklisted(Monster m) =>
        _blacklistType switch
        {
            BlacklistType.SameMonster => m == _source,
            BlacklistType.SameType => m.Name == _source.Name,
            BlacklistType.Everything => true,
            BlacklistType.None => false,
            _ => throw new ArgumentOutOfRangeException(nameof(_blacklistType), _blacklistType, null)
        };

    public bool ShouldClearDueToTimer(float deltaTimeSeconds)
    {
        _secondsSinceCreation += deltaTimeSeconds;
        return _blacklistDuration == BlacklistDuration.Time &&
               _secondsSinceCreation >= ModEntry.ConfigSingleton.BlacklistDurationSeconds;
    }

    public bool ShouldClearDueToWarp() => _blacklistDuration == BlacklistDuration.UntilLeave;
}