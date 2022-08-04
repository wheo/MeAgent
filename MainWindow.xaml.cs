using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using MeAgent.config;
using MeAgent.model;
using log4net;
using Newtonsoft.Json;
using System.Windows.Threading;
using MeAgent.global;
using System.Collections.ObjectModel;
using Microsoft.Win32;
using System.Media;
using System.Drawing;
using Brushes = System.Windows.Media.Brushes;

namespace MeAgent
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private DispatcherTimer _dispatchertimer = null;
        private int _counter = 0;
        private bool _isAlive = false;
        private Settings _settings = null;
        private SoundPlayer _soundPlayer = null;

        private bool _alarm = true;

        public MainWindow()
        {
            InitializeComponent();
            LoadConfig();
            //Refresh();

            _isAlive = true;
        }

        private bool LoadConfig()
        {
            logger.Info("MeAgent is Starting...");
            try
            {
                _settings = new Settings();
                if (!File.Exists(_settings.configFileName))
                {
                    MessageBox.Show(string.Format($"{_settings.configFileName} 파일이 없습니다.\n환경설정 파일을 읽지 못했습니다.\n기본값으로 설정합니다."), "경고", MessageBoxButton.OK);
                    _settings.ip = "172.21.220.124";
                    _settings.port = 3306;
                    _settings.id = "tnmtech";
                    _settings.pw = "tnmtech";
                    _settings.DatabaseName = "tmecon";
                }
                else
                {
                    using (StreamReader file = File.OpenText(_settings.configFileName))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        _settings = (Settings)serializer.Deserialize(file, typeof(Settings));
                    }
                }

                logger.Info(string.Format($"min variance is {_settings.min_variance}"));
                DataBaseManager.getInstance().SetConnectionString(_settings.ip, _settings.port, _settings.id, _settings.pw, _settings.DatabaseName);

                _settings.Save();
                txtAlarm.Text = _settings.alarmpath;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "프로그램을 종료합니다", MessageBoxButton.OK);
            }
            return true;
        }

        private void Binding()
        {
#if false
            List<Server> ss = new List<Server>();

            for (int i = 0; i < 200; i++)
            {
                Server s = new Server { ip = "192.168.2.66", name = "테스트", bps = 45, throughput = 10, connString = "server=192.168.2.66;port=3306;uid=tnmtech;pwd=tnmtech;database=tme;charset=utf8mb4;SslMode=none" };
                s.detail = new ObservableCollection<Detail>();
                s.detail = new ObservableCollection<Detail>(s.GetDetail());
                ss.Add(s);
            }

#else
            List<Server> ss = null;
            try
            {
                ss = Server.GetServerList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                logger.Error(ex.ToString());
                Environment.Exit(0);
            }

#endif
            if (ss != null)
            {
                ViewMoodel.GetInstance().servers = new ObservableCollection<Server>(ss);

                GetServerDetail();

                trvServer.ItemsSource = ViewMoodel.GetInstance().servers;
                LvActive.ItemsSource = ViewMoodel.GetInstance().GetActiveLogs();
                LvHistory.ItemsSource = ViewMoodel.GetInstance().GetHistoryLogs();
            }
            else
            {
                MessageBox.Show("NMS 정보가 로드되지 않았습니다 프로그램을 종료합니다", "경고");
                Environment.Exit(0);
            }
        }

        private void DispatcherTimer()
        {
            _dispatchertimer = new DispatcherTimer(DispatcherPriority.ContextIdle);
            _dispatchertimer.Tick += new EventHandler(Service);
            _dispatchertimer.Interval = TimeSpan.FromSeconds(1);
            _dispatchertimer.Start();
        }

        private void GetServerDetail()
        {
            //UserInterface.GetInstance().servers = new ObservableCollection<Server>(Server.GetServerList());
            for (int i = 0; i < ViewMoodel.GetInstance().servers.Count(); i++)
            {
                Server server = ViewMoodel.GetInstance().servers[i];
                logger.Info(string.Format($"({server.ip}) {server.name}"));
                try
                {
                    server.detail = new ObservableCollection<Detail>(server.GetDetail());
                }
                catch (Exception ex)
                {
                    logger.Error(ex.Message);
                }
            }
        }

        private Server FindServerID(List<Server> servers, int id)
        {
            IEnumerable<Server> s = from x in servers
                                    where x.id == id
                                    select x;
            return s.FirstOrDefault();
        }

        private void Service(object sender, EventArgs e)
        {
            //logger.Info(++_counter);

            List<Server> newestServer = null;
            try
            {
                newestServer = Server.GetServerList();
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }

            for (int i = 0; i < ViewMoodel.GetInstance().servers.Count(); i++)
            {
                Server server = ViewMoodel.GetInstance().servers[i];
                Server new_server = FindServerID(newestServer, server.id);

                if (new_server != null)
                {
                    server.status = new_server.status;
                }

                List<Detail> newestDetail = null;
                try
                {
                    newestDetail = server.GetDetail();
                    server.bps = newestDetail.Sum(x => x.bps);

                    if (server.throughput >= server.bps)
                    {
                        if (server.status == (int)Server.EnumStatus.working)
                        {
                            server.errorCounter++;
                            logger.Info(string.Format($"({server.name}) status({((Server.EnumStatus)server.status).ToString()} ({server.throughput})Mbps e_cnt({server.errorCounter}))"));
                        }
                        if (!server.isDanger && server.errorCounter > _settings.limit_error_count)
                        {
                            server.isDanger = true;
                            server.color = "Red";
                            server.thickness = 4;
                            server.GetLog().ip = server.ip;
                            server.GetLog().name = server.name;
                            server.GetLog().value = string.Format($"({server.bps}) throughput is danger !!!");
                            ViewMoodel.GetInstance().activeLogs.Add(server.GetLog());
                            logger.Info(string.Format($"(begin)({server.ip})({server.name}) status({((Server.EnumStatus)server.status).ToString()}) ({server.throughput})Mbps e_cnt({server.errorCounter}))"));

                            if (_alarm)
                            {
                                PlaySound();
                            }
                        }
                    }
                    else
                    {
                        if (server.isDanger)
                        {
                            server.isDanger = false;
                            server.errorCounter = 0;
                            ViewMoodel.GetInstance().activeLogs.Remove(server.GetLog());
                            //ViewMoodel.GetInstance().historyLogs.Insert(0, server.GetLog());
                            ViewMoodel.GetInstance().historyLogs.InsertItem(0, server.GetLog());
                            server.ClearLog();
                            logger.Info(string.Format($"(end)({server.ip})({server.name}) status({((Server.EnumStatus)server.status).ToString()}) ({server.throughput})Mbps e_cnt({server.errorCounter}))"));
                        }
                        if (server.detail.Count(x => x.isDanger == true) == 0)
                        {
                            //서버의 모든 이슈가 해소 되었을 때
                            server.color = "";
                            server.thickness = 0;
                        }
                        else
                        {
                            server.color = "Yellow";
                            server.thickness = 2;
                        }
                    }
                    for (int j = 0; j < server.detail.Count(); j++)
                    {
                        server.detail[j].name = newestDetail[j].name;
                        server.detail[j].profile_name = newestDetail[j].profile_name;
                        server.detail[j].variance = newestDetail[j].variance;

                        if (server.detail[j].variance <= _settings.min_variance)
                        {
                            if (server.status == (int)Server.EnumStatus.working)
                            {
                                server.detail[j].errorCounter++;
                                logger.Info(string.Format($"({server.name})-({server.detail[j].profile_name}) status({((Server.EnumStatus)server.status).ToString()}) variance({server.detail[j].variance}) e_cnt({server.detail[j].errorCounter}))"));
                            }

                            if (!server.detail[j].isDanger && server.detail[j].errorCounter > _settings.limit_error_count)
                            {
                                server.detail[j].isDanger = true;
                                server.detail[j].color = "Red";
                                server.detail[j].thickness = 4;
                                server.detail[j].GetLog().ip = server.ip;
                                server.detail[j].GetLog().name = server.detail[j].profile_name;
                                server.detail[j].GetLog().value = string.Format($"({server.name}-{server.detail[j].name}) ({server.detail[j].variance}) variance is danger !!!");
                                logger.Info(string.Format($"(begin)({server.ip})({server.name})-({server.detail[j].profile_name}) status({((Server.EnumStatus)server.status).ToString()}) variance({server.detail[j].variance}) e_cnt({server.detail[j].errorCounter}))"));
                                ViewMoodel.GetInstance().activeLogs.Add(server.detail[j].GetLog());
                                // 알람
                                if (_alarm)
                                {
                                    PlaySound();
                                }
                            }
                        }
                        else
                        {
                            if (server.detail[j].isDanger)
                            {
                                server.detail[j].isDanger = false;
                                server.detail[j].errorCounter = 0;
                                ViewMoodel.GetInstance().activeLogs.Remove(server.detail[j].GetLog());
                                ViewMoodel.GetInstance().historyLogs.InsertItem(0, server.detail[j].GetLog());
                                server.detail[j].ClearLog();
                                logger.Info(string.Format($"(end)({server.ip})({server.name})-({server.detail[j].profile_name}) status({((Server.EnumStatus)server.status).ToString()}) variance({server.detail[j].variance}) e_cnt({server.detail[j].errorCounter}))"));
                            }
                            server.detail[j].color = "";
                            server.detail[j].thickness = 0;
                            if (server.detail.Count(x => x.isDanger == true) == 0)
                            {
                                //모든 서버의 이슈가 해소 되었을 때
                                server.color = "";
                                server.thickness = 0;
                            }
                            else
                            {
                                server.color = "Yellow";
                                server.thickness = 2;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex.ToString());
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Binding();
            DispatcherTimer();
        }

        private void BtnAck_Click(object sender, RoutedEventArgs e)
        {
            StopSound();
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Sound (*.WAV;*.MP3)|*.WAV;*.MP3";
            Nullable<bool> result = dlg.ShowDialog();
            if (result == true)
            {
                string filename = dlg.FileName;
                string subpath = "alarm";

                logger.Info(string.Format($"Upload Alarm File Ready ({filename})"));
                if (File.Exists(filename))
                {
                    if (!Directory.Exists(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, subpath)))
                    {
                        Directory.CreateDirectory(System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, subpath));
                    }
                    string destfilepath = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, subpath, System.IO.Path.GetFileName(filename));
                    File.Copy(filename, destfilepath, true);
                    if (System.IO.Path.GetExtension(destfilepath).ToLower() == ".mp3")
                    {
                        Util.ConvertMp3ToWav(destfilepath);
                    }
                    _settings.alarmpath = System.IO.Path.Combine(subpath, System.IO.Path.ChangeExtension(System.IO.Path.GetFileName(filename), "WAV"));
                    logger.Info(string.Format($"{_settings.alarmpath} is uploaded"));
                    txtAlarm.Text = _settings.alarmpath;
                    _settings.Save();

                    MessageBox.Show("업로드 완료", "정보", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void PlaySound()
        {
            if (File.Exists(_settings.alarmpath))
            {
                Task.Run(() => PlaySoundAsync(_settings.alarmpath));
            }
            else
            {
                //MessageBox.Show("알람 파일이 없습니다\n알람을 업로드 해주세요", "경고");
            }
        }

        private async Task PlaySoundAsync(string path)
        {
            if (_soundPlayer == null)
            {
                _soundPlayer = new SoundPlayer(path);
                _soundPlayer.PlayLooping();
                TimeSpan t = new TimeSpan(0, 0, 0, _settings.alarmsec);
                await Task.Delay(t);
                if (_soundPlayer != null)
                {
                    _soundPlayer.Stop();
                    _soundPlayer = null;
                }
            }
        }

        private void StopSound()
        {
            if (_soundPlayer != null)
            {
                _soundPlayer.Stop();
                _soundPlayer = null;
            }
        }

        private void btnPreviewSound_Click(object sender, RoutedEventArgs e)
        {
            PlaySound();
        }

        private void btnReload_Click(object sender, RoutedEventArgs e)
        {
            _dispatchertimer.Stop();
            _dispatchertimer = null;
            ServerClear();
            Binding();
            DispatcherTimer();
        }

        private void ServerClear()
        {
            ViewMoodel.GetInstance().servers.Clear();
            ViewMoodel.GetInstance().servers = null;
            ViewMoodel.GetInstance().activeLogs.Clear();
            ViewMoodel.GetInstance().activeLogs = null;
            ViewMoodel.GetInstance().historyLogs.Clear();
            ViewMoodel.GetInstance().historyLogs = null;
        }

        private void btnToggleAlarm_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            _alarm = !_alarm;
            if (_alarm)
            {
                btn.Content = "Alarm ON";
                btn.Background = Brushes.Red;
            }
            else
            {
                btn.Content = "Alarm OFF";
                btn.Background = Brushes.Blue;
            }
        }
    }
}