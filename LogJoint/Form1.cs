using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace LogJoint
{
    public partial class Form1 : Form
    {

        /// <summary>
        /// LOG文件比较器
        /// </summary>
        public class LogNameComparer : IComparer<string>
        {
            string logType = "";
            public LogNameComparer(string type)
            {
                logType = type;
            }

            public int Compare(string x, string y)
            {
                int n1 = 0;
                int n2 = 0;
                Regex reg = new Regex(@"^*(\d+)$");

                Match m1 = reg.Match(x);
                Match m2 = reg.Match(y);

                string s1 = m1.Groups[0].ToString();
                if (s1 == "")
                {
                    n1 = 0;
                }
                else
                {
                    n1 = int.Parse(s1);
                }

                string s2 = m2.Groups[0].ToString();
                if (s2 == "")
                {
                    n2 = 0;
                }
                else
                {
                    n2 = int.Parse(s2);
                }

                return (n1 > n2 ? 1 : -1);
            }
        }






        public Form1()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 选择文件路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelectFolder_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();

            // 设置根在桌面
            folderBrowserDialog.RootFolder = System.Environment.SpecialFolder.Desktop;
            // 设置当前选择的路径
            folderBrowserDialog.SelectedPath = "C:";
            // 设置对话框的说明信息
            folderBrowserDialog.Description = "请选择LOG目录";

            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                string fName = folderBrowserDialog.SelectedPath;
                textFolderPath.Text = fName;

                btnStart.Enabled = true;
            }
        }

        /// <summary>
        /// 开始拼接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStart_Click(object sender, EventArgs e)
        {
            string logtype = "";
            string resultLog = "";

            btnStart.Enabled = false;

            string[] logTypes = { "logcat", "aplog", "main_log", "mainlog" };

            resultLog = "output_log.txt";

            StreamReader sReader = null;
            StreamWriter sWriter = null;
            try
            {
                string serachFolder = textFolderPath.Text.Trim();

                // 1. 先搜索目录，看是否存在指定格式的LOG文件
                int fileCount = 0;
                List<string> mFileList = new List<string>();
                foreach(string type in logTypes){

                    string[] allFiles = Directory.GetFiles(serachFolder, type + "*");
                    if (allFiles.Length > 0)
                    {
                        // 找到LOG了
                        logtype = type;
                        foreach (string file in allFiles)
                        {
                            fileCount++;
                            mFileList.Add(file);
                        }
                        break;
                    }
                }


                if (fileCount == 0)
                {
                    MessageBox.Show("未找到LOG文件，请确认目录是否选择正确");
                    return;
                }
                else
                {
                    mFileList.Sort(new LogNameComparer(logtype));
                }


                // 2. 删除已存在文件
                string desFilePath = textFolderPath.Text.Trim() + Path.DirectorySeparatorChar + resultLog;
                if (File.Exists(desFilePath))
                {
                    File.Delete(desFilePath);
                }


                // 3. 开始转换 
                string sourceFilePaht = textFolderPath.Text.Trim() + Path.DirectorySeparatorChar;

                string strOpen;
                for(int i = fileCount-1; i >= 0; i--)     
                {
                    string source = mFileList[i].ToString();
                    if (source.Contains(".boot"))
                    {
                        continue;
                    }
                    sWriter = new StreamWriter(desFilePath, true, Encoding.UTF8);
                    sReader = new StreamReader(source, Encoding.UTF8);
                    strOpen = sReader.ReadToEnd();
                    sReader.Close();
                    sWriter.WriteLine(strOpen);
                    sWriter.WriteLine();
                    sWriter.Close();     
                } 
               

                MessageBox.Show("拼接成功!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("拼接失败：" + ex.Message);
            }
            finally
            {
                btnStart.Enabled = true;

                if (null != sReader)
                {
                    sReader.Close();
                    sReader = null;
                }

                if (null != sWriter)
                {
                    sWriter.Close();
                    sWriter = null;
                }
            }



        }





    }
}
