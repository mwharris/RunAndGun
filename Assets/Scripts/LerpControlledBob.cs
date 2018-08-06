using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpControlledBob : MonoBehaviour
{
    [SerializeField] private float BobDuration;
    [SerializeField] private float BobAmount;
    [SerializeField] private float JumpRaiseAmount;

    //Provides the current offset that can be used to manipulate the camera
    private float m_Offset = 0f;
    public float Offset()
    {
        return m_Offset;
    }

    public IEnumerator DoBobCycle(bool landing)
    {
        if (landing)
        {
            // make the camera move down slightly
            float t = 0f;
            while (t < BobDuration)
            {
                m_Offset = Mathf.Lerp(JumpRaiseAmount, BobAmount, t / (BobDuration / 2));
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            t = 0f;
            while (t < BobDuration)
            {
                m_Offset = Mathf.Lerp(BobAmount, 0f, t / (BobDuration / 2));
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            m_Offset = 0f;
        }
        else
        {
            // make it to our raise head height
            float t = 0f;
            while (t < BobDuration)
            {
                m_Offset = Mathf.Lerp(0f, JumpRaiseAmount, t / BobDuration);
                t += Time.deltaTime;
                yield return new WaitForFixedUpdate();
            }
            m_Offset = JumpRaiseAmount;
        }
    }
}