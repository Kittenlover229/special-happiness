using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using Object = UnityEngine.Object;

public class ProcGenAdjacencyDataDeserializer : MonoBehaviour
{
    public Asset[] Assets;

    public void Start()
    {
        Assets = LoadJsonAssetArr("ProcGen/basic2d");

        Dictionary<string, Sprite> sprites = LoadSprites(Assets[0].FileName);

        foreach (Asset asset in Assets)
        {
            asset.Sprite = sprites[asset.AssetName];
        }
    }

    public Asset[] LoadJsonAssetArr(string _path)
    {
        TextAsset assetArr = Resources.Load<TextAsset>(_path);

        Asset[] assets = JsonUtility.FromJson<Root>(assetArr.text).Asset;

        return assets;
    }

    public Dictionary<string, Sprite> LoadSprites(string _path)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(_path);

        return sprites.ToDictionary(sprite => sprite.name);
    }
    
    

    [Serializable]
    public class Asset
    {
        public string FileName;
        public string AssetName;
        public string[] Adjacency;
        public string[] Tags;

        public Sprite Sprite;
    }

    [Serializable]
    public class Root
    {
        public Asset[] Asset;
    }
}