using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Dexel.Library;

namespace Dexel.Editor.CustomControls
{

    public class SelectionItemsControl : ItemsControl
    {

        public SelectionItemsControl()
        {
            SelectedItems = new ObservableCollection<ISelectable>();

            SelectedItems.CollectionChanged += (sender, args) => Update();

        }


        private void Update()
        {
            foreach (var item in Items)
            {
                var i = (ISelectable) item;
                i.IsSelected = false;
            }
           
            SelectedItems.ForEach(x => x.IsSelected = true);
            foreach (ISelectable item in SelectedItems)
            {
                item.IsSelected = true;
            }
        }


        public static DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
                "SelectedItems",typeof(ObservableCollection<ISelectable>),typeof(SelectionItemsControl));

        public ObservableCollection<ISelectable> SelectedItems
        {
            get { return (ObservableCollection<ISelectable>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static DependencyProperty SelectedItemProperty = DependencyProperty.Register(
               "SelectedItem", typeof(ISelectable), typeof(SelectionItemsControl));

        public ISelectable SelectedItem
        {
            get { return (ISelectable)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        
    }

    public interface ISelectable
    {
        bool IsSelected { get; set; }
    }

}