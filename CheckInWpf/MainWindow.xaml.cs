using System;
using System.IO;
using System.Windows;
using System.Data.SQLite;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Threading;
using AForge.Video.DirectShow;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Iai.V20200303.Models;
using TencentCloud.Iai.V20200303;
using System.Runtime.Serialization.Formatters.Binary;
using System.Drawing.Imaging;

namespace CheckInWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string DB_PATH;//数据库文件路径
        string query = "SELECT * FROM 'check' order by time desc";
        string addtime = "UPDATE 'check' SET time=time+1";
        static int count=0;
        SQLiteConnection connection = null;
        SQLiteCommand command = null;
        SQLiteDataReader reader;
        List<string> member;
        public MainWindow()
        {
            InitializeComponent();
            DB_PATH = @"f:\checkin.d";
            
            if (!File.Exists(DB_PATH))
            {
                connection = new SQLiteConnection("data source="+DB_PATH);
                connection.Open();
            }
            else
            {
                connection = new SQLiteConnection("data source="+DB_PATH);
                connection.Open();
            }

            
            command = new SQLiteCommand(query,connection);
            reader = command.ExecuteReader();

            member=new List<string>();
            for(int i=0; reader.Read();i++)
            {
                member.Add(reader.GetString(1)+":  "+reader.GetInt32(2)+" min");
            }
            list.ItemsSource = member;
            list.FontSize = 24;
            date.Content = DateTime.Now.ToShortTimeString();
            Timer threadTimer = new Timer(refreshUI);
            threadTimer.Change(1000, 1000);
            
            
            
        }

        private void refreshUI(object state)
        {
            member.Clear();
            reader.Close();
            command.CommandText = addtime ;
            command.ExecuteNonQuery();
            command.CommandText = query;
            reader = command.ExecuteReader();
            for (int i = 0; reader.Read(); i++)
            {
                member.Add(reader.GetString(1) + ":  " + reader.GetInt32(2) + " min");
            }
            try
            {
                Dispatcher.Invoke(new Action(delegate
                            {
                                list.ItemsSource = null;
                                list.ItemsSource = member;
                                date.Content = DateTime.Now.ToShortTimeString();
                            }));
            }catch (Exception e)
            {
                
            }
        }

        string ToBase64()
        {
            System.Drawing.Image img = System.Drawing.Image.FromHbitmap(videosourceplayer.GetCurrentVideoFrame().GetHbitmap());
            img.Save("f:\\face.cache",ImageFormat.Png);
            MemoryStream memStream = new MemoryStream();
            System.Drawing.Image image = System.Drawing.Image.FromFile("f:\\face.cache");
            image.Save(memStream,image.RawFormat);
            byte[] imageBytes = memStream.ToArray();
            return Convert.ToBase64String(imageBytes);
        }

        private void check(object sender, RoutedEventArgs e)
        {
            FilterInfoCollection videoDevices;
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                MessageBox.Show("未发现视频设备！");
                return;
            }

            VideoCaptureDevice source = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videosourceplayer.SignalToStop();
            videosourceplayer.WaitForStop();
            videosourceplayer.VideoSource = source;
            videosourceplayer.Start();
            btn.Content = "正在识别";
            notice.Content="Tips:请使面部处于画面中央";
            btn.IsEnabled = false;

            Timer timer = new Timer(face);
            timer.Change(1000, 1000);
            for(; ; )
            {
                if(count==1)
                {
                    timer.Dispose();
                }
            }
        }

        private void face(object state)
        {
            try
            {
                Credential cred = new Credential
                {
                    SecretId = "AKIDAHE4whBesvRySnu85qTzOgqocIn94KPO",
                    SecretKey = "yrZ5RlvJZiiXHwRmuujR2b3KPJ2e9Vk0",
                };

                ClientProfile clientProfile = new ClientProfile();
                clientProfile.SignMethod = "TC3-HMAC-SHA256";
                HttpProfile httpProfile = new HttpProfile();
                httpProfile.Endpoint = ("iai.tencentcloudapi.com");
                clientProfile.HttpProfile = httpProfile;

                IaiClient client = new IaiClient(cred, "ap-beijing", clientProfile);
                SearchFacesRequest req = new SearchFacesRequest();
                req.Image = ToBase64();
                req.GroupIds = new string[] { "0" };
                SearchFacesResponse resp = client.SearchFacesSync(req);

                MessageBox.Show(AbstractModel.ToJsonString(resp));
                btn.Content = "签到";
                btn.IsEnabled = true;
                count++;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
