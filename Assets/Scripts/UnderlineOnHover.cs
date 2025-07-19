using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UnderlineOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI textComponent;
    private string originalText;
    
    void Start()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();
        originalText = textComponent.text;
        
        // Make sure the Image is transparent
        Image image = GetComponent<Image>();
        image.color = new Color(0, 0, 0, 0);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Mouse entered!");
        textComponent.text = "<u>" + originalText + "</u>";
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("Mouse exited!");
        textComponent.text = originalText;
    }
}