using System;
using System.DrawingCore;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace MangaCollector
{
    class Program
    {
        static string tempPath = @"temp";
        static string userAgent = "";
        static void Main(string[] args)
        {

            if (File.Exists("list.txt"))
            {
                ListSteal("list.txt");
            }
        }
        static void Test()
        {
            DirectoryInfo[] ds = new DirectoryInfo("temp").GetDirectories();
            foreach (DirectoryInfo d in ds)
            {
                string h = "";
                if (d.Name.Contains("青柠"))
                {
                    h = "[青柠漢化組]";
                }
                if (d.Name.Contains("Kirara"))
                {
                    h = "[Kirara漢化組]";
                }
                if (h == "") continue;
                for (int i = 1; i < 30; i++)
                {
                    if (d.Name.Contains(NumToNo(i, 2)))
                    {
                        Directory.Move(d.FullName, "(まんがタイムきららＭＡＸ)[はんざわかおり] Comic Girls 第" + NumToNo(i, 2) + "話" + h);
                    }
                }
            }

        }
        static void ListSteal(string path)
        {
            string[] lines = File.ReadAllLines(path);
            foreach (string p in lines)
            {
                if (p.Contains("yamibo.com"))
                {
                    StealImages_Yamibo(p, tempPath);
                }
                if (p.Contains("tieba.baidu.com"))
                {
                    StealImages_Tieba(p, tempPath);
                }
                if (p.Contains("2cat.cf"))
                {
                    string[] cmd = p.Split(" ");
                    if (cmd.Length == 3)
                    {
                        StealImages_Komica("", cmd[0], cmd[1], cmd[2]);
                    }
                    else
                    {
                        StealImages_Komica("", cmd[0]);
                    }
                }
            }
        }
        static void SavePage(string url, string cookieStr = "", string tempFilePath = "temp.html")
        {
            HttpWebRequest r = (HttpWebRequest)WebRequest.Create(url);
            r.Headers.Add("Cookie", cookieStr);
            r.UserAgent = userAgent;
            Stream stm = r.GetResponse().GetResponseStream();
            Stream stream = new FileStream(tempFilePath, FileMode.OpenOrCreate);
            stm.CopyTo(stream);
            stream.Close();
            stm.Close();
            Console.WriteLine("文档Get:" + url);
        }
        static void ImageSteal(string imageUrl, string savePath, string referer = "", string cookieStr = "")
        {
            Console.WriteLine("Stealing: " + imageUrl);
            HttpWebRequest r = (HttpWebRequest)HttpWebRequest.Create(imageUrl);
            r.Headers.Add("Cookie", cookieStr);
            r.Referer = referer;
            try
            {
                Stream stm = r.GetResponse().GetResponseStream();
                Stream stream = new FileStream(savePath, FileMode.Create);
                byte[] bArr = new byte[1024];
                int size = stm.Read(bArr, 0, (int)bArr.Length);
                while (size > 0)
                {
                    stream.Write(bArr, 0, size);
                    size = stm.Read(bArr, 0, (int)bArr.Length);
                }
                stream.Close();
                stm.Close();


            }
            catch (Exception e) { Console.WriteLine("ImageSteal:\n" + e); }

            Console.WriteLine("Saved: " + savePath);
        }
        static void MassSteal(string src, string path)
        {
            string txt = File.ReadAllText(src);
            Directory.CreateDirectory(path);

            string keyfor = " src=\"http";
            // keyfor = "file=\"http";
            string temp = "";
            int i = 0;
            int count = 1;
            do
            {
                i = txt.IndexOf(keyfor, i);
                if (i == -1) { Console.WriteLine(i); break; }
                i += keyfor.ToCharArray().Length;
                temp = txt.Substring(i - 4, txt.IndexOf("\"", i) - i + 4);
                Console.WriteLine("    " + temp + "===》" + NumToNo(count, 2) + ".jpg");
                ImageSteal(temp, Path.Combine(path, NumToNo(count, 2) + ".jpg"), "");
                count++;
            }
            while (i >= 0);

        }

        static void StealImages_Komica(string dir, string url, string startName = "", string endName = "")
        {
            string doman = "https://2cat.cf";
            string tempFilePath = Path.Combine(dir, "Temp.html");
            SavePage(url, "", tempFilePath);
            Console.WriteLine("文档Get！");
            string content = File.ReadAllText(tempFilePath);
            string title = "KomicaGet";
            Regex titleRegex = new Regex("<span class=\"title\">(.*)</span>");
            Match titleMatch = titleRegex.Match(content);
            if (titleMatch.Success)
            {
                title = titleMatch.Groups[1].Value;
            }
            Directory.CreateDirectory(title);
            string saveDir = Path.Combine(dir, title);
            Regex regex = new Regex("画像ファイル名：<a href=\"(.*?)\"");

            MatchCollection mc = regex.Matches(content);
            bool getting = false;
            if (startName == "") getting = true;
            int i = 0;
            foreach (Match m in mc)
            {
                if (!getting)
                {
                    if (m.Groups[1].Value.Contains(startName))
                    {
                        getting = true;
                    }
                }
                if (getting)
                {
                    i++;
                    ImageSteal(doman + m.Groups[1].Value, Path.Combine(saveDir, NumToNo(i, 3) + ".jpg"));
                    if (endName != "")
                        if (m.Groups[1].Value.Contains(endName)) { getting = false; }
                }


            }
            File.Delete(tempFilePath);

        }
        static void StealImages_Tieba(string url, string dir, int minWid = 800, int max = 50)
        {
            string saveDir = dir;
            Directory.CreateDirectory(saveDir);
            string tempFilePath = Path.Combine(saveDir, "tiebaTemp.html");
            string tempImagePath = Path.Combine(saveDir, "temp.jpg");
            string img = "http://imgsrc.baidu.com/forum/pic/item/";
            SavePage(url, "", tempFilePath);
            string content = File.ReadAllText(tempFilePath);
            Regex regextitle = new Regex("<title>(.*)</title>");
            Match match = regextitle.Match(content);
            if (match.Success)
            {
                string title = match.Groups[1].Value;
                saveDir = Path.Combine(saveDir, title);
                Directory.CreateDirectory(saveDir);
            }
            int index = 0; int i = 0;
            do
            {
                index = content.IndexOf("class=\"BDE_Image\"", index + 5);
                if (index < 0) break;
                int startIndex = content.IndexOf("src=", index);
                string src = content.Substring(startIndex + 5, content.IndexOf("\"", startIndex + 5) - startIndex - 5);

                ImageSteal(img + src.Substring(src.LastIndexOf('/')), tempImagePath, "");
                Image imgTemp;
                if ((imgTemp = Image.FromFile(tempImagePath)).Width < minWid)
                {
                    imgTemp.Dispose();
                    File.Delete(tempImagePath);
                }
                else
                {
                    i++;
                    imgTemp.Dispose();
                    string savePath = Path.Combine(saveDir, NumToNo(i, 2) + ".jpg");
                    File.Move(tempImagePath, savePath);
                }

            } while (i <= max);
            Console.WriteLine(i + "个图片Get！");
            File.Delete(tempFilePath);
        }
        static void StealPageImages(string url, string dir, int minWid = 800, int max = 50)
        {
            Directory.CreateDirectory(dir);
            string tempFilePath = Path.Combine(dir, "Temp.html");
            string tempImagePath = Path.Combine(dir, "temp.jpg");
            string[] keys = new string[] { "src=\"http", "file=\"http" };
            HttpWebRequest r = (HttpWebRequest)WebRequest.Create(url);
            string cookieStr = "";
            r.Headers.Add("Cookie", cookieStr);
            Stream stm = r.GetResponse().GetResponseStream();

            Stream stream = new FileStream(tempFilePath, FileMode.OpenOrCreate);
            stm.CopyTo(stream);
            stream.Close();
            stm.Close();
            Console.WriteLine("文档Get！");
            string content = File.ReadAllText(tempFilePath);
            string urlTemp;
            int index = 0, count = 0, keyIndex = 0;
            do
            {
                int j = -1;
                for (int ii = 0; ii < keys.Length; ii++)
                {
                    int k = content.IndexOf(keys[ii], index);
                    if (k == -1) continue;
                    if (j == -1) { j = k; keyIndex = ii; continue; }
                    if (k < j) { j = k; keyIndex = ii; }
                }
                if (j < 0) break;
                index = j + keys[keyIndex].Length;
                urlTemp = content.Substring(index - 4, content.IndexOf("\"", index) - index + 4);
                if (!urlTemp.Contains(".jpg") && !urlTemp.Contains(".png")) { continue; }
                ImageSteal(urlTemp, tempImagePath, "");
                try
                {
                    Image imgTemp;
                    imgTemp = Image.FromFile(tempImagePath);
                    if (imgTemp.Width < minWid)
                    {
                        imgTemp.Dispose();
                        File.Delete(tempImagePath);
                    }
                    else
                    {
                        count++;
                        imgTemp.Dispose();
                        string savePath = Path.Combine(dir, NumToNo(count, 2) + ".jpg");
                        File.Move(tempImagePath, savePath);
                    }
                }
                catch (Exception e) { Console.WriteLine("StealLoop:\n" + e); }

            } while (count <= max);
            Console.WriteLine(count + "个图片Get！");
            File.Delete(tempFilePath);
        }
        static void StealImages_Yamibo(string url, string dir, int minWid = 800, int max = 50)
        {
            string saveDir = dir;
            Directory.CreateDirectory(dir);
            string tempFilePath = Path.Combine(dir, "Temp.html");
            string[] keys = new string[] { "zoomfile=\"" };
            string cookieStr = "";
            try { cookieStr = File.ReadAllText("cookies_yamibo"); }
            catch (FileNotFoundException)
            {
                Console.WriteLine("cookies_yamibo not found!");
            }
            SavePage(url, cookieStr, tempFilePath);
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            string content = File.ReadAllText(tempFilePath, Encoding.GetEncoding(936));//GB
            Regex regextitle = new Regex("<title>(.*)</title>");
            Match match = regextitle.Match(content);
            if (match.Success)
            {
                string title = match.Groups[1].Value;
                saveDir = Path.Combine(saveDir, title);
                Directory.CreateDirectory(saveDir);
            }
            Regex regex = new Regex("zoomfile=\"(.*?)\"");
            MatchCollection matchCollection = regex.Matches(content);
            int i = 0;
            foreach (Match m in matchCollection)
            {
                i++;
                string imgUrl = "https://bbs.yamibo.com/" + m.Groups[1].Value;
                ImageSteal(imgUrl, Path.Combine(saveDir, NumToNo(i, 3) + ".jpg"), "", cookieStr);
            }
            File.Delete(tempFilePath);
        }
        static string NumToNo(int i, int n)//将一个100以内数字转换成n位的编号
        {
            string r = i.ToString();
            for (int j = n - r.Length; j > 0; j--) r = "0" + r;
            return r;
        }

    }
}
