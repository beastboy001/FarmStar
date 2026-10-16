using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TileCoordinateTool : EditorWindow
{
    [MenuItem("Tools/自动设置Tile坐标")]
    public static void ShowWindow()
    {
        GetWindow<TileCoordinateTool>("Tile坐标工具");
    }

    void OnGUI()
    {
        GUILayout.Label("批量设置Tile坐标", EditorStyles.boldLabel);

        if (GUILayout.Button("根据位置自动设置所有Tile坐标", GUILayout.Height(40)))
        {
            SetAllTileCoordinates();
        }

        if (GUILayout.Button("验证所有Tile坐标是否唯一", GUILayout.Height(30)))
        {
            ValidateTileCoordinates();
        }
    }

    void SetAllTileCoordinates()
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        int count = 0;

        foreach (var tile in allTiles)
        {
            Undo.RecordObject(tile, "Set Tile Coordinates");
            tile.gridX = Mathf.RoundToInt(tile.transform.position.x);
            tile.gridY = Mathf.RoundToInt(tile.transform.position.z);
            EditorUtility.SetDirty(tile);
            count++;
        }

        Debug.Log($"已设置 {count} 个Tile的坐标");
    }

    void ValidateTileCoordinates()
    {
        Tile[] allTiles = FindObjectsOfType<Tile>();
        Dictionary<Vector2Int, Tile> tileMap = new Dictionary<Vector2Int, Tile>();
        List<string> duplicates = new List<string>();

        foreach (var tile in allTiles)
        {
            Vector2Int pos = new Vector2Int(tile.gridX, tile.gridY);
            if (tileMap.ContainsKey(pos))
            {
                duplicates.Add($"({pos.x}, {pos.y}) 被 {tile.name} 和 {tileMap[pos].name} 重复使用");
            }
            else
            {
                tileMap[pos] = tile;
            }
        }

        if (duplicates.Count > 0)
        {
            Debug.LogError("发现重复坐标：\n" + string.Join("\n", duplicates));
        }
        else
        {
            Debug.Log($"所有 {allTiles.Length} 个Tile坐标唯一，验证通过");
        }
    }
}