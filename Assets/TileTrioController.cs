using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileTrioController : MonoBehaviour
{
    public Tile[] Tiles;
    public float FallingSpeed = 1f;
    public float DropSpeed = 10f;

    public GameManager Manager;
    public int CurrentColumnIndex = 2;

	// Use this for initialization
	void Start () {
	    CurrentColumnIndex = 2;
    }
	
	// Update is called once per frame
	void Update ()
	{
	    if (Input.GetKeyDown(KeyCode.Space))
	    {
	        ShuffleTiles();
	    }

	    if (Input.GetKeyDown(KeyCode.LeftArrow) && CanMoveToColumn(CurrentColumnIndex - 1))
	    {
            transform.Translate(Vector3.left);
	        CurrentColumnIndex--;

	    }

	    if (Input.GetKeyDown(KeyCode.RightArrow) && CanMoveToColumn(CurrentColumnIndex + 1))
	    {
	        transform.Translate(Vector3.right);
	        CurrentColumnIndex++;
	    }

	    if (Input.GetKey(KeyCode.DownArrow) && CanFallInColumn(CurrentColumnIndex))
	    {
	        transform.Translate(Vector3.down * DropSpeed * Time.smoothDeltaTime);
        }
	    else if(CanFallInColumn(CurrentColumnIndex))
	    {
	        transform.Translate(Vector3.down * FallingSpeed * Time.smoothDeltaTime);
        }
	    else
	    {
            Manager.TileTrioArrived();
	    }
    }

    private bool CanMoveToColumn(int newColumnIndex)
    {
        var destinationColumnExists = newColumnIndex >= Manager.LeftMostColumnIndex && newColumnIndex <= Manager.RightMostColumnIndex;
        //var destinationColumnHasRoom = CanFallInColumn(newColumnIndex); //This will cause an IndexOutOfRangeException!
        //return destinationColumnExists && destinationColumnHasRoom;
        return destinationColumnExists && CanFallInColumn(newColumnIndex); //Second condition is not evaluated if first is false.
    }

    private bool CanFallInColumn(int columnIndex)
    {
        return Manager.GetColumnTop(columnIndex) < transform.position.y;
    }

    private void ShuffleTiles()
    {
        var firstTile = Tiles[0];
        var shuffledTiles = new Tile[Tiles.Length];
        for (int i = 1; i < Tiles.Length; i++)
        {
            shuffledTiles[i - 1] = Tiles[i];
        }
        shuffledTiles[Tiles.Length - 1] = firstTile;
        Tiles = shuffledTiles;

        UpdateTilePositions();
    }

    private void UpdateTilePositions()
    {
        for (int y = 0; y < Tiles.Length; y++)
        {
            Tiles[y].SetLocalPosition(Vector3.zero + Vector3.up * y); 
        }
    }

    public Transform TileParent
    {
        get { return transform; }
    }
}
