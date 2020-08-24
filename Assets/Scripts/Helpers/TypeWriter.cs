using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// attach to UI Text component (with the full text already there)

public class TypeWriter : MonoBehaviour {
    [NonSerialized]
    public bool finishedTyping;
    
    private string _fullString;

    private Coroutine routine;

    public void QuickFinish() {
        StopCoroutine(routine);
        GetComponent<Text>().text = _fullString;
        finishedTyping = true;
    }

    public void StartTypeWriting(string msg) {
        _fullString = msg;
        GetComponent<Text>().text = "";
        finishedTyping = false;
        routine = StartCoroutine(PlayText());
    }

    private IEnumerator PlayText() {
        var text = GetComponent<Text>();
        foreach (char c in _fullString) {
            text.text += c;
            yield return new WaitForSeconds(0.05f);
        }

        finishedTyping = true;
    }
}