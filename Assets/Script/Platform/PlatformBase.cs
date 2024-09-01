using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlatformBase
{
    public static PlatformBase Instance = null;

    private readonly float m_kDoubleClickAllowDistance = 10;
    private readonly float m_kDoubleClickAllowTime = 0.3f;
    private float m_doubleClickCheckTime;
    private Vector3 m_beforeTouchedPos = new Vector3();
    private Vector3 m_touchBeganPos = new Vector3();
    private Vector3 m_posForCameraMoving = new Vector3();

    protected bool CheckDoubleTap(in Vector3 kTouchedPos)
    {
        if( m_kDoubleClickAllowTime > Time.time - m_doubleClickCheckTime &&
            m_kDoubleClickAllowDistance > (kTouchedPos - m_beforeTouchedPos).magnitude )
        {
            RenewLastClickedTime(0);

            return true;
        }
        
        RenewLastClickedTime();

        return false;
    }

    protected void ScreenDrag(in Vector3 kNewPos)
    {
    }

    protected void RenewLastClickedTime(float newTime = -1)
    {
        if( newTime == -1 )
            m_doubleClickCheckTime = Time.time;
        else
            m_doubleClickCheckTime = newTime;
    }

    protected void RenewLastClickedPos(in Vector3 kNewPos)
    {
        m_beforeTouchedPos = kNewPos;
    }

    protected void OnTouchBegan(in Vector3 kTouchedPos)
    {
        m_touchBeganPos = m_posForCameraMoving = kTouchedPos;
    }

    protected void OnTouchMoved(in Vector3 kTouchedPos)
    {
        ScreenDrag(kTouchedPos);
    }

    protected void OnTouchEnded(in Vector3 kTouchedPos)
    {
        ResetClickInfo(kTouchedPos);
    }

    private void ResetClickInfo(in Vector3 kTouchedPos)
    {
        RenewLastClickedPos(kTouchedPos);
        RenewLastClickedTime();
    }

    public virtual void CheckOnTouches(){ throw new NotImplementedException(); }
}