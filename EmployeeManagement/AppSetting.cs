﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using MyDictionary = System.Collections.Generic.Dictionary<string, string>;

namespace EmployeeManagementApplicationSetting
{
    public partial class AppSetting : Form
    {
        public string cameraUrl = "";
        public string imageDir = "";
        public string connectionString = "";
        public bool isDisplayTime = false;
        public int minTimeBetweenScanSteps = 3600; //second
        public bool consoleEnabled = false;

        //DB
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Store config
        MyDictionary globalDictionary = new MyDictionary();
        Configuration config;

        private string[] cameralist = { ""};
        //private string[] cameralist = {
        //    "rtsp://192.168.1.119:554/user=admin_password=tlJwpbo6_channel=1_stream=0.sdp?real_stream",
        //    "rtsp://192.168.1.117:554/user=admin_password=tlJwpbo6_channel=1_stream=0.sdp?real_stream"
        //    };

        //Button OK clicked
        public bool isOKButtonClicked = false;

        public AppSetting()
        {
            InitializeComponent();
            intialValue();

            config = new Configuration();

            updateAllFormValueFromDictionary(config.loadDictionaryFromFile());

            // Db tester + prepgress bar
            progressbarWorker.WorkerReportsProgress = true;
            progressbarWorker.WorkerSupportsCancellation = true;
            progressBar1.Visible = false;
        }

        void updateAllFormValueFromDictionary(MyDictionary readConfigDict)
        {
            if (readConfigDict.ContainsKey("camip") &&  readConfigDict["camip"] != "")
            {
                txtIp.Text = readConfigDict["camip"];
            }
            if (readConfigDict.ContainsKey("camport") && readConfigDict["camport"] != "")
            {
                txtPort.Text = readConfigDict["camport"];
            }
            if (readConfigDict.ContainsKey("camuser") && readConfigDict["camuser"] != "")
            {
                txtUser.Text = readConfigDict["camuser"];
            }
            if (readConfigDict.ContainsKey("campass") && readConfigDict["campass"] != "")
            {
                txtPasswd.Text = readConfigDict["campass"];
            }
            if (readConfigDict.ContainsKey("camindex") && readConfigDict["camindex"] != "")
            {
                cboxSuffix.SelectedIndex = Int32.Parse(readConfigDict["camindex"]);
            }
            if (readConfigDict.ContainsKey("url") && readConfigDict["url"] != "")
            {
                txtUrl.Text = readConfigDict["url"];
            }
            if (readConfigDict.ContainsKey("imagedir") && readConfigDict["imagedir"] != "")
            {
                txtImageDir.Text = readConfigDict["imagedir"];
            }

            if (readConfigDict.ContainsKey("dbip") && readConfigDict["dbip"] != "")
            {
                txtDbIP.Text = readConfigDict["dbip"];
            }
            if (readConfigDict.ContainsKey("dbport") && readConfigDict["dbport"] != "")
            {
                txtDbPort.Text = readConfigDict["dbport"];
            }
            if (readConfigDict.ContainsKey("dbuser") && readConfigDict["dbuser"] != "")
            {
                txtDbUser.Text = readConfigDict["dbuser"];
            }
            if (readConfigDict.ContainsKey("dbpass") && readConfigDict["dbpass"] != "")
            {
                txtDbPasswd.Text = readConfigDict["dbpass"];
            }
            if (readConfigDict.ContainsKey("dbname") && readConfigDict["dbname"] != "")
            {
                txtDbName.Text = readConfigDict["dbname"];
            }

            if (readConfigDict.ContainsKey("displaytime") && readConfigDict["displaytime"] != "")
            {
                chkDisplayTime.Checked = readConfigDict["displaytime"].Contains("rue") ? true : false;
            }
            if (readConfigDict.ContainsKey("mintimescan") && readConfigDict["mintimescan"] != "")
            {
                int readValue = Int32.Parse(readConfigDict["mintimescan"]);
                if (readValue <minTimeScan.Minimum || readValue > minTimeScan.Maximum)
                {
                    readValue = 3600;
                }
                minTimeScan.Value = readValue;
            }
            if (readConfigDict.ContainsKey("consoleEnable") && readConfigDict["consoleEnable"] != "")
            {
                chkConsoleEnable.Checked = readConfigDict["consoleEnable"].Contains("rue") ? true : false;
            }
        }

        void updateDictionaryEvent()
        {
            globalDictionary["camip"] = txtIp.Text;
            globalDictionary["camport"] = txtPort.Text;
            globalDictionary["camuser"] = txtUser.Text;
            globalDictionary["campass"] = txtPasswd.Text;
            globalDictionary["camindex"] = cboxSuffix.SelectedIndex.ToString();
            globalDictionary["url"] = txtUrl.Text;
            globalDictionary["imagedir"] = txtImageDir.Text;
            globalDictionary["connectstring"] = this.connectionString;

            globalDictionary["dbip"] = txtDbIP.Text;
            globalDictionary["dbport"] = txtDbPort.Text;
            globalDictionary["dbuser"] = txtDbUser.Text;
            globalDictionary["dbpass"] = txtDbPasswd.Text;
            globalDictionary["dbname"] = txtDbName.Text;

            globalDictionary["displaytime"] = chkDisplayTime.Checked.ToString();
            globalDictionary["mintimescan"] = minTimeScan.Value.ToString();
            globalDictionary["consoleEnable"] = chkConsoleEnable.Checked.ToString();
        }

        void intialValue()
        {
            string[] valueList = {
                "",
                "user=admin_password=tlJwpbo6_channel=1_stream=0.sdp?real_stream",
                "Streaming/Channels/1/",
                "Streaming/Channels/2/",
                "Streaming/Channels/101/",
                "Streaming/Channels/102/",
                "ch1/main/av_stream",
                "ch1/sub/av_stream",
                "1/d1"};

            foreach(string val in valueList )
            {
                cboxSuffix.Items.Add(val);
            }
            cboxSuffix.SelectedIndex = 4;

            updateUrlEvent();
        }

        void updateUrlEvent()
        {
            string ip       = txtIp.Text.Trim();
            string port     = txtPort.Text.Trim();
            string user     = txtUser.Text.Trim();
            string pass     = txtPasswd.Text.Trim();
            string suffix = cboxSuffix.Text + txtAddtionalSuffix.Text.Trim();

            txtUrl.Text = "rtsp://";
            if (user != "" && pass != "")
            {
                txtUrl.Text += user + ":" + pass + "@";
            }
            txtUrl.Text += ip + ":" + port + "/" + suffix;
        }


        string updateConnectionString()
        {
            server = txtDbIP.Text.Trim();
            database = txtDbName.Text.Trim();
            uid = txtDbUser.Text.Trim();
            password = txtDbPasswd.Text.Trim();
            string _connectionString = "SERVER=" + server + ";" + "DATABASE=" + database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            this.connectionString = _connectionString;
            return _connectionString;
        }

        string testConnection()
        {
            string result = "";
            result = "Connecting...";
            connection = new MySqlConnection(updateConnectionString());
            try
            {
                connection.Open();
                connection.Close();
                result = "Successful!";
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        result = "Cannot connect";
                        break;
                    case 1045:
                        result = "Invalid user/passwd";
                        break;
                    default:
                        result = "Cannot connect to host";
                        break;
                }
                MessageBox.Show("Exception: " + ex.Message, "Exception!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return result;
        }


        private void btnOK_Click(object sender, EventArgs e)
        {
            if(txtUrl.Text == "" || txtImageDir.Text == "")
            {
                MessageBox.Show("Please fill in the empty fields!");
                return;
            }
            cameraUrl = txtUrl.Text;
            imageDir = txtImageDir.Text;
            isDisplayTime = chkDisplayTime.Checked;
            minTimeBetweenScanSteps = Decimal.ToInt32(minTimeScan.Value);
            consoleEnabled = chkConsoleEnable.Checked;

            updateConnectionString();

            updateDictionaryEvent();

            config.saveDictionaryToFile(globalDictionary);
            isOKButtonClicked = true;
            this.Close();
        }

        private void btnImageDirSelect_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrEmpty(fbd.SelectedPath))
                {
                    txtImageDir.Text = fbd.SelectedPath;
                }
            }

            updateDictionaryEvent();
        }

        private void cboxSuffix_SelectedIndexChanged(object sender, EventArgs e)
        {
            updateUrlEvent();
            updateDictionaryEvent();
        }

        private void txtIp_TextChanged(object sender, EventArgs e)
        {
            updateUrlEvent();
            updateDictionaryEvent();
        }

        private void txtUrl_TextChanged(object sender, EventArgs e)
        {
            updateDictionaryEvent();
        }
        private void txtImageDir_TextChanged(object sender, EventArgs e)
        {
            updateDictionaryEvent();
        }

        private void btnTestDb_Click(object sender, EventArgs e)
        {
            lblDbStatus.Text = "Connecting...";

            if (dbConnectionTester.IsBusy != true)
            {
                dbConnectionTester.RunWorkerAsync();

                if (progressbarWorker.WorkerSupportsCancellation == true)
                {
                    progressbarWorker.CancelAsync();
                }

                btnTestDb.Enabled = false;
                progressBar1.Visible = true;
                progressBar1.Value = 0;
                try
                {
                    progressbarWorker.RunWorkerAsync();
                }
                catch { }
            }
        }
        private void dbConnectionTester_DoWork(object sender, DoWorkEventArgs e)
        {
            string result = testConnection();
            e.Result = result;
        }

        private void dbConnectionTester_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            btnTestDb.Enabled = true;
            progressBar1.Visible = false;
            progressBar1.Value = 0;

            if (progressbarWorker.WorkerSupportsCancellation == true)
            {
                progressbarWorker.CancelAsync();
            }

            if (e.Error == null)
            {
                lblDbStatus.Text = (string)e.Result;
            }
        }

        private void progressbarWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            for (int i = 1; i <= 100; i++)
            {
                if (worker.CancellationPending == true)
                {
                    e.Cancel = true;
                    break;
                }
                else
                {
                    System.Threading.Thread.Sleep(150);
                    worker.ReportProgress(i);
                }
            }
        }

        private void progressbarWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = Int32.Parse(e.ProgressPercentage.ToString());
        }

        private void txtDbIP_TextChanged(object sender, EventArgs e)
        {
            updateConnectionString();
        }

        private void btnCamSelect_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Feature not support!", "Infomation", MessageBoxButtons.OK, MessageBoxIcon.Information);
            //CameraURLBuilderWF myCameraUrlBuilder = new CameraURLBuilderWF();
            //var result = myCameraUrlBuilder.ShowDialog();

            //if (result == DialogResult.OK)
            //{
            //    string resultStr = myCameraUrlBuilder.CameraURL;
            //    Regex ip = new Regex(@"\b(\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}):(\d+).*\b");
            //    Match match = ip.Match(resultStr);
            //    if (match.Success)
            //    {
            //        string _ip = match.Groups[1].ToString();
            //        txtIp.Text = _ip;
            //        //txtPort.Text = match.Groups[2].ToString(); //port is not changed: 554
            //        foreach (string url in cameralist)
            //        {
            //            if (url.Contains(_ip))
            //            {
            //                txtUrl.Text = url;
            //                break;
            //            }
            //        }
            //    }
            //}
        }

        private void chkDisplayTime_CheckStateChanged(object sender, EventArgs e)
        {
            globalDictionary["displaytime"] = chkDisplayTime.Checked.ToString();
            Console.WriteLine(globalDictionary["displaytime"]);
        }
    }
}
