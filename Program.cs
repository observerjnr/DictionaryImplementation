using System;
using System.Collections.Generic;

namespace DictionaryImplementation
{
    class Program
    {
        class Dictionary<TKey, TValue> where TKey : IEquatable<TKey>
        {
            const int InitialCapacity = 11;
            const double MaxLoadFactor = 0.75;

            List<KeyValuePair<TKey, TValue>>[] container;


            double LoadFactor { get => Count / Capacity; }

            public int Count { get; private set; }
            public int Capacity { get; private set; }

            public Dictionary(int capacity = InitialCapacity)
            {
                Capacity = capacity;
                container = new List<KeyValuePair<TKey, TValue>>[capacity];
            }

            int GetIndex(TKey key)
            {
                if (key is null)
                {
                    throw new ArgumentNullException("key is null");
                }
                return (key.GetHashCode() % Capacity + Capacity) % Capacity;
            }

            int FindListIndexByKey(List<KeyValuePair<TKey, TValue>> list, TKey key) =>
                list is null
                ? -1
                : list.FindIndex(pair => pair.Key.Equals(key));

            int FindListIndexByValue (List<KeyValuePair<TKey, TValue>> list, TValue value) => 
                list is null
                ? -1
                : list.FindIndex(pair => pair.Value.Equals(value));

            TValue GetValue(TKey key)
            {
                var index = GetIndex(key);
                var list = container[index];
                var listIndex = FindListIndexByKey(list, key);
                if (listIndex < 0)
                {
                    throw new KeyNotFoundException("key doesn`t exist");
                }

                return list[listIndex].Value;
            }

            int TryGetListIndexOfEntry(TKey key, out List<KeyValuePair<TKey, TValue>> list)
            {
                int index = GetIndex(key);
                list = container[index]
                    ??= new List<KeyValuePair<TKey, TValue>>();
                return FindListIndexByKey(list, key);
            }

            void SetValue(TKey key, TValue value, bool allowModifyExisting = false)
            {
                var pair = new KeyValuePair<TKey, TValue>(key, value);
                if (!ContainsKey(key))
                {
                    if(LoadFactor > MaxLoadFactor)
                    {
                        Resize();
                    }
                    var listIndex = TryGetListIndexOfEntry(key, out var list);
                    list.Add(pair);
                    ++Count;
                }
                else if (allowModifyExisting)
                {
                    var listIndex = TryGetListIndexOfEntry(key, out var list);
                    list[listIndex] = pair;
                }
                else
                {
                    throw new ArgumentException("key already exists");
                }
            }

            public bool ContainsKey(TKey key)
            {
                int containerIndex = GetIndex(key);
                var listIndex = FindListIndexByKey(container[containerIndex], key);
                return listIndex >= 0;
            }

            public bool ContainsValue(TValue value)
            {
                int containerIndex = 0;
                int listIndex = -1;
                while(containerIndex < container.Length)
                {
                    listIndex = FindListIndexByValue(container[containerIndex], value);
                    ++containerIndex;
                }
                return listIndex >= 0;
            }

            public bool TryGetValue(TKey key, out TValue value)
            {
                var index = GetIndex(key);
                var list = container[index];
                var listIndex = FindListIndexByKey(list, key);
                var hasEntry = listIndex >= 0;
                value = hasEntry ? default : list[listIndex].Value;
                return hasEntry;
            }

            public void Add(TKey key, TValue value) => SetValue(key, value);

            public bool Remove(TKey key)
            {
                var index = GetIndex(key);
                var list = container[index];
                var listIndex = FindListIndexByKey(list, key);
                var hasEntry = listIndex >= 0;
                if(hasEntry)
                {
                    list.RemoveAt(listIndex);
                    --Count;
                }

                return hasEntry;
            }

            public void Clear()
            {
                foreach(var list in container)
                {
                    list.Clear();
                }
                Count = 0;
            }

            public TValue this[TKey key]
            {
                get => GetValue(key);
                set => SetValue(key, value, true);
            }

            void Resize()
            {
                var oldContainer = container;
                container = new List<KeyValuePair<TKey, TValue>>[Capacity *= 2];
                foreach(var list in oldContainer)
                {
                    foreach (var pair in list)
                    {
                        this[pair.Key] = pair.Value;
                    }
                }
            }
        }

        static void Main()
        {
            var dictionary = new Dictionary<string, int>(1);
            dictionary["a"] = 1;
            dictionary["b"] = 2;

            dictionary["b"] = 3;

            Console.WriteLine(dictionary["a"]);

            Console.WriteLine(dictionary["b"]);
        }
    }
}
