using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class DeathManager : MonoBehaviour
{
    public GameObject player;
    public GameObject map;
    public float deathTimer = 2f;
    public int lives;
    public TMP_Text lifeText;
    public GameObject gameOverUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        lifeText.text = lives.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddLife()
    {
        lives += 1;
        lifeText.text = lives.ToString();
    }

    public void OnPlayerDeath()
    {
        lives -= 1;
        lifeText.text = lives.ToString();
        if(lives > 0)
        {
            player.GetComponent<PlayerController>().OnDeath();
        }
        else 
        {
            gameOverUI.SetActive(true);
            Time.timeScale = 0;
        }
        GetComponent<EnemyManager>().furnitureTimer = 0;
    }
    
    public void StartPlayerDeathCoroutine()
    {
        StartCoroutine(PlayerDeathCoroutine());
    }

    public IEnumerator PlayerDeathCoroutine()
    {
        if (player != null)
        {
            player.SetActive(false);
            yield return new WaitForSeconds(deathTimer);
            if (map != null)
            {
                var mapHandler = map.GetComponent<MapHandler>();
                if (mapHandler != null && mapHandler.topBuilding != null)
                {
                    var pc = player.GetComponent<PlayerController>();
                    if (pc != null)
                    {
                        pc.currentBuilding = mapHandler.topBuilding;
                        pc.targetBuilding = null;
                    }
                    player.transform.position = mapHandler.topBuilding.transform.position;
                    
                }
            }
            player.SetActive(true);
            player.GetComponent<JumpController>().isJumping = false;
        }
    }

    public void Restart() 
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
