using UnityEngine;
using UnityEngine.UI;

public class Radio : MonoBehaviour
{
    public float volume;

    void Start() {
        DontDestroyOnLoad(this.gameObject);
    }

    public void Awake()
    {
        GameObject.Find("VolumeScroll").GetComponent<Scrollbar>().value = volume;
    }
}
