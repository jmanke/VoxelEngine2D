namespace Hazel
{
    public class FlatArray2D<T>
    {
        private readonly T[] array;

        /// <summary>
        /// Width of array
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Height of array
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Total length of array width * height
        /// </summary>
        public int Length
        {
            get
            {
                return this.array.Length;
            }
        }

        public FlatArray2D(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.array = new T[width * height];
        }

        public FlatArray2D(int width, int height, T initialValue) : this(width, height)
        {
            for (int i = 0; i < this.array.Length; i++)
            {
                this.array[i] = initialValue;
            }
        }

        public FlatArray2D(T[,] array) : this(array.GetLength(0), array.GetLength(1))
        {
            int lenX = array.GetLength(0);
            int lenY = array.GetLength(1);

            for (int x = 0; x < lenX; x++)
            {
                for (int y = 0; y < lenY; y++)
                {
                    this.Set(x, y, array[x, y]);
                }
            }
        }

        /// <summary>
        /// Get value in array
        /// </summary>
        /// <param name="x">x index</param>
        /// <param name="y">y index</param>
        /// <returns>Value</returns>
        public T Get(int x, int y)
        {
            return this.array[x * this.Height + y];
        }

        /// <summary>
        /// Sets value in array
        /// </summary>
        /// <param name="x">x index</param>
        /// <param name="y">y index</param>
        /// <param name="val">value to set</param>
        public void Set(int x, int y, T val)
        {
            this.array[x * this.Height + y] = val;
        }
    }
}
