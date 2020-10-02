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
using System.Drawing.Imaging;
using Newtonsoft.Json;

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
        SQLiteConnection connection = null;
        SQLiteCommand command = null;
        SQLiteDataReader reader;
        List<string> member;
        List<string> ID_List=new List<string>();
        public MainWindow()
        {
            InitializeComponent();
            videosourceplayer.BorderColor = System.Drawing.Color.Transparent;
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
            threadTimer.Change(1000, 3000);



        }

        private void refreshUI(object state)
        {
            string UpdateData= "UPDATE 'check' SET time=time+1 where id=";

            if (ID_List.Count != 0)
            {
                foreach (string ID in ID_List)
                {
                    if (ID_List[ID_List.Count - 1] == ID)
                    {
                        UpdateData += ID;
                        break;
                    }
                    UpdateData += ID + ",";
                }
            }
            
            member.Clear();
            reader.Close();
            if (ID_List.Count!=0)
            {
                command.CommandText = UpdateData;
                command.ExecuteNonQuery();
            }
            
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

        private void Check(object sender, RoutedEventArgs e)//考勤点击事件
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
            notice.Content="Tips:请使面部处于画面中央，不要移动";
            btn.IsEnabled = false;

            Thread thread = new Thread(new ThreadStart(Face));
            thread.Priority = ThreadPriority.Highest;
            thread.Start();
            

        }

        private void Face()
        {
            Thread.Sleep(500);
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
                req.MaxPersonNum = 1;
                req.Image = ToBase64();
                req.GroupIds = new string[] { "0" };
                SearchFacesResponse resp = client.SearchFacesSync(req);
                string jsonObject = AbstractModel.ToJsonString(resp);
                FaceResult convert = JsonConvert.DeserializeObject<FaceResult>(jsonObject);
                
                string id = convert.Results[0].Candidates[0].PersonId;
                float score = convert.Results[0].Candidates[0].Score;
                if (ID_List.Exists(t => t == id))
                {
                    Dispatcher.Invoke(new Action(delegate
                    {
                        messages.Text = "签退成功!";
                    }));
                    ID_List.Remove(id);
                }
                else
                {
                    ID_List.Add(id);
                    Dispatcher.Invoke(new Action(delegate
                    {
                        messages.Text = "签到成功!";
                    }));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                Dispatcher.Invoke(new Action(delegate
                {
                    btn.Content = "考勤";
                    btn.IsEnabled = true;
                    videosourceplayer.Stop();
                }));
            }
        }
    }
    class FaceResult
    {
        public List<Results> Results { get; set; }
    }
    public class Results
    {
        public List<Candidates> Candidates { get; set; }
    }
    public class Candidates
    {
        public string PersonId { get; set; }
        public float Score { get; set; }
    }
}
