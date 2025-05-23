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

    const float ACTION_COOLDOWN = 0.5f;
    const float DODGE_COOLDOWN = 9.0f;
    float actionTimer = 0f;
    float dodgeTimer = 0f;

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

        actionTimer = ACTION_COOLDOWN;
        dodgeTimer = DODGE_COOLDOWN;
    }

    void Update()
    {
        if (isDead) return;

        actionTimer += Time.deltaTime;
        dodgeTimer += Time.deltaTime;

        isCollideWithCharacter = chCllChecker.isCollideWithCharacter && !isDead;

        if (cur_hp <= 0)
            Die();
    }

    public bool CanMove()
    {
        return state == PlayerState.Idle || state == PlayerState.Moving;
    }
    public void HandleMovementInput(float hAxis, float vAxis)
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

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
        return actionTimer > ACTION_COOLDOWN && stateCheck;
    }
    public void StartAttack()
    {
        state = PlayerState.Attacking;
        anim.SetTrigger("doAttack");
        sword.use();
        Invoke(nameof(EndAttack), sword.activationTime + 0.4f);
    }
    public void EndAttack()
    {
        state = PlayerState.Idle;
        actionTimer = 0f;
    }

    public bool CanDefence()
    {
        bool stateCheck = state == PlayerState.Idle || state == PlayerState.Moving;
        return actionTimer > ACTION_COOLDOWN && stateCheck;
    }
    public void StartDefence()
    {
        state = PlayerState.Defending;
        isBlocking = true;
        anim.SetTrigger("doDefence");
        shld.use();
        Invoke(nameof(EndDefence), shld.activationTime + 1.1f);
    }
    public void EndDefence()
    {
        isBlocking = false;
        anim.SetTrigger("releaseDefence");
        state = PlayerState.Idle;
        actionTimer = 0f;
    }

    public bool CanDodge()
    {
        bool stateCheck = state == PlayerState.Idle || state == PlayerState.Moving;
        return actionTimer > ACTION_COOLDOWN && stateCheck;
    }
    public void StartDodge()
    {
        state = PlayerState.Dodging;
        isDodging = true;
        dodgeVec = transform.forward;
        anim.SetTrigger("doDodge");
        StartCoroutine(DodgeMove());
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
        dodgeTimer = 0f;
    }

    void Die()
    {
        state = PlayerState.Dead;
        isDead = true;
        anim.SetBool("isDead",true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Weapon") && !isDead)
        {
            if (isDodging)
                dodgeCounter++;
            else if (isBlocking)
                blockCounter++;
            else
            {
                Weapon wpn = other.GetComponent<Weapon>();
                cur_hp -= wpn.damage;
                anim.SetTrigger("hit");
                Debug.Log("HIT!!");
            }
        }
     }
}
