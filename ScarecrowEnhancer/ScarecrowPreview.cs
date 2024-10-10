
using UnityEngine;
using Wish;

namespace ScarecrowEnhancer;

public class ScarecrowPreview : MonoBehaviour
{
    private Scarecrow _scarecrow;
    private SpriteRenderer _preview;

    private void Start()
    {
        _scarecrow = gameObject.GetComponent<Scarecrow>();
        
        _preview = new GameObject("Scarecrow preview").AddComponent<SpriteRenderer>();
        _preview.sprite = Plugin.sprite;
        _preview.sortingOrder = -1;
        Transform transform1;
        (transform1 = _preview.transform).SetParent(_scarecrow.transform);
        transform1.localPosition = new Vector3(0, -4f, -4f);
        transform1.localScale = new Vector3(1f, 1f, 1f);
        transform1.eulerAngles = new Vector3(-45.0f, 0.0f, 0.0f);
        _preview.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f);
    }
    
    private void LateUpdate()
    {
        var pos = _scarecrow.RealCenter;
        pos.y -= pos.z;
        
        var distance = Vector2.Distance(Utilities.MousePositionExact(), pos);

        if (distance < 1 && !_preview.gameObject.activeSelf)
        {
            _preview.gameObject.SetActive(true);
        }
        else if (distance >= 1 && _preview.gameObject.activeSelf)
        {
            _preview.gameObject.SetActive(false);
        }
    }
}