
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Tabs : UdonSharpBehaviour
{
    public GameObject[] tabs;
    public GameObject[] content;
    public TextMeshProUGUI label;
    public float highlightOffset = 10f;

    private RectTransform[] rects;
    private Vector2[] originalPositions;

    void Start()
    {
        rects = new RectTransform[tabs.Length];
        originalPositions = new Vector2[tabs.Length];

        for (int i = 0; i < tabs.Length; i++)
        {
            rects[i] = tabs[i].GetComponent<RectTransform>();
            originalPositions[i] = rects[i].anchoredPosition;
        }

        SetActiveTab(0);
    }

    public void SetActiveTab(int index)
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isActive = i == index;
            rects[i].anchoredPosition = originalPositions[i] + (isActive ? Vector2.up * highlightOffset : Vector2.zero);
            content[i].SetActive(isActive);
        }

        if (label != null)
        {
            label.text = tabs[index].name;
        }
    }

    public void ShowPersonal()
    {
        SetActiveTab(0);
    }

    public void ShowLeaderboard()
    {
        SetActiveTab(1);
    }

    public void ShowAchievements()
    {
        SetActiveTab(2);
    }

    public void ShowSettings()
    {
        SetActiveTab(3);
    }
}
