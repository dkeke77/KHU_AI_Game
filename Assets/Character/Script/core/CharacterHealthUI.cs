// CharacterHealthUI.cs
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CharacterInfo))]
public class CharacterHealthWidthUI : MonoBehaviour
{
    [Header("Character Data")]
    public CharacterInfo info;     // CharacterInfo 연결

    [Header("UI Settings")]
    public RectTransform barRect;  // HealthBar 이미지가 붙은 RectTransform
    public float maxWidth = 400f;  // full HP일 때 width 값 (400)

    [Header("Optional Text")]
    public TextMeshProUGUI hpText; // 체력 숫자 표시 (선택)

    void Start()
    {
        if (info == null) info = GetComponent<CharacterInfo>();

        if (barRect == null)
            Debug.LogError("barRect 슬롯에 RectTransform을 연결하세요.", this);
        if (hpText == null)
            Debug.LogWarning("hpText는 선택 사항입니다.", this);
    }

    void Update()
    {
        if (info == null || barRect == null) return;

        // 0~1 범위 비율 계산
        float ratio = Mathf.Clamp01(info.CurrentHP / (float)CharacterCore.MAX_HP);

        // sizeDelta.x 를 비율에 맞춰 조절
        var size = barRect.sizeDelta;
        size.x = ratio * maxWidth;
        barRect.sizeDelta = size;

        // 숫자 표시 (있으면)
        if (hpText != null)
            hpText.text = $"{info.CurrentHP:0}";
    }
}
