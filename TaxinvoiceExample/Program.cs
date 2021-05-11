using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TaxinvoiceExample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                //ASP.NET Core 호스트는 호스트에서 사용할 서비스와 Request Pipline 등을 지정하기 위해 Framework에서 Startup 어셈블리를 호출하는데, 
                //어떤 Startup 어셈블리를 호출해야 하는지 설정하기위해 UseStartup()메서드를 사용
                //아래는 Startup이라는 클래스를 사용할것을 지정한것, 프로젝트 템플릿에서 생성한 Startup.cs 파일이 이 클래스를 포함하고 있음
                //필요해 따라 다른 Startup을 지정할 수 있음
                .UseStartup<Startup>()
                .Build();
    }
}