using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieTrashZone : MonoBehaviour
{
    int _trashed;

    public int Trashed
    {
        get => _trashed;
        set
        {
            _trashed = value;
            trashBag.text = Trashed.ToString();
        }
    }

    [SerializeField]
    TMPro.TextMeshProUGUI trashBag;

    bool hovered = false;

    public void OnHoverStart()
    {
        if (Die.DraggedDie != null)
        {
            trashBag.text = (Trashed + 1).ToString();
            hovered = true;
        }
    }

    public void OnHoverEnd()
    {
        if (hovered)
        {
            trashBag.text = Trashed.ToString();
            hovered = false;
        }
    }

    private void OnEnable()
    {
        trashBag.text = Trashed.ToString();
        Die.OnDropDie += Die_OnDropDie;
    }

    private void OnDisable()
    {
        Die.OnDropDie -= Die_OnDropDie;
    }

    private void Die_OnDropDie(Die die)
    {
        if (!hovered || !die.Interactable) return;

        Trashed++;
        die.NoDice();
    }
}
