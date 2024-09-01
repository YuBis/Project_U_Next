using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Security.Cryptography;
using UnityEngine;
using Newtonsoft.Json.Linq;

public enum StatType
{
    TARGET_COUNT,
    HIT_COUNT,
    DAMAGE_PER,

    HP,
    MP,
    DAMAGE,
    MOVE_SPEED,
    JUMP_RANGE,

    EXP,
    SIGHT,

    HP_COST,
    MP_COST,

    KNOCK_BACK,

    Count,
}

public class StatSheetContainer
{
    public List<StatSheetData> STAT_SHEET { get; set; }
}

public class StatSheetInfo
{
    [JsonConverter(typeof(StringEnumConverter))]
    public StatType STAT_NAME { get; set; }

    public double VALUE { get; set; }
    public double VALUE_PER_LV { get; set; }
}

[JsonName(StaticString.STAT_SHEET)]
public class StatSheetData : BaseData, IListToDicConverterModel, IListModel<StatSheetInfo>
{
    public List<StatSheetInfo> _UNUSED_LIST_ { get; set; } = new();
    public Dictionary<StatType, StatSheetInfo> DIC_INFO { get; set; } = new();

    public void ConvertListToDictionary()
    {
        DIC_INFO.Clear();
        foreach (var item in _UNUSED_LIST_)
        {
            DIC_INFO[item.STAT_NAME] = item;
        }

        _UNUSED_LIST_.Clear();
    }
}