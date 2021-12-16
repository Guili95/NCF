using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Senparc.CO2NET;
using Senparc.Ncf.Core.Models;
using Senparc.Ncf.Database;
//using Senparc.Ncf.Database.MySql;//根据需要添加或删除，使用需要引用包： Senparc.Ncf.Database.MySql
//using Senparc.Ncf.Database.Sqlite;//根据需要添加或删除，使用需要引用包： Senparc.Ncf.Database.Sqlite
//using Senparc.Ncf.Database.PostgreSQL;//根据需要添加或删除，使用需要引用包： Senparc.Ncf.Database.PostgreSQL
using Senparc.Ncf.Database.SqlServer;//根据需要添加或删除，使用需要引用包： Senparc.Ncf.Database.SqlServer

namespace Senparc.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //指定数据库类型
            /* AddDatabase<TDatabaseConfiguration>() 泛型类型说明：
             * 
             *                  方法                            |         说明
             * -------------------------------------------------|-------------------------
             *  AddDatabase<SQLServerDatabaseConfiguration>()   |  使用 SQLServer 数据库
             *  AddDatabase<SqliteMemoryDatabaseConfiguration>()|  使用 SQLite 数据库
             *  AddDatabase<MySqlDatabaseConfiguration>()       |  使用 MySQL 数据库
             *  AddDatabase<PostgreSQLDatabaseConfiguration>()  |  使用 PostgreSQL 数据库
             *  更多数据库可扩展，依次类推……
             *  
             */
            services.AddDatabase<SQLServerDatabaseConfiguration>();//默认使用 SQLServer数据库，根据需要改写

            //添加（注册） Ncf 服务（重要，必须！）
            services.AddNcfServices(Configuration, Env);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env,
            IOptions<SenparcCoreSetting> senparcCoreSetting, IOptions<SenparcSetting> senparcSetting)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //Use NCF（必须）
            app.UseNcf(env, senparcCoreSetting, senparcSetting);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCookiePolicy();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

        }
    }
}
