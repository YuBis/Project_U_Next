using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor.U2D.Animation;
using UnityEngine;

public class GameCharacterModel : IDynamicStatManageable, ISkillManageable
{
    public ReactiveProperty<int> Level { get; private set; } = new();
    public ReactiveProperty<double> Exp { get; private set; } = new();
    public ReactiveProperty<double> MaxHP { get; private set; } = new();
    public ReactiveProperty<double> CurrentHP { get; private set; } = new();
    public ReactiveProperty<double> MaxMP { get; private set; } = new();
    public ReactiveProperty<double> CurrentMP { get; private set; } = new();
    public double MoveSpeed { get; private set; }
    public double JumpRange { get; private set; }
    public bool IsJumping { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsAttacking { get; private set; }
    public float JumpCheckDelay { get; private set; }
    public Dictionary<string, SkillInstanceData> Skills { get; private set; } = new();
    public StatData StatData { get; private set; }

    public string TemplateKey { get; private set; }
    public TeamType Team { get; private set; }

    public CharacterTemplateData CharacterData { get; private set; }
    public StatSheetData StatSheet { get; private set; }
    public CharacterState State { get; private set; }
    public double Delay { get; private set; }
    public Vector2 Position { get; private set; }
    public AIStateType AIState { get; private set; }

    public GameCharacterPresenter Presenter { get; private set; }

    public GameCharacterModel(string templateKey, TeamType teamType)
    {
        TemplateKey = templateKey;
        Team = teamType;

        _LoadInitialStats();

        IsJumping = false;
        IsGrounded = true;
    }

    void _LoadInitialStats()
    {
        if (!TableManager.Instance.CharacterTable.TryGetValue(TemplateKey, out var charData))
        {
            Universe.LogError($"{TemplateKey} : Character template not found!");
            return;
        }
        CharacterData = charData;

        if(!TableManager.Instance.StatSheetTable.TryGetValue(CharacterData.STAT_SHEET, out var statSheet))
        {
            Universe.LogError($"{TemplateKey} : Character stat not found!");
            return;
        }

        Skills = SkillInstanceManager.Instance.MakeDefaultSkills(CharacterData);
        StatSheet = statSheet;

        Level.Value = 1;
        Exp.Value = 1;

        StatData = StatManager.Instance.GetCurrentStat(in statSheet, Level.Value);

        MaxHP.Value = StatData.GetStat(StatType.HP);
        CurrentHP.Value = MaxHP.Value;

        MaxMP.Value = StatData.GetStat(StatType.MP);
        CurrentMP.Value = MaxMP.Value;

        JumpRange = StatData.GetStat(StatType.JUMP_RANGE);
        MoveSpeed = StatData.GetStat(StatType.MOVE_SPEED);
    }

    public void DecreaseHP(double amount)
    {
        CurrentHP.Value -= Math.Min(CurrentHP.Value, amount);
    }

    public void DecreaseMP(double amount)
    {
        CurrentMP.Value -= Math.Min(CurrentMP.Value, amount);
    }

    public void AddDelay(double delay)
    {
        Delay += delay;
    }

    public SkillInstanceData GetSkill(string skillKey) =>
        Skills.TryGetValue(skillKey, out var skill) ? skill : null;

    public SkillInstanceData GetSkill(SkillType skillType) =>
        Skills.Values.FirstOrDefault(x => x.SKILL_TEMPLATE.TYPE == skillType);

    public void SetState(CharacterState state) => State = state;
    public void SetPosition(Vector2 pos) => Position = pos;
    public void SetAIState(AIStateType state) => AIState = state;
    public void SetAttackStatus(bool status) => IsAttacking = status;
    public void SetJumping(bool jumping)
    {
        IsJumping = jumping;
        IsGrounded = !jumping;
    }

    public string GetAnimationName()
    {
        return AIState switch
        {
            AIStateType.IDLE => "idle_1",
            AIStateType.MOVE or AIStateType.PATROL or AIStateType.TARGETING => "walk",
            AIStateType.ATTACK => "sword_attack",
            AIStateType.DIE => "dead",
            _ => "walk"
        };
    }

    public string GetSubAnimationName()
    {
        return AIState switch
        {
            AIStateType.IDLE => "idle",
            AIStateType.MOVE or AIStateType.PATROL or AIStateType.TARGETING => "walk",
            AIStateType.ATTACK => "attack",
            AIStateType.DIE => "dead",
            _ => "walk"
        };
    }
}