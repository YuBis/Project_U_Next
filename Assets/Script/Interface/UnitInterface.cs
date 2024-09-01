using System.Collections.Generic;
using UniRx;
using UnityEngine;

public interface IMovable
{
    void Move(float direction);
}

public interface IJumpable
{
    void Jump();
    void OnLand();
}

public interface IAttackable
{
    void Attack(GameCharacterPresenter target);
    void UseSkill(string skillKey, GameCharacterPresenter[] targets);
}

public interface IDynamicStatManageable
{
    void DecreaseHP(double amount);
    void DecreaseMP(double amount);
    ReactiveProperty<double> CurrentHP { get; }
    ReactiveProperty<double> CurrentMP { get; }
}

public interface ISkillManageable
{
    SkillInstanceData GetSkill(string skillKey);
}