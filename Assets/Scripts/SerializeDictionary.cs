using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class SerializeDictionary<TKey, TValue>
{
    [SerializeField] private List<DicElement> _dicElements = new List<DicElement>();
    private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

    [Serializable]
    public class DicElement
    {
        public TKey Key;
        public TValue Value;

        public DicElement(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    public Dictionary<TKey, TValue> GetDictionary
    {
        get
        {
            if(_dicElements != null)
            {
                _dictionary.Clear();
                foreach(var element in _dicElements)
                {
                    _dictionary.Add(element.Key, element.Value);
                }
                return _dictionary;
            }
            return null;
        }
    }

}