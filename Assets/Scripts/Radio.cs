using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Radio : MonoBehaviour
{
    public float volume;

    void Awake() 
    {
        DontDestroyOnLoad(this.gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        GameObject.Find("Radio").GetComponent<AudioSource>().volume = volume;
        GameObject.Find("VolumeScroll").GetComponent<Scrollbar>().value = volume;
    }

    public void Update()
    {
        volume = GameObject.Find("Radio").GetComponent<AudioSource>().volume;
    }
}
