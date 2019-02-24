using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public TileSpritesConfig TileSprites;
    public Tile TilePrefab;

    public TileTrioController TileTrioController;

    public float SpawnHeight = 7f;
    public float SpawnBoundLeft = -2f;
    public float SpawnBoundRight = 2;

    public Vector3 TrioSpawnPosition;

    public float StartFallingSpeed = 1f;
    private float _currentFallingSpeed;
    private int xDimension = 5;
    private int yDimension = 13;
    private Tile[,] _tiles2DArray;

    public int[] NextFreeColumnRow = {0, 0, 0, 0, 0};

    // Use this for initialization
    void Start ()
    {
        _currentFallingSpeed = StartFallingSpeed;
        _tiles2DArray = new Tile[xDimension, yDimension];
    }
	
	// Update is called once per frame
	void Update () {

	}

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.white;
        for (int i = 0; i < NextFreeColumnRow.Length; i++)
        {
            Gizmos.DrawSphere(new Vector2(i, NextFreeColumnRow[i]), 0.5f);
        }
    }

    public float FallingSpeed
    {
        get { return _currentFallingSpeed; }
    }

    public int GetColumnTop(int columnIndex)
    {
        return NextFreeColumnRow[columnIndex];
    }

    public int LeftMostColumnIndex
    {
        get { return 0; }
    }

    public int RightMostColumnIndex
    {
        get { return NextFreeColumnRow.Length - 1; }
    }

    public void TileTrioArrived()
    {
        foreach (var tile in TileTrioController.Tiles)
        {
            AddTileToColumn(tile, TileTrioController.CurrentColumnIndex);
        }
        CheckMatches(TileTrioController.Tiles);
        ResetTrio();
    }

    private void AddTileToColumn(Tile tile, int column)
    {
        var x = column;
        var y = NextFreeColumnRow[column];
        _tiles2DArray[x, y] = tile;
        tile.SetPosition(new Vector2Int(x, y));
        tile.transform.parent = transform;
        NextFreeColumnRow[column]++;
    }

    private void ResetTrio()
    {
        TileTrioController.transform.position = TrioSpawnPosition;
        TileTrioController.CurrentColumnIndex = (int)TrioSpawnPosition.x;
        for (int i = 0; i < 3; i++)
        {
            var tile = Instantiate(TilePrefab, Vector3.zero, Quaternion.identity);
            tile.TileColor = GetRandomGemColorSprite();
            tile.Sprite = TileSprites.TileSprites.First(t => t.Color == tile.TileColor).Sprite;
            tile.SetLocalPosition(Vector3.zero + Vector3.up * i);
            tile.SetParent(TileTrioController.TileParent);
            TileTrioController.Tiles[i] = tile;
        }
    }

    private TileColor GetRandomGemColorSprite()
    {
        var randomColorNumber = Random.Range(0, 6);  
        return (TileColor)randomColorNumber; 
    }

    private void CheckMatches(Tile[] freshTiles)
    {
        var matches = new HashSet<Tile>();
        foreach (var tile in freshTiles)
        {
            var foundMatches = CheckForMatches(tile);
            matches.UnionWith(foundMatches);
        }

        RemoveMatches(matches);
    }

    private List<Tile> CheckForMatches(Tile tile)
    {
        var matches = new List<Tile>();
        var neighbors = new List<Tile>
        {
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.right),
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.left),
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.up),
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.down)
        };
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null)
            {
                matches.AddRange(EvaluateNeighbors(neighbor));
            }
        }

        return matches;
    }

    private void RemoveMatches(HashSet<Tile> matches)
    {
        foreach (var match in matches)
        {
            RemoveTileAtCoordinates(match.GridPosition);
            match.DeSpawn();
        }

        var movedTiles = new List<Tile>();

        for (int x = 0; x < xDimension; x++)
        {
            int holes = 0;
            for (int y = 0; y < yDimension; y++)
            {
                var tile = GetTileAtCoordinates(new Vector2Int(x, y));

                if (tile == null)
                {
                    holes++;
                }
                else if(holes > 0)
                {
                    RemoveTileAtCoordinates(tile.GridPosition);
                    tile.SetPosition(tile.GridPosition + Vector2Int.down * holes);
                    SetTileAtCoordinates(tile, tile.GridPosition);
                    movedTiles.Add(tile);
                }
            }
        }

        UpdateFreeColumnRows();
        if(movedTiles.Count > 0) CheckMatches(movedTiles.ToArray());
    }

    private List<Tile> EvaluateNeighbors(Tile tile)
    {
        var similarNeighbors = new List<Tile>();

        var left = GetTileAtCoordinates(tile.GridPosition + Vector2Int.left);
        var right = GetTileAtCoordinates(tile.GridPosition + Vector2Int.right);
        if (left != null && right != null)
        {
            if (left.TileColor == tile.TileColor && right.TileColor == tile.TileColor)
            {
                similarNeighbors.Add(left);
                similarNeighbors.Add(right);
                similarNeighbors.Add(tile);
            }
        }

        var top = GetTileAtCoordinates(tile.GridPosition + Vector2Int.up);
        var bottom = GetTileAtCoordinates(tile.GridPosition + Vector2Int.down);
        if (top != null && bottom != null)
        {
            if (top.TileColor == tile.TileColor && bottom.TileColor == tile.TileColor)
            {
                similarNeighbors.Add(top);
                similarNeighbors.Add(bottom);
                similarNeighbors.Add(tile);
            }
        }


        return similarNeighbors;
    }

    private Tile GetTileAtCoordinates(Vector2Int position)
    {
        if (position.x < 0 || position.x >= xDimension || position.y < 0 || position.y >= yDimension)
        {
            return null;
        }
        else
        {
            return _tiles2DArray[position.x, position.y];
        }
    }

    private void SetTileAtCoordinates(Tile tile, Vector2Int position)
    {
        _tiles2DArray[position.x, position.y] = tile;
    }

    private Tile RemoveTileAtCoordinates(Vector2Int position)
    {
        var tile = GetTileAtCoordinates(position);
        _tiles2DArray[position.x, position.y] = null;
        return tile;
    }

    private void UpdateFreeColumnRows()
    {
        for (int x = 0; x < xDimension; x++)
        {
            var count = 0;
            for (int y = 0; y < yDimension; y++)
            {
                if (_tiles2DArray[x, y] != null)
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            NextFreeColumnRow[x] = count;
        }
    }

    private void ScoreMatchingTiles(List<Tile> scoringTiles)
    {
        Debug.Log("Scoring " + scoringTiles);
        foreach (var tile in scoringTiles)
        {
            tile.DeSpawn();
        }
    }
}