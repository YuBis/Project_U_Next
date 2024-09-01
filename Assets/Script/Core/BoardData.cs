using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoardData : BaseObject
{
    [SerializeField]
    BaseBoard[] m_listBoard = null;

    public BaseBoard GetBoard(BoardType boardType) => m_listBoard.FirstOrDefault(b => b.BOARD_TYPE == boardType);
}
