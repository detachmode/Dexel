using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign
{

    public static class Interactions
    {

        public static void AddNewIOCell(Point pos)
        {
            var cell = new IOCellViewModel {IsSelected = true};

            pos.X -= 100;
            pos.Y -= 20;
            cell.Position = new Point(pos.X, pos.Y);

            MainViewModel.Instance().SoftwareCells.Add(cell);
        }


        public static void DeselectAll()
        {
            MainViewModel.Instance().SoftwareCells.ToList().ForEach( i => i.Deselect());
        }

        internal static void OnItemDragged(IOCellViewModel cellvm, DragDeltaEventArgs dragDeltaEventArgs)
        {
            cellvm.Move(dragDeltaEventArgs.HorizontalChange, dragDeltaEventArgs.VerticalChange);
        }

        public static void DragSelection(DragDeltaEventArgs e)
        {
            GetSelection().ToList().ForEach(c => c.Move(e.HorizontalChange,e.VerticalChange));
        }



        public static IEnumerable<IOCellViewModel> GetSelection()
        {
            return MainViewModel.Instance().SoftwareCells.Where(c => c.IsSelected);
        }


        public static void ToggleSelection(IOCellViewModel cellvm)
        {

            if (cellvm.IsSelected)
            {
                cellvm.Deselect();
            }
            else
            {
                cellvm.Select();
            }
        }



        public enum DragMode
        {
            Single,
            Multiple
        }


        public static void AddNewConnectionNoDestination(IOCellViewModel ioCellViewModel)
        {
            MainViewModel.Instance().TemporaryConnection = new ConnectionViewModel(ioCellViewModel, null)
            {
                IsDragging = true
            };
        }
    }




}