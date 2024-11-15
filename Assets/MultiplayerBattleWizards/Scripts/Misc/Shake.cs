using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// General class used to shake objects.
/// </summary>
public class Shake : MonoBehaviour
{
    // Instance
    public static Shake inst;
    
    void Awake ()
    {
        #region Singleton

        // If the instance already exists, destroy this one.
        if(inst != this && inst != null)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this script.
        inst = this;

        #endregion
    }

    /// <summary>
    /// Shakes an object.
    /// </summary>
    /// <param name="duration">Length of the shake in seconds.</param>
    /// <param name="amount">Max distance the object can move from its origin.</param>
    /// <param name="intensity">Speed at which the object shakes.</param>
    public void ShakeObject (GameObject obj, float duration, float amount, float intensity)
    {
        StartCoroutine(ApplyShake(obj, duration, amount, intensity));
    }

    // Shakes the object over time.
    IEnumerator ApplyShake (GameObject obj, float duration, float amount, float intensity)
    {
        Vector3 originPos = obj.transform.localPosition;
        Vector3 targetPos = originPos;
        float rate = 1.0f;

        // The time we'll end the shake.
        float endTime = Time.time + duration;

        // Shake for the duration.
        while(Time.time < endTime)
        {
            // If the object is at the target pos - get a new random target pos.
            if(obj.transform.localPosition == targetPos)
            {
                Vector2 randomCircle = Random.insideUnitCircle * amount * rate;
                targetPos = originPos + new Vector3(randomCircle.x, randomCircle.y, 0);
            }

            rate -= (duration / 1.0f) * Time.deltaTime;

            // Move towards the target pos.
            obj.transform.localPosition = Vector3.MoveTowards(obj.transform.localPosition, targetPos, intensity * Time.deltaTime);

            yield return null;
        }

        obj.transform.localPosition = originPos;
    }
}