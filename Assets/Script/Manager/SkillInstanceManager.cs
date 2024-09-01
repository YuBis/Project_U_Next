using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;
using System.Linq;

public class SkillInstanceManager : BaseInstanceDataManager<SkillInstanceManager, SkillInstanceData>
{
    public SkillInstanceData CreateInstanceData(SkillTemplateData skillTemplateData)
    {
        SkillInstanceData skillInstanceData = new SkillInstanceData(skillTemplateData);
        skillInstanceData.Initialize();

        return skillInstanceData;
    }

    public Dictionary<string, SkillInstanceData> MakeDefaultSkills(CharacterTemplateData characterData)
    {
        Dictionary<string, SkillInstanceData> dicSkills = new();

        if (characterData == null)
            return dicSkills;

        SkillTemplateData skillData = null;

        foreach(var skillKey in characterData.SKILL_LIST)
        {
            if (dicSkills.ContainsKey(skillKey))
                continue;

            if (!TableManager.Instance.SkillTable.TryGetValue(skillKey, out skillData))
                continue;

            if (skillData.TYPE != SkillType.ATTACK_NORMAL && skillData.TYPE != SkillType.BODY_CHECK)
                continue;

            dicSkills[skillKey] = CreateInstanceData(skillData);
        }

        return dicSkills;
    }

    public List<SkillInstanceData> GetSkillListByType(SkillType skillType) =>
        m_dicData.Values.Where(x => x.SKILL_TEMPLATE.TYPE == skillType).ToList();
}