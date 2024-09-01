using System.Collections.Generic;
using UnityEngine;

public class SkillInstanceData : BaseInstanceData
{
    int m_level = 0;
    double m_lastSkillUseTime = 0;
    SkillTemplateData m_skillTemplateData = null;
    StatSheetData m_statSheetData = null;

    public int LEVEL => m_level;
    public SkillTemplateData SKILL_TEMPLATE => m_skillTemplateData;

    public SkillInstanceData() { }
    public SkillInstanceData(SkillTemplateData skillTemplateData)
    {
        m_skillTemplateData = skillTemplateData;
    }

    public void Initialize()
    {
        KEY = m_skillTemplateData.KEY;
        m_level = 1;
        TableManager.Instance.StatSheetTable.TryGetValue(m_skillTemplateData.STAT_SHEET, out m_statSheetData);
    }

    public double GetStatValue(StatType statType)
    {
        if (m_statSheetData == null)
            return 0;

        if (!m_statSheetData.DIC_INFO.TryGetValue(statType, out var targetStat))
            return 0;

        return targetStat.VALUE + (targetStat.VALUE_PER_LV * (m_level - 1));
    }
}