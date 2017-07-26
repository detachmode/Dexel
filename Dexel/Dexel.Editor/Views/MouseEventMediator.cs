using System;
using System.Windows;
using System.Windows.Input;
using Dexel.Editor.Common;
using Dexel.Editor.ViewModels;
using Dexel.Editor.ViewModels.DrawingBoard;
using Dexel.Editor.Views.CustomControls;
using Dexel.Editor.Views.DragAndDrop;
using Dexel.Editor.Views.UserControls.DrawingBoard;
using Dexel.Library;
using Dexel.Model.DataTypes;

namespace Dexel.Editor.Views
{

    public static class MouseEventMediator
    {
        private const double DragThreshold = 5;
        private static bool _isLeftMouseButtonDownOnWindow;
        private static bool _isDraggingSelectionRect;
        public static Point OrigMouseDownPoint;
        public static Point ProjectedMousePosition;
        public static Point ScreenMousePosition;
        private static bool _isLeftMouseDownOnFunctionUnit;
        private static bool _isLeftMouseAndControlDownOnFunctionUnit;
        private static bool _isDraggingFunctionUnit;
        private static bool _isCTRLDraggingFunctionUnit;
        private static FunctionUnit _mouseDownOnFunctionUnit;


        public static void MouseDown(object sender, MouseButtonEventArgs e)
        {
            sender.TryCast<FunctionUnitView>(fu => FunctionUnitMouseDown(fu, e));
            sender.TryCast<UserControls.DrawingBoard.DrawingBoard>(board => DrawingBoardMouseDown(board, e));

            // reset picking in any case
            Interactions.PickState = false;
            Mouse.OverrideCursor = null;
        }


        private static void DrawingBoardMouseDown(UserControls.DrawingBoard.DrawingBoard sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (FrameworkElementDragBehavior.DragDropInProgressFlag)
            {
                FrameworkElementDragBehavior.DragDropInProgressFlag = false;
                return;
            }

            if (e.ChangedButton != MouseButton.Left) return;
            if (FrameworkElementDragBehavior.DragDropInProgressFlag) return;
            _isLeftMouseButtonDownOnWindow = true;

            var mainViewModel = sender.DataContext as MainViewModel;
            if (mainViewModel == null)
            {
                var functionUnitViewModel = (FunctionUnitViewModel)sender.DataContext;
                mainViewModel = functionUnitViewModel.MainViewModel;
            }
            mainViewModel.ClearSelection();
            sender.CaptureMouse();
        }


        public static void MouseUp(object sender, MouseButtonEventArgs e)
        {
            sender.TryCast<FunctionUnitView>(fu => FunctionUnitMouseUp(fu, e));
            sender.TryCast<UserControls.DrawingBoard.DrawingBoard>(board => DrawingBoardMouseUp(board, e));
            DraggingOFF();
        }


        private static void DrawingBoardMouseUp(UserControls.DrawingBoard.DrawingBoard sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left) return;

            var mainViewModel = sender.DataContext as MainViewModel;
            if (mainViewModel == null)
            {
                var functionUnitViewModel = (FunctionUnitViewModel)sender.DataContext;
                mainViewModel = functionUnitViewModel.MainViewModel;
            }

            mainViewModel.ClearSelection();
            ResetFocus();

            if (_isDraggingSelectionRect)
            {
                _isDraggingSelectionRect = false;
                sender.ApplyDragSelectionRect();
            }
            
            if (!_isLeftMouseButtonDownOnWindow) return;
            _isLeftMouseButtonDownOnWindow = false;
            sender.ReleaseMouseCapture();
        }


        private static void ResetFocus()
        {
            Keyboard.ClearFocus();

            //var mainWindow = ((MainWindow) Application.Current.MainWindow);
            //mainWindow.DrawingBoard.ResetView(); //Fix: TO-DO
            
        }


        public static void MouseMove(object sender, MouseEventArgs e)
        {
            FunctionUnitMouseMove(sender, e);
            sender.TryCast<UserControls.DrawingBoard.DrawingBoard>(board => DrawingBoardMouseMove(board, e));
        }


        private static void DrawingBoardMouseMove(UserControls.DrawingBoard.DrawingBoard drawingboard, MouseEventArgs e)
        {
            e.Handled = true;
            if (_isDraggingSelectionRect)
            {
                drawingboard.UpdateDragSelectionRect(OrigMouseDownPoint, ScreenMousePosition);
            }
            else if (_isLeftMouseButtonDownOnWindow)
            {
                var dragDelta = ScreenMousePosition - OrigMouseDownPoint;
                var dragDistance = Math.Abs(dragDelta.Length);
                if (dragDistance < DragThreshold) return;
                _isDraggingSelectionRect = true;
                drawingboard.InitDragSelectionRect(OrigMouseDownPoint, ScreenMousePosition);
            }
        }


        private static void FunctionUnitMouseDown(FunctionUnitView fuVm, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _mouseDownOnFunctionUnit = fuVm.ViewModel().Model;

            if (FrameworkElementDragBehavior.DragDropInProgressFlag)
                return;
            if (e.ChangedButton != MouseButton.Left)
                return;

            if (Interactions.PickState)
            {
                var mainViewModel = ((FunctionUnitViewModel)fuVm.DataContext).MainViewModel;
                Interactions.SetPickedIntegration(mainViewModel, fuVm.ViewModel().Model);
                return;
            }

            _isLeftMouseDownOnFunctionUnit = true;

            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                _isLeftMouseAndControlDownOnFunctionUnit = true;
            }
            else
            {
                _isLeftMouseAndControlDownOnFunctionUnit = false;
                var mainViewModel = ((FunctionUnitViewModel)fuVm.DataContext).MainViewModel;
                mainViewModel.SetSelection(fuVm.ViewModel());
            }

            fuVm.CaptureMouse();
        }


        private static void FunctionUnitMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isLeftMouseDownOnFunctionUnit)
            {
                ResetFocus();
                var fuView = (FrameworkElement) sender;
                var functionUnitViewModel = fuView.DataContext as FunctionUnitViewModel;
                if (functionUnitViewModel == null)
                    return;

                if (!_isDraggingFunctionUnit && !_isCTRLDraggingFunctionUnit)
                {
                    var mainViewModel = (MainViewModel)((FrameworkElement)sender).DataContext;
                    if (_isLeftMouseAndControlDownOnFunctionUnit)
                        mainViewModel.SetSelectionCTRLMod(functionUnitViewModel);
                    else
                        mainViewModel.SetSelection(functionUnitViewModel);
                }
               
                fuView.ReleaseMouseCapture();
                _isLeftMouseDownOnFunctionUnit = false;
                _isLeftMouseAndControlDownOnFunctionUnit = false;

                e.Handled = true;
            }

          
        }


        private static void DraggingOFF()
        {
            MyDebug.WriteLineIfDifferent("DraggingOFF");
            _isDraggingFunctionUnit = false;
            _isLeftMouseDownOnFunctionUnit = false;
            _isCTRLDraggingFunctionUnit = false;
        }


        // ReSharper disable once UnusedParameter.Local
        private static void FunctionUnitMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isLeftMouseDownOnFunctionUnit) return;
            e.Handled = true;

            var element = (FrameworkElement)sender;
            
            
           var functionUnitMainViewModel = (FunctionUnitViewModel) element.DataContext;
            

            //if (DragThresholdReached()) _isDraggingFunctionUnit = true;

            ModifiersKeysState(
                ctrlAndShift: () => DoCtrlShiftDraggingFunctionUnit(functionUnitMainViewModel.MainViewModel),
                onlyShift: () => DoShiftDraggingFunctionUnit(functionUnitMainViewModel.MainViewModel),
                onlyCtrl: () => _isCTRLDraggingFunctionUnit = true
                );

            _isDraggingFunctionUnit = true;

            if (_isCTRLDraggingFunctionUnit)
                DoCtrlDraggingFunctionUnit(functionUnitMainViewModel.MainViewModel);

            if (_isDraggingFunctionUnit)
                DraggingSelectedFunctionUnits(functionUnitMainViewModel.MainViewModel);
        }


        private static void DoCtrlShiftDraggingFunctionUnit(MainViewModel mainViewModel)
        {
            if (_isDraggingFunctionUnit)
                return;

            _isCTRLDraggingFunctionUnit = true;

            _mouseDownOnFunctionUnit =
                Interactions.DuplicateFunctionUnitIncludingChildrenAndIntegrated(mainViewModel, _mouseDownOnFunctionUnit);
        }


        private static void ModifiersKeysState(Action ctrlAndShift = null, Action onlyCtrl = null,
            Action onlyShift = null)
        {
            if (IsCtrlDown() && IsShiftDown())
                ctrlAndShift?.Invoke();

            if (IsShiftDown() && !IsCtrlDown())
                onlyShift?.Invoke();

            if (IsCtrlDown() && !IsShiftDown())
                onlyCtrl?.Invoke();
        }


        private static bool IsShiftDown()
        {
            return Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
        }


        private static bool IsCtrlDown()
        {
            return Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        }


        private static void DoShiftDraggingFunctionUnit(MainViewModel mainViewModel)
        {
            if (_isDraggingFunctionUnit)
                return;
            mainViewModel.DuplicateSelectionAndSelectNew();
        }


        private static void DraggingSelectedFunctionUnits(MainViewModel mainViewModel)
        {
            MyDebug.WriteLineIfDifferent($"DraggingSelectedFunctionUnits {_isDraggingFunctionUnit}");
            var dragDelta = ProjectedMousePosition - OrigMouseDownPoint;
            OrigMouseDownPoint = ProjectedMousePosition;
            mainViewModel.MoveSelectedFunctionUnit(dragDelta);
        }


        private static void DoCtrlDraggingFunctionUnit(MainViewModel mainViewModel)
        {
            var dragDelta = ProjectedMousePosition - OrigMouseDownPoint;
            OrigMouseDownPoint = ProjectedMousePosition;


            if (_mouseDownOnFunctionUnit == null) return;

            Interactions.MoveFunctionUnitIncludingChildrenAndIntegrated(
                _mouseDownOnFunctionUnit, dragDelta, mainViewModel.Model);
        }
    }

}