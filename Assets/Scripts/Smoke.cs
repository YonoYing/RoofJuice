using UnityEngine;

public class Smoke : MonoBehaviour
{
    public Color[] colors;
    
    void Start()
    {
        Color color = colors[Random.Range(0, colors.Length)];
        GetComponent<Animator>().SetTrigger(Random.Range(0, 2) > 0 ? "Pink" : "Red");
        GetComponent<SpriteRenderer>().color = color;
    }

    public void Destroy() {
        Destroy(transform.parent.gameObject);
    }
}
