using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// Sword Mechanics Script
// This script controls the behavior of the sword entity, including:
// 1. **Charging Phases (Stage 1 - 4):** The sword goes through different charge levels (low, medium, high power).
// 2. **Swing States:** Tracks whether the sword is swinging and at what power level.
// 3. **Player Interaction:**
//    - Updates the `PlayerController` about the sword's current state (e.g., charging, swinging).
//    - Custom behaviors, like stalling the player mid-air, can be tied to swing events based on charge level.
// 4. **Visuals and Effects:** Includes sprite updates, trail rendering, and rotation for the sword.
// 5. **Audio:** Plays sound effects for charging and swinging based on the stage.
// 6. **Energy Management:** Reduces player energy when performing high-power actions.


public class sword1_1_script : MonoBehaviour {

    Player_Stats_Storage playerStats;

    //Charge Stages
    private bool stage1, stage2, stage3, stage4;
    public static int swordStage;
    private bool stage1Faster;
    private bool endSwing;
    //Swings
    private bool swing2, swing3, swing4;

    public float stage2Wait = 10f, stage3Wait = 80f, stage4Wait = 0f;
    private float stage2Waiter, stage3Waiter, stage4Waiter;

    private SpriteRenderer srr;
    public Sprite swingSprite1;
    public Sprite chargeSprite1;

    //Audio
    private AudioSource as1;
    public AudioClip charge1;
    public AudioClip charge2;
    public AudioClip charge3;
    public AudioClip charge4;
    public AudioClip swing2Sound;
    public AudioClip swing3Sound;

    //Rotation
    public float smooth = 12f;
    private float time;
    Transform player;
    public float radius;
    private float stage2Rotate;

    //Energy Cost
    private GameObject playerObject;
    [SerializeField]
    private int energyCost = 0;
    [SerializeField]
    private float deltaSpeed = 100f;
    private bool energyReduced;

    //Scale
    private Vector2 defaultScale;



    private bool rightsword;

    private bool paused;
    private bool pauseRelease;

    //Trail
    [SerializeField]
    private GameObject trail;
    [SerializeField]
    private float trailPeriod = 0.03f, trailFadeTime = 3f, trailOpacityPercent = 30f;
    private float trailTimer;

    private TrailRenderer myTRR;

    [SerializeField]
    private float trailWidthMultiplier1 = 0.7f, trailWidthMultiplier2 = 1.5f, trailWidthMultiplier3 = 2f;

    

    //Functions//
    void Awake()
    {
        if (transform.rotation == Quaternion.Euler(0, 0, 181))
        {
            rightsword = true;
        }
        else
        {
            rightsword = false;
        }
    }






	void Start () {
        player = GameObject.FindWithTag("Player").transform;
        playerStats = GameObject.FindWithTag("Event System").GetComponent<Player_Stats_Storage>();
        playerObject = GameObject.FindWithTag("Player");
        stage1 = true;
        stage2 = false; stage3 = false; stage4 = false;
        swing3 = false;
        as1 = GetComponent<AudioSource>();
        stage2Waiter = stage2Wait;
        stage3Waiter = stage3Wait;
        stage4Waiter = stage4Wait;
        srr = GetComponent<SpriteRenderer>();
        radius = 0f;
        defaultScale = transform.localScale;
        endSwing = false;
        time = 0f;
        transform.localScale = new Vector2(2f, 1f);

        pauseRelease = false;

        stage2Rotate = 1f;
        energyReduced = false;

        myTRR = GetComponent<TrailRenderer>();
    }
	


	


	void Update () {
        if (pauser1.paused == false)
        {
           
            ////STAGE 1////
            ////STAGE 1////
            if (stage1 && !stage2)
            {
                myTRR.widthMultiplier = trailWidthMultiplier1;
                srr.sprite = swingSprite1;

                float timeRad = (Mathf.PI / 180) * (time + (90 / smooth)) * smooth;

                timeRad -= Mathf.PI;

                if (!stage1Faster)
                {
                    radius += 0.1f;
                }
                




                if (rightsword)
                {
                    transform.rotation = Quaternion.Euler(0, 0, ((time) * smooth) + 180);
                    //transform.position = new Vector2(player.position.x + 2.5f * radius * Mathf.Cos(timeRad), player.position.y + radius * Mathf.Sin(timeRad));
                    transform.position = Vector2.MoveTowards(this.transform.position, 
                                                    new Vector2(player.position.x + 2.5f * radius * Mathf.Cos(timeRad), player.position.y + radius * Mathf.Sin(timeRad)),
                                                    deltaSpeed * Time.deltaTime);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 0, -((time) * smooth) - 180);
                    //transform.position = new Vector2(player.position.x - 2.5f * radius * Mathf.Cos(timeRad), player.position.y + radius * Mathf.Sin(timeRad));
                    transform.position = Vector2.MoveTowards(this.transform.position,
                                                    new Vector2(player.position.x - 2.5f * radius * Mathf.Cos(timeRad), player.position.y + radius * Mathf.Sin(timeRad)),
                                                    deltaSpeed * Time.deltaTime);
                }




                if(Input.GetKeyUp("x") || pauseRelease)
                {
                    stage1Faster = true;

                }



                //Allow Stage 2 to activate, or Destroy
                if (time < (360 / smooth) / 1.75f)
                {
                    if (!stage1Faster)
                    { time++; }
                    else
                    { time += 2;
                      radius += 0.15f;
                    }
                }
                else
                {
                    if (Input.GetKey("x") && !stage1Faster)
                    {
                        stage2 = true;
                        myTRR.enabled = false;
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }





            }




            ////STAGE 2////
            ////STAGE 2////
            if (stage2 && !stage3)
            {
                radius = 1f;
                float timeRad = (Mathf.PI / 180) * (time + (90 / smooth)) * smooth;
                timeRad += Mathf.PI;
                gameObject.layer = 0;
                srr.sprite = chargeSprite1;
                myTRR.widthMultiplier = trailWidthMultiplier2;

                if (stage1)
                {
                    srr.sprite = null;




                    if (stage2Waiter > 0)
                    {
                        stage2Waiter--;
                    }
                    else
                    {
                        as1.PlayOneShot(charge1);
                        stage1 = false;
                        transform.localScale = new Vector2(0f, 0f);
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                        stage3Waiter = stage3Wait;
                    }

                }

                if (!stage1)
                { 
                    if (PlayerController.faceright)
                    {
                        //transform.position = new Vector2(player.position.x + 1f, player.position.y);
                        transform.position = Vector2.MoveTowards(this.transform.position,
                                                                new Vector2(player.position.x + 1f, player.position.y),
                                                                deltaSpeed * Time.deltaTime);
                    }
                    else
                    {
                        transform.position = new Vector2(player.position.x - 1f, player.position.y);
                    }
                    stage2Rotate += 0.2f;
                    transform.Rotate(0f, 0f, stage2Rotate);
                    transform.localScale = new Vector2(transform.localScale.x + 0.07f, transform.localScale.y + 0.07f);




                    if (Input.GetKey("x"))
                    {
                        if (stage3Waiter > 0)
                        {
                            stage3Waiter--;
                        }
                        else
                        {
                            stage3 = true;
                        }
                        
                    }
                    
                }

                if (Input.GetKeyUp("x") || pauseRelease)
                {

                    if (stage1)
                    {
                        //Cancel Sword Charge, do nothing. 
                        Destroy(gameObject);
                    }
                    else if (!endSwing)
                    {
                        //Allow Swing2 to start
                        swing2 = true;
                        time = 0;
                        radius = 0f;
                        as1.Stop();
                        as1.pitch += 0.4f;
                        as1.PlayOneShot(swing2Sound);
                        transform.localScale = new Vector2(defaultScale.x*2, defaultScale.y * 1.5f);
                        gameObject.layer = 19;
                        if (PlayerController.faceright)
                        {
                            rightsword = true;
                        }else
                        {
                            rightsword = false;
                        }
                        endSwing = true;
                    }
                    


                }
            }


           
                if(swing2)
                {
                    if (!myTRR.enabled)
                    {
                        myTRR.enabled = true;
                    }
                    srr.color = Color.red;
                    float timeRad = (Mathf.PI / 180) * (time + (90 / smooth)) * smooth;
                    timeRad -= Mathf.PI;
                    gameObject.layer = 19;
                    if(radius < 1f) { radius += 0.4f; }
                    srr.sprite = swingSprite1;

                if (rightsword)
                    {
                        transform.rotation = Quaternion.Euler(0, 0, ((time) * smooth) + 180);
                        transform.position = new Vector2(player.position.x + 2.5f * radius * Mathf.Cos(timeRad), player.position.y + radius * Mathf.Sin(timeRad));
                    }
                    else
                    {
                        transform.rotation = Quaternion.Euler(0, 0, -((time) * smooth) - 180);
                        transform.position = new Vector2(player.position.x - 2.5f * radius * Mathf.Cos(timeRad), player.position.y + radius * Mathf.Sin(timeRad));
                    }

                    if(time < (360 / smooth) * 1.3f)
                    {
                        time += 1.5f;
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }





            ////STAGE 3////
            ////STAGE 3////
            if (stage3 && !swing3 && !swing2)
            {
                if(!myTRR.enabled)
                {
                    myTRR.enabled = true;
                }
                srr.sprite = chargeSprite1;
                myTRR.widthMultiplier = trailWidthMultiplier3;

                radius = 1f;
                float timeRad = (Mathf.PI / 180) * (time + (90 / smooth)) * smooth;
                timeRad += Mathf.PI;
                gameObject.layer = 0;

                if (stage2)
                {
                    srr.sprite = null;

                    
                    if (stage3Waiter > 0)
                    {
                        stage3Waiter--;
                    }
                    else
                    {
                        as1.Stop();
                        as1.pitch += 0.1f;
                        as1.PlayOneShot(charge2);
                        stage2 = false;
                        transform.localScale = new Vector2(defaultScale.x * 3.5f, defaultScale.y * 3.5f);
                        srr.color = Color.yellow;
                        transform.rotation = Quaternion.Euler(0, 0, 0);
                    }

                } else
                if (!stage2)
                {
                    if (PlayerController.faceright)
                    {
                        transform.position = new Vector2(player.position.x + 1f, player.position.y);
                    }
                    else
                    {
                        transform.position = new Vector2(player.position.x - 1f, player.position.y);
                    }
                    transform.Rotate(0f, 0f, 1f);
                    
                }

                if (Input.GetKeyUp("x") || pauseRelease)
                {

                    if (stage2)
                    {
                        //Cancel Sword Charge, do nothing. 
                        Destroy(gameObject);
                    }
                    else
                    {
                        //Allow Swing2 to start
                        swing3 = true;
                        time = 0;
                        radius = 1f;
                        as1.Stop();
                        as1.pitch += 0.6f;
                        as1.PlayOneShot(swing3Sound);
                        transform.localScale = new Vector2(defaultScale.x * 4f, defaultScale.y * 3f);
                        gameObject.layer = 19;
                        if (PlayerController.faceright)
                        {
                            rightsword = true;
                        }
                        else
                        {
                            rightsword = false;
                        }
                    }

                }
            }



            if (swing3)
            {
                srr.sprite = swingSprite1;
                srr.color = Color.white;
                float timeRad = (Mathf.PI / 180) * (time + (90 / smooth)) * smooth;
                timeRad -= Mathf.PI;
                //if (radius < 1f) { radius += 0.1f; }
                if(!energyReduced)
                {
                    int playerEnergy = playerStats.playerEnergy;
                    if (playerEnergy >= energyCost)
                    {
                        playerStats.playerEnergy -= energyCost;
                        energyReduced = true;
                    }else
                    {
                        Destroy(gameObject);
                    }
                    
                }

                if (rightsword)
                {
                    transform.rotation = Quaternion.Euler(0, 0, ((time) * smooth) + 180);
                    transform.position = new Vector2(player.position.x + 2.5f * radius * Mathf.Cos(timeRad), player.position.y + radius * Mathf.Sin(timeRad));
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 0, -((time) * smooth) - 180);
                    transform.position = new Vector2(player.position.x - 2.5f * radius * Mathf.Cos(timeRad), player.position.y + radius * Mathf.Sin(timeRad));
                }

                if (time < (360 / smooth) * 3f)
                {
                    time += 1.75f;
                }
                else
                {
                    Destroy(gameObject);
                }
            }


           







            //Trail
            if(trailTimer > 0)
            {
                trailTimer -= Time.deltaTime * 10;
            }else
            {
                GameObject newTrail = Instantiate(trail, transform.position, transform.rotation);
                newTrail.GetComponent<faderScript>().fadeTime = trailFadeTime;
                newTrail.GetComponent<faderScript>().usingOpacityPercent = true;
                newTrail.GetComponent<SpriteRenderer>().sprite = srr.sprite;
                newTrail.GetComponent<SpriteRenderer>().color = srr.color;
                newTrail.transform.localScale = this.transform.localScale;
                newTrail.GetComponent<faderScript>().opacityPercent = trailOpacityPercent;
                trailTimer = trailPeriod;
            }









            //Global Sword Stage checker:
            if(stage1)
            {
                swordStage = 1;
            }else if(stage2)
            {
                swordStage = 2;
            }else if(stage3)
            {
                swordStage = 3;
            }else if(stage4)
            {
                swordStage = 4;
            }
            

        } //end of Pauser Check
        else
        {
            paused = true;
            if (Input.GetKeyUp("x"))
            {
                pauseRelease = true;
            }
        }

    }//end of Update










}
