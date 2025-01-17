using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player_Stats_Storage : MonoBehaviour
{

    [HideInInspector]
    public int playerSouls;
    [HideInInspector]
    public int playerEnergy;
    [HideInInspector]
    public int playerChosenWeapon, playerPowerMove;
    PlayerController playerScript;

    public static bool updatingStats;
    public bool giveStatsNow;
    [HideInInspector]
    public bool playerDied;

    [HideInInspector]
    public int playerHealth;
    
    public static int playerHealthMax = 3;
    [SerializeField]
    public static int playerEnergyMax = 600;

    public int currentWorld;
    public int checkpointNumber;

    void Awake()
    {
        playerSouls = 0;
        playerEnergy = 200;
        playerChosenWeapon = 1;
        updatingStats = false;

        giveStatsNow = false;

    }

    void Start()
    {
        playerHealth = 3;
        playerHealthMax = 3;
    }
    // Update is called once per frame
    void Update()
    {
        
        if(playerDied)
        {
            playerSouls = 0;
            playerHealth = 3;
            playerEnergy = 100;
            playerChosenWeapon = 0;
            int scene = SceneManager.GetActiveScene().buildIndex;
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
            playerDied = false;
        }

        if(playerChosenWeapon != canvas_weaponUI.selectedW)
        {
            playerChosenWeapon = canvas_weaponUI.selectedW;
        }
    }
}
