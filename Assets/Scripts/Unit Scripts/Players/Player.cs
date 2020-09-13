using System.Collections.Generic;
using UnityEngine;

#pragma warning disable CS0414
public abstract class Player : Humanoid, IPlayer
{
    bool selected = false;
    Material defaultMat;
    public Material selectedMat;


    public int Ability1Range;
    public int Ability2Range;

    public abstract void AbilityOne(List<Humanoid> targets);
    public abstract void AbilityTwo(List<Humanoid> targets);
    public abstract void NormalAttack(Humanoid target);

    public override void Start()
    {
        defaultMat = GetComponent<MeshRenderer>().material;
        if (selectedMat == null) selectedMat = Resources.Load<Material>("SelectedMat");

        base.Start();
    }

    public void UnitSelected()
    {
        print("Player selected");
        GetComponent<MeshRenderer>().material = selectedMat;
        State = HumanoidState.Selected;

        CombatSystem.Instance.ActivateCombatButtons();
        CombatSystem.Instance.SetPlayer(this);
        selected = true;
    }

    public void UnitDeselected()
    {
        print("Player deselected");
        GetComponent<MeshRenderer>().material = defaultMat;
        State = HumanoidState.Idle;
        CombatSystem.Instance.SetPlayer(null);
        CombatSystem.Instance.DeactivateCombatButtons();
        selected = false;
    }

    /// <summary>
    /// Raises the defense stat of the player temporarily.
    /// </summary>
    public void Defend()
    {

    }

    public void Pass()
    {
        HasAttacked = true;
        HasMoved = true;

        CharacterSelector.Instance.SelectedPlayerUnit = null;
        State = HumanoidState.Done;
    }

    //public void OnMouseOver()
    //{
    //    gameObject.GetComponent<MeshRenderer>().material.color = Color.blue;
    //}

    //public void OnMouseExit()
    //{
    //    gameObject.GetComponent<MeshRenderer>().material.color = Color.white;
    //}
}
