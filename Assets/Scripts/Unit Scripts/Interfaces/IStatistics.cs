public interface IStatistics
{
    /// <summary>Health of the unit.</summary>
    int Health { get; set; }

    /// <summary>Attack stat of the unit.</summary>
    int AttackStat { get; set; }

    /// <summary>Defense stat of the unit.</summary>
    int DefenseStat { get; set; }

    /// <summary>Movement range of the unit.</summary>
    int MovementStat { get; set; }

    /// <summary>Dexterity value (or dodge chance) of the unit.</summary>
    float DexterityStat { get; set; }
}
