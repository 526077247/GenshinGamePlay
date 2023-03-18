using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShakeSource : ISignalSource6D
{
    public float delay;
    public float shakeRange;
    public Vector3 shakeDir;
    public Vector3 velocity;
    public float cycleTime;
    public float duration;

    /// <summary>
    /// 添加振动源
    /// </summary>
    /// <param name="delay">开始延迟的时间</param>
    /// <param name="shakeRange">振动幅度</param>
    /// <param name="shakeDir">振动范围</param>
    /// <param name="cycleTime">振动一次的时间，即总振动次数为duration/cycleTime</param>
    /// <param name="duration">总振动持续时间</param>
    public CameraShakeSource(float delay, float shakeRange, Vector3 shakeDir, float cycleTime, float duration)
    {
        this.delay = delay;
        this.shakeRange = shakeRange;
        this.cycleTime = cycleTime;
        this.duration = duration;

        Vector3 randomRange = Random.insideUnitCircle * shakeRange;
        this.velocity = (randomRange + shakeDir.normalized).normalized ;
    }

    public float SignalDuration
    {
        get
        {
            return delay + duration;
        }
    }

    public void GetSignal(float timeSinceSignalStart, out Vector3 pos, out Quaternion rot)
    {
        if (timeSinceSignalStart <= delay)
        {
            pos = Vector3.zero;
        }
        else
        {
            float times = timeSinceSignalStart / (cycleTime / 4);
            int cycle25Count = Mathf.FloorToInt(times);
            float inCycle25Time = times - cycle25Count;
            if (cycle25Count % 4 == 0)
            {
                pos = Vector3.Lerp(Vector3.zero, velocity, inCycle25Time);
            }
            else if (cycle25Count % 4 == 1)
            {
                pos = Vector3.Lerp(velocity, Vector3.zero, inCycle25Time);
            }
            else if (cycle25Count % 4 == 2)
            {
                pos = Vector3.Lerp(Vector3.zero, -velocity, inCycle25Time);
            }
            else
            {
                pos = Vector3.Lerp(-velocity, Vector3.zero, inCycle25Time);
            }
        }
        rot = Quaternion.identity;
    }
}