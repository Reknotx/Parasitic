using UnityEngine;

public abstract class Player : Humanoid, IPlayer
{
    public abstract void AbilityOne(Humanoid target);
    public abstract void AbilityTwo(Humanoid target);
    public abstract void NormalAttack(Humanoid target);
    bool selected = false;
    Material defaultMat;
    public Material selectedMat;


    public override void Start()
    {
        defaultMat = GetComponent<MeshRenderer>().material;
        base.Start();
    }

    public void UnitSelected()
    {
        GetComponent<MeshRenderer>().material = selectedMat;
        selected = true;
    }

    public void UnitDeselected()
    {
        GetComponent<MeshRenderer>().material = defaultMat;
        selected = false;
    }

    public void Hello()
    {
        Debug.Log("Hello");
    }



    public override void Move(Transform start, Transform target)
    {
        // Player movement based on player input
        Debug.Log("Player movement");
    }

    public void Defend()
    {

    }

    public void Pass()
    {
        throw new System.NotImplementedException();
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
