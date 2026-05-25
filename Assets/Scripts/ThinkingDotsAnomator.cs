using System.Collections;
using UnityEngine;
using TMPro;

public class ThinkingDotsAnimator : MonoBehaviour
{
    private TMP_Text _text;
    private Coroutine _routine;

    private void OnEnable()
    {
        _text = GetComponent<TMP_Text>();
        _routine = StartCoroutine(Animate());
    }

    private void OnDisable()
    {
        if (_routine != null) StopCoroutine(_routine);
    }

    private IEnumerator Animate()
    {
        string[] frames = { "●○○", "●●○", "●●●", "○●●", "○○●", "○○○" };
        int i = 0;
        while (true)
        {
            if (_text) _text.text = frames[i % frames.Length];
            i++;
            yield return new WaitForSeconds(0.22f);
        }
    }
}