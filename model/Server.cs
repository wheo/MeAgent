using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MeAgent.config;
using MeAgent.model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MeAgent.model
{
    internal class Server : INotifyPropertyChanged
    {
        private Logitem log { get; set; }

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

        public int id { get; set; }

        public bool isDanger { get; set; } = false;

        public bool isFreeze { get; set; } = false;

        public UInt16 errorCounter { get; set; }

        private UInt16 _freeze_varianceCounter;

        public UInt16 freeze_varianceCounter
        {
            get { return _freeze_varianceCounter; }
            set
            {
                if (this._freeze_varianceCounter != value)
                {
                    this._freeze_varianceCounter = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("freeze_varianceCounter"));
                }
            }
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

        public string ip { get; set; }
        private string _name;

        public string name
        {
            get { return this._name; }
            set
            {
                if (this._name != value)
                {
                    this._name = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("name"));
                }
            }
        }

        public double throughput { get; set; }

        public int status { get; set; }

        public enum EnumStatus
        {
            fail = 0,
            idle = 1,
            _pending = 2,
            pending = 3,
            working = 4,
            stopping = 5
        }

        private int s { get; set; }

        private double _bps;

        public double bps
        {
            get
            {
                return _bps;
            }
            set
            {
                if (this._bps != value)
                {
                    this._bps = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("bps"));
                }
            }
        }

        public ObservableCollection<Detail> detail { get; set; }

        public string connString { get; set; }

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

        public static List<Server> GetServerList()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT _id, ip, name, CAST(status AS int) as status, throughput FROM servers ORDER BY name ASC";
            using (MySqlConnection conn = new MySqlConnection(DataBaseManager.getInstance().ConnectionString))
            {
                conn.Open();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }

            return dt.AsEnumerable().Select(row => new Server
            {
                id = row.Field<int>("_id"),
                ip = row.Field<string>("ip"),
                name = row.Field<string>("name"),
                status = row.Field<int>("status"),
                throughput = row.Field<double>("throughput"),
                connString = String.Format($"server={row.Field<string>("ip")};port=3306;uid=tnmtech;pwd=tnmtech;database=tme;charset=utf8mb4;SslMode=none"),
            }).ToList();
        }

        public List<Detail> GetDetail()
        {
            DataTable dt = new DataTable();
            string query = string.Format(@"SELECT C.chnum
, C.name as name
, P._id
, P.name as profile_name
, P.ntrack
, P.codec as vcodec
, P.profile
, P.width
, P.height
, P.fpsn
, P.fpsd
, P.bitrate AS vrate
, O.dst_ip
, O.dst_port
, P.muxrate
, S.bps
, S.variance
 FROM channels C
 LEFT JOIN outputs O ON C.out_id = O._id
 LEFT JOIN profiles P ON C.pf_id = P._id
 LEFT JOIN status S ON C.chnum = S.channel AND C.pfnum = S.profile
 WHERE C.enable = 1
 AND C.t_id = (SELECT S.tid FROM system S WHERE S._id=1)");
            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                conn.Open();
                MySqlDataAdapter adpt = new MySqlDataAdapter(query, conn);
                adpt.Fill(dt);
            }

            return dt.AsEnumerable().Select(row => new Detail
            {
                name = row.Field<string>("name"),
                profile_name = row.Field<string>("profile_name"),
                bps = row.Field<double>("bps"),
                variance = row.Field<double>("variance")
            }).ToList();
        }
    }
}