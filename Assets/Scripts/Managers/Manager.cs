﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [SerializeField] private GameObject ball;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject soundWave;
    private const int MAX_SCORE = 10;
   [SerializeField] int resetDelay;

    [SerializeField]
    private GameStates state;
    public List<Vector3> spawnPoints;
    public static Manager manager;
    public KeyCode[] keyCodes;
    public List<GameObject> PlayerObjects { get; set; }
    public GameObject Ball { get; set; }
    public Ball BallScript { get; set; }

    [SerializeField]
    private List<int> playerScores;
	
	private void Awake () {
		DontDestroyOnLoad(gameObject);

        manager = this;
		PlayerObjects = new List<GameObject>();
        playerScores = new List<int> { 0,0 };
        spawnPoints = new List<Vector3>();
		
		InitializeGame();
	}

    public void SpawnLevel1()
    {
        state = GameStates.Initializing;
        StartCoroutine(PopulateLevel1(GameStates.Introduction));
        SceneManager.LoadScene("GameScene");
    }


    public void RestartLevel1()
    {
        state = GameStates.Initializing;
        StartCoroutine(PopulateLevel1(GameStates.Restarting));
        SceneManager.LoadScene("GameScene");
    }

    private IEnumerator PopulateLevel1(GameStates newState)
    {
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "GameScene");
        state = newState;

        if (PlayerObjects.Count > 0)
        {
            PlayerObjects = new List<GameObject>();
            spawnPoints = new List<Vector3>();
        }

        GameObject player1 = GameObject.Instantiate(player);
        GameObject player2 = GameObject.Instantiate(player);
        if (newState == GameStates.Introduction)
        {
            player2.GetComponent<PlayerMechanics>().camera.GetComponent<CameraFollower>().enabled = false;
            player1.GetComponent<PlayerMechanics>().camera.GetComponent<CameraFollower>().enabled = false;
            GameObject.Find("CameraRailPlayer1").GetComponent<CameraRailScript>().cam = player1.GetComponent<PlayerMechanics>().camera.GetComponent<Camera>();
            GameObject.Find("CameraRailPlayer2").GetComponent<CameraRailScript>().cam = player2.GetComponent<PlayerMechanics>().camera.GetComponent<Camera>();
        }
        else
        {
            player2.GetComponent<PlayerMechanics>().camera.GetComponent<CameraFollower>().enabled = true;
            player1.GetComponent<PlayerMechanics>().camera.GetComponent<CameraFollower>().enabled = true;
            GameObject.Find("CameraRailPlayer1").GetComponent<CameraRailScript>().enabled = false;
            GameObject.Find("CameraRailPlayer2").GetComponent<CameraRailScript>().enabled = false;
        }
        PlayerObjects.Add(player1);
        PlayerObjects.Add(player2);

        PlayerObjects[0].GetComponent<PlayerMechanics>().id = 0;
        PlayerObjects[1].GetComponent<PlayerMechanics>().id = 1;
        PlayerObjects[0].GetComponent<PlayerMechanics>().CustomStart();
        PlayerObjects[1].GetComponent<PlayerMechanics>().CustomStart();

        Ball = GameObject.Find("Ball");
        Ball.GetComponent<Rigidbody>().isKinematic = true;
        int random = (int)Mathf.Round(Random.Range(0, 2));
        spawnPoints.Add(GameObject.Find("Ball Spawn 1").transform.position);
        spawnPoints.Add(GameObject.Find("Ball Spawn 2").transform.position);
        Ball.transform.position = spawnPoints[random];
        BallScript.PlayerY = player.transform.position.y;
        if (newState != GameStates.Introduction)
        {
            StartCoroutine(StartLevel1());
            yield return new WaitForSeconds(Manager.manager.ResetDelay);
        }
       
    }

    public IEnumerator StartLevel1()
    {
        GameObject.Find("CameraRailPlayer1").GetComponent<CameraRailScript>().enabled = false;
        GameObject.Find("CameraRailPlayer2").GetComponent<CameraRailScript>().enabled = false;

        yield return new WaitForSeconds(1);
        PlayerObjects[0].GetComponent<PlayerMechanics>().camera.GetComponent<CameraFollower>().enabled = true;
        PlayerObjects[1].GetComponent<PlayerMechanics>().camera.GetComponent<CameraFollower>().enabled = true;

        yield return new WaitForSeconds(1);
        state = GameStates.Playing;
        Ball.GetComponent<Rigidbody>().isKinematic = false;
        Ball.GetComponent<Rigidbody>().AddForce(Vector3.down * 3f, ForceMode.Impulse);
    }

    private void InitializeGame()
	{
		state = GameStates.Playing;
		SceneManager.LoadScene("MenuScene");
	}

    public void AddScore(int shooterType)
    {
        GameObject.Destroy(Ball);
        // Player who shot the ball, wins
        foreach (GameObject player in PlayerObjects)
        {
            PlayerMechanics playerMech = player.GetComponent<PlayerMechanics>();
            if (playerMech.Id == shooterType)
            {
                // Winner
                Debug.Log(playerMech.Id + " won the game!");
                StartCoroutine(FinishMatch(playerMech.Id));
            }
            else
            {
                // Loser
            }
        }
    }

    private IEnumerator FinishMatch(int winnerId)
    {
        if (state == GameStates.Finishing)
        {
            yield break;
        }

        state = GameStates.Finishing;

        PlayerScores[winnerId] += 1;
        if (PlayerScores[winnerId] >= MAX_SCORE)
        {
            state = GameStates.Ended;
            if (winnerId == 0)
            {
                GameObject.FindGameObjectWithTag("LoserImage").transform.position = new Vector3(Screen.width / 2, Screen.height * 0.75f, 0);
                GameObject.FindGameObjectWithTag("WinnerImage").transform.position = new Vector3(Screen.width / 2, Screen.height * 0.25f, 0);
            }
            else
            {
                GameObject.FindGameObjectWithTag("LoserImage").transform.position = new Vector3(Screen.width / 2, Screen.height * 0.25f, 0);
                GameObject.FindGameObjectWithTag("WinnerImage").transform.position = new Vector3(Screen.width / 2, Screen.height * 0.75f, 0);
            }
            GameObject.FindGameObjectWithTag("LoserImage").GetComponent<Image>().enabled = true;
            GameObject.FindGameObjectWithTag("WinnerImage").GetComponent<Image>().enabled = true;
            yield return new WaitForSeconds(15);
            SpawnLevel1();
            //SceneManager.LoadScene("EndScreen");
        }
        else
        {
            //Reset level
            RestartLevel1();
        }
    }
    private void Update()
    {
        if (state == GameStates.Ended)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StopAllCoroutines();
                SpawnLevel1();
            }
        }
    }

    public List<int> PlayerScores
    {
        get { return playerScores; }
        set
        {
            playerScores = value;
        }
    }

    public int ResetDelay
    {
        get { return resetDelay; }
    }

	public GameStates State
	{
		get { return state; }
		set { state = value; }
	}

    public GameObject SoundWave
    {
        get { return soundWave; }
    }
}
