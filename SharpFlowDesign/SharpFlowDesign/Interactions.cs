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
        private static MainViewModel _vm;
        private static DragMode _dragMode;


        public static void AddNewIOCell(Point pos)
        {
            var cell = new IOCellViewModel();
            cell.IsSelected = true;

            pos.X -= 100;
            pos.Y -= 20;
            cell.Position = new Point(pos.X, pos.Y);

            _vm.SoftwareCells.Add(cell);
        }


        public static void DeselectAll()
        {
            _vm.SoftwareCells.ToList().ForEach( i => i.Deselect());
        }

        internal static void OnItemDragged(IOCellViewModel cellvm, DragDeltaEventArgs dragDeltaEventArgs)
        {
            cellvm.Move(dragDeltaEventArgs.HorizontalChange, dragDeltaEventArgs.VerticalChange);
//            if (_dragMode == DragMode.Single)
//            {
//                DeselectAll();
//                cellvm.Select();
//                
//            }
//
//            if (_dragMode == DragMode.Multiple)
//            {
//                DragSelection(dragDeltaEventArgs);
//            }

        }

        public static void DragSelection(DragDeltaEventArgs e)
        {
            GetSelection().ToList().ForEach(c => c.Move(e.HorizontalChange,e.VerticalChange));
        }



        public static IEnumerable<IOCellViewModel> GetSelection()
        {
            return _vm.SoftwareCells.Where(c => c.IsSelected);
        }


        public static void SetViewModel(MainViewModel dataContext)
        {
            _vm = dataContext;
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


        public static void DecideDragMode(IOCellViewModel ioCellViewModel)
        {

            _dragMode = ioCellViewModel.IsSelected ? DragMode.Multiple : DragMode.Single;
        }


        public enum DragMode
        {
            Single,
            Multiple
        }
    }




}