﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Popbill.Kakao;
using ControllerDI.Services;


namespace ControllerDI.Services
{
    public class KakaoInstance
    {
        //링크허브에서 발급받은 고객사 고객사 인증정보로 링크아이디(LinkID)와 비밀키(SecretKey) 값을 변경하시기 바랍니다.
        private string linkID = "TESTER";
        private string secretKey = "SwWxqU+0TErBXy/9TVjIPEnI0VTUMMSQZtJf3Ed8q3I=";

        public KakaoService kakaoService;

        public KakaoInstance()
        {
            //카카오 서비스 객체 초기화
            kakaoService = new KakaoService(linkID, secretKey);

            //연동환경 설정값, 개발용(true), 상업용(false)
            kakaoService.IsTest = true;
        }
    }
}

namespace KakaoExample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            
            //카카오 서비스 객체 의존성 주입
            services.AddSingleton<KakaoInstance>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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
                    template: "{controller=Kakao}/{action=Index}");
            });
        }
    }
}