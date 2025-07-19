using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class DeathManager : MonoBehaviour
{
    public GameObject player;
    public GameObject map;
    public float deathTimer = 2f;
    public int lives;
    public Image[] lifeSprites;
    public GameObject gameOverUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPlayerDeath()
    {
        Debug.Log(lives);
        lives -= 1;
        lifeSprites[lives].enabled = false;
        if(lives > 0)
        {
            StartCoroutine(PlayerDeathCoroutine());
            player.GetComponent<PlayerController>().OnDeath();
        }
        else 
        {
            gameOverUI.SetActive(true);
            Time.timeScale = 0;
        }
    }

    private IEnumerator PlayerDeathCoroutine()
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
