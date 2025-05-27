//
// 추후 완성된 기본 기능 구현 프로젝트에 맞추어 변수명 등을 통일해서 사용해야 함!!
// GameManager.cs 스크립트를 GameManager 오브젝트에 Add Component 하는 것부터 진행하면 됨.
// -> Scene 안에 하나뿐인 GameManager가 생성되고, 모든 전투 로직과 결과 UI를 이 오브젝트 하나로 관리.
//

//using UnityEngine;
//using TMPro;

//public class GameManager : MonoBehaviour
//{
//    public HealthSystem playerHS;
//    public HealthSystem enemyHS;

//    public TextMeshProUGUI winCountPlayer;
//    public TextMeshProUGUI winCountEnemy;

//    public GameObject resultPanel;
//    public TextMeshProUGUI resultText;

//    private int winsPlayer = 0;
//    private int winsEnemy = 0;

//    void Start()
//    {
//        // 체력 0 이벤트 구독
//        playerHS.OnDead.AddListener(OnPlayerDead);
//        enemyHS.OnDead.AddListener(OnEnemyDead);

//        // 초기 UI
//        resultPanel.SetActive(false);
//        UpdateWinCountUI();
//    }

//    private void OnPlayerDead()
//    {
//        // 적 승리
//        winsEnemy++;
//        ShowResult("Defense Agent"); // Enemy
//    }

//    private void OnEnemyDead()
//    {
//        // 플레이어 승리
//        winsPlayer++;
//        ShowResult("Attack Agent"); // Player
//    }

//    private void ShowResult(string winner)
//    {
//        // 결과 패널 표시
//        resultText.text = $"WINNER: {winner}";
//        resultPanel.SetActive(true);

//        // 승리 횟수 갱신
//        UpdateWinCountUI();

//        // 자동 리셋 혹은 버튼 대기
//        Invoke(nameof(RestartMatch), 2.0f);
//    }

//    private void UpdateWinCountUI()
//    {
//        winCountPlayer.text = winsPlayer.ToString();
//        winCountEnemy.text = winsEnemy.ToString();
//    }

//    private void RestartMatch()
//    {
//        // 패널 끄고 체력 리셋
//        resultPanel.SetActive(false);
//        playerHS.Heal(playerHS.maxHP);
//        enemyHS.Heal(enemyHS.maxHP);
//    }
//}
