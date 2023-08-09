using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseTransition : MonoBehaviour
{
    [SerializeField]
    BattlePhase[] TriggerPhase;

    [SerializeField]
    AnimationCurve[] YAnimation;

    [SerializeField]
    float[] TargetY;

    private void OnEnable()
    {
        Battle.OnChangePhase += Battle_OnChangePhase;
    }

    private void OnDisable()
    {
        Battle.OnChangePhase -= Battle_OnChangePhase;
    }

    private void Battle_OnChangePhase(BattlePhase phase)
    {
        for (int i = 0, l = Mathf.Min(TriggerPhase.Length, YAnimation.Length, TargetY.Length); i<l; i++)
        {
            if (TriggerPhase[i] == phase)
            {
                StopAllCoroutines();
                StartCoroutine(EaseIn(YAnimation[i], TargetY[i]));
                return;
            }
        }
    }

    IEnumerator<WaitForSeconds> EaseIn(AnimationCurve yAxisEasing, float targetY)
    {
        var maxT = yAxisEasing.length;
        var t0 = Time.timeSinceLevelLoad;
        var progress = 0f;

        var rt = transform as RectTransform;

        var startPosition = rt.anchoredPosition;

        Debug.Log($"Lerping Y from {startPosition.y} => {targetY}");

        while (progress < maxT)
        {
            progress = Mathf.Min(Time.timeSinceLevelLoad - t0, maxT);
            rt.anchoredPosition = new Vector2(
                startPosition.x, 
                Mathf.Lerp(startPosition.y, targetY, yAxisEasing.Evaluate(progress)) 
            );
            yield return new WaitForSeconds(0.02f);
        }
    }
}
