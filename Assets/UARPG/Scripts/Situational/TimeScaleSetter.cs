using System.Collections;
using UnityEngine;

public class TimeScaleSetter : MonoBehaviour
{

    [SerializeField] float timeScale = 1;

    void Update() => Time.timeScale = timeScale;
}
