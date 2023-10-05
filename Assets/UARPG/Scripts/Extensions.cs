using UnityEngine;
using System;
using System.Collections.Generic;

public enum Operation
{
    add,
    subtract,
    divide,
    multiply,
    power,
}

public static class Extensions
{
    public static float CommitOperation(this float float_, float operand, Operation operation)
    {
        switch (operation)
        {
            case Operation.add: return float_ + operand;
            case Operation.subtract: return float_ - operand;
            case Operation.divide: return float_ / operand;
            case Operation.multiply: return float_ * operand;
            case Operation.power: return Mathf.Pow(float_, operand);
            default: return float_;
        }
    }

    public static bool Override(this bool bool_,BoolOverride override_)

    {
        switch (override_)
        {
            default: return bool_;
            case BoolOverride.toFalse: return false;
            case BoolOverride.toTrue: return true;
        }
    }
    public static GameObject FindInChildren(this GameObject obj, string name)
    {
        GameObject ret = null;
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            if (obj.transform.GetChild(i).gameObject.name == name) ret = obj.transform.GetChild(i).gameObject;
            else ret = obj.transform.GetChild(i).gameObject.FindInChildren(name);
            if (ret != null) return ret;
        }
        return null;
    }

    public static T[] Copy<T>(this T[] array)
    {
        if (array != null)
        {
            T[] ret = new T[array.Length];
            for (int i = 0; i < array.Length; i++) ret.SetValue((T)array.GetValue(i), i);            
            return ret;
        }
        return null;
    }
    public static bool Contains<T>(this T[] array, T item)
    {
        if (array != null)
        {
            for (int i = 0; i < array.Length; i++) if ((array[i] as IComparable).Equals(item)) return true;           
            return false;
        }
        else return false;
    }
    public static T Find<T>(this T[] array, Predicate<T> match) => Array.Find(array, match);
    public static int FindIndex<T>(this T[] array, Predicate<T> match) => Array.FindIndex(array, match);

    public static List<T> Copy<T>(this List<T> list)
    {
        List<T> ret = new List<T>();
        ret.AddRange(list);
        return ret;
    }

    public static void RotateToTarget(this Transform transform, Vector3 vector, float rotationSpeed, bool direction = false, bool lockX = true, bool lockZ = true)
    {
        Vector3 forward;
        if (!direction) forward = transform.DirectionToTarget(vector, !lockX);
        else forward = vector;
        Quaternion lookAt = Quaternion.LookRotation(forward, transform.up);
        Vector3 angles = Quaternion.Lerp(transform.rotation, lookAt, rotationSpeed).eulerAngles;
        if (lockX) angles.x = 0;
        if (lockZ) angles.z = 0;
        transform.eulerAngles = angles;
    }
    public static float AngleToTarget(this Transform transform, Vector3 vector, bool includeY, bool direction = false)
    {
        Vector3 mnp2;
        if (direction) mnp2 = vector;
        else mnp2 = transform.DirectionToTarget(vector, includeY);
        float m1 = transform.forward.x; float m2 = mnp2.x;
        float n1 = transform.forward.y; float n2 = mnp2.y;
        float p1 = transform.forward.z; float p2 = mnp2.z;

        float cos = (m1 * m2 + n1 * n2 + p1 * p2) / (Mathf.Sqrt(m1 * m1 + n1 * n1 + p1 * p1) * Mathf.Sqrt(m2 * m2 + n2 * n2 + p2 * p2));
    
        return Mathf.Acos(cos) * Mathf.Rad2Deg;
    }
    public static Vector3 DirectionToTarget(this Transform transform, Vector3 position, bool includeY)
    {
        return new Vector3
        (
            position.x - transform.position.x,
           (position.y - transform.position.y) * (includeY ? 1f : 0f),
            position.z - transform.position.z
        ).normalized;        
    }

    public static Vector3 CutLow(this Vector3 vector, Vector3 cut)
    {
        Vector3 ret = vector;
        if (ret.x < cut.x) ret.x = cut.x;
        if (ret.y < cut.y) ret.y = cut.y;
        if (ret.z < cut.z) ret.z = cut.z;
        return ret;
    }
    public static Vector3 CutHigh(this Vector3 vector, Vector3 cut)
    {
        Vector3 ret = vector;
        if (ret.x > cut.x) ret.x = cut.x;
        if (ret.y > cut.y) ret.y = cut.y;
        if (ret.z > cut.z) ret.z = cut.z;
        return ret;
    }
    public static Vector3 CutLow(this Vector3 vector, float cut)
    {
        Vector3 ret = vector;
        if (ret.x < cut) ret.x = cut;
        if (ret.y < cut) ret.y = cut;
        if (ret.z < cut) ret.z = cut;
        return ret;
    }
    public static Vector3 CutHigh(this Vector3 vector, float cut)
    {
        Vector3 ret = vector;
        if (ret.x > cut) ret.x = cut;
        if (ret.y > cut) ret.y = cut;
        if (ret.z > cut) ret.z = cut;
        return ret;
    }
    public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
    {
        Vector3 ret = vector;
        ret.x = Mathf.Clamp(ret.x, min.x, max.x);
        ret.y = Mathf.Clamp(ret.y, min.y, max.y);
        ret.z = Mathf.Clamp(ret.z, min.z, max.z);
        return ret;
    }
    public static Vector3 Clamp(this Vector3 vector, float min, float max)
    {
        Vector3 ret = vector;
        ret.x = Mathf.Clamp(ret.x, min, max);
        ret.y = Mathf.Clamp(ret.y, min, max);
        ret.z = Mathf.Clamp(ret.z, min, max);
        return ret;
    }
    public static bool AreAllWithin(this Vector3 vector, Vector3 min, Vector3 max)
    {
        return
            (
                vector.x >= min.x && vector.x <= max.x &&
                vector.y >= min.y && vector.y <= max.y &&
                vector.z >= min.z && vector.z <= max.z
            );
    }
    public static bool AreAllWithin(this Vector3 vector, float min, float max)
    {
        return
            (
                vector.x >= min && vector.x <= max &&
                vector.y >= min && vector.y <= max &&
                vector.z >= min && vector.z <= max
            );
    }

    public static string ToHexString(this Color color)
    {
        const int alphabetStart = (int)'a';

        string ret = "";
        int next, remain;
        for (int i = 0; i < 4; i++)
        {
            next = (int)(255f * color[i]);
            remain = next % 16;
            next /= 16;
            next %= 16;

            add(next);
            add(remain);
        }

        return ret;

        void add(int num)
        {
            if (num < 10) ret += num.ToString();
            else ret += (char)(num - 10 + alphabetStart);
        }
    }
}