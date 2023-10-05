using System;
using UnityEngine;

public enum PhysicalSlot
{
    none,
    head,
    base_,
    upperBody,
    lowerBody,
    arms,
    rightUpperArm,
    rightLowerArm,
    rightHand,
    leftUpperArm,
    leftLowerArm,
    leftHand,
    legs,
    rightUpperLeg,
    rightLowerLeg,
    rightFoot,
    leftUpperLeg,
    leftLowerLeg,
    leftFoot,
}


[Serializable] public struct Slots
{
    [SerializeField] GameObject Head;
    [SerializeField] GameObject UpperBody;
    [SerializeField] GameObject LowerBody;
    [SerializeField] GameObject Base_;
    [SerializeField] GameObject Arms;
    [SerializeField] GameObject RightUpperArm;
    [SerializeField] GameObject RightLowerArm;
    [SerializeField] GameObject RightHand;
    [SerializeField] GameObject LeftUpperArm;
    [SerializeField] GameObject LeftLowerArm;
    [SerializeField] GameObject LeftHand;
    [SerializeField] GameObject Legs;
    [SerializeField] GameObject RightUpperLeg;
    [SerializeField] GameObject RightLowerLeg;
    [SerializeField] GameObject RightFoot;
    [SerializeField] GameObject LeftUpperLeg;
    [SerializeField] GameObject LeftLowerLeg;
    [SerializeField] GameObject LeftFoot;

    public GameObject head            { get => Head;            private set => Head = value;            }
    public GameObject upperBody       { get => UpperBody;       private set => UpperBody = value;       }
    public GameObject lowerBody       { get => LowerBody;       private set => LowerBody = value;       }
    public GameObject base_           { get => Base_;           private set => Base_ = value;           }
    public GameObject arms            { get => Arms;            private set => Arms = value;            }
    public GameObject rightUpperArm   { get => RightUpperArm;   private set => RightUpperArm = value;   }
    public GameObject rightLowerArm   { get => RightLowerArm;   private set => RightLowerArm = value;   }
    public GameObject rightHand       { get => RightHand;       private set => RightHand = value;       }
    public GameObject leftUpperArm    { get => LeftUpperArm;    private set => LeftUpperArm = value;    }
    public GameObject leftLowerArm    { get => LeftLowerArm;    private set => LeftLowerArm = value;    }
    public GameObject leftHand        { get => LeftHand;        private set => LeftHand = value;        }
    public GameObject legs            { get => Legs;            private set => Legs = value;            }
    public GameObject rightUpperLeg   { get => RightUpperLeg;   private set => RightUpperLeg = value;   }
    public GameObject rightLowerLeg   { get => RightLowerLeg;   private set => RightLowerLeg = value;   }
    public GameObject rightFoot       { get => RightFoot;       private set => RightFoot = value;       }
    public GameObject leftUpperLeg    { get => LeftUpperLeg;    private set => LeftUpperLeg = value;    }
    public GameObject leftLowerLeg    { get => LeftLowerLeg;    private set => LeftLowerLeg = value;    }
    public GameObject leftFoot        { get => LeftFoot;        private set => LeftFoot = value;        }

    public int length => typeof(PhysicalSlot).GetEnumNames().Length;

    public GameObject this[PhysicalSlot slot]
    {
        get
        {
            switch (slot)
            {
                case PhysicalSlot.base_: return Base_;
                case PhysicalSlot.head: return Head;
                case PhysicalSlot.upperBody: return UpperBody;
                case PhysicalSlot.lowerBody: return LowerBody;
                case PhysicalSlot.arms: return Arms;
                case PhysicalSlot.rightUpperArm: return RightUpperArm;
                case PhysicalSlot.rightLowerArm: return RightLowerArm;
                case PhysicalSlot.rightHand: return RightHand;
                case PhysicalSlot.leftUpperArm: return LeftUpperArm;
                case PhysicalSlot.leftLowerArm: return LeftLowerArm;
                case PhysicalSlot.leftHand: return LeftHand;
                case PhysicalSlot.legs: return Legs;
                case PhysicalSlot.rightUpperLeg: return RightUpperLeg;
                case PhysicalSlot.rightLowerLeg: return RightLowerLeg;
                case PhysicalSlot.rightFoot: return RightFoot;
                case PhysicalSlot.leftUpperLeg: return LeftUpperLeg;
                case PhysicalSlot.leftLowerLeg: return LeftLowerLeg;
                case PhysicalSlot.leftFoot: return LeftFoot;
                case PhysicalSlot.none: return default;
                default: throw new NotImplementedException();
            }
        }
        set
        {
            switch (slot)
            {
                case PhysicalSlot.base_: Base_ = value; break;
                case PhysicalSlot.head: Head = value; break;
                case PhysicalSlot.upperBody: UpperBody = value; break;
                case PhysicalSlot.lowerBody: LowerBody = value; break;
                case PhysicalSlot.arms: Arms = value; break;
                case PhysicalSlot.rightUpperArm: RightUpperArm = value; break;
                case PhysicalSlot.rightLowerArm: RightLowerArm = value; break;
                case PhysicalSlot.rightHand: RightHand = value; break;
                case PhysicalSlot.leftUpperArm: LeftUpperArm = value; break;
                case PhysicalSlot.leftLowerArm: LeftLowerArm = value; break;
                case PhysicalSlot.leftHand: LeftHand = value; break;
                case PhysicalSlot.legs: Legs = value; break;
                case PhysicalSlot.rightUpperLeg: RightUpperLeg = value; break;
                case PhysicalSlot.rightLowerLeg: RightLowerLeg = value; break;
                case PhysicalSlot.rightFoot: RightFoot = value; break;
                case PhysicalSlot.leftUpperLeg: LeftUpperLeg = value; break;
                case PhysicalSlot.leftLowerLeg: LeftLowerLeg = value; break;
                case PhysicalSlot.leftFoot: LeftFoot = value; break;
                case PhysicalSlot.none: break;
                default: throw new NotImplementedException();
            }
        }
    }
    public GameObject this[int slot]
    {
        get
        {
            if (slot >= 0 && slot <= length) return this[(PhysicalSlot)slot];
            else throw new IndexOutOfRangeException();
        }
        set
        {
            if (slot >= 0 && slot <= length) this[(PhysicalSlot)slot] = value;
            else throw new IndexOutOfRangeException();
        }
    }
}
