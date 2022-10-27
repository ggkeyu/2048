using _2048.Framework;

namespace _2048.Models
{
    public class BlockInfo : ModelBase, IBlockItem
    {
        private int index = -1;

        public int Index
        {
            get => index;
            set
            {
                index = value;

                OnPropertyChanged(nameof(Index));
            }
        }

        private int number = 0;

        public int Number
        {
            get => number;
            set
            {
                number = value;

                if (number < 0)
                {
                    number = 0;
                }

                OnPropertyChanged(nameof(Number));
            }
        }

        public override string ToString()
        {
            return $"{Number}-";
        }
    }
}