using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class Drawing : MonoBehaviour
{
    public bool drawing;
    public HexagonalRuleTile newTile;
    private Tilemap tilemap;

    public Tilemap tilemap1;
    public Tilemap tilemap2;
    public Tilemap tilemap3;
    public Tilemap tilemap4;

    public void SetTileMap(int tileMapNumber)
    {
        if (tileMapNumber == 2)
        {
            tilemap = tilemap2;
        }
        else if (tileMapNumber == 3)
        {
            tilemap = tilemap3;
        }
        else if (tileMapNumber == 4)
        {
            tilemap = tilemap4;
        }
        else
            tilemap = tilemap1;
    }

    public void OnClickDraw()
    {
        drawing = true;
    }
    public void OnClickErase()
    {
        drawing = false;
    }
    private void Start()
    {
        SetTileMap(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (drawing)
        {
            if (Input.GetMouseButton(0))
            {
                //Get position of the mouseclick
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //Convert position of the mouseclick to the position of the tile located at the mouseclick
                Vector3Int coordinate = tilemap.WorldToCell(pos);
                //Display tile position in log
                //Debug.Log(coordinate);

                tilemap.SetTile(coordinate, newTile);

            }
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                //Get position of the mouseclick
                Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                //Convert position of the mouseclick to the position of the tile located at the mouseclick
                Vector3Int coordinate = tilemap.WorldToCell(pos);
                //Display tile position in log
                //Debug.Log(coordinate);

                tilemap.SetTile(coordinate, null);

            }
        }
    }

}
