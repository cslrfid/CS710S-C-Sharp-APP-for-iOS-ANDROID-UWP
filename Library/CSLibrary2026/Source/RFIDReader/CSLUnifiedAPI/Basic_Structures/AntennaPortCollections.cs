using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary.Structures
{
    using CSLibrary.Constants;
    /// <summary>
    /// Antenna Port collections
    /// </summary>
    public class AntennaPortCollections : List<AntennaPort>
    {
        /// <summary>
        /// Initializes a new instance of the AntennaPortCollections class
        /// that is empty and has the default initial capacity.
        /// </summary>
        public AntennaPortCollections() : base() { }
        /// <summary>
        /// Initializes a new instance of the AntennaPortCollections class
        /// that is empty and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new list can initially store.</param>
        public AntennaPortCollections(int capacity)
            : base(capacity)
        {

        }
        /// <summary>
        /// Initializes a new instance of the AntennaPortCollections class
        /// that contains elements copied from the specified collection and has sufficient
        /// capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        public AntennaPortCollections(IEnumerable<AntennaPort> collection)
            : base(collection)
        {

        }
    }
}
