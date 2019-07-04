using JiebaNet.Analyser;
using JiebaNet.Segmenter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spidert
{
    class JieBa
    {

        public static void Main()
        {
            string str = "e:/page";
            operateDirectory(str);
        }
        public static void operateDirectory(string path)
        {
            DirectoryInfo theFolder = new DirectoryInfo(@path);

            //遍历文件夹中的文件，并进行处理
            foreach (FileInfo nextFile in theFolder.GetFiles())
            {   
                string filename = nextFile.FullName;
                oprateJieBa(filename);
            }

            //遍历文件夹
            foreach (DirectoryInfo nextfolder in theFolder.GetDirectories())
            {
                operateDirectory(nextfolder.FullName);
            }
        }

        private static void oprateJieBa(string filename)
        {
            string[] filenames = filename.Split('\\');

            string filename1 = "E:\\词云\\JieBaResult\\" + filenames[2] + ".csv";   //用来存储jieba分析后的结果                                                                  
            string text = File.ReadAllText(filename);
            var segmenter = new JiebaSegmenter();
            var segments = segmenter.Cut(text);
            var extractor = new TfidfExtractor();
            var keywords = extractor.ExtractTags(text, 30, Constants.NounAndVerbPos);
            Console.WriteLine(filename);
            string str = null;
            foreach (var keyword in keywords)
            {
                str = str + keyword + "\n";
                Console.WriteLine(keyword);
            }
            StreamWriter fz = new StreamWriter(filename1,true);

            fz.Write(str);
            fz.Close();
        }
    }
}
