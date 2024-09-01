using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

[JsonName(StaticString.CHARACTER_TABLE)]
public class CharacterTemplateData : BaseData
{
    public string PREFAB { get; set; }
    public string STAT_SHEET { get; set; }
    public List<string> SKILL_LIST { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public AIType AI_TYPE { get; set; }

    public string NAME { get; set; }
    public bool BODY_CHECK { get; set; }
}