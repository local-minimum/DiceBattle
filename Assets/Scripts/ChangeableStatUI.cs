using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChangeableStatUI : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI StatText;

    [SerializeField]
    TMPro.TextMeshProUGUI ChangeText;

    [SerializeField]
    RectTransform AnimationTarget;

    [SerializeField]
    AnimationCurve XScaleAnimation;

    [SerializeField]
    AnimationCurve YScaleAnimation;

    [SerializeField]
    AnimationCurve AlphaAnimation;

    [SerializeField]
    bool ApplyAlphaToText;

    [SerializeField]
    bool NaturalNumbers;

    Image AnimationTargetImage;

    float CalculateAnimationDuration(AnimationCurve curve)
    {
        if (curve == null || curve.keys.Length == 0) return 0;


        return curve.keys.Last().time;
    }

    float AnimationDuration => Mathf.Max(
        CalculateAnimationDuration(XScaleAnimation),
        CalculateAnimationDuration(YScaleAnimation),
        CalculateAnimationDuration(AlphaAnimation)
    );

    int _value;

    bool animateChange;
    float animStartTime;
    float animEndTime;

    public int Value
    {
        get => _value;
        set
        {
            var diff = value - _value;

            if (NaturalNumbers)
            {
                diff = Mathf.Max(diff, -_value);
            }

            _value += diff;
            StatText.text = _value.ToString();

            if (diff != 0) {
                ChangeText.text = diff.ToString();

                animStartTime = Time.timeSinceLevelLoad;
                animEndTime = animStartTime + AnimationDuration;
                animateChange = true;

                SyncAnmation(0);

                AnimationTarget.gameObject.SetActive(true);
            }
        }
    }

    public void SetValueWithoutChange(int value)
    {
        _value = Mathf.Max(NaturalNumbers ? 0 : value, value);
        StatText.text = _value.ToString();
    }

    void HideChange()
    {
        AnimationTarget.gameObject.SetActive(false);
    }

    void SyncAnmation(float t)
    {
        AnimationTarget.localScale = new Vector3(XScaleAnimation.Evaluate(t), YScaleAnimation.Evaluate(t), 1);
        if (AnimationTargetImage != null)
        {
            var color = AnimationTargetImage.color;
            color.a = AlphaAnimation.Evaluate(t);
            AnimationTargetImage.color = color;
        }

        if (ApplyAlphaToText)
        {
            var color = ChangeText.color;
            color.a = AlphaAnimation.Evaluate(t);
            ChangeText.color = color;
        }
    }

    private void Start()
    {
        AnimationTargetImage = AnimationTarget.GetComponent<Image>();
        HideChange();
    }

    private void Update()
    {
        if (!animateChange) return;

        if (Time.timeSinceLevelLoad > animEndTime)
        {
            HideChange();
            return;
        }

        SyncAnmation(Time.timeSinceLevelLoad - animStartTime);
    }
}
