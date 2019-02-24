using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    public TileColor TileColor;
    public Vector2Int GridPosition;
    public ParticleSystem ParticleEffect;

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
        var particles = Instantiate(ParticleEffect, transform.position, Quaternion.identity);
        var particlesMain = particles.main;
        particlesMain.startColor = GetParticleColor();
        Destroy(particles.gameObject, 4f);
        Destroy(gameObject);
    }

    private ParticleSystem.MinMaxGradient GetParticleColor()
    {
        switch (TileColor)
        {
            case TileColor.Red:
                return Color.red;
            case TileColor.White:
                return Color.white;
            case TileColor.Yellow:
                return Color.yellow;
            case TileColor.Green:
                return Color.green;
            case TileColor.Blue:
                return Color.blue;
            case TileColor.Purple:
                return new Color(0.427f, 0f, 0.960f);
            case TileColor.Orange:
                return new Color(0.960f, 0.588f, 0f);
            default:
                throw new ArgumentOutOfRangeException();
        }
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

    public void SetPosition(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent, false);
    }
}
