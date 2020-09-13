using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum HumanoidState
{
    Idle,
    Selected,
    Moving,
    Targetting,
    Attacking,
    Defending,
    Done
}

#pragma warning disable CS0649
public class Humanoid : MonoBehaviour, IMove, IStatistics
{
    /// <summary> The range of the normal attack. </summary>
    public int AttackRange;

    /// <summary> The max health of this unit. </summary>
    private int _maxHealth;

    /// <summary> Health of the unit. </summary>
    [HideInInspector] public int Health { get; set; }

    /// <summary>Attack of the unit. </summary>
    [HideInInspector] public int BaseAttack { get; set; }

    /// <summary>Defense of the unit.</summary>
    [HideInInspector] public int BaseDefense { get; set; }

    /// <summary>Movement value of the unit. </summary>
    [HideInInspector] public int Movement { get; set; }

    /// <summary>Dexterity (or dodge chance) of the unit.</summary>
    [HideInInspector] public float Dexterity { get; set; }

    /// <summary> Tile the unit currently occupies </summary>
    [HideInInspector] public Tile currentTile;

    ///time it takes the player to move across a single tile
    public float tileCrossTime = 0.3f;
    bool moving = false;

    public bool HasMoved { get; set; }
    public bool HasAttacked { get; set; }

    /// <summary>The base stats of the unit.</summary>
    [SerializeField] private CharacterStats _baseStats;

    public Text healthText;
    public Text damageText;

    public Slider healthBar;

    /// <summary>
    /// The state of the humanoid in combat.
    /// </summary>
    public HumanoidState State { get; set; }
    
    public virtual void Start()
    {
        Health = _baseStats.Health;
        BaseAttack = _baseStats.BaseAttack;
        BaseDefense = _baseStats.BaseDefense;
        Movement = _baseStats.Movement;
        Dexterity = _baseStats.Dexterity;
        _maxHealth = Health;

        if (healthText == null) { healthText = GetComponentInChildren<Text>(); }
        if (healthBar == null) { healthBar = GetComponentInChildren<Slider>(); }
        if(healthText)
        healthText.text = Health + "/" + _maxHealth;

        if(healthBar)
        healthBar.value = 1f;

        currentTile = MapGrid.Instance.TileFromPosition(transform.position);
        currentTile.occupied = true;

        State = HumanoidState.Idle;
        currentTile.occupant = this;

        HasMoved = false;
        HasAttacked = false;
    }

    /// <summary>
    /// Begins the movement coroutine for moving on map.
    /// </summary>
    /// <param name="path">The path the unit will take.</param>
    public virtual void Move(List<Tile> path)
    {
        if (path != null)
        {
            CombatSystem.Instance.SetBattleState(BattleState.PerformingAction);
            State = HumanoidState.Moving;
            StartCoroutine(MoveCR(path));
        }
    }

    IEnumerator MoveCR(List<Tile> path)
    {
        Vector3 p0;
        Vector3 p1;
        Vector3 p01;
        float timeStart;
        foreach (Tile tile in path)
        {


            timeStart = Time.time;
            moving = true;

            //get the position of the tile the unit is starting on
            p0 = currentTile.transform.position;


            //get the positon of the tile to move to
            p1 = tile.transform.position;

            // set the y position to be that of the moving unit
            p0 = new Vector3(p0.x, transform.position.y, p0.z);
            p1 = new Vector3(p1.x, transform.position.y, p1.z);

            //mark the starting tile as no longer occupied
            currentTile.occupied = false;
            currentTile.occupant = null;
            //change the current tile to the tile being moved to
            currentTile = tile;
            //mark it as occupied
            currentTile.occupied = true;
            currentTile.occupant = this;
            //interpolate between the two points
            while (moving)
            {
                float u = (Time.time - timeStart) / tileCrossTime;
                if (u >= 1)
                {
                    u = 1;
                    moving = false;
                }

                p01 = (1 - u) * p0 + u * p1;
                transform.position = p01;
                yield return new WaitForFixedUpdate();
            }
        }

        State = HumanoidState.Idle;
        HasMoved = true;
    }

    /**
     * <summary>Deals damage to unit.</summary>
     * 
     * <param name="damage">Damage unit will take.</param>
     * 
     * <returns>True if unit is dead, false otherwise.</returns>
     */
    public bool TakeDamage(int damage)
    {
        Health -= damage;
        damageText.text = damage.ToString();
        healthText.text = Health + "/" + _maxHealth;
        //Update the image fill
        

        healthBar.value = (float)Health / (float) _maxHealth;

        return Health <= 0 ? true : false;
    }

    /// <summary>
    /// Sets the unit's HasAttacked variable to true.
    /// </summary>
    protected void AttackComplete() { HasAttacked = true; }

    public void SetHumanoidState(HumanoidState state) { State = state; }


}
