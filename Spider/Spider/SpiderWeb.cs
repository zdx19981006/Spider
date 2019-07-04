using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spider;
namespace Spider
{
    class SpiderWeb
    {
       
        static void Main(string[] args)
        {
           
           
            GetUrl gu = new GetUrl();
            gu.startGetUrl("");          
            Console.ReadKey();
            
        }

    }
}
