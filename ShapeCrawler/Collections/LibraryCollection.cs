﻿using System.Collections;
using System.Collections.Generic;

namespace ShapeCrawler.Collections
{
    /// <summary>
    ///     Represents a base class for all library collections.
    /// </summary>
    public class LibraryCollection<T> : IReadOnlyCollection<T> // TODO: make internal
    {
        #region Fields

        internal List<T> CollectionItems;

        #endregion Fields

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="LibraryCollection{T}"/> class.
        /// </summary>
        public LibraryCollection()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LibraryCollection{T}"/> class from paragpaths.
        /// </summary>
        public LibraryCollection(IEnumerable<T> paragraphItems)
        {
            this.CollectionItems = new List<T>(paragraphItems);
        }

        #endregion Constructors

        /// <summary>
        ///     Gets the number of series items in the collection.
        /// </summary>
        public int Count => this.CollectionItems.Count;
        
        /// <summary>
        ///     Gets the element at the specified index.
        /// </summary>
        public T this[int index] => this.CollectionItems[index];

        /// <summary>
        ///     Gets the generic enumerator that iterates through the collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return this.CollectionItems.GetEnumerator();
        }

        /// <summary>
        ///     Gets an enumerator that iterates through the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.CollectionItems.GetEnumerator();
        }
    }
}