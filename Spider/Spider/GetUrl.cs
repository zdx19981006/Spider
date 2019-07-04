using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Spider
{
    class GetUrl 
    {
        public static HashSet<String> unAnalyUrls;
        public static Queue<String> unAnalyUrlsQueu;
        public HashSet<String> AnalyedUrls;
        public HashSet<String> queue;
        public static Queue<String> getUrls;

        public static bool analyed;
       

        public string responseFromServer;

        public AnalysisWebPage analysis;

        public GetUrl()
        {
            GetUrl.unAnalyUrls = new HashSet<String>();
            GetUrl.unAnalyUrlsQueu = new Queue<string>();
            this.AnalyedUrls = new HashSet<String>();
            this.queue = new HashSet<String>();
            GetUrl.getUrls = new Queue<String>();
            getUrls.Enqueue("https://www.jlu.edu.cn/index/tzgg.htm");
            analysis = new AnalysisWebPage();
            //getUrls.Enqueue("https://www.jlu.edu.cn/index/tzgg/39.htm");
            GetUrl.analyed = false;            
        }
        public void getHtmlContent()
        {
            lock (GetUrl.unAnalyUrlsQueu)
            {
                while (GetUrl.unAnalyUrlsQueu.Count > 0)
                {                  
                    ThreadPool.QueueUserWorkItem(new WaitCallback(analysis.getContent),GetUrl.unAnalyUrlsQueu.Dequeue());
                }
            }
        }
        public  void startGetUrl(Object c)
        {
            lock (GetUrl.getUrls)
            {
                while (GetUrl.getUrls.Count > 0)
                {

                    if (this.AnalyedUrls.Add(GetUrl.getUrls.Peek()))
                    {

                        //Console.WriteLine("************************************分析网页" + GetUrl.getUrls.Peek());

                        //this.analyHtmlUrl(GetUrl.getUrls.Dequeue());
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this.analyHtmlUrl), GetUrl.getUrls.Dequeue());
                    }
                    else
                    {
                        GetUrl.getUrls.Dequeue();
                    }


                }

            }               
                      
        }
        public void analyHtmlUrl(Object urls)
        {
           
            try
            {
                String url = urls.ToString();
                HttpWebRequest request = (System.Net.HttpWebRequest)WebRequest.Create(url);//起始URL
                request.Credentials = CredentialCache.DefaultCredentials;
                request.UserAgent = "Mozilla/5.0(WindowsNT10.0;Win64;x64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/67.0.3396.99Safari/537.36";
                request.Method = "GET"; request.AllowAutoRedirect = false;
                request.ContentType = "application/x-www-form-urlencoded";
                request.CookieContainer = new CookieContainer();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                String cookies;
                cookies = response.Headers.Get("Set-Cookie");/*request.UserAgent="Mozilla/5.0(WindowsNT10.0;Win64;x64)AppleWebKit/537.36(KHTML,likeGecko)Chrome/67.0.3396.99Safari/537.36";request.Headers.Add("cookie","userIp="+ipa.ToString()+";"+cookies);response=(HttpWebResponse)request.GetResponse();*/
                // Console.WriteLine(response.StatusDescription);
                Stream dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                this.responseFromServer = reader.ReadToEnd();
                this.getURL(this.responseFromServer, 1,url);
            }catch(Exception e)
            {
                return;
            }
        
           
           
        }
        public void getURL(String html,int type,String rooturl)
        {
            var pattern = @"<a\s+(?:[^>]+\s+)?href\s*=\w*(?:'([^']*)'|""([^""""]*)""|([^\w>]+))[^>]*>";
            var regx = new Regex(pattern);
            int urls = 0;
            int htmlUrls = 0;
            var matches = regx.Matches(html);
            foreach (Match match in matches)
            {
                for (int group = 1; group <= 3; group++)
                {
                    if (match.Groups[group].Length > 0)
                    {


                        lock (this)
                        {
                            String url = this.formatUrl(rooturl, match.Groups[group].Value);
                            if (GetUrl.unAnalyUrls.Add(url) && !url.Equals("") && url.Contains("jlu.edu.cn") && url.Contains("info"))
                            {
                                GetUrl.unAnalyUrlsQueu.Enqueue(url);
                                //Console.WriteLine(url);
                                htmlUrls++;
                            }
                            if (htmlUrls != 0)
                            {
                                this.getHtmlContent();
                            }



                            if (this.queue.Add(url) && !url.Equals("") && url.Contains("jlu.edu.cn")&&(url.Contains("www") || url.Contains("news") || url.Contains("jwc")))
                            {
                                GetUrl.getUrls.Enqueue(url);
                                //Console.WriteLine(url);
                                urls++;
                            }
                            if (urls != 0)
                            {
                                this.startGetUrl("");
                            }
                        }
                            
                        
                    }

                }

            }
            
            

        }

        public String formatUrl(string rootUrl, string oldUrl)
        {
            try
            {
            Uri baseUri = new Uri(rootUrl); // http://www.enet.com.cn/enews/inforcenter/designmore.jsp
            Uri absoluteUri = new Uri(baseUri,oldUrl);//相对绝对路径都在这里转 这里的urlx ="../test.html"
            return absoluteUri.ToString();//   http://www.enet.com.cn/enews/test.html 
            }catch(Exception e)
            {
                return "";
            }
            
        }
           

    }

}
