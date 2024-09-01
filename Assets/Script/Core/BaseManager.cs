using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public abstract class BaseManager<T> : IModel
    where T : BaseManager<T>, new()
{
    private static readonly Lazy<T> instance = new Lazy<T>(() => new T());

    protected BaseManager()
    {
        _InitManager();
    }

    public static T Instance
    {
        get { return instance.Value; }
    }

    protected abstract void _InitManager();
}
