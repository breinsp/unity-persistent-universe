using System.Collections.Generic;
using System.Linq;

public class PriorityDictionary<T>
{
	private Dictionary<int, T> dictionary;
	private List<KeyPriority> keys;

	public PriorityDictionary()
	{
		dictionary = new Dictionary<int, T>();
		keys = new List<KeyPriority>();
	}

	public void Add(int key, T item, int priority)
	{
		dictionary[key] = item;
		foreach (var k in keys.ToList())
		{
			if (k.Key == key)
			{
				lock (keys)
				{
					keys.Remove(k);
				}
			}
		}
		lock (keys)
		{
			keys.Add(new KeyPriority { Key = key, Priority = priority });
		}
		OrderKeys();
	}

	public T Remove()
	{
		T item = default(T);
		if (keys.Count > 0)
		{
			lock (keys)
			{
				int key = keys[0].Key;
				item = dictionary[key];
				keys.RemoveAt(0);
				dictionary.Remove(key);
			}
		}
		return item;
	}

	public int Count { get { return dictionary.Count; } }

	public bool ContainsKey(int key)
	{
		return dictionary.ContainsKey(key);
	}

	private void OrderKeys()
	{
		keys.Sort((k1, k2) => k1.Priority.CompareTo(k2.Priority));
	}

	private struct KeyPriority
	{
		public int Key;
		public int Priority;
	}
}
