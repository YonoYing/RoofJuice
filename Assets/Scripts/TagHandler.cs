using UnityEngine;
using System.Linq;
using System.Collections;

public class TagHandler : MonoBehaviour
{
    public Material[] tags;
    public Material emptyTag;
    public Material puddleTag;
    public bool tagged, puddled;
    public GameObject smokePrefab;
    public float puddleClearDelay;

    void OnTriggerEnter(Collider other)
    {
        if(puddled)
        {
            GameObject.Find("GameManager").GetComponent<DeathManager>().OnPlayerDeath();
            return;
        }
        if(!tagged)
        {

            BuildingHandler buildingHandler = GetComponent<BuildingHandler>();
            var usedMaterials = buildingHandler.buildings
                .Select(b => b != null ? b.transform.Find("Tag") : null)
                .Where(tagChild => tagChild != null)
                .Select(tagChild => {
                    var renderer = tagChild.GetComponent<Renderer>();
                    return renderer != null ? renderer.sharedMaterial : null;
                })
                .Where(mat => mat != null)
                .ToList();

            var availableTags = tags.Where(tag => !usedMaterials.Contains(tag)).ToList();

            Material chosenTag = availableTags[Random.Range(0, availableTags.Count)];

            Transform tagChildTransform = transform.Find("Tag");
            Renderer tagRenderer = tagChildTransform.GetComponent<Renderer>();
            tagRenderer.material = chosenTag;

            Instantiate(smokePrefab, transform.position, Quaternion.Euler(0,90,-90));
            
            tagged = true;
            GameObject.Find("GameManager").GetComponent<LevelHandler>().CheckLevel();
        }
    }

    public void PuddleTag()
    {
        Transform tagChildTransform = transform.Find("Tag");
        Renderer tagRenderer = tagChildTransform.GetComponent<Renderer>();
        tagRenderer.material = puddleTag;
        puddled = true;
        tagged = false;
        StartCoroutine(ClearTag());
    }

    public IEnumerator ClearTag()
    {
        yield return new WaitForSeconds(puddleClearDelay);
        Transform tagChildTransform = transform.Find("Tag");
        Renderer tagRenderer = tagChildTransform.GetComponent<Renderer>();
        tagRenderer.material = emptyTag;
        puddled = false;
        tagged = false;
    }
}
