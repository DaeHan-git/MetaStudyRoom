using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonDownImageChange : MonoBehaviour, IPointerDownHandler
{
    public Image targetImage; // 이미지가 바뀔 다른 버튼(또는 이미지)
    public Sprite nextSprite; // 바꿀 스프라이트

    public void OnPointerDown(PointerEventData eventData)
    {
        if (targetImage != null && nextSprite != null)
        {
            targetImage.sprite = nextSprite;
        }
    }
}