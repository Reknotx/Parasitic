using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#pragma warning disable CS0649
public class Humanoid : MonoBehaviour, IMove, IStatistics
{
    public int AttackRange;
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

    /// <summary>The base stats of the unit.</summary>
    [SerializeField] private CharacterStats _baseStats;

    

    public Text healthText;
    public Text damageText;

    public Color selectedColor = Color.green;
    public Color unselectedColor = Color.white;

    public Slider healthBar;
    
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
        currentTile.occupant = this;
    }

    public virtual void Move(Transform start, Transform target)
    {
        throw new System.NotImplementedException();
    }

    public void BeginMovement(List<Tile> path)
    {
        if (path != null)
        {
            StartCoroutine(Move(path));
        }
    }

    IEnumerator Move(List<Tile> path)
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
}
