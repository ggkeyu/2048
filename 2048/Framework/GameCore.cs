using System;
using System.Collections.Generic;
using System.Linq;

namespace _2048.Framework
{
    public sealed class IntItem : IWeightedItem
    {
        public int Value { get; set; }

        public int Weight { get; set; }
    }

    public interface IBlockItem
    {
        int Number { get; set; }

        int Index { get; set; }
    }

    public class GameCore<TItem>
        where TItem : IBlockItem, new()
    {
        private readonly IList<TItem> gBlocks;

        public int Column { get; }

        public int Row { get; }

        /// <summary>
        /// 游戏结束(是否胜利)
        /// </summary>
        public Action<bool> OnGameOver { get; set; }

        /// <summary>
        /// 分数改变(分数，是否追加)
        /// </summary>
        public Action<int, bool> SetScore { get; set; }

        /// <summary>
        /// 获取分数(返回分数)
        /// </summary>
        public Func<int> GetScore { get; set; }

        /// <summary>
        /// 最大缓存数量
        /// </summary>
        private readonly int maxCacheCount = 3;

        /// <summary>
        /// 缓存的状态
        /// </summary>
        private readonly List<GameStateArchive> states = new List<GameStateArchive>();

        /// <summary>
        /// 刷新数值权重
        /// </summary>
        private readonly IntItem[] weightedItems = Array.Empty<IntItem>();

        /// <summary>
        /// 出现此数值时胜利
        /// </summary>
        private readonly int winNumber = 2048;

        /// <summary>
        /// 2048核心类
        /// </summary>
        /// <param name="row">行数</param>
        /// <param name="column">列表</param>
        /// <param name="blocks">元素源列表</param>
        /// <param name="cacheStateCount">最大可以缓存的状态数量</param>
        /// <param name="weightedItems">每一次刷新时出现的数值列表</param>
        /// <param name="winNumber">出现此数值时则胜利</param>
        public GameCore(int row, int column, IList<TItem> blocks, int cacheStateCount = 3, IntItem[] weightedItems = null, int winNumber = 2048)
        {
            Row = row;
            Column = column;
            gBlocks = blocks;
            maxCacheCount = cacheStateCount;

            if (weightedItems == null)
            {
                this.weightedItems = new IntItem[2]
                {
                    new IntItem(){ Value = 2, Weight = 98},
                    new IntItem(){ Value = 4, Weight = 2}
                };
            }

            this.winNumber = winNumber;
        }

        /// <summary>
        /// 重置所有元素为0
        /// </summary>
        public void ResetBlocks()
        {
            int total = Row * Column;
            //重置或者生成
            if (gBlocks.Count == 0)
            {
                for (int i = 0; i < total; ++i)
                {
                    var block = new TItem() { Number = 0, Index = i };
                    gBlocks.Add(block);
                }
            }
            else
            {
                for (int i = 0; i < total; ++i)
                {
                    var block = gBlocks[i];

                    block.Number = 0;
                }
            }
        }

        /// <summary>
        /// 随机刷新元素
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public bool Reflush(out TItem[] result, int count = 1)
        {
            result = Array.Empty<TItem>();

            var emptyList = GetEmptyValueList();

            if (emptyList.Count == 0)
            {
                return false;
            }

            var random = new Random();

            List<TItem> found = new List<TItem>();

            for (int i = 0; i < count; ++i)
            {
                TItem block = RandomGetBlock(emptyList, random);
                if (block == null)
                {
                    continue;
                }
                found.Add(block);
            }

            if (found.Count != count)
            {
                return false;
            }

            foreach (var block in found)
            {
                RandomSetNumber(block, random: random);
            }
            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="index"></param>
        /// <param name="number"></param>
        public void SetBlockNumber(int index, int number)
        {
            if (index >= 0 && index < gBlocks.Count)
            {
                gBlocks[index].Number = number;
            }
        }

        /// <summary>
        /// 返回上一个状态
        /// </summary>
        public void Return()
        {
            var state = GetState();
            if (state != null)
            {
                FromArchive(state);
            }
        }

        /// <summary>
        /// 随机设置元素值
        /// </summary>
        /// <param name="TItem"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private bool RandomSetNumber(TItem TItem, Random random = null)
        {
            if (random == null)
            {
                random = new Random();
            }

            var weightedRandom = new WeightedRandom<IntItem>();
            weightedRandom.AddItems(weightedItems);

            var item = weightedRandom.First(random);

            if (item == null)
            {
                return false;
            }

            TItem.Number = item.Value;

            return true;
        }

        /// <summary>
        /// 从数组中随机获取一个元素 然后移除
        /// </summary>
        /// <param name="list"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private TItem RandomGetBlock(List<TItem> list, Random random = null)
        {
            if (random == null)
            {
                random = new Random();
            }

            if (list.Count == 0)
            {
                return default;
            }

            int index = random.Next(0, list.Count);

            TItem num = list[index];

            list.RemoveAt(index);

            return num;
        }

        /// <summary>
        /// 获取某一列
        /// </summary>
        /// <param name="index"></param>
        /// <param name="allEmpty"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public TItem[] GetColumn(int index, out bool allEmpty, bool reverse = false)
        {
            allEmpty = true;

            List<TItem> blocks = new List<TItem>();
            for (int i = 0; i < Row; ++i)
            {
                int index0 = i * Column + index;
                var block = gBlocks[index0];
                if (allEmpty)
                {
                    if (block.Number != 0)
                    {
                        allEmpty = false;
                    }
                }
                blocks.Add(block);
            }

            if (reverse)
            {
                blocks.Reverse();
            }

            return blocks.ToArray();
        }

        /// <summary>
        /// 获取某一行
        /// </summary>
        /// <param name="index"></param>
        /// <param name="allEmpty"></param>
        /// <param name="reverse"></param>
        /// <returns></returns>
        public TItem[] GetRow(int index, out bool allEmpty, bool reverse = false)
        {
            allEmpty = true;

            if (index < 0 || index >= Row)
            {
                return Array.Empty<TItem>();
            }

            List<TItem> blocks = new List<TItem>();
            for (int i = 0; i < Column; ++i)
            {
                int index0 = index * Column + i;
                var block = gBlocks[index0];
                if (allEmpty)
                {
                    if (block.Number != 0)
                    {
                        allEmpty = false;
                    }
                }
                blocks.Add(block);
            }

            if (reverse)
            {
                blocks.Reverse();
            }

            return blocks.ToArray();
        }

        /// <summary>
        /// 获取空元素列表
        /// </summary>
        /// <returns></returns>
        public List<TItem> GetEmptyValueList()
        {
            List<TItem> blocks = new List<TItem>();

            blocks.AddRange(gBlocks.Where(b => b.Number == 0));

            return blocks;
        }

        /// <summary>
        /// 是否游戏结束
        /// </summary>
        /// <returns></returns>
        public bool IsGameOver()
        {
            for (int i = 0; i < Row; ++i)
            {
                for (int j = 0; j < Column; ++j)
                {
                    var index = i * Column + j;

                    var block = gBlocks[index];

                    //有元素的值为0表示有空格 不可能结束了
                    if (block.Number == 0)
                    {
                        return false;
                    }

                    //判断当前元素的上下左右四个位置是否有相同的值 如果有表示没有结束
                    var leftIndex = index - 1;
                    var rightIndex = index + 1;
                    var topIndex = index - Column;
                    var bottomIndex = index + Column;
                    if (j != 0 && leftIndex >= 0 && leftIndex < gBlocks.Count)
                    {
                        if (gBlocks[leftIndex].Number == block.Number)
                        {
                            return false;
                        }
                    }
                    if ((j != Column - 1) && rightIndex >= 0 && rightIndex < gBlocks.Count)
                    {
                        if (gBlocks[rightIndex].Number == block.Number)
                        {
                            return false;
                        }
                    }
                    if (topIndex >= 0 && topIndex < gBlocks.Count)
                    {
                        if (gBlocks[topIndex].Number == block.Number)
                        {
                            return false;
                        }
                    }
                    if (bottomIndex >= 0 && bottomIndex < gBlocks.Count)
                    {
                        if (gBlocks[bottomIndex].Number == block.Number)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private void TickItems(TItem[] items, out bool win, ref bool needReflush)
        {
            win = false;

            //清除前面值为0的元素
            ClearItems(items, ref needReflush);

            //合并元素
            for (int i = 0; i < items.Length; ++i)
            {
                var block = items[i];
                if (block.Number == 0)
                {
                    continue;
                }
                if (i + 1 >= items.Length)
                {
                    continue;
                }
                var block1 = items[i + 1];
                if (block1.Number == block.Number)
                {
                    block1.Number += block.Number;

                    //已经出现了胜利的数值
                    if (block1.Number >= winNumber)
                    {
                        win = true;
                    }

                    SetScore?.Invoke(block.Number, true);

                    block.Number = 0;

                    needReflush = true;

                    i++;
                }
            }

            //再次清除
            ClearItems(items, ref needReflush);
        }

        private void ClearItems(TItem[] items, ref bool needReflush)
        {
            List<TItem> newList = new List<TItem>();

            bool needMove = false;
            bool hasZero = false;
            for (int j = items.Length - 1; j >= 0; --j)
            {
                var block = items[j];
                if (block.Number != 0)
                {
                    if (hasZero)
                    {
                        needMove = true;
                    }
                    newList.Add(block);
                }
                else
                {
                    hasZero = true;
                }
            }

            if (newList.Count == items.Length || !needMove)
            {
                return;
            }

            needReflush = true;

            for (int j = items.Length - 1, k = 0; j >= 0; --j, ++k)
            {
                if (k < newList.Count)
                {
                    items[j].Number = newList[k].Number;
                }
                else
                {
                    items[j].Number = 0;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void MoveBack()
        {
            TickColumns(false);
        }

        /// <summary>
        ///
        /// </summary>
        public void MoveForward()
        {
            TickColumns(true);
        }

        /// <summary>
        ///
        /// </summary>
        public void MoveLeft()
        {
            TickRows(true);
        }

        /// <summary>
        ///
        /// </summary>
        public void MoveRight()
        {
            TickRows(false);
        }

        /// <summary>
        /// 更新列
        /// </summary>
        /// <param name="reverse"></param>
        private void TickColumns(bool reverse)
        {
            AddState(ToArchive());

            bool win = false;
            bool needReflush = false;
            for (int i = 0; i < Column; ++i)
            {
                var items = GetColumn(i, out bool allEmpty, reverse);
                if (items.Length == 0 || allEmpty)
                {
                    continue;
                }

                TickItems(items, out win, ref needReflush);
            }

            TickEnd(needReflush, win);
        }

        /// <summary>
        /// 更新行
        /// </summary>
        /// <param name="reverse"></param>
        private void TickRows(bool reverse)
        {
            AddState(ToArchive());

            bool win = false;
            bool needReflush = false;
            for (int i = 0; i < Row; ++i)
            {
                var items = GetRow(i, out bool allEmpty, reverse);
                if (items.Length == 0 || allEmpty)
                {
                    continue;
                }

                TickItems(items, out win, ref needReflush);
            }

            TickEnd(needReflush, win);
        }

        private void TickEnd(bool needReflush, bool win)
        {
            bool gameOver = IsGameOver();

            if (!gameOver && needReflush)
            {
                gameOver = !Reflush(out _);
            }

            //游戏结束
            if (gameOver)
            {
                OnGameOver?.Invoke(win);
            }
        }

        private void AddState(GameStateArchive archive)
        {
            if (states.Count > maxCacheCount && states.Count > 0)
            {
                states.RemoveAt(states.Count - 1);
            }
            states.Insert(0, archive);
        }

        private GameStateArchive GetState()
        {
            if (states.Count == 0)
            {
                return null;
            }
            var state = states.First();
            states.RemoveAt(0);
            return state;
        }

        /// <summary>
        /// 清除缓存的状态
        /// </summary>
        public void ClearStates()
        {
            states.Clear();
        }

        /// <summary>
        /// 从文件中还原状态
        /// </summary>
        /// <param name="archivePath"></param>
        public void Load(string archivePath)
        {
            if (string.IsNullOrEmpty(archivePath))
            {
                return;
            }

            var archive = GameStateArchive.Load(archivePath);

            FromArchive(archive);
        }

        /// <summary>
        /// 保存当前状态到文件中
        /// </summary>
        /// <param name="archivePath"></param>
        public void Save(string archivePath)
        {
            if (string.IsNullOrEmpty(archivePath))
            {
                return;
            }

            var archive = ToArchive();

            archive.Save(archivePath);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public GameStateArchive ToArchive()
        {
            GameStateArchive archive = new GameStateArchive
            {
                Name = "archive",
                Score = GetScore?.Invoke() ?? 0
            };

            List<int> blocks = new List<int>();
            foreach (var block in gBlocks)
            {
                blocks.Add(block.Number);
            }

            archive.SetBlocks(blocks.ToArray());

            return archive;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="archive"></param>
        public void FromArchive(GameStateArchive archive)
        {
            if (archive != null)
            {
                SetScore?.Invoke(archive.Score, false);

                var blocks = archive.GetBlocks();

                if (blocks.Length == gBlocks.Count)
                {
                    for (int i = 0; i < blocks.Length; ++i)
                    {
                        gBlocks[i].Number = blocks[i];
                    }
                }
            }
        }
    }
}