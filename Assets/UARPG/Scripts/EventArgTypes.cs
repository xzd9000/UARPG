using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region object event args
public class ObjectEventArgs<T> : EventArgs
{
    public readonly T obj;

    public ObjectEventArgs(T obj) => this.obj = obj;
}
public class ObjectEventArgs<T1, T2> : EventArgs
{
    public readonly T1 obj1;
    public readonly T2 obj2;

    public ObjectEventArgs(T1 obj1, T2 obj2)
    {
        this.obj1 = obj1;
        this.obj2 = obj2;
    }
}
public class ObjectEventArgs<T1, T2, T3> : EventArgs
{
    public readonly T1 obj1;
    public readonly T2 obj2;
    public readonly T3 obj3;

    public ObjectEventArgs(T1 obj1, T2 obj2, T3 obj3)
    {
        this.obj1 = obj1;
        this.obj2 = obj2;
        this.obj3 = obj3;
    }
}
#endregion

public class OldNewEventArgs<TO, TN> : EventArgs
{
    public readonly TO old;
    public readonly TN new_;

    public OldNewEventArgs(TO old, TN new_)
    {
        this.old = old;
        this.new_ = new_;
    }
}

public class SourceRecieverEventArgs : EventArgs
{
    public readonly Character source;
    public readonly Character reciever;

    public SourceRecieverEventArgs(Character s, Character r)
    {
        source = s;
        reciever = r;
    }
}

public enum AddRemove
{
    added,
    removed
}

public class AddRemoveEventArgs<T> : EventArgs
{

    public readonly T obj;
    public readonly AddRemove change;

    public AddRemoveEventArgs(T obj, AddRemove change)
    {
        this.obj = obj;
        this.change = change;
    }
}