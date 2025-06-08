using UnityEngine;
using TMPro;

public class EpisodeController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI episodeText;   // 하단 중앙 텍스트 연결 슬롯

    int currentEpisode = 1;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        // 키 테스트용: E 키를 누르면 다음 에피소드
        if (Input.GetKeyDown(KeyCode.E))
            NextEpisode();
    }

    // 외부에서 호출하려면 public으로
    public void NextEpisode() // 나중에 ML-Agents에서 한 에피소드가 끝날 때마다 호출해서 더해주면 됨!
    { // EpisodeController.NextEpisode() 나중에 호출하도록
        currentEpisode++;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (episodeText != null)
            episodeText.text = $"EPISODE {currentEpisode}";
    }
}
