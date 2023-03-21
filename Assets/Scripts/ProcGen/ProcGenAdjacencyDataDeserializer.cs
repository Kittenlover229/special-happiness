using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace ProcGen
{
	public class ProcGenAdjacencyDataDeserializer : MonoBehaviour
	{
		public ProcGenAsset[] Assets;

		public ProcGenAsset[] Deserialize(string _path)
		{
			Assets = LoadJsonAssetArr(_path);

			Dictionary<string, Sprite> sprites = LoadSprites(Assets[0].FileName);

			foreach (ProcGenAsset asset in Assets)
			{
				asset.Sprite = Resources.Load<TileBase>(asset.FileName + asset.AssetName);
			}
			
			return Assets;
		}
		
		public ProcGenAsset[] LoadJsonAssetArr(string _path)
		{
			TextAsset assetArr = Resources.Load<TextAsset>(_path);

			ProcGenAsset[] assets = JsonUtility.FromJson<Root>(assetArr.text).Asset;

			return assets;
		}

		public Dictionary<string, Sprite> LoadSprites(string _path)
		{
			Sprite[] sprites = Resources.LoadAll<Sprite>(_path);

			return sprites.ToDictionary(sprite => sprite.name);
		}

	}
	
	[Serializable]
	public class ProcGenAsset
	{
		public string FileName;
		public string AssetName;
		public string[] Adjacency;
		public string[] Tags;

		public TileBase Sprite;
	}

	[Serializable]
	public class Root
	{
		public ProcGenAsset[] Asset;
	}
}
