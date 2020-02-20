﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TerraFormmingMars.Logics;

namespace TerraFormmingMars.Entity.Board
{
    public class Tile : MonoBehaviour
    {
        [SerializeField]
        private TileData myTileData;
        public TileData MyTileData { get { return myTileData; } }

        private List<Tile> nearTiles;

        public int GetScore()
        {
            int score = 0;

            if(MyTileData.TileType == TileType.Tree)
            {
                score = 1;
            }
            else if(MyTileData.TileType == TileType.City)
            {
                //주변의 나무 1개마다 1점씩 추가
            }
            else if(MyTileData.TileType == TileType.Capital)
            {
                //주변의 물과 나무 1개마다 1점씩 추가
            }
            else if(MyTileData.TileType == TileType.Market)
            {
                //주변의 도시 1개 마다 1점씩 추가
            }

            return score;
        }

        private void InitNearTiles()
        {
            string lineName = transform.parent.name;
            string myName = transform.name;

            int lineNum = int.Parse(lineName[4].ToString());
            int tileNum = int.Parse(myName[4].ToString());
        }
    }
}