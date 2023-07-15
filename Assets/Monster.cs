using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Monster : MonoBehaviour
{
    public static Monster HoveredMonster { get; set; }

    [SerializeField]
    TMPro.TextMeshProUGUI HealthText;

    [SerializeField]
    TMPro.TextMeshProUGUI NameText;

    [SerializeField]
    int startHealth;

    [SerializeField]
    GameObject BaseCard;

    private int _health;
    public int Health { 
        get => _health; 
        set
        {
            _health = Mathf.Max(0, value);
            HealthText.text = _health.ToString();

            if (_health == 0)
            {
                deathHide = Time.timeSinceLevelLoad + DelayDeathHide;
            }
        }
    }

    [SerializeField]
    float DelayDeathHide = 0.5f;

    float deathHide;

    [SerializeField]
    TMPro.TextMeshProUGUI DefenceText;

    [SerializeField]
    int startDefence;

    [SerializeField]
    int xpReward;
    public int XpReward => xpReward;

    int _defence;
    public int Defence { 
        get => _defence; 
        set
        {
            _defence = value;
            DefenceText.text = _defence.ToString();
        }
    }

    public string Name => NameText.text;

    public bool Alive => Health > 0;

    private void OnEnable()
    {
        Health = startHealth;
        Defence = startDefence;
    }

    private void Update()
    {
        if (!Alive && BaseCard.activeSelf && Time.timeSinceLevelLoad > deathHide)
        {
            BaseCard.SetActive(false);
        }
    }
}
