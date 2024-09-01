using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

public enum SkillType
{
    SKILL_NORMAL,
    ATTACK_NORMAL,
    BODY_CHECK,

    BUFF,
}

[JsonName(StaticString.SKILL_DATA)]
public class SkillTemplateData : BaseData, IPostProcessAfterLoadModel
{
    public double RUNNING_TIME { get; set; }
    public string STAT_SHEET { get; set; }
    public string EFFECT { get; set; }
    public string EFFECT_HIT { get; set; }
    public string NAME { get; set; }
    public string DESC { get; set; }
    public string ICON { get; set; }
    public int MAX_LEVEL { get; set; }

    public int RANGE_LB_X;
    public int RANGE_LB_Y;
    public int RANGE_WIDTH;
    public int RANGE_HEIGHT;

    [JsonConverter(typeof(StringEnumConverter))]
    public SkillType TYPE { get; set; }

    public Rect RANGE { get; set; }


    public void PostProcessAfterLoad()
    {
        RANGE = new Rect(
            RANGE_LB_X,
            RANGE_LB_Y,
            RANGE_WIDTH,
            RANGE_HEIGHT
            );
    }
}