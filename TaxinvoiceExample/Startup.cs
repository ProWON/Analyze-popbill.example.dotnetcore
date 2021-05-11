using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popbill.Taxinvoice;

// SDK 옵션을 설정하기 위해서는 TaxinvoiceInstance 클래스를 사용하는듯
// TaxinvoiceInstance 클래스에서 TaxinvoiceSerivce 인스턴스 생성사고 값 세팅
// TaxinvoiceInstance 를 DI할때 싱글톤을 이용하게 ConfigureService에 설정되어 있으니 TaxinvoiceService도 한번만 인스턴스 생성하겠지
public class TaxinvoiceInstance
{
    //파트너 신청 후 메일로 발급받은 링크아이디(LinkID)와 비밀키(SecretKey)값 으로 변경하시기 바랍니다.
    private string linkID = "TESTER";
    private string secretKey = "SwWxqU+0TErBXy/9TVjIPEnI0VTUMMSQZtJf3Ed8q3I=";

    public TaxinvoiceService taxinvoiceService;
    // 
    public TaxinvoiceInstance()
    {
        //세금계산서 서비스 객체 초기화
        taxinvoiceService = new TaxinvoiceService(linkID, secretKey);

        //연동환경 설정값, 개발용(true), 상업용(false)
        taxinvoiceService.IsTest = true;

        //인증토큰의 IP제한기능 사용여부, 권장(true)
        taxinvoiceService.IPRestrictOnOff = true;

        // 팝빌 API 서비스 고정 IP 사용여부(GA), true-사용, false-미사용, 기본값(false)
        taxinvoiceService.UseStaticIP = false;

        // 로컬 시스템시간 사용 여부, true(사용), fasle(미사용) - 기본값
        taxinvoiceService.UseLocalTimeYN = false;

    }
}

namespace TaxinvoiceExample
{   
    // Startup 클래스는 반드시 Configure()메서드를 가져야 하며, Optional로 ConfigureServices() 메서드를 가질 수 있음.
    // 둘다 웹 프로그램이 구동될 때 호출되는데, 만약 둘다 존재한다면 ConfigureServices()메서드가 Configure()보다 먼저 호출
    // ASP.NET Core앱은 규칙에 따라 Startup으로 이름이 지정된 Startup 클래스를 사용함
    // Configure() 메서드는 HTTP Request가 들어오면 ASP.NET이 어떻게 반응해야 하는지 지정하는 것으로 여러 Request Pipeline을 설정하는 역활을 함
    //
    // This method gets called by the runtime. Use this method to add services to the container.
    // 서비스(IServiceCollection)는 ConfigureServices에서 등록되며 DI 또는 ApplicationServices를 통해 앱 전체에서 사용
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        //선택사항
        // Configure 메서드 전에 호스트에 의해 호출되어 앱의 서비스를 구성
        // 웹 프로그램에서 어떤 Framework Service를 사용할 지를 지정하거나 개발자의 Custom Type에 대한 Dependancy Injection을 정의하는 일을 함
        // ConfigureService 메서드는 IServiceCollection을 입력파라미터로 갖는데 IServiceCollection은 여러 Add* 메서드를 갖고있음
        public void ConfigureServices(IServiceCollection services)
        {
            // Framwork Service로 MVC를 사용하기 위해 사용
            // AddAntiforgery(), addIdentity(), 등등 다양한 서비스를 IServiceCollection에 추가함으로써 웹 프로그램에서 그 기능을 사용할 수 있게됨
            // ASP.NETCore는 자체적으로 DI기능을 내장하고 있음, DI는 어떤 타입의 객체가 필료할 때 FrameWork이 알아서 해당 타입의 객체를 생성해서 제공해 주는
            // FrameWork 서비스임
            // DI는 보통 Constructor 또는 Property를 통해 Inject 되는데 , ASP.NEt Core는 Constructor Injection을 사용
            services.AddMvc();
               
            // 개발자가 작성한 Custom Type DI를 지정하기 위해서 IServiceCollection의 AddTransient(), AddScoped(), AddSingleton()등의 메서드를 사용
            // DI에서 또하나 중요한 옵션은 객체를 매번 새로 생성하느냐 ,이미 있는 객체를 제공하느냐를 결정하는 것이 있는데, DI는 이를 통해
            // 자신의 내부 컨테이너 안의 객체들에 대한 Lifetime을 관리하게 됨
            // DI 설정을 위해 사용되는 AddTransient() 메서드는 매번 인터페이스가 요청될 때마다 새로운 객체를 생성해서 돌려줌
            // AddScoped() 메서드는 하나의 HTTP Request 당 하나의 객체를 사용한다. 즉 WebRequest가 하나 들어왔을 때 ,여러 클래스에서 동일한 인터페이스를
            // 요청하더라도 이미 생성된 객체를 돌려준다. 한번 요청에서 여러 클래스에서 동일한 DI 받을때 사용한 객체를 계속 사용한다.
            // AddSingleton() 웹 프로그램 전체를 통해 오직 하나의 객체만을 사용한다. 즉 처음 그 인터페이스가 요청되면 새로 생성하고,
            // 나머지 요청에 대해서는 항상 이미 생성된 객체를 리턴한다.
            //세금계산서 서비스 객체 의존성 주입
          
            services.AddSingleton<TaxinvoiceInstance>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // pipeline 은 한 데이터 처리 단계의 출력이 다음 단계의 입력으로 이어지는 형태로 연결된 구조를 가르킴.
        // Confifure 메서드는 Request Pipeline를 설정하는 역활을함 처음 미들웨어가 실행된 후 다음 미들웨어로 계속 Pipeline처럼 이동함
        // I 붙어있으면 인터페이스
        // IApplicationBuilder, IHostingEnvironment 모두 인터페이스 app, env 파라미터는 ASP.NETCore에 내장되어 있는 DependancyInjection 프레임워크에 의해 처리
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {  
           
            // app.Use* 사용되는 함수들을 미들웨어라고 생각하자
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Taxinvoice}/{action=Index}");
            });
        }
    }
}