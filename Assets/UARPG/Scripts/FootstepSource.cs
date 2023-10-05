using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootstepSource : MonoBehaviour
{
    [SerializeField] AudioClip StepSound;

    public AudioClip stepSound => StepSound;
}
