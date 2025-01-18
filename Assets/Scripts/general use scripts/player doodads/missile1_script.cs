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
    SpriteRenderer srr;

    [SerializeField] private float startSpeed = 3f;

    private float speed;
    private float rotateSpeed;

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
    GameObject explosion;

    private AudioSource audio1;
    public AudioClip seeking;

    // Actual random wobble values per missile
    private float currentWobbleFrequency;
    private float currentWobbleAmplitude;

    // Wobble & seeking
    [SerializeField, Range(1f, 20f)]
    private float trackingDistance = 10f;

    [SerializeField, Range(0.1f, 3f)]
    private float wobbleFrequencyMin = 0.5f;

    [SerializeField, Range(0.1f, 3f)]
    private float wobbleFrequencyMax = 2.0f;

    [SerializeField, Range(0.1f, 3f)]
    private float wobbleAmplitudeMin = 0.5f;

    [SerializeField, Range(0.1f, 3f)]
    private float wobbleAmplitudeMax = 2.0f;

    // Speeds & rotation
    [SerializeField, Range(1f, 20f)]
    private float maxDirectionalSpeed = 10f;

    [SerializeField, Range(50f, 1000f)]
    private float maxRotationalSpeed = 600f;

    [SerializeField, Range(0f, 1f)]
    private float speedIncreaseMultiplier = 0.05f;

    [SerializeField, Range(0f, 50f)]
    private float rotationSpeedIncrement = 2f;

    [SerializeField, Range(0f, 2f)]
    private float slowDownRate = 0.5f;


    private bool noTargetDirectionSet = false;
    private Vector2 savedDirection;


    // --- Original Functions (Unchanged) ---
    // (Keeping these exactly as requested.)
    GameObject FindEnemy()
    {
        GameObject go;
        go = GameObject.FindGameObjectWithTag("Enemy");
        return go;
    }

    GameObject FindClosestEnemy()
    {
        GameObject[] gos;
        gos = GameObject.FindGameObjectsWithTag("Enemy");
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

    // --- Start ---
    // We now get `rb` and `srr` here, so we removed the "rb = GetComponent<Rigidbody2D>()" line from Update().
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

        // Assign random wobble values for this missile
        currentWobbleFrequency = Random.Range(wobbleFrequencyMin, wobbleFrequencyMax);
        currentWobbleAmplitude = Random.Range(wobbleAmplitudeMin, wobbleAmplitudeMax);
    }

    // --- Update ---
    void Update()
    {
        if (pauser1.paused == false)
        {
            CheckIfAnyEnemiesExist();
            CheckForHit();
            AcquireClosestTarget();
            CheckForDeath();
            if (target!=null) HandleSpeedAndRotateAcceleration();
            SeekAndMove(); // includes wobble
        }
    }

    // --- Splits out your existing logic into smaller chunks. ---

    void CheckIfAnyEnemiesExist()
    {
        // If there are no enemies at all, we destroy ourselves immediately
        if (!FindEnemy())
        {
            explosion = Instantiate(mexplo1, transform.position, Quaternion.identity) as GameObject;
            Destroy(gameObject);
            PlayerController.soulcount++;
        }
    }

    void CheckForHit()
    {
        // OverlapCircle to see if we hit something
        hit = Physics2D.OverlapCircle(hitcheck.position, hitcheckradius, whatishit);

        if (hit && hitDeath == false)
        {
            speed = 0;
            hitDeath = true;
            srr.color = new Color(0f, 0f, 0f, 0f);
            explosion = Instantiate(mexplo1, transform.position, Quaternion.identity) as GameObject;
        }

        // If we've already hit, count down to final destruction
        if (hitDeath == true)
        {
            hitTimer--;
            if (hitTimer <= 0 && hitDeath == true)
            {
                gameObject.layer = 0;
                Destroy(gameObject);
            }
        }
    }

    void AcquireClosestTarget()
    {
        // Try finding the nearest enemy
        if (FindClosestEnemy() != null)
        {
            target = FindClosestEnemy().transform;
        }
    }

    void CheckForDeath()
    {
        // If lifeTimer expired but we haven't triggered longdeath yet
        if (lifeTimer <= 0 && longdeath == false)
        {
            speed = 0;
            srr.color = new Color(0f, 0f, 0f, 0f);
            explosion = Instantiate(mexplo1, transform.position, Quaternion.identity) as GameObject;
            longdeath = true;
            lifeTimer = longdeathtime;
        }

        // If we've been in the longdeath state and time is up
        if (longdeath == true && lifeTimer <= 0)
        {
            Destroy(gameObject);
        }

        lifeTimer--;
    }

    void HandleSpeedAndRotateAcceleration()
    {
        // Increase speed gradually
        if (speed < maxDirectionalSpeed)
            speed = speed + speed * speedIncreaseMultiplier;

        // Increase rotateSpeed gradually
        if (rotateSpeed < maxRotationalSpeed)
            rotateSpeed += rotationSpeedIncrement;
    }

    void SeekAndMove()
    {
        // If we have a valid target in range, steer toward it with wobble
        if (target != null)
        {
            float dist = Vector2.Distance(target.position, transform.position);
            if (dist <= trackingDistance)
            {
                Vector2 direction = (target.position - transform.position).normalized;
                float rotateAmount = Vector3.Cross(direction, transform.up).z;

                // Add wobble
                rotateAmount += GetWobbleOffset();

                // Manual rotation (multiply by Time.deltaTime or fixedDeltaTime to keep it smooth)
                float newRotation = rb.rotation - (rotateAmount * rotateSpeed * Time.fixedDeltaTime);
                rb.rotation = newRotation;

                // Move forward
                rb.velocity = transform.up * speed;
                return;
            }
        }

        // No target in range: slow down and only wobble
        // 1) Gradually reduce speed to 0
        speed -= slowDownRate * Time.fixedDeltaTime;
        if (speed < 0f) speed = 0f;

        // 2) Stop using rotateSpeed so you don't keep spinning
        //    Just rotate from wobble alone
        float wobble = GetWobbleOffset();
        rb.rotation += wobble * Time.fixedDeltaTime;

        // Move forward at the reduced speed in whatever direction we’re currently facing
        rb.velocity = transform.up * speed;
    }



    // --- Wobble helper ---
    float GetWobbleOffset()
    {
        // Rotational offset from a simple sine wave
        return Mathf.Sin(Time.time * currentWobbleFrequency) * currentWobbleAmplitude;
    }
}
