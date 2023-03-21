using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = System.Random;

namespace ProcGen
{
	public class WaveFunctionCollapse
	{
		public Vector2Int[] Adjacency = new[]
		{ 
			new Vector2Int(0, 1), new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0),
		};
		
		public Dictionary<int, Tile> Tiles; // this is a fast lookup for tiles, the int describes the sides of a given tile in four enum bytes

		private int SideTypeLength = Enum.GetValues(typeof(SideType)).Length;

		private Random random = new Random();

		public WaveFunctionCollapse(ProcGenAsset[] _tiles)
		{
			Tiles = new Dictionary<int, Tile>();
			
			foreach (ProcGenAsset tile in _tiles)
			{
				Tile newTile = new Tile()
				{
					SideType = ConvertStringArrToSideType(tile.Adjacency),
					Sprite = tile.Sprite
				};

				int key = 0;
				foreach (byte value in newTile.SideType)
				{
					key = (key << 4) + value;
				}
				
				Tiles.Add(key, newTile);
			}
		}

		private SideType[] ConvertStringArrToSideType(string[] _values)
		{
			SideType[] enumValues = new SideType[_values.Length];

			for (int i = 0; i < _values.Length; i++)
			{
				if (!SideType.TryParse(_values[i], out SideType value))
					Debug.LogError($"Invalid Json side data {_values[i]}");
				
				enumValues[i] = value;
			}

			return enumValues;
		}

		public Tile[,] GenerateBoard(int _width, int _height)
		{
			Tile[,] grid = new Tile[_width, _height];

			SideType[] _restrictions = new SideType[4];
			
			for (int x = 0; x < _width; x++)
			{
				for (int y = 0; y < _height; y++)
				{
					for (int i = 0; i < Adjacency.Length; i++)
					{
						Vector2Int curr = Adjacency[i];

						try
						{
							/*
							 the side of the tile has to match up with the opposite side of the other tile
							 ie if the relative direction for one tile is (1,0) the relative direction for the tile next to it should be (-1, 0), 
							 this code maps it for these particular indecies of the array, 
							 not the best practice but it has to be relied on everywhere that directions go clockwise from the top
							*/

							int oppositeSide = i + 2;
							oppositeSide = oppositeSide > 3 ? oppositeSide - 4 : oppositeSide;

							_restrictions[i] = grid[curr.x + x, curr.y + y].SideType[oppositeSide];
						}
						catch (Exception ex)
						{
							if (ex is NullReferenceException || ex is IndexOutOfRangeException)
							{
								// trying to index a tile out of the grid or a tile which doesn't exist yet
								_restrictions[i] = SideType.Null;
								continue;
							}
							else
							{
								throw;
							}

						}
					}

					grid[x, y] = GetRandomTile(_restrictions);
				}
			}

			return grid;
		}

		public Tile GetRandomTile(SideType[] _restrictions)
		{
			int finalKey = 0;

			foreach (SideType type in _restrictions)
			{
				if (type == SideType.Null)
				{
					finalKey = (finalKey << 4) + random.Next(1, SideTypeLength);
					continue;
				}

				finalKey = (finalKey << 4) + (byte)type;
			}

			return Tiles[finalKey];
		}
		
		public (int, int) RandomVec2Int(int _maxOne, int _maxTwo)
		{
			return (random.Next(_maxOne), random.Next(_maxTwo));
		}
	}

	public struct Tile
	{
		// other stuff on the tile should go here
		public TileBase Sprite;
		public SideType[] SideType;
	}

	public enum SideType // will run into problems if there are more than a byte of possiblities here, however that can be fixed above, we shouldn't go higher than that though
	{
		Null,
		Grass,
		Mountain
	}
}