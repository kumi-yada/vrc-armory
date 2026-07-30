using UdonSharp;
using UnityEngine;
using TMPro;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class LeaderRow : UdonSharpBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI levelText;

    public void SetEntry(string name, int level, float exp)
    {
        gameObject.SetActive(true);
        if (nameText != null)
            nameText.text = name;
        if (levelText != null)
            levelText.text = string.Format("Lv.{0}  ({1} XP)", level, Mathf.FloorToInt(exp));
    }

    public void Clear()
    {
        gameObject.SetActive(false);
        if (nameText != null)
            nameText.text = "";
        if (levelText != null)
            levelText.text = "";
    }
}
