using System;
using System.Collections.Generic;
using Dexel.Editor.ViewModels;

namespace Dexel.Editor.Views.DragAndDrop
{

    public interface IDropable
    {
        List<Type> AllowedDropTypes { get; }
        void Drop(object data);
    }
}
