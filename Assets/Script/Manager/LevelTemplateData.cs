using System;
using System.Collections.Generic;
using UnityEngine;

[JsonName(StaticString.LEVEL_TABLE)]
public class LevelTemplateData : BaseData
{
    public int LEVEL { get; set; }
    public long EXP { get; set; }

    public override string KEY => LEVEL.ToString();
}