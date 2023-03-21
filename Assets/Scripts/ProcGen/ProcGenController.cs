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
	}
}