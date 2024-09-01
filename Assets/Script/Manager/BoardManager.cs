using Com.LuisPedroFonseca.ProCamera2D;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Cryptography;
using UnityEngine;

public enum BoardType
{
    NONE,
    HP,
    DAMAGE,
}

public class BoardManager : BaseManager<BoardManager>
{
    Dictionary<BoardType, List<BaseBoard>> m_dicBoard = new();
    Transform m_rootTransform = null;
    BoardData m_boardData = null;

    Camera m_uiCamera = null;

    protected override void _InitManager()
    {
        ObjectPoolManager.Instance.GetObject("Pf_Ui_BoardRoot", _CallbackInit);
    }

    public void _CallbackInit(GameObject go)
    {
        m_rootTransform = go.GetComponent<Transform>();
        GameObject.DontDestroyOnLoad(go);

        m_boardData = go.GetComponent<BoardData>();
        m_uiCamera = go.GetComponentInChildren<Camera>();
    }

    public void MakeBoard(GameCharacterPresenter owner, BoardType boardType, Action<BaseBoard> callBack = null)
    {
        if (m_boardData == null)
            return;

        var targetBoard = m_boardData.GetBoard(boardType);
        if (targetBoard == null)
            return;

        ObjectPoolManager.Instance.GetObject(targetBoard.name, (go) =>
        {
            var boardObj = go.GetComponent<BaseBoard>();
            if (boardObj == null)
                return;

            _CallbackMakeBoard(owner, boardObj, callBack);
        });
    }

    void _CallbackMakeBoard(GameCharacterPresenter owner, BaseBoard boardObj, Action<BaseBoard> finalCallback)
    {
        var boardType = boardObj.BOARD_TYPE;
        if (!m_dicBoard.TryGetValue(boardType, out var boardList))
            boardList = new();

        boardList.Add(boardObj);

        m_dicBoard[boardType] = boardList;

        if (m_rootTransform != null)
            boardObj.GetComponent<Transform>().SetParent(m_rootTransform, false);

        boardObj.OWNER = owner;

        finalCallback?.Invoke(boardObj);
    }

    public void ReleaseBoard(BoardType boardType, BaseBoard boardObj)
    {
        if (boardObj == null)
            return;

        var targetList = m_dicBoard[boardType];
        if (targetList == null)
            return;

        boardObj.OnRelease();

        targetList.Remove(boardObj);

        ObjectPoolManager.Instance.ReleaseObject(boardObj.GAMEOBJECT);
    }

    public Vector3 GetUIPosition(Vector3 boardPos)
    {
        var worldPos = GameMaster.Instance.MAIN_CAMERA.WorldToViewportPoint(boardPos);
        var uiPos = m_uiCamera.ViewportToWorldPoint(worldPos);

        return uiPos;
    }
}