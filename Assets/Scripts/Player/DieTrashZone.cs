using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DieTrashZone : MonoBehaviour
{
    [SerializeField]
    ChangeableStatUI _trashed;

    public int Trashed
    {
        get => _trashed.Value;
        set
        {
            _trashed.Value = value;
        }
    }


    bool hovered = false;

    public void OnHoverStart()
    {
        if (Die.DraggedDie != null)
        {
            _trashed.PreviewChange(1);
            hovered = true;
        }
    }

    public void OnHoverEnd()
    {
        if (hovered)
        {
            _trashed.EndPreview();
            hovered = false;
        }
    }

    private void OnEnable()
    {
        _trashed.SetValueWithoutChange(0);
        Die.OnDropDie += Die_OnDropDie;
        Die.OnTrashDie += Die_OnTrashDie;
    }


    private void OnDisable()
    {
        Die.OnDropDie -= Die_OnDropDie;
        Die.OnTrashDie -= Die_OnTrashDie;
    }

    private void Die_OnTrashDie(Die die)
    {
        Debug.Log("[Dice Trash] Trashed a die");
        Trashed++;
    }

    private void Die_OnDropDie(Die die)
    {
        if (!hovered || !die.Interactable) return;

        Trashed++;
        die.NoDice();
    }
}
