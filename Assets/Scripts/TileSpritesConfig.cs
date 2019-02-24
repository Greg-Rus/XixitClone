using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileSpritesConfig", menuName = "Config/TileSpritesConfig", order = 1)]
public class TileSpritesConfig : ScriptableObject
{
    public List<TileSprite> TileSprites;
}

[CreateAssetMenu(fileName = "GameConfig", menuName = "Config/GameConfig", order = 2)]
public class GameConfig : ScriptableObject
{
    public float BoardHeight;
    public float BoardWidth;
}