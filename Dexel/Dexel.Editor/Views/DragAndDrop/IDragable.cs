using System;

namespace Dexel.Editor.Views.DragAndDrop
{
    interface IDragable
    {
        /// <summary>
        /// Type of the data item
        /// </summary>
        Type DataType { get; }

    }
}
