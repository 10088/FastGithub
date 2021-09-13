using FastGithub.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Threading.Tasks;

namespace FastGithub
{
    /// <summary>
    /// ������
    /// </summary>
    public class Startup
    {
        public IConfiguration Configuration { get; }

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="configuration"></param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// ���÷���
        /// </summary>
        /// <param name="services"></param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<FastGithubOptions>(this.Configuration.GetSection(nameof(FastGithub)));

            services.AddConfiguration();
            services.AddDomainResolve();
            services.AddDnsServer();
            services.AddHttpClient();
            services.AddReverseProxy();
            services.AddHostedService<VersonHostedService>();
        }

        /// <summary>
        /// �����м��
        /// </summary>
        /// <param name="app"></param>
        public void Configure(IApplicationBuilder app)
        {
            app.UseRequestLogging();
            app.UseDnsOverHttps();
            app.UseReverseProxy();

            app.UseRouting();
            app.UseEndpoints(endpoint =>
            {
                endpoint.Map("/", async context =>
                {
                    var certFile = $"CACert/{nameof(FastGithub)}.cer";
                    context.Response.ContentType = "application/x-x509-ca-cert";
                    context.Response.Headers.Add("Content-Disposition", $"attachment;filename={Path.GetFileName(certFile)}");
                    await context.Response.SendFileAsync(certFile);
                });
                endpoint.MapFallback(context =>
                {
                    context.Response.Redirect("https://github.com/dotnetcore/FastGithub");
                    return Task.CompletedTask;
                });
            });
        }
    }
}
