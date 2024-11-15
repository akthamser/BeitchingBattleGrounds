using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Script added to all buttons in the game. Adds consistent hover and click behaviours.
/// </summary>
public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private float scaleRate = 1.05f;
    private float defaultScale;
    private float targetScale;

    void Start ()
    {
        defaultScale = transform.localScale.x;
        targetScale = defaultScale;
    }

    void OnEnable ()
    {
        if(defaultScale != 0)
            targetScale = defaultScale;
    }

    void Update ()
    {
        // Scale button.
        if(transform.localScale.x != targetScale)
        {
            transform.localScale = Vector3.MoveTowards(transform.localScale, new Vector3(targetScale, targetScale, targetScale), Time.deltaTime * 2);
        }
    }

    public void OnPointerEnter (PointerEventData data)
    {
        targetScale = defaultScale * scaleRate;

        // Play sound effect.
        AudioManager.inst.Play(AudioManager.inst.buttonHover);
    }

    public void OnPointerExit (PointerEventData data)
    {
        targetScale = defaultScale;
    }

    public void OnPointerDown (PointerEventData data)
    {
        targetScale = defaultScale;

        // Play sound effect.
        AudioManager.inst.Play(AudioManager.inst.buttonClick);
    }

    public void OnPointerUp (PointerEventData data)
    {
        targetScale = defaultScale * scaleRate;

        // Play sound effect.
        AudioManager.inst.Play(AudioManager.inst.buttonHover);
    }
}