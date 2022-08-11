using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeAgent.model
{
    internal class Detail : INotifyPropertyChanged
    {
        public bool isDanger { get; set; } = false;

        public UInt16 errorCounter { get; set; }

        public double bps { get; set; }

        private Logitem log { get; set; }

        public bool changed { get; set; } = true;

        public Logitem GetLog()
        {
            if (log == null)
            {
                log = new Logitem();
                return log;
            }
            else
            {
                return log;
            }
        }

        public void ClearLog()
        {
            log = null;
        }

        private string _color;

        public string color
        {
            get { return _color; }

            set
            {
                if (this._color != value)
                {
                    this._color = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("color"));
                }
            }
        }

        private int _thickness;

        public int thickness
        {
            get
            {
                return _thickness;
            }
            set
            {
                if (this._thickness != value)
                {
                    this._thickness = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("thickness"));
                }
            }
        }

        private string _name;

        public string name
        {
            get { return _name; }
            set
            {
                if (!value.Equals(this._name))
                {
                    this._name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("name"));
                }
            }
        }

        public string profile_name { get; set; }
        private double _variance;

        public double variance
        {
            get { return this._variance; }
            set
            {
                Console.WriteLine(value);
                if (this._variance != value)
                {
                    this._variance = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("variance"));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
                /*
                if (e.PropertyName.Equals("Status") ||
                    e.PropertyName.Equals("Name") ||
                        e.PropertyName.Equals("Location"))
                {
                    //특정 이름일 때 호출
                }
                */
            }
        }
    }
}