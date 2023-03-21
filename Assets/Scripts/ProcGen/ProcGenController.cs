using System;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace ProcGen
{
	[RequireComponent(typeof(ProcGenAdjacencyDataDeserializer))]
	public class ProcGenController : MonoBehaviour
	{
		public WaveFunctionCollapse wfc;

		public ProcGenAdjacencyDataDeserializer Deserializer;

		public Tilemap Tilemap;
		
		public Tile[,] Board;
		public Vector2Int BoardDimensions = new Vector2Int(10,10);
		
		public void Start()
		{
			Deserializer.Deserialize("ProcGen/basic2d");
			
			wfc = new WaveFunctionCollapse(Deserializer.Assets);
			
			Board = wfc.GenerateBoard(BoardDimensions.x, BoardDimensions.y);
			SetTiles();
		}

		public void SetTiles()
		{
			for (int x = 0; x < BoardDimensions.x; x++)
			{
				for (int y = 0; y < BoardDimensions.y; y++)
				{
					Tilemap.SetTile(new Vector3Int(x,y), Board[x,y].Sprite);
				} 
			}
		}
		
		
		public bool TryGetTile(Vector3 _worldPos, out Tile tile)
		{
			Vector3Int position = Tilemap.WorldToCell(_worldPos);
			try
			{
				tile = Board[position.x, position.y];
				return true;
			}
			catch (IndexOutOfRangeException)
			{
				tile = default(Tile);
				return false;
			}
		}
		
		public bool TrySetTile(Vector3 _worldPos, Tile tile)
		{
			Vector3Int position = Tilemap.WorldToCell(_worldPos);
			try
			{
				Board[position.x, position.y] = tile;
				return true;
			}
			catch (IndexOutOfRangeException)
			{
				tile = default(Tile);
				return false;
			}
		}
	}
}