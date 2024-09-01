using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using UnityEngine;

public class StatManager : BaseManager<StatManager>
{
    protected override void _InitManager()
    {
    }

    public StatData GetCurrentStat(in StatSheetData baseStatData, int level)
    {
        if (level == -1)
            return null;

        var retData = new StatData(level, baseStatData);

        return retData;
    }
}