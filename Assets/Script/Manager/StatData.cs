using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using UnityEngine;

public class StatData
{
    Dictionary<StatType, double> m_dicStatInfo = new();

    int m_level = -1;

    public StatData(int level, in StatSheetData baseStatData)
    {
        m_level = level;
        
        if(baseStatData != null)
        {
            var statList = baseStatData.DIC_INFO;
            foreach(var statInfo in statList.Values)
            {
                m_dicStatInfo[statInfo.STAT_NAME] = statInfo.VALUE + (statInfo.VALUE_PER_LV * (m_level - 1));
            }
        }
    }

    public double GetStat(StatType statType)
    {
        if (m_dicStatInfo.TryGetValue(statType, out var value))
            return value;

        return 0;
    }

    public double GetAttackDamage(double attackDamage)
    {
        var damagePer = GetStat(StatType.DAMAGE_PER);
        return attackDamage * damagePer * 0.01;
    }
}