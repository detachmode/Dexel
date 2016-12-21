using System;
using System.Collections.Generic;

namespace Dexel.Editor.Views.DragAndDrop
{

    public interface IDropable
    {
        /// <summary>
        /// Type of the data item
        /// </summary>
        List<Type> AllowedDropTypes { get; }


        /// <summary>
        /// Drop data into the collection.
        /// </summary>
        /// <param name="data">The data to be dropped</param>
        void Drop(object data);
    }
}
