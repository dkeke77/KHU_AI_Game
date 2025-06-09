using TMPro;
using UnityEngine;
using UnityEngine.Events; // UI 매핑 추가_0607

public enum PlayerState
{
    Idle,
    Moving,
    Attacking,
    Defending,
    Dodging,
    Dead
}

public class CharacterCore : MonoBehaviour
{
    public float speed = 5f;
    public const int MAX_HP = 100;

    public int cur_hp = 100;

    [Header("Label")] // UI 매핑 추가_0607
    public GameObject labelPrefab; // 머리 위 라벨 프리팹 연결할 슬롯

    [Header("Cooldown UI")]
    public CooldownManager_Attack attackUI; // 공격형 에이전트용 쿨타임 UI
    public CooldownManager_Defense defenseUI; // 수비형 에이전트용 쿨타임 UI

    // 전투 데이터
    public int dodgeCounter = 0;
    public int dodgeSucCounter = 0;
    public int blockCounter = 0;
    public int blockSucCounter = 0;
    public int attackCounter = 0;
    public int attackSucCounter = 0;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anim;
    Weapon sword;
    Shield shld;
    CharacterCollideChecker chCllChecker;
    ParticleSystem blockEffect;

    // 바닥 정보
    public Transform floor;
    Bounds bnd;

    //const float ACTION_COOLDOWN = 0.5f;
    //float actionTimer = 0f;

    // 공격/방어 쿨타임 추가
    const float ATTACK_COOLDOWN = 2.5f;
    const float DEFENCE_COOLDOWN = 2.5f;
    const float DODGE_COOLDOWN = 5.0f;

    // 공격/방어 타이머 추가
    public float attackTimer = 0f;
    public float defenceTimer = 0f;
    public float dodgeTimer = 0f;

    // 피격 시 무적
    public float hitTimer = 0f;
    const float HIT_COOLDOWN = 1.0f;


    // 액션 flag 추가
    public bool isAttacking = false;
    public bool isDodging = false;
    public bool isBlocking = false;
    public bool isDead = false;

    public bool isCollideWithCharacter = false;

    public PlayerState state = PlayerState.Idle;

    // 사망 시 호출할 이벤트
    public UnityEvent OnDead = new UnityEvent();

    // 태그로 수비형 에이전트인지 구분
    bool IsDefenseAgent()
    {
        return CompareTag("DefenseAgent");
    }

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        sword = GetComponentInChildren<Weapon>();
        shld = GetComponentInChildren<Shield>();
        chCllChecker = GetComponentInChildren<CharacterCollideChecker>();
        blockEffect = GetComponentInChildren<ParticleSystem>();

        bnd = floor.GetComponent<MeshCollider>().bounds;

        // 수정
        //attackTimer = 0;
        //defenceTimer = 0;
        //dodgeTimer = 0;
    }

    void Start() // UI 매핑 추가_0607
    {
        // 라벨 생성 -> 캐릭터 머리 위 구분 라벨(공격형/수비형)
        if (labelPrefab != null)
        {
            var lblGO = Instantiate(labelPrefab, transform);
            // 머리 위 오프셋
            lblGO.transform.localPosition = new Vector3(0, 2.2f, 0);

            // 텍스트 세팅
            var tmp = lblGO.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
            {
                bool isAtk = CompareTag("AttackAgent");
                tmp.text = isAtk ? "AT Agent" : "DF Agent";
                tmp.color = isAtk ? Color.red : Color.blue;
            }
        }
    }

    void Update()
    {
        if (isDead) return;

        // 수정
        attackTimer -= Time.deltaTime;
        if (attackTimer < 0f) attackTimer = 0f;

        defenceTimer -= Time.deltaTime;
        if (defenceTimer < 0f) defenceTimer = 0f;

        dodgeTimer -= Time.deltaTime;
        if (dodgeTimer < 0f) dodgeTimer = 0f;

        hitTimer -= Time.deltaTime;
        if (hitTimer < 0f) hitTimer = 0f;

        if (cur_hp <= 0)
            Die();

        // 동작 테스트용 점검 코드
        if (Input.GetKeyDown(KeyCode.Space)) // 공격 -> 스페이스바
        {
            Attack();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) // 방어 -> 왼쪽 시프트
        {
            Defence();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl)) // 회피 -> 왼쪽 컨트롤
        {
            Dodge();
        }
    }

    public bool CanMove()
    {
        return state == PlayerState.Idle || state == PlayerState.Moving;
    }
    public void HandleMovement(float xAxis, float zAxis)
    {
        moveVec = new Vector3(xAxis, 0, zAxis).normalized;

        if (moveVec != Vector3.zero)
        {
            transform.position = ClampVector3(transform.position + moveVec * speed * Time.deltaTime);

            transform.LookAt(transform.position + moveVec);
            anim.SetBool("isWalk", true);
            state = PlayerState.Moving;
        }
        else
        {
            anim.SetBool("isWalk", false);
            state = PlayerState.Idle;
        }
    }
    public bool CanAttack()
    {
        bool stateCheck = state == PlayerState.Idle || state == PlayerState.Moving;
        return attackTimer <= 0 && stateCheck; // 수정
    }
    public void Attack()
    {
        if (CanAttack()) // 추가
        {
            state = PlayerState.Attacking;
            isAttacking = true; // 추가
            anim.SetTrigger("doAttack");
            sword.use();
            attackCounter++;
            Invoke(nameof(EndAttack), sword.activationTime + 0.4f);
        }
    }
    void EndAttack()
    {
        isAttacking = false; // 추가
        state = PlayerState.Idle;
        attackTimer = ATTACK_COOLDOWN; // 수정

        // Attack UI 트리거 (index 0)
        if (attackUI != null)
        {
            attackUI.TriggerCooldown(0);
        }
        else if (defenseUI != null)
        {
            // 수비 에이전트인 경우 defenseUI 로도 동일 슬롯(0)을 트리거
            defenseUI.TriggerCooldown(0);
        }
    }

    public bool CanDefence()
    {
        bool stateCheck = state == PlayerState.Idle || state == PlayerState.Moving;
        return defenceTimer <= 0 && stateCheck; // 수정
    }
    public void Defence()
    {
        if (CanDefence()) // 추가
        {
            state = PlayerState.Defending;
            isBlocking = true;
            anim.SetTrigger("doDefence");
            shld.use();
            blockCounter++;
            Invoke(nameof(EndDefence), shld.activationTime + 0.4f);
        }
    }
    void EndDefence()
    {
        isBlocking = false;
        anim.SetTrigger("releaseDefence");
        state = PlayerState.Idle;
        defenceTimer = DEFENCE_COOLDOWN; // 수정

        // Defence UI 트리거 (index 1)
        if (IsDefenseAgent() && defenseUI != null)
            defenseUI.TriggerCooldown(1);
        else if (attackUI != null)
            attackUI.TriggerCooldown(1);
    }

    public bool CanDodge()
    {
        bool stateCheck = state == PlayerState.Idle || state == PlayerState.Moving;
        return dodgeTimer <= 0 && stateCheck; // 수정
    }
    public void Dodge()
    {
        if (CanDodge()) // 추가
        {
            state = PlayerState.Dodging;
            isDodging = true;
            dodgeVec = transform.forward;
            anim.SetTrigger("doDodge");
            dodgeCounter++;
            StartCoroutine(DodgeMove());
        }
    }

    System.Collections.IEnumerator DodgeMove()
    {
        float duration = 1.5f;
        float timer = 0f;
        float dodgeSpeed = 6.5f;
        while (timer < duration)
        {
            float scale = -Mathf.Pow(timer - 0.4f, 2.0f) * 3 + 1;
            if (!isCollideWithCharacter)
                transform.position = ClampVector3(transform.position + dodgeVec * dodgeSpeed * Time.deltaTime * Mathf.Max(scale, 0));

            timer += Time.deltaTime;
            yield return null;
        }
        isDodging = false;
        state = PlayerState.Idle;
        dodgeTimer = DODGE_COOLDOWN;

        // Dodge UI 트리거 (index 2)
        if (IsDefenseAgent() && defenseUI != null)
            defenseUI.TriggerCooldown(2);
        else if (attackUI != null)
            attackUI.TriggerCooldown(2);
    }

    void Die()
    {
        state = PlayerState.Dead;
        isDead = true;
        if (anim != null) // 추가
            anim.SetBool("isDead", true);

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false; // 추가
    }

    Vector3 ClampVector3(Vector3 input)
    {
        input.x = Mathf.Clamp(input.x, bnd.min.x, bnd.max.x);
        input.z = Mathf.Clamp(input.z, bnd.min.z, bnd.max.z);
        return input;
    }

    public bool isInBoundary(float tolerance = 0.15f)
    {
        float dist_x0 = Mathf.Abs(transform.position.x - bnd.min.x);
        float dist_x1 = Mathf.Abs(transform.position.x - bnd.max.x);
        float dist_y0 = Mathf.Abs(transform.position.y - bnd.min.y);
        float dist_y1 = Mathf.Abs(transform.position.y - bnd.max.y);

        if (dist_x0 <= tolerance || dist_x1 <= tolerance || dist_y0 <= tolerance || dist_y1 <= tolerance)
            return true;
        else
            return false;
    }

    public void Spawn()
    {
        // 복잡하지 않게 floor 정보 받아오지않고 고정값으로 함
        float randX = Random.Range(bnd.min.x, bnd.max.x);
        float randZ = Random.Range(bnd.min.z, bnd.max.z);
        transform.position = new Vector3(randX, transform.position.y, randZ);
        transform.forward = Vector3.forward;

        attackTimer = 1.0f;
        defenceTimer = 1.0f;
        dodgeTimer = 1.0f;

        cur_hp = MAX_HP;
        state = PlayerState.Idle;

        isAttacking = false;
        isDodging = false;
        isBlocking = false;
        isCollideWithCharacter = false;

        if (isDead)
        {
            isDead = false;
            anim.speed = 1f;
            anim.SetBool("isDead", false);
            anim.Play("Idle", 0, 0f);
        }

        attackCounter = 0;
        blockCounter = 0;
        dodgeCounter = 0;
        attackSucCounter = 0;
        blockSucCounter = 0;
        dodgeSucCounter = 0;

        OnDead.Invoke(); // OnDead 이벤트 발동_0607 추가
    }

    // 다음 에피소드 준비용 초기화
    public void ResetCharacter()
    {
        Spawn();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        isCollideWithCharacter = false;

        foreach (ContactPoint contact in collision.contacts)
        {
            Collider myCollider = contact.thisCollider; // 내 collider
            Collider theirCollider = contact.otherCollider; // 상대 collider

            Debug.Log(this.name + " : "+ myCollider.name + " : " + theirCollider.name);

            if (myCollider.CompareTag("CharacterBody") && theirCollider.CompareTag("CharacterBody"))
            {
                isCollideWithCharacter = true;
            }
            // 공격함
            else if (myCollider.CompareTag("Weapon") && theirCollider.CompareTag("CharacterBody"))
            {
                CharacterCore eCore = theirCollider.gameObject.GetComponentInParent<CharacterCore>();

                if (!eCore.isDodging && !eCore.isBlocking && (eCore.hitTimer <= 0f || eCore.hitTimer == HIT_COOLDOWN))
                    attackSucCounter++;
            }
            // 공격받음
            else if (myCollider.CompareTag("CharacterBody") && theirCollider.CompareTag("Weapon"))
            {
                if (hitTimer <= 0f)
                {
                    if (isDodging)
                    {
                        dodgeSucCounter++;
                    }
                    else if (isBlocking)
                    {
                        blockSucCounter++;
                        if (blockEffect != null)
                            blockEffect.Play();
                    }
                    else
                    {
                        // 상대 오브젝트에서 Weapon 컴포넌트 가져오기
                        Weapon wpn = theirCollider.gameObject.GetComponentInChildren<Weapon>();
                        if (wpn == null) return;

                        cur_hp -= wpn.damage;
                        hitTimer = HIT_COOLDOWN;

                        if (anim != null)
                            anim.SetTrigger("hit");

                        Debug.Log("HIT!! " + wpn.damage);
                        Debug.Log("Weapon Collision: " + collision.gameObject.name);
                    }
                }
            }
        }
    }
}