﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityStandardAssets.Effects;
using PostProcess;
using Valve.VR;

public class LevelSelect : MonoBehaviour {

    public GameObject PlayerVehicle;
    public GameObject Terrain;
    public GameObject RoadNetwork;

    public Transform Level1Start;
    public Transform Level2Start;

    public static int numberOfLevels = 4;

    [SerializeField] private GameObject[] levelObjects = new GameObject[numberOfLevels]; // Level 0: Game Start UI, Level 1: Day time, Level 2: Night time, Level 3: Ending

    [SerializeField] private Material[] levelSkybox = new Material[numberOfLevels]; // Level 0: Game Start UI, Level 1: Day time, Level 2: Night time, Level 3: Ending

    [SerializeField] public GameObject[] levelCameras = new GameObject[numberOfLevels]; // Level 0: Game Start UI, Level 1: Day time, Level 2: Night time, Level 3: Ending

    [SerializeField] public PostProcessingProfile[] postProcessingProfiles = new PostProcessingProfile[3];

    public PostProcessingProfile profile;

    public int Level = 0;

    public delegate void LevelChangedHandler(int level);
    public event LevelChangedHandler OnLevelChanged;

    public int FadeOutTime = 1;
    public int FadeInTime = 5;

    private CarFailureStates carStates;

    private bool HMDActive = false;

    // Use this for initialization
    void Start()
    {
        HMDActive = OpenVR.IsHmdPresent();
        StartCoroutine(SetLevel(Level, 0, false));
        carStates = PlayerVehicle.GetComponent<CarFailureStates>();
        carStates.OnCollision += PlayerVehicleCollisionHandler;
        carStates.OnLevelTriggerEntered += PlayerVehicleLevelChangeHandler;

        RenderSettings.ambientSkyColor = new Color32(54, 58, 66, 0);
        RenderSettings.ambientEquatorColor = new Color32(29, 32, 34, 0);
        RenderSettings.ambientGroundColor = new Color32(12, 11, 9, 0);
        RenderSettings.fog = true;
    }

    // Update is called once per frame
    void FixedUpdate () {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            StartCoroutine(SetLevel(0, 0, false));
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
           StartCoroutine(SetLevel(1, 0, false));
        }
        else if (Input.GetKeyDown(KeyCode.F3))
        {
            StartCoroutine(SetLevel(2, 3, false));
        }
        else if (Input.GetKeyDown(KeyCode.F4))
        {
            StartCoroutine(SetLevel(3, 0, false));
        }
    }

    public IEnumerator SetLevel(int level, int difficulty, bool delay)
    {
        if (delay && HMDActive)
        {
            SteamVR_Fade.View(Color.black, FadeOutTime);
            Invoke("FadeIn", FadeOutTime);
            yield return new WaitForSeconds(FadeOutTime);
        }

        EnableObjects(level, levelCameras);
        EnableObjects(level, levelObjects);
        SetSkybox(level);
        Level = level;
        CancelInvoke("BlinkRandomizer");
        CancelInvoke("Blink");
        switch (level)
        {
            case 0:
                PlayerVehicle.SetActive(false);
                Terrain.SetActive(false);
                RoadNetwork.SetActive(false);
                RenderSettings.fogDensity = 0;
                break;
            case 1:
                PlayerVehicle.SetActive(true);
                Terrain.SetActive(true);
                RoadNetwork.SetActive(true);
                SetVehiclePosition(Level1Start);
                RenderSettings.ambientIntensity = 0;
                RenderSettings.fogDensity = 0.0005f;
                break;
            case 2:
                PlayerVehicle.SetActive(true);
                Terrain.SetActive(true);
                RoadNetwork.SetActive(true);
                SetVehiclePosition(Level2Start);
                TriggerLevelTwoWithDifficulty(difficulty);
                RenderSettings.ambientIntensity = -2;
                RenderSettings.fogDensity = 0.005f;
                break;
            case 3:
                PlayerVehicle.SetActive(false);
                Terrain.SetActive(false);
                RoadNetwork.SetActive(false);
                RenderSettings.fogDensity = 0;
                break;
        }

        if (OnLevelChanged != null)
        {
            print("Level Changing Fire");
            OnLevelChanged(level);
        }
    }
    
    private void TriggerLevelTwoWithDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case 1: // easy
                break;
            case 2: // medium
                InvokeRepeating("BlinkRandomizer", 5, 14);
                break;
            case 3: // hard
                InvokeRepeating("BlinkRandomizer", 5, 7);
                break;
        }
    }

    private void EnableObjects(int level, GameObject[] objects)
    {
        if (objects[level] != null)
        {
            objects[level].SetActive(true); // We do this first to make sure we don't have weird camera flicker
        }
        
        for (int i = 0; i < numberOfLevels; i++)
        {
            if (i != level && objects[i] != null)
            {
                objects[i].SetActive(false);
            }
        }
    }

    private void SetVehiclePosition(Transform t)
    {
        PlayerVehicle.transform.eulerAngles = t.eulerAngles;
        PlayerVehicle.transform.position = t.transform.position;
        PlayerVehicle.GetComponent<Rigidbody>().velocity = Vector3.zero;
        PlayerVehicle.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
    }

    private void SetSkybox(int level)
    {
        if(levelSkybox[level] != null)
        {
            RenderSettings.skybox = levelSkybox[level];
        }
    }

    private void FadeIn()
    {
        SteamVR_Fade.View(Color.clear, FadeInTime);
    }

    private void PlayerVehicleCollisionHandler()
    {
        if (Level == 1)
        {
            StartCoroutine(SetLevel(1, 0, true));
        }
        else if (Level == 2)
        {
            StartCoroutine(SetLevel(3, 0, true));
        }
    }

    private void PlayerVehicleLevelChangeHandler(string levelTriggerName)
    {
        if (levelTriggerName == "Level2Trigger" && Level == 1)
        {
            StartCoroutine(DecelerateVehicle());  
            StartCoroutine(SetLevel(0, 0, true));
        }
        else if (levelTriggerName == "Level3Trigger" && Level == 2)
        {
            StartCoroutine(SetLevel(3, 0, true));
        }
    }

    void BlinkRandomizer()
    {
        Invoke("Blink", Random.Range(1, 3));
    }

    void Blink()
    {
        levelCameras[2].GetComponent<BlinkEffect>().Blink();
    }

    public IEnumerator DecelerateVehicle()
    {
        for (int i = 0; i < FadeOutTime; i++)
        {
            Rigidbody r = PlayerVehicle.GetComponent<Rigidbody>();
            r.velocity *= 1 / 2;
            yield return new WaitForSeconds(1);
        }
    }
}
