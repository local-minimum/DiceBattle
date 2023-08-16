using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ShopItemCard : MonoBehaviour
{
    [SerializeField]
    TMPro.TextMeshProUGUI TitleUI;

    [SerializeField]
    TMPro.TextMeshProUGUI CostUI;

    [SerializeField]
    TMPro.TextMeshProUGUI DescriptionUI;

    [SerializeField]
    Image image;

    [SerializeField]
    Image cantAffordImage;

    [SerializeField, Range(0, 1)]
    float cantAffordAlpha = 0.75f;

    [SerializeField]
    GameObject flipSideImage;

    System.Action OnSelect;
    int cost;

    public void Prepare(
        string title,
        string description,
        Sprite sprite,
        int cost,
        System.Action onSelect
    )
    {
        TitleUI.text = title;
        DescriptionUI.text = description;

        image.sprite = sprite;

        this.cost = cost;
        CostUI.text = $"Cost:\t{cost}xp";

        OnSelect = onSelect;

        flipSideImage.SetActive(false);
        ValidateCost();
    }

    public void ValidateCost()
    {
        var color = cantAffordImage.color;
        if (!flipSideImage.activeSelf && cost > GameProgress.XP)
        {
            color.a = cantAffordAlpha;
        } else
        {
            color.a = 0;
        }
        cantAffordImage.color = color;
    }

    public void Flip()
    {
        flipSideImage.SetActive(true);
    }

    public void OnClick()
    {
        if (flipSideImage.activeSelf || cost > GameProgress.XP) return;

        Debug.Log($"[Shop Item Card] Bought <{TitleUI.text}>");
        OnSelect?.Invoke();
        Flip();
    }
}
