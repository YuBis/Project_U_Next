using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class CombatManager : BaseManager<CombatManager>
{
    protected override void _InitManager()
    {
    }

    public void ProcessApplySkill(GameCharacterPresenter thrower, GameCharacterPresenter target, SkillInstanceData skillInstance)
    {
        if (thrower == null || target == null || skillInstance == null)
            return;

        if (thrower.GetState() == CharacterState.DIE ||
            target.GetState() == CharacterState.DIE)
            return;

        var hitCount = skillInstance.GetStatValue(StatType.HIT_COUNT);
        if (hitCount == 0)
            hitCount = 1;

        var atk = thrower.GetStat(StatType.DAMAGE);
        var skillDamagePer = skillInstance.GetStatValue(StatType.DAMAGE_PER);

        var damage = atk * skillDamagePer * 0.01;

        for (int i = 0; i < hitCount; i++)
        {
            target.DecreaseHP(damage);

            //Universe.LogDebug(target.name + " Received " + damage + " Damage! Left HP : " + target.CURRENT_HP);

            if (target.GetCurrentHP() > 0)
                target.GetAI().AddNextAI(AIStateType.HIT);
            else
            {
                target.GetAI().AddNextAI(AIStateType.DIE);
            }
        }

        var knockBackRange = skillInstance.GetStatValue(StatType.KNOCK_BACK);
        if (knockBackRange != 0)
            target.Knockback(thrower, (float)knockBackRange);
    }
}