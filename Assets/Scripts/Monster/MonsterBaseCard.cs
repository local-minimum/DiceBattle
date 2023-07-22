using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MonsterBaseCard : MonoBehaviour
{
    [SerializeField]
    Color BaseBGColor;

    [SerializeField]
    Color HoverBTColor;

    [SerializeField]
    Image BGImage;

    Monster monster;

    bool hovering;

    private void Start()
    {
        monster = GetComponentInParent<Monster>();
    }

    public void ShowHover()
    {
        if (ActionCard.DraggedCard != null && monster.Alive)
        {
            hovering = true;
            BGImage.color = HoverBTColor;

            Monster.HoveredMonster = monster;
        }
    }

    private void Update()
    {
        if (hovering && ActionCard.DraggedCard == null)
        {
            hovering = false;
            BGImage.color = BaseBGColor;

            if (Monster.HoveredMonster == this)
            {
                Monster.HoveredMonster = null;
            }
        }
    }
}
