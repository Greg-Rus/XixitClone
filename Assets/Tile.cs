using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public TileColor TileColor;
    public Vector2Int GridPosition;

    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>(); //Can safely do that because RequireComponent ensures there is something to get
    }

    public Sprite Sprite
    {
        set { _spriteRenderer.sprite = value; }
    }

    public void DeSpawn()
    {
        Destroy(gameObject);
    }

    public void SetLocalPosition(Vector3 newLocalPosition)
    {
        transform.localPosition = newLocalPosition;
    }

    public void SetPosition(Vector2Int newPosition)
    {
        transform.position = new Vector3(newPosition.x, newPosition.y, 0f);
        GridPosition = newPosition;
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent, false);
    }

    public void DEBUG_SetColor(Color color)
    {
        _spriteRenderer.color = color;
    }
}
