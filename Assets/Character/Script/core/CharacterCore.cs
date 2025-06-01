using UnityEngine;

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

    public int dodgeCounter = 0;
    public int blockCounter = 0;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anim;
    Weapon sword;
    Shield shld;
    CharacterCollideChecker chCllChecker;
    ParticleSystem blockEffect;

    //const float ACTION_COOLDOWN = 0.5f;
    //float actionTimer = 0f;

    // ����/��� ��Ÿ�� �߰�
    const float ATTACK_COOLDOWN = 2.5f;
    const float DEFENCE_COOLDOWN = 2.5f;
    const float DODGE_COOLDOWN = 5.0f;
    

    // ����/��� Ÿ�̸� �߰�
    public float attackTimer = 0f;
    public float defenceTimer = 0f;
    public float dodgeTimer = 0f;

    // �׼� flag �߰�
    public bool isAttacking = false;
    public bool isDodging = false;
    public bool isBlocking = false;
    public bool isDead = false;

    public bool isCollideWithCharacter = false;

    public PlayerState state = PlayerState.Idle;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        sword = GetComponentInChildren<Weapon>();
        shld = GetComponentInChildren<Shield>();
        chCllChecker = GetComponentInChildren<CharacterCollideChecker>();
        blockEffect = GetComponentInChildren<ParticleSystem>();

        // ����
        //attackTimer = 0;
        //defenceTimer = 0;
        //dodgeTimer = 0;
    }

    void Update()
    {
        if (isDead) return;

        // ����
        attackTimer -= Time.deltaTime;
        if (attackTimer < 0f) attackTimer = 0f;
        defenceTimer -= Time.deltaTime;
        if (defenceTimer < 0f) defenceTimer = 0f;
        dodgeTimer -= Time.deltaTime;
        if (dodgeTimer < 0f) dodgeTimer = 0f;

        isCollideWithCharacter = chCllChecker.isCollideWithCharacter && !isDead;

        if (cur_hp <= 0)
            Die();
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
            transform.position += moveVec * speed * Time.deltaTime;
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
        return attackTimer <= 0 && stateCheck; // ����
    }
    public void Attack()
    {
        if (CanAttack()) // �߰�
        {
            state = PlayerState.Attacking;
            isAttacking = true; // �߰�
            anim.SetTrigger("doAttack");
            sword.use();
            Invoke(nameof(EndAttack), sword.activationTime + 0.4f);
        }
    }
    void EndAttack()
    {
        isAttacking = false; // �߰�
        state = PlayerState.Idle;
        attackTimer = ATTACK_COOLDOWN; // ����
    }

    public bool CanDefence()
    {
        bool stateCheck = state == PlayerState.Idle || state == PlayerState.Moving;
        return defenceTimer <= 0 && stateCheck; // ����
    }
    public void Defence()
    {
        if (CanDefence()) // �߰�
        {
            state = PlayerState.Defending;
            isBlocking = true;
            anim.SetTrigger("doDefence");
            shld.use();
            Invoke(nameof(EndDefence), shld.activationTime + 0.4f);
        }
    }
    void EndDefence()
    {
        isBlocking = false;
        anim.SetTrigger("releaseDefence");
        state = PlayerState.Idle;
        defenceTimer = DEFENCE_COOLDOWN; // ����
    }

    public bool CanDodge()
    {
        bool stateCheck = state == PlayerState.Idle || state == PlayerState.Moving;
        return dodgeTimer <= 0 && stateCheck; // ����
    }
    public void Dodge()
    {
        if (CanDodge()) // �߰�
        {
            state = PlayerState.Dodging;
            isDodging = true;
            dodgeVec = transform.forward;
            anim.SetTrigger("doDodge");
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
            transform.position += dodgeVec * dodgeSpeed * Time.deltaTime * Mathf.Max(scale,0);
            timer += Time.deltaTime;
            yield return null;
        }
        isDodging = false;
        state = PlayerState.Idle;
        dodgeTimer = DODGE_COOLDOWN;
    }

    void Die()
    {
        state = PlayerState.Dead;
        isDead = true;
        if (anim != null) // �߰�
            anim.SetBool("isDead",true);

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false; // �߰�
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDead) return; // �߰�

        if (other.CompareTag("Weapon") && !isDead)
        {
            if (isDodging)
                dodgeCounter++;
            else if (isBlocking)
            {
                blockCounter++;
                if (blockEffect != null) // �߰�
                    blockEffect.Play();
            }
            else
            {
                Weapon wpn = other.GetComponent<Weapon>();
                if (wpn == null) return; // �߰�

                cur_hp -= wpn.damage;
                if (anim != null) // �߰�
                    anim.SetTrigger("hit");
                Debug.Log("HIT!!" + wpn.damage);

                // ����׿�
                Debug.Log("Weapon Trigger: " + other.name);
            }
        }
     }
}
