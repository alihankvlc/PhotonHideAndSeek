using System.Collections;
using UnityEngine;

public interface IInteractable
{
    void Interact();
}
public interface IDamageable
{
    public float Health { get; set; }
    public bool IsAlive { get; set; }
    void TakeDamage(float amount);
    void Death();

}
public interface IMoveable
{
    void Movement();
    void Jump();
    void Crouch();
}
public interface IShapeShifter
{
    void ChangeToShapeForm();
    void ChangeToHumanForm();
    void ChangeToGhostForm();
}
public interface IAttackable
{
    void Attack();
}
