using System;
using System.Collections.Generic;

namespace _2048.Framework
{
    public interface IWeightedItem
    {
        int Weight { get; set; }
    }

    public class WeightedRandom<TItem> where TItem : IWeightedItem
    {
        /// <summary>
        /// 所有物品
        /// </summary>
        private readonly List<TItem> items = new List<TItem>();

        /// <summary>
        ///
        /// </summary>
        public IReadOnlyList<TItem> Items => items;

        /// <summary>
        ///
        /// </summary>
        public int TotalWeight { get; private set; } = 0;

        /// <summary>
        /// 添加物品
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(TItem item)
        {
            if (item == null || item.Weight <= 0)
            {
                throw new DivideByZeroException("");
            }
            if (!items.Contains(item))
            {
                int index = -1;
                for (int i = 0; i < items.Count; ++i)
                {
                    var it = items[i];
                    if (it.Weight < item.Weight)
                    {
                        continue;
                    }
                    index = i;
                    break;
                }
                if (index == -1)
                {
                    items.Add(item);
                }
                else
                {
                    items.Insert(index, item);
                }
                TotalWeight += item.Weight;
            }
        }

        /// <summary>
        /// 移除物品
        /// </summary>
        /// <param name="item"></param>
        public void RemoveItem(TItem item)
        {
            if (items.Remove(item))
            {
                TotalWeight -= item.Weight;
            }
        }

        /// <summary>
        /// 添加物品
        /// </summary>
        /// <param name="items"></param>
        public void AddItems(TItem[] items)
        {
            this.items.AddRange(items);

            foreach (var item in items)
            {
                TotalWeight += item.Weight;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="random"></param>
        private List<KeyValuePair<TItem, int>> GetSortedItems(Random random)
        {
            List<KeyValuePair<TItem, int>> wList = new List<KeyValuePair<TItem, int>>();

            if (items.Count == 0)
            {
                return wList;
            }

            if (TotalWeight == 0)
            {
                TotalWeight = 1;
            }

            foreach (var item in items)
            {
                int w = (item.Weight + 1) + random.Next(0, TotalWeight);

                wList.Add(new KeyValuePair<TItem, int>(item, w));
            }

            wList.Sort((kvp1, kvp2) => kvp2.Value - kvp1.Value);

            return wList;
        }

        /// <summary>
        /// 获取随机后的第一个元素
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public TItem First(Random random)
        {
            var wList = GetSortedItems(random);
            if (wList.Count != 0)
            {
                return wList[0].Key;
            }
            return default;
        }

        /// <summary>
        /// 获取随机后的最后一个元素
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        public TItem Last(Random random)
        {
            var wList = GetSortedItems(random);
            if (wList.Count != 0)
            {
                return wList[wList.Count - 1].Key;
            }
            return default;
        }

        /// <summary>
        /// 获取排序后的元素
        /// </summary>
        /// <param name="random"></param>
        /// <param name="count">-1表示获取所有元素</param>
        /// <returns></returns>
        public TItem[] Get(Random random, int count = -1)
        {
            var wList = GetSortedItems(random);

            List<TItem> newItems = new List<TItem>();

            int pCount = 0;
            foreach (var wItem in wList)
            {
                pCount++;
                newItems.Add(wItem.Key);
                if (pCount >= count)
                {
                    break;
                }
            }
            return newItems.ToArray();
        }

        /// <summary>
        /// 测试 返回每一项出现的次数
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="random"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public IReadOnlyDictionary<TItem, int> DoTest(Random random, int count)
        {
            Dictionary<TItem, int> result = new Dictionary<TItem, int>();
            for (int i = 0; i < count; ++i)
            {
                var myitem = First(random);
                if (result.ContainsKey(myitem))
                {
                    result[myitem] = result[myitem] + 1;
                }
                else
                {
                    result.Add(myitem, 1);
                }
            }
            return result;
        }
    }
}