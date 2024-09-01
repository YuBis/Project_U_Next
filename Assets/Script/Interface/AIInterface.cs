using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public interface IAIState
{
    void EnterState(BaseAI ai);
    UniTask UpdateState(BaseAI ai);
    void ExitState(BaseAI ai);
}