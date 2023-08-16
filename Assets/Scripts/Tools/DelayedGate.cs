using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DelayedGate
{
    [SerializeField, Range(0, 10)]
    float delayDuration = 1f;

    bool delay;

    float openTime;

    public DelayedGate(float delayDuration)
    {
        this.delayDuration = delayDuration;
    }

    public DelayedGate()
    {

    }

    public bool Locked => delay && Time.timeSinceLevelLoad < openTime;

    public void Lock()
    {
        delay = true;
        openTime = Time.timeSinceLevelLoad + delayDuration;
    }

    public bool Open(out bool toggled)
    {
        if (delay && Time.timeSinceLevelLoad < openTime)
        {
            toggled = false;
            return false;
        }

        toggled = delay;
        delay = false;

        return true;
    }

    public void Reset()
    {
        delay = false;
    }
}
