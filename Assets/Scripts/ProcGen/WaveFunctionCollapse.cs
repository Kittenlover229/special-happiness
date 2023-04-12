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

		public Dictionary<uint, Tile>
			Tiles; // this is a fast lookup for tiles, the int describes the sides of a given tile in four enum bytes

		private int SideTypeLength = Enum.GetValues(typeof(SideType)).Length;

		private Random random = new Random();

		private Tile[,] grid;

		public WaveFunctionCollapse(ProcGenAsset[] _tiles)
		{
			Tiles = new Dictionary<uint, Tile>();

			foreach (ProcGenAsset tile in _tiles)
			{
				Tile newTile = new Tile()
				{
					SideType = ConvertStringArrToSideType(tile.Adjacency),
					Sprite = tile.Sprite
				};

				uint key = 0;
				for (int i = newTile.SideType.Length - 1; i > -1; i--)
				{
					key = (key << 8) + (byte)newTile.SideType[i];
				}

				Tiles.Add(key, newTile);
			}
		}

		private SideType[] ConvertStringArrToSideType(string[] _values)
		{
			SideType[] enumValues = new SideType[_values.Length];

			for (int i = 0; i < _values.Length; i++)
			{
				if (!Enum.TryParse(_values[i], out SideType value))
					Debug.LogError($"Invalid Json side data {_values[i]}");

				enumValues[i] = value;
			}

			return enumValues;
		}

		public Tile[,] GenerateBoard(int _width, int _height)
		{
			const int maxTries = 10;

			for (int tries = 0; tries < maxTries; tries++)
			{
				try
				{
					Tile[,] tiles = GenerateBoardWithPossibleFailure(_width, _height);

					return tiles;
				}
				catch (InconsistentWaveFunctionException)
				{
					continue;
				}
			}

			throw new InconsistentWaveFunctionException();
		}
		
		private Tile[,] GenerateBoardWithPossibleFailure(int _width, int _height)
		{
			grid = new Tile[_width, _height];

			Dictionary<Vector2Int, (uint, uint)> Positions = new Dictionary<Vector2Int, (uint, uint)>();

			Positions.Add(new Vector2Int(random.Next(_width - 1), random.Next(_height - 1)), (0, uint.MaxValue));

			SideType[] _restrictions = new SideType[4];

			while (Positions.Count > 0)
			{
				KeyValuePair<Vector2Int, (uint, uint)> min = GetTileWithMinimumOptions(ref Positions);

				uint newTile = 0;

				newTile = GetTileWhichDoesntInvalidateNeighbours(min, ref Positions);


				UpdateNeighbours(min.Key, newTile, ref Positions);

				Tile tile = Tiles[newTile];

				grid[min.Key.x, min.Key.y] = tile;

				Positions.Remove(min.Key);
			}

			return grid;
		}

		private uint GetTileWhichDoesntInvalidateNeighbours(KeyValuePair<Vector2Int, (uint, uint)> min, ref Dictionary<Vector2Int, (uint, uint)> Positions)
		{
			uint newTile;
			List<uint> possibleTiles = Tiles.Keys.Where(sides => (sides ^ min.Value.Item1) == (sides & min.Value.Item2)).ToList();

			while (possibleTiles.Count > 0)
			{
				int index = random.Next(possibleTiles.Count);
				
				if (CheckIfNeighboursAreValid(min.Key, possibleTiles[index], ref Positions))
				{
					return possibleTiles[index];
				}
				
				possibleTiles.RemoveAt(index);
			}

			throw new InconsistentWaveFunctionException();
		}

		private KeyValuePair<Vector2Int, (uint, uint)> GetTileWithMinimumOptions(
			ref Dictionary<Vector2Int, (uint, uint)> _positions)
		{
			KeyValuePair<Vector2Int, (uint, uint)> min =
				new KeyValuePair<Vector2Int, (uint, uint)>(new Vector2Int(0, 0), (0, 0));
			int options = int.MaxValue;

			foreach (KeyValuePair<Vector2Int, (uint, uint)> position in _positions)
			{
				int currentOptions = GetNumberOfOptions(position.Value.Item1, position.Value.Item2);

				if (currentOptions < options)
				{
					options = currentOptions;
					min = position;
				}
			}

			return min;
		}

		private bool CheckIfNeighboursAreValid(Vector2Int _tile, uint _change,
			ref Dictionary<Vector2Int, (uint, uint)> _positions)
		{
			int width = grid.GetLength(0);
			int height = grid.GetLength(1);

			for (int i = 0; i < Adjacency.Length; i++)
			{
				Vector2Int curr = Adjacency[i];

				Vector2Int position = curr + _tile;

				if (position.x >= width || position.y >= height || position.x < 0 || position.y < 0)
					continue;

				(uint shiftedAdjacentTile, uint shiftedMask) = GetAdjacentConstraintsAndMask(_change, i);

				(uint, uint) currTileData;

				currTileData = GetTileData(_positions, position);

				if (GetNumberOfOptions(currTileData.Item1 + shiftedAdjacentTile, currTileData.Item2 - ~shiftedMask) == 0)
				{
					return false;
				}
			}

			return true;
		}

		private (uint, uint) GetTileData(Dictionary<Vector2Int, (uint, uint)> _positions, Vector2Int position)
		{
			return _positions.ContainsKey(position)
				? _positions[position]
				: ((uint, uint))(0, uint.MaxValue);
		}

		private (uint shiftedAdjacentTile, uint shiftedMask) GetAdjacentConstraintsAndMask(uint _change, int i)
		{
			byte adjacentTileType = (byte)((_change & (byte.MaxValue << (i * 8))) >> (i * 8));

			int oppositeSide = (i + 2) % 4;

			uint shiftedAdjacentTile = (uint)((int)adjacentTileType << (oppositeSide * 8));
			uint shiftedMask = ~(uint)(byte.MaxValue << (oppositeSide * 8));
			return (shiftedAdjacentTile, shiftedMask);
		}

		public void UpdateNeighbours(Vector2Int _tile, uint _tiledata,
			ref Dictionary<Vector2Int, (uint, uint)> _positions)
		{
			int width = grid.GetLength(0);
			int height = grid.GetLength(1);

			for (int i = 0; i < Adjacency.Length; i++)
			{
				Vector2Int curr = Adjacency[i];

				Vector2Int position = curr + _tile;

				if (position.x >= width || position.y >= height || position.x < 0 || position.y < 0)
					continue;

				(uint shiftedAdjacentTile, uint shiftedMask) = GetAdjacentConstraintsAndMask(_tiledata, i);

				if (_positions.ContainsKey(position))
				{
					(uint, uint) currTileData = _positions[position];
					currTileData.Item1 += shiftedAdjacentTile;
					currTileData.Item2 -= ~shiftedMask;
					_positions[position] = currTileData;
				}
				else if (grid[position.x, position.y].SideType is null)
				{
					_positions.Add(position, (shiftedAdjacentTile, uint.MaxValue - ~shiftedMask));
				}
			}
		}

		public int GetTileData(int _xPos, int _yPos)
		{
			int width = grid.GetLength(0);
			int height = grid.GetLength(1);

			int finalData = 0;

			for (int i = 0; i < Adjacency.Length; i++)
			{
				Vector2Int curr = Adjacency[i];
				/*
				 the side of the tile has to match up with the opposite side of the other tile
				 ie if the relative direction for one tile is (1,0) the relative direction for the tile next to it should be (-1, 0), 
				 this code maps it for these particular indecies of the array, 
				 not the best practice but it has to be relied on everywhere that directions go clockwise from the top
				*/
				if (curr.x + _xPos >= width || curr.y + _yPos >= height)
				{
					finalData <<= 8;
					continue;
				}

				Tile adjacentTile = grid[curr.x + _xPos, curr.y + _yPos];

				if (adjacentTile.SideType is not null)
				{
					int oppositeSide = i + 2;
					oppositeSide = oppositeSide > 3 ? oppositeSide - 4 : oppositeSide;

					finalData = (finalData << 8) + (int)adjacentTile.SideType[oppositeSide];
				}
				else
				{
					finalData <<= 8;
				}
			}

			return finalData;
		}

		public Tile GetRandomTile(SideType[] _restrictions)
		{
			uint finalKey = 0;
			uint wildCardMask = 0;

			foreach (SideType type in _restrictions)
			{
				if (type == SideType.Null)
				{
					finalKey <<= 8;
					wildCardMask = (wildCardMask << 8) + byte.MaxValue;
				}
				else
				{
					finalKey = (finalKey << 8) + (byte)type;
					wildCardMask <<= 8;
				}
			}

			return GetRandomTile(finalKey, wildCardMask);
		}

		public (int, int) RandomVec2Int(int _maxOne, int _maxTwo)
		{
			return (random.Next(_maxOne), random.Next(_maxTwo));
		}

		public Tile GetRandomTile(uint _sides, uint _wildcardMask)
		{
			return Tiles[GetRandomTileData(_sides, _wildcardMask)];
		}
		

		public uint GetRandomTileData(uint _constraints, uint _wildcardMask)
		{
			List<uint> possibleTiles =
				Tiles.Keys.Where(sides => (sides ^ _constraints) == (sides & _wildcardMask)).ToList();
			return possibleTiles[random.Next(possibleTiles.Count)];
		}

		public int GetNumberOfOptions(uint _sides, uint _wildcardMask)
		{
			return Tiles.Keys.Sum(data => (data ^ _sides) == (data & _wildcardMask) ? 1 : 0);
		}
	}

	public class InconsistentWaveFunctionException : System.Exception {}
	
	
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
		Mountain,
		DeepMountain,
		DeepGrass
	}
}