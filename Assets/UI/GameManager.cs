//
// 추후 완성된 기본 기능 구현 프로젝트에 맞추어 변수명 등을 통일해서 사용해야 함!!
// GameManager.cs 스크립트를 GameManager 오브젝트에 Add Component 하는 것부터 진행하면 됨.
// -> Scene 안에 하나뿐인 GameManager가 생성되고, 전투 로직과 결과 UI를 이 오브젝트 하나로 관리.
//

// GameManager.cs
using UnityEngine;
using TMPro;
using System.Collections;  // 코루틴 쓰려면 필요

public class GameManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI winCountPlayer;
    public TextMeshProUGUI winCountEnemy;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;

    [Header("Agents")]
    public CharacterCore attackAgent;
    public CharacterCore defenseAgent;

    [Header("Episode Controller")]
    public EpisodeController episodeCtrl;

    int winsPlayer = 0, winsEnemy = 0;
    bool hasResultShown = false;

    void Start()
    {
        // 초기화
        winsPlayer = winsEnemy = 0;
        UpdateWinUI();
        // 결과 패널은 반드시 비활성화 상태에서 시작
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }

    void Update()
    {
        /*
       // 두 에이전트가 모두 살아 있으면 다음 싸움을 위해 플래그 리셋
       if (attackAgent.cur_hp > 0 && defenseAgent.cur_hp > 0)
       {
            hasResultShown = false;
       }

        if (hasResultShown) return;   // 이미 한 번 결과를 띄웠으면 더 이상 처리 안 함

        bool atkDead = attackAgent != null && attackAgent.cur_hp <= 0;
        bool defDead = defenseAgent != null && defenseAgent.cur_hp <= 0;

        if (atkDead || defDead)
        {
            // 사망이 감지되면
            hasResultShown = true;
            //ShowAndCount(atkDead, defDead);
            // 즉시 ShowAndCount가 아니라, 지연 코루틴을 시작
            StartCoroutine(OnDeathDelay(atkDead, defDead));
        }*/
    }
    IEnumerator OnDeathDelay(bool atkDead, bool defDead)
    {
        // 2.5초 동안 대기 (죽는 모션 재생 시간 고려)
        yield return new WaitForSeconds(2.5f);

        // 죽음 지연 후에 결과 패널 띄우기
        ShowAndCount(atkDead, defDead);
    }

    void ShowAndCount(bool atkDead, bool defDead)
    {
        // 1) Draw
        if (atkDead && defDead)
        {
            resultText.text = "Draw!";
        }
        // 2) 공격형 승리 (수비만 죽었을 때)
        else if (!atkDead && defDead)
        {
            winsPlayer++;
            UpdateWinUI();
            resultText.text = "AT Agent Win!";
        }
        // 3) 수비형 승리 (공격만 죽었을 때)
        else if (atkDead && !defDead)
        {
            winsEnemy++;
            UpdateWinUI();
            resultText.text = "DF Agent Win!";
        }

        // 4) 패널 켜기
        if (resultPanel != null) resultPanel.SetActive(true);

        // 5) 에피소드 올리기
        episodeCtrl?.NextEpisode();

        // 6) 5초 뒤 패널 숨기기 & 다음 에피소드 준비
        Invoke(nameof(HideResultPanel), 5.0f);
    }

    void UpdateWinUI()
    {
        if (winCountPlayer != null) 
            winCountPlayer.text = winsPlayer.ToString();
        if (winCountEnemy != null) 
            winCountEnemy.text = winsEnemy.ToString();
    }

    void HideResultPanel()
    {
        if (resultPanel != null) 
            resultPanel.SetActive(false);
    }
}


