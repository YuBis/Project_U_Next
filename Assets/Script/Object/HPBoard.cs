using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBoard : BaseBoard
{
    [SerializeField]
    Image m_fillImage = null;

    [SerializeField]
    TMPro.TextMeshProUGUI m_hpLabel = null;

    public override BoardType BOARD_TYPE => BoardType.HP;

    public override void OnRelease()
    {
        
    }

    public override void Refresh(double curValue, double maxValue)
    {
        if (OWNER == null)
            return;

        m_fillImage.fillAmount = (float)(curValue / maxValue);
        m_hpLabel.text = $"{curValue} / {maxValue}";
    }
}
