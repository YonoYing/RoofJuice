using UnityEngine;
using System.Linq;

public class TagHandler : MonoBehaviour
{
    public Material[] tags;
    public Material emptyTag;
    public bool tagged;

    void OnTriggerEnter(Collider other)
    {
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
            tagged = true;
            GameObject.Find("GameManager").GetComponent<LevelHandler>().CheckLevel();
        }
    }

    public void ClearTag()
    {
        if(tagged)
        {
            Transform tagChildTransform = transform.Find("Tag");
            Renderer tagRenderer = tagChildTransform.GetComponent<Renderer>();
            tagRenderer.material = emptyTag;
            tagged = false;
        }
    }
}
