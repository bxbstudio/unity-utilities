#region Namespaces

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Utilities.Core
{
    [Serializable]
    public class SerializedDictionary<TKey, TValue>
    {
        #region Variables

        [SerializeField, HideInInspector]
        private List<TKey> keys = new List<TKey>();

        [SerializeField, HideInInspector]
        private List<TValue> values = new List<TValue>();

        #endregion

        #region Methods

        /// <summary>
        /// Converts the SerializedDictionary to a Dictionary.
        /// </summary>
        /// <returns>A Dictionary containing the same key-value pairs.</returns>
        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

            for (int i = 0; i < keys.Count; i++)
                dictionary.Add(keys[i], values[i]);

            return dictionary;
        }

        /// <summary>
        /// Converts a Dictionary to a SerializedDictionary.
        /// </summary>
        /// <param name="dictionary">The Dictionary to convert.</param>
        /// <returns>A new SerializedDictionary containing the same key-value pairs.</returns>
        public void FromDictionary(Dictionary<TKey, TValue> dictionary)
        {
            keys = new List<TKey>(dictionary.Keys);
            values = new List<TValue>(dictionary.Values);
        }

        /// <summary>
        /// Creates a new SerializedDictionary from a Dictionary.
        /// </summary>
        /// <param name="dictionary">The Dictionary to convert.</param>
        /// <returns>A new SerializedDictionary containing the same key-value pairs.</returns>
        public static SerializedDictionary<TKey, TValue> Create(Dictionary<TKey, TValue> dictionary)
        {
            if (dictionary == null)
                return null;

            return new SerializedDictionary<TKey, TValue>(dictionary);
        }

        #endregion

        #region Constructor

        private SerializedDictionary(Dictionary<TKey, TValue> dictionary)
        {
            keys = new List<TKey>(dictionary.Keys);
            values = new List<TValue>(dictionary.Values);
        }

        #endregion
    }
}
