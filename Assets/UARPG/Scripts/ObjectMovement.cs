using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMovement : MonoBehaviour
{
    [SerializeField] ProjectileTransformation[] transformations;

    private int transformationIndex = -1;

    private void OnEnable() => StartCoroutine(Movement());
    private void OnDisable() => StopAllCoroutines();

    private void Update()
    {
        if (transformationIndex >= 0 && transformationIndex < transformations.Length)
        {
            transform.Translate(transformations[transformationIndex].movement * Time.deltaTime * Global.instance.moveSpeedMultiplier, transformations[transformationIndex].space);
            transform.Rotate(transformations[transformationIndex].rotation * Time.deltaTime * Global.instance.moveSpeedMultiplier, transformations[transformationIndex].space);
            transform.localScale = transform.localScale + transformations[transformationIndex].scale * Time.deltaTime * Global.instance.moveSpeedMultiplier;
        }
    }

    private IEnumerator Movement()
    {
        for (int i = 0; i < transformations.Length; i++)
        {
            transformationIndex = i;
            if (transformations[i].duration > 0) yield return new WaitForSeconds(transformations[i].duration);
            else yield break;
        }
        transformationIndex = -1;
    }
}
