using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using SharpFlowDesign.UserControls;
using SharpFlowDesign.ViewModels;

namespace SharpFlowDesign
{

    public static class Interactions
    {
        private static MainViewModel _vm;
        private static DragMode _dragMode;


        public static void AddNewIOCell(Point pos)
        {
            var cell = new IOCellViewModel {Input = "neuer input"};
            cell.IsSelected = true;

            pos.X -= 100;
            pos.Y -= 20;
            cell.Position = new Point(pos.X, pos.Y);

            _vm.Items.Add(cell);
        }


        public static void DeselectAll()
        {
            _vm.Items.ToList().ForEach( i => i.Deselect());
        }

        internal static void OnItemDragged(IOCellViewModel cellvm, DragDeltaEventArgs dragDeltaEventArgs)
        {

            if (_dragMode == DragMode.Single)
            {
                DeselectAll();
                cellvm.Select();
                cellvm.Move(dragDeltaEventArgs.HorizontalChange, dragDeltaEventArgs.VerticalChange);
            }

            if (_dragMode == DragMode.Multiple)
            {
                DragSelection(dragDeltaEventArgs);
            }

        }

        public static void DragSelection(DragDeltaEventArgs e)
        {
            GetSelection().ToList().ForEach(c => c.Move(e.HorizontalChange,e.VerticalChange));
        }



        public static IEnumerable<IOCellViewModel> GetSelection()
        {
            return _vm.Items.Where(c => c.IsSelected);
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