
using Microsoft.AspNetCore.Http;
using PartUpload.Controllers;
using System;
using System.Globalization;
using System.Text.Json;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Options;
using XSX.MultiPartUploadFile.Controllers;
using XSX.MultiPartUploadFile.Dto;
using XSX.MultiPartUploadFile.Middlewares;
using XSX.MultiPartUploadFile.Options;
using Microsoft.AspNetCore.Builder;

namespace PartUpload
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            var mvcBuilder = builder.Services.AddControllers();
            builder.AddMultiPartUploadFile(mvcBuilder);
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseAuthorization();

            //var mpufOptions = app.Services.GetRequiredService<IOptions<MultiPartUploadFileOptions>>().Value;
            //var controlerName = nameof(MultiPartUploadFileController).Replace("Controller", "");
            //var actionName = nameof(MultiPartUploadFileController.UploadFile);
            //app.MapControllerRoute("MultiPartUploadFile", mpufOptions.UploadApiPath, new { controller = controlerName, action = actionName });

            app.UseAutoRedirectToMultiPartUploadMiddleware();
            //app.MapControllers();

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapControllerRoute("MultiPartUploadFile", mpufOptions.UploadApiPath, new { controller = controlerName, action = actionName });
                
                endpoints.MapControllers();
            });


            app.Run();
        }
    }
}
