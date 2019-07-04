using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Spider
{
    class news
    {
        public String title ;
        public String date ;
        public String depart ;
        public String content;
        public String url;
        public news(String url,String title, String date,String depart,String content)
        {
            this.url = url;
            this.title= title;
            this.date= date;
            this.depart= depart;
            this.content= content;
        }
    }
    class AnalysisWebPage
    {
        public static List<news> newsinfor;
        public string responseFromServer;
        public AnalysisWebPage()
        {
            AnalysisWebPage.newsinfor = new List<news>();
        }
        public void getContent(Object urls)
        {
            String url = urls.ToString();
            //Console.WriteLine("获取页面" + url + "内容");
            try
            {
                HttpWebRequest request = (System.Net.HttpWebRequest)WebRequest.Create(url);//起始URL
                request.Credentials = CredentialCache.DefaultCredentials;
                request.UserAgent = "Mozilla/5.0(WindowsNT10.0;Win64;x64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/67.0.3396.99Safari/537.36";
                request.Method = "GET"; request.AllowAutoRedirect = false;
                request.ContentType = "application/x-www-form-urlencoded";
                request.CookieContainer = new CookieContainer();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                String cookies;
                cookies = response.Headers.Get("Set-Cookie");/*request.UserAgent="Mozilla/5.0(WindowsNT10.0;Win64;x64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/67.0.3396.99Safari/537.36";request.Headers.Add("cookie","userIp="+ipa.ToString()+";"+cookies);response=(HttpWebResponse)request.GetResponse();*/
                                                             //Console.WriteLine(response.StatusDescription);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                this.responseFromServer = reader.ReadToEnd();
                String title = "";
                String date = "";
                String depart = "";
                String content = "";
                var contentPattern = @"(?is)<div id=""vsb_content.*?>(?<cont>.*?)<div id=""div_vote_id""></div>";
                var contentRegex = new Regex(contentPattern);
                Match contentMatch = contentRegex.Match(this.responseFromServer);
                String cont = contentMatch.Groups["cont"].Value;
                String contentpattern = @"<p(.*?)/p>";
                MatchCollection matches = Regex.Matches(cont, contentpattern);
                String content1 = "";
                foreach (Match match in matches)
                {
                    String scon = this.getcon(match.Groups[1].Value);
                    content1 += scon + " ";
                    content += scon + "\r\n";
                }
                content1 = content1.Trim();
                //Console.WriteLine(content);
                String[] segs = content1.Split(' ');
                for (int i = 0; i < segs.Length; i++)
                {
                    if (this.IsDate(segs[i]))
                    {
                        date = segs[i];
                        depart = segs[i - 1];
                    }
                }

                //Console.WriteLine(date);
                //Console.WriteLine(depart);
                var titlepattern = @"(?is)<div class="".*?title.*?""(?<title>.*?)div id=""vsb_content";
                var titleregx = new Regex(titlepattern);
                Match titlematch = titleregx.Match(this.responseFromServer);
                title = titlematch.Groups["title"].Value;


                title += @"div id=""vsb_content";
                titlematch = titleregx.Match(title);
                String titleCopy = "";
                while (!titlematch.Groups["title"].Value.Equals(""))
                {

                    titleCopy = titlematch.Groups["title"].Value;
                    title = titlematch.Groups["title"].Value;
                    title += @"div id=""vsb_content";
                    titlematch = titleregx.Match(title);

                }
                titleCopy = title;
                title = this.getfirCon(title);


                //Console.WriteLine(title);
                if (date.Equals("") && !titleCopy.Equals(""))
                {
                    var pattern = @">(.*?)<";
                    MatchCollection datmatches = Regex.Matches(titleCopy, pattern);
                    for (int i = 0; i < datmatches.Count; i++)
                    {

                        if (this.IsDate(datmatches[i].Groups[1].Value))
                        {
                            date = datmatches[i].Groups[1].Value;
                            depart = datmatches[i + 2].Groups[1].Value;
                        }

                    }
                }
                /*if (date.Equals("") || DateTime.Compare(DateTime.Parse(date), DateTime.Parse("2019 - 06 - 01")) < 0 || DateTime.Compare(DateTime.Parse(date), DateTime.Parse("2018 - 01 - 01")) > 0)
                {
                    
                    return;
                }*/

                if (date.Contains("年"))
                {
                    date = date.Trim();
                    String[] sdate = date.Split(new char[3] { '年', '月', '日' });
                    date = "";
                    foreach (var s in sdate)
                    {
                        if (s.Length > 2)
                            date += s;
                        else
                        {
                            if (s.Length == 1 && !s.Equals(" "))
                                date += "-0" + s;
                            else if (s.Length == 2)
                                date += "-" + s;

                        }

                    }
                }
                date = date.Trim();
                if (date.Length < 10)
                {
                    String[] sdate = date.Split('-');
                    if (sdate.Length == 3)
                    {
                        if (sdate[1].Length < 2)
                            sdate[1] = "0" + sdate[1];
                        if (sdate[2].Length < 2)
                            sdate[2] = "0" + sdate[2];
                    }
                    date = sdate[0] + "-" + sdate[1] + "-" + sdate[2];
                }
                if (!title.Equals("") && !date.Contains("."))
                {
                    try
                    {
                        AnalysisWebPage.newsinfor.Add(new news(url, title, date, depart, content));

                        Console.WriteLine(title + "---- " + date);
                        Console.WriteLine("***************************************" + url);
                        String path = @"E:\page\" + date;
                        if (!Directory.Exists(path))//判断文件夹是否存在 
                        {
                            Directory.CreateDirectory(path);//不存在则创建文件夹 
                        }
                        String newsInfor = title + "\r\n";
                        newsInfor += "日期：" + date + "\r\n";
                        newsInfor += "部门：" + depart + "\r\n";
                        newsInfor += content;
                        //Console.WriteLine(newsInfor);
                        lock (this)
                        {
                            try
                            {
                                System.IO.File.WriteAllText(path + "\\" + title + ".txt", newsInfor, Encoding.UTF8);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(title + " xxxxxxxxxxxxxxxxxx " + url);
                            }
                        }

                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(title + " xxxxxxxxxxxxxxxxxx " + url);
                    }


                }
                else
                {
                    return;
                }

            }
            catch (Exception)
            {
                return;
            }
            

                    

            return ;
        }


        public bool IsDate(string strDate)
        {
            try
            {
                DateTime.Parse(strDate);  //不是字符串时会出现异常
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool CompanyDate(string dateStr1, string dateStr2)
        {
            //将日期字符串转换为日期对象
            DateTime t1 = Convert.ToDateTime(dateStr1);
            DateTime t2 = Convert.ToDateTime(dateStr2);
            //通过DateTIme.Compare()进行比较（）
            int compNum = DateTime.Compare(t1, t2);

            //t1> t2
            if (compNum > 0||compNum == 0)
            {
                return true;
            }          
            else 
            {
                return false;
            }
        }
        public String getcon(String s)
        {
            String con = "";
            String getTitle = @">(.*?)<";
            MatchCollection gettitlematches = Regex.Matches(s, getTitle);
            
            foreach (Match match in gettitlematches)
            {

                con += match.Groups[1].Value.Replace("&nbsp;", " ");

            }
            con = Regex.Replace(con, @"\s+", "");
            con = con.Trim();

            return con;

        }
        public String getfirCon(String s)
        {
            String con = "";
            String getTitle = @">(.*?)<";
            MatchCollection gettitlematches = Regex.Matches(s, getTitle);
            if (gettitlematches.Count < 1) return con;
            con += gettitlematches[0].Groups[1].Value.Replace("&nbsp;", " ");
            con = Regex.Replace(con, @"\s+", "");
            con = con.Trim();

            return con;
        }
    }
}
