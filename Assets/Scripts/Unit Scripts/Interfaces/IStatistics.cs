public interface IStatistics
{
    /// <summary>Health of the unit.</summary>
    int Health { get; set; }

    /// <summary>Attack stat of the unit.</summary>
    int BaseAttack { get; set; }

    /// <summary>Defense stat of the unit.</summary>
    int BaseDefense { get; set; }

    /// <summary>Movement range of the unit.</summary>
    int Movement { get; set; }

    /// <summary>Dexterity value (or dodge chance) of the unit.</summary>
    float Dexterity { get; set; }
}
