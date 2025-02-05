using UnityEngine;
public static class Utils
{
    public static Vector2Int GetRandomCardinalDirection()
    {
        var direction = Random.Range(0, 4);
        return direction switch {
            0 => Vector2Int.up,
            1 => Vector2Int.right,
            2 => Vector2Int.down,
            _ => Vector2Int.left,
        };
    }
}