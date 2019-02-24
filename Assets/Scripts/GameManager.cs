using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public TileSpritesConfig TileSprites;
    public Tile TilePrefab;

    public TileTrioController TileTrioController;

    public float tileDropTime = 0.5f;

    public Vector3 TrioSpawnPosition;

    public float StartFallingSpeed = 1f;
    private float _currentFallingSpeed;
    private int xDimension = 5;
    private int yDimension = 13;
    private Tile[,] _tiles2DArray;

    public int[] NextFreeColumnRow = {0, 0, 0, 0, 0};
    public List<Tile> droppingTiles; 

    // Use this for initialization
    void Start ()
    {
        _currentFallingSpeed = StartFallingSpeed;
        _tiles2DArray = new Tile[xDimension, yDimension];
        droppingTiles = new List<Tile>();
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

        TileTrioController.enabled = false;
        CheckMatches(TileTrioController.Tiles);

    }

    private void AddTileToColumn(Tile tile, int column)
    {
        var x = column;
        var y = NextFreeColumnRow[column];
        if (y > yDimension -1)
        {
            SceneManager.LoadScene(0);
        }
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
        TileTrioController.enabled = true;
        //ValidateBoard();
    }

    private TileColor GetRandomGemColorSprite()
    {
        var randomColorNumber = Random.Range(0, 5);  
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
            tile,
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.right),
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.left),
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.up),
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.up + Vector2Int.right),
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.up + Vector2Int.left),
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.down),
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.down + Vector2Int.right ),
            GetTileAtCoordinates(tile.GridPosition + Vector2Int.down + Vector2Int.left ),

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
        droppingTiles.Clear();
        foreach (var match in matches)
        {
            RemoveTileAtCoordinates(match.GridPosition);
            match.DeSpawn();
        }


        for (int x = 0; x < xDimension; x++)
        {
            bool holes = false;
            for (int y = 0; y < yDimension; y++)
            {
                var tile = GetTileAtCoordinates(new Vector2Int(x, y));

                if (tile == null)
                {
                    holes = true;
                }
                else if(holes)
                {
                    RemoveTileAtCoordinates(tile.GridPosition);
                    tile.SetPosition(tile.GridPosition + Vector2Int.down);
                    SetTileAtCoordinates(tile, tile.GridPosition);
                    droppingTiles.Add(tile);
                }
            }
        }

        if (droppingTiles.Count > 0)
        {
            StartCoroutine(DropBlocks());
        }
        else
        {
            ResetTrio();
        }
    }

    private List<Tile> EvaluateNeighbors(Tile tile)
    {
        var similarNeighbors = new List<Tile>();

        similarNeighbors.AddRange(EvaluateNeighbor(tile, Vector2Int.up, Vector2Int.down));
        similarNeighbors.AddRange(EvaluateNeighbor(tile, Vector2Int.left, Vector2Int.right));
        similarNeighbors.AddRange(EvaluateNeighbor(tile, Vector2Int.up + Vector2Int.left, Vector2Int.down + Vector2Int.right));
        similarNeighbors.AddRange(EvaluateNeighbor(tile, Vector2Int.down + Vector2Int.left, Vector2Int.up + Vector2Int.right));

        return similarNeighbors;
    }

    private List<Tile> EvaluateNeighbor(Tile tile, Vector2Int firstNeighbor, Vector2Int secondNeighbor)
    {
        var similarNeighbors = new List<Tile>();
        var firstNeighborTile = GetTileAtCoordinates(tile.GridPosition + firstNeighbor);
        var secondNeighborTile = GetTileAtCoordinates(tile.GridPosition + secondNeighbor);
        if (firstNeighborTile != null && secondNeighborTile != null)
        {
            if (firstNeighborTile.TileColor == tile.TileColor && secondNeighborTile.TileColor == tile.TileColor)
            {
                similarNeighbors.Add(firstNeighborTile);
                similarNeighbors.Add(secondNeighborTile);
                similarNeighbors.Add(tile);
                Debug.DrawLine(firstNeighborTile.transform.position, secondNeighborTile.transform.position, Color.green, 0.2f);
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

    private void ValidateBoard()
    {
        for (int x = 0; x < xDimension; x++)
        {
            for (int y = 0; y < xDimension; y++)
            {
                var tile = GetTileAtCoordinates(new Vector2Int(x, y));
                if (tile != null)
                {
                    if (tile.GridPosition.x != x || 
                        tile.GridPosition.y != y )
                    {
                        Debug.LogError("Expected " + x + " " + y +" Grid: " + tile.GridPosition.x + " " + tile.GridPosition.y);
                    }

                    if (!Mathf.Approximately(tile.transform.position.x, x) ||
                        !Mathf.Approximately(tile.transform.position.y, y))
                    {
                        Debug.LogError("Expected " + x + " " + y + " Position: " + tile.transform.position.x + " " + tile.transform.position.y);
                    }
                }

            }
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

    private IEnumerator DropBlocks()
    {
        float timer = tileDropTime;
        while (timer > 0)
        {
            foreach (var tile in droppingTiles)
            {
                Vector3 newPosition = new Vector3(tile.GridPosition.x, tile.GridPosition.y, 0f) + Vector3.up * (timer / tileDropTime);
                tile.SetPosition(newPosition);
            }

            timer -= Time.smoothDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        foreach (var tile in droppingTiles)
        {
            tile.SetPosition(tile.GridPosition);
        }
        CheckMatches(droppingTiles.ToArray());
        UpdateFreeColumnRows();
    }
}