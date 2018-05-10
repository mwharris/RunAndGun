using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpControlledBob : MonoBehaviour
{
    [SerializeField] private float BobDuration;
    [SerializeField] private float BobAmount;

    //Provides the current offset that can be used to manipulate the camera
    private float m_Offset = 0f;
    public float Offset()
    {
        return m_Offset;
    }

    public IEnumerator DoBobCycle()
    {
        // make the camera move down slightly
        float t = 0f;
        while (t < BobDuration)
        {
            m_Offset = Mathf.Lerp(0f, BobAmount, t/BobDuration);
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }

        // make it move back to neutral
        t = 0f;
        while (t < BobDuration)
        {
            m_Offset = Mathf.Lerp(BobAmount, 0f, t/BobDuration);
            t += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        m_Offset = 0f;
    }
}