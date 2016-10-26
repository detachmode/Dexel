using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;

namespace Dexel.Editor.Behavior
{

    public class FrameworkElementDropBehavior : Behavior<FrameworkElement>
    {
        private List<Type> allowedDropTypes; //the type of the data that can be dropped into this control


        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.AllowDrop = true;
            AssociatedObject.DragEnter += AssociatedObject_DragEnter;
            AssociatedObject.DragOver += AssociatedObject_DragOver;
            AssociatedObject.DragLeave += AssociatedObject_DragLeave;
            AssociatedObject.Drop += AssociatedObject_Drop;
        }


        private void AssociatedObject_Drop(object sender, DragEventArgs e)
        {
            CanItDropHere(e,
                canDrop: () =>
                {
                    var type = allowedDropTypes.First(t => e.Data.GetDataPresent(t));
                    var draggedItem = e.Data.GetData(type) as IDragable;

                    var droppedOn = AssociatedObject.DataContext as IDropable;
                    droppedOn?.Drop(draggedItem);                  
                });


            e.Handled = true;
        }


        


        private void AssociatedObject_DragLeave(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }


        private void AssociatedObject_DragOver(object sender, DragEventArgs e)
        {
           
            CanItDropHere(e,
                canDrop: () => e.Effects = DragDropEffects.Move,
                rejected:() => e.Effects = DragDropEffects.None);
            e.Handled = true;
        }


        private void CanItDropHere(DragEventArgs dragEventArgs, Action canDrop, Action rejected = null)
        {
            if (allowedDropTypes != null &&
                allowedDropTypes.Exists(type => dragEventArgs.Data.GetDataPresent(type)))
            {
                canDrop();
            }
            else
                rejected?.Invoke();
        }


        private void AssociatedObject_DragEnter(object sender, DragEventArgs e)
        {
            if (allowedDropTypes == null)
            {
                var dropObject = AssociatedObject.DataContext as IDropable;
                if (dropObject != null)
                {
                    allowedDropTypes = dropObject.AllowedDropTypes;
                }
            }

            e.Handled = true;
        }
    }

}