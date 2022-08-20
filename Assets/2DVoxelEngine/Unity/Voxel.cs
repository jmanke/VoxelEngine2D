namespace Hazel.VoxelEngine2D.Unity
{
    [System.Serializable]
    public class Voxel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Empty { get; set; }

        public int HitPoints { get; set; }

        public int[] SpriteCoord { get; set; }

        public int Hardness { get; set; }
    }
}
