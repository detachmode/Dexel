using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SharpFlowDesign.UserControls;

namespace SharpFlowDesign.ViewModels
{
    public class IOCellViewModel : INotifyPropertyChanged
    {
        private string name;
        private Point position;
        private string input;
        private string output;

        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
            }
        }

        public string Input
        {
            get
            {
                return input;
            }

            set
            {
                input = value;
            }
        }

        public string Output
        {
            get
            {
                return output;
            }

            set
            {
                output = value;
            }
        }

        public Point Position
        {
            get
            {
                return position;
            }

            set
            {
                if (value == position) return;
                position = value;
                OnPropertyChanged();
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get
            {
                return isSelected;
            }

            set
            {
                if (value == isSelected) return;
                isSelected = value;
                OnPropertyChanged();

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        public void Move(double x, double y)
        {
            var pos = this.Position;
            pos.X += x;
            pos.Y += y;
            this.Position = pos;
        }



        public void Deselect()
        {
            this.IsSelected = false;
        }


        public void Select()
        {
            this.IsSelected = true;
        }
    }
}
