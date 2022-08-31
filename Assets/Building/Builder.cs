using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hazel.Building
{
    public class Builder : Singleton<Builder>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Checks if the area defined between bottom left and top right (rectagle region) is buildable
        /// </summary>
        /// <param name="bottomLeft">Bottom left coordinate of extent</param>
        /// <param name="topRight">Top right coordinate of extent</param>
        /// <returns>True if the area is buildable</returns>
        public bool CanBuild(Vector2Int bottomLeft, Vector2Int topRight)
        {
            return true;
        }

        /// <summary>
        /// Checks if all tiles are buildable
        /// </summary>
        /// <param name="tiles">Tiles to check if all are buildable</param>
        /// <returns>True if all tiles are buildable</returns>
        public bool CanBuild(Vector2Int[] tiles)
        {
            return true;
        }
    }
}
