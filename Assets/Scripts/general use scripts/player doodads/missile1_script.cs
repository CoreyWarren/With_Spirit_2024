using System.Collections;
using System.Collections.Generic;
// PLAYER MISSILE //
// PLAYER MISSILE //
// PLAYER MISSILE //
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class missile1_script : MonoBehaviour
{
    // --- Original Public/Private Fields ---
    public Transform target;

    private Rigidbody2D rb;
    private SpriteRenderer srr;

    [SerializeField] private float startSpeed = 3f;
    private float speed;

    // Two separate rotation speeds:
    [SerializeField] private float seekRotateSpeed = 2f;     // how quickly we lerp toward the target angle
    [SerializeField] private float wobbleRotateSpeed = 2f;   // how strongly wobble influences rotation

    GameObject closest;

    public bool newTarget;
    public bool longdeath;
    public float longdeathtime = 20;

    public float lifeTimerMax;
    public float lifeTimer;

    public Transform hitcheck;
    public float hitcheckradius;
    public LayerMask whatishit;
    private bool hit = false;

    private float hitTimer;
    private float hitTimerMax = 20;
    private bool hitDeath = false;

    public GameObject mexplo1;
    private GameObject explosion;

    private AudioSource audio1;
    public AudioClip seeking;

    // Random wobble
    private float currentWobbleFrequency;
    private float currentWobbleAmplitude;
    private float wobblePhaseOffset;

    // Wobble & seeking
    [SerializeField, Range(1f, 20f)]
    private float trackingDistance = 10f;

    [SerializeField, Range(0.1f, 3f)]
    private float wobbleFrequencyMin = 0.5f;

    [SerializeField, Range(0.1f, 3f)]
    private float wobbleFrequencyMax = 2.0f;

    [SerializeField, Range(0.01f, 300f)]
    private float wobbleAmplitudeMin = 0.5f;

    [SerializeField, Range(0.1f, 300f)]
    private float wobbleAmplitudeMax = 2.0f;

    // Speeds
    [SerializeField, Range(1f, 20f)]
    private float maxDirectionalSpeed = 10f;

    [SerializeField, Range(0f, 1f)]
    private float speedIncreaseMultiplier = 0.05f;

    [SerializeField, Range(0f, 2f)]
    private float slowDownRate = 0.5f;

    // --- Start ---
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        srr = GetComponent<SpriteRenderer>();
        audio1 = GetComponent<AudioSource>();

        lifeTimer = lifeTimerMax + Random.Range(-20, 20);
        hitTimer = hitTimerMax;
        newTarget = false;

        audio1.pitch += Random.Range(-0.3f, 0.3f);
        audio1.PlayOneShot(seeking);
        longdeath = false;

        speed = startSpeed;

        // Assign random wobble values
        currentWobbleFrequency = Random.Range(wobbleFrequencyMin, wobbleFrequencyMax);
        currentWobbleAmplitude = Random.Range(wobbleAmplitudeMin, wobbleAmplitudeMax);

        // Random phase so they're not all in sync
        wobblePhaseOffset = Random.Range(0f, 1000f);
    }

    void Update()
    {
        if (!pauser1.paused)
        {
            CheckIfAnyEnemiesExist();
            CheckForHit();
            AcquireClosestTarget();
            CheckForDeath();

            // Only increase speed if there's a target
            if (target != null)
            {
                HandleSpeedAcceleration();
            }
        }
    }

    // Run physics-related movement in FixedUpdate, using Time.fixedDeltaTime
    void FixedUpdate()
    {
        if (!pauser1.paused)
        {
            SeekAndMove(); // includes wobble
        }
    }

    // --- Unchanged original methods ---

    GameObject FindEnemy()
    {
        GameObject go = GameObject.FindGameObjectWithTag("Enemy");
        return go;
    }

    GameObject FindClosestEnemy()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("Enemy");
        float distance = Mathf.Infinity;
        Vector3 position = transform.position;
        foreach (GameObject go in gos)
        {
            Vector3 diff = go.transform.position - position;
            float curDistance = diff.sqrMagnitude;
            if (curDistance < distance)
            {
                closest = go;
                distance = curDistance;
            }
        }
        return closest;
    }

    void CheckIfAnyEnemiesExist()
    {
        if (!FindEnemy())
        {
            explosion = Instantiate(mexplo1, transform.position, Quaternion.identity);
            Destroy(gameObject);
            PlayerController.soulcount++;
        }
    }

    void CheckForHit()
    {
        hit = Physics2D.OverlapCircle(hitcheck.position, hitcheckradius, whatishit);
        if (hit && !hitDeath)
        {
            speed = 0;
            hitDeath = true;
            srr.color = new Color(0f, 0f, 0f, 0f);
            explosion = Instantiate(mexplo1, transform.position, Quaternion.identity);
        }

        if (hitDeath)
        {
            hitTimer--;
            if (hitTimer <= 0)
            {
                gameObject.layer = 0;
                Destroy(gameObject);
            }
        }
    }

    void AcquireClosestTarget()
    {
        if (FindClosestEnemy() != null)
        {
            target = FindClosestEnemy().transform;
        }
        else
        {
            target = null;
        }
    }

    void CheckForDeath()
    {
        if (lifeTimer <= 0 && !longdeath)
        {
            speed = 0;
            srr.color = new Color(0f, 0f, 0f, 0f);
            explosion = Instantiate(mexplo1, transform.position, Quaternion.identity);
            longdeath = true;
            lifeTimer = longdeathtime;
        }

        if (longdeath && lifeTimer <= 0)
        {
            Destroy(gameObject);
        }

        lifeTimer--;
    }

    void HandleSpeedAcceleration()
    {
        if (speed < maxDirectionalSpeed)
        {
            speed += speed * speedIncreaseMultiplier;
        }
    }

    // --- The new SeekAndMove using fixedDeltaTime ---
    void SeekAndMove()
    {
        // 1) If we have a target in range, LERP from current rotation to target angle
        if (target != null)
        {
            float dist = Vector2.Distance(target.position, transform.position);
            if (dist <= trackingDistance)
            {
                // This is why we don't instantly snap:
                // We're using Mathf.LerpAngle from the missile's current angle
                // to the target angle, scaled by a small seekRotateSpeed.
                // The smaller "seekRotateSpeed," the slower it turns => no snap.

                // (a) Determine angle toward target
                Vector2 dir = (target.position - transform.position).normalized;
                float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

                // (b) Lerp toward that angle
                float lerpedAngle = Mathf.LerpAngle(rb.rotation, targetAngle, seekRotateSpeed * Time.fixedDeltaTime);

                // (c) Add the wobble on top
                float wobble = GetWobbleAngle() * wobbleRotateSpeed * Time.fixedDeltaTime;
                float finalAngle = lerpedAngle + wobble;

                rb.rotation = finalAngle;

                // Move forward
                rb.velocity = transform.up * speed;
                return;
            }
        }

        // 2) No target => slow down & do gentle wobble around the current direction
        speed -= slowDownRate * Time.fixedDeltaTime;
        if (speed < 0f) speed = 0f;

        // Only the wobble offset
        float noTargetWobble = GetWobbleAngle() * wobbleRotateSpeed * Time.fixedDeltaTime;
        rb.rotation += noTargetWobble;

        rb.velocity = transform.up * speed;
    }

    // --- Wobble helper ---
    float GetWobbleAngle()
    {
        // Rotational offset from a sine wave + random phase
        return Mathf.Sin((Time.time + wobblePhaseOffset) * currentWobbleFrequency) * currentWobbleAmplitude;
    }
}
