using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class Stopwatch : MonoBehaviour
{
    [SerializeField] float offset;
    [SerializeField] bool downcount;

    private Text text;
    private float time;

    private void Awake()
    {
        text = GetComponent<Text>();
        time = offset;
    }

    private void Update()
    {
        text.text = time.ToString();
        time += Time.deltaTime * (downcount ? -1f : 1f);
    }
}
