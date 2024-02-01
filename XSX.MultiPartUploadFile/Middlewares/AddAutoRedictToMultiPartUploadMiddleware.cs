using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XSX.MultiPartUploadFile.Controllers;
using XSX.MultiPartUploadFile.Dto;
using XSX.MultiPartUploadFile.Options;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using System.Text.Json;
using Microsoft.Extensions.Primitives;

namespace XSX.MultiPartUploadFile.Middlewares
{
    public static class AddAutoRedictToMultiPartUploadMiddleware
    {
        /// <summary>
        /// 使用路由重定向，把参数符合IMultiPartUploadFileInput类型的接口转发到MultiPartUploadFileOptions.UploadApiPath地址
        /// 注意不能和app.MapControllers()中间件同时使用，app.MapControllers会使重定向失效
        /// 应该使用app.UseRouting();和app.UseEndpoints(endpoints => endpoints.MapControllers());
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseAutoRedirectToMultiPartUploadMiddleware(this IApplicationBuilder app)
        {
            // 使用自定义中间件。
            return app.UseMiddleware<AutoRedirectToMultiPartUploadMiddleware>();
        }
    }
    public class AutoRedirectToMultiPartUploadMiddleware
    {
        private readonly RequestDelegate _next;
        /// <summary>
        /// 分段上传参数IMultiPartUploadFileInput的全部属性
        /// </summary>
        private readonly IReadOnlyCollection<PropertyInfo> AllPropertyInfo;
        /// <summary>
        /// 分段上传参数IMultiPartUploadFileInput的必填属性
        /// </summary>
        private readonly IReadOnlyCollection<PropertyInfo> RequiredAllPropertyInfo;
        /// <summary>
        /// 设置json序列化忽略大小写
        /// </summary>
        private readonly JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        public AutoRedirectToMultiPartUploadMiddleware(RequestDelegate next)
        {
            _next = next;
            AllPropertyInfo = typeof(IMultiPartUploadFileInput).GetProperties();
            RequiredAllPropertyInfo = AllPropertyInfo.Where(d => d.GetCustomAttribute(typeof(RequiredAttribute)) != null).ToList();
        }

        public async Task InvokeAsync(HttpContext context
            , ILogger<AutoRedirectToMultiPartUploadMiddleware> logger
            , IOptions<MultiPartUploadFileOptions> options
            , IHostEnvironment environment)
        {
            var option = options.Value;

            #region 判断不需要转发的情况

            if (context.Request.Path == option.UploadApiPath)
            {
                await _next(context);
                return;
            }
            if (!context.Request.HasFormContentType)
            {
                logger.LogDebug("没有检测到表单数据，不转发到分段上传");
                await _next(context);
                return;
            }
            // 获取请求的正文。
            var formCollection = context.Request.Form;
            //没有文件不转发
            if (!formCollection.Files.Any())
            {
                logger.LogDebug("没有检测到文件数据，不转发到分段上传");
                await _next(context);
                return;
            }
            var requiredNotSendProperty =
                AllPropertyInfo.Where(d => d.PropertyType != typeof(IFormFile)).Where(d => formCollection.All(f => !string.Equals(f.Key, d.Name, StringComparison.CurrentCultureIgnoreCase))).ToList();
            if (requiredNotSendProperty.Any())
            {
                var propsNames = string.Join(',', requiredNotSendProperty.Select(d => d.Name));
                logger.LogDebug("检测到文件数据，但是以下必填属性没有填写：{propsNames}，不转发到分段上传", propsNames);
                await _next(context);
                return;
            }
            #endregion


            var name = formCollection.Files[0].Name;
            var fileName = formCollection.Files[0].FileName;
            var originPath = context.Request.Path;
            context.Request.Path = option.UploadApiPath;
            //context.SetEndpoint(null);
            //var newFormFileCollection = new FormFileCollection();
            //newFormFileCollection.Add(new FormFile());
            //var newFormCollection = formCollection.ToDictionary();
            //newFormCollection[nameof(IMultiPartUploadFileInput.IsFinish)] = "true";
            //context.Request.Form = new FormCollection(newFormCollection, newFormFileCollection);

            // 将请求转发到 UploadApi(IMultiPartUploadFileInput input)。
            //IMultiPartUploadFileInput multiPartUploadFileInput = FromToMultiPartUploadFileInput(context.Request.Form);
            //await context.Request.HttpContext.RequestServices.GetService<MultiPartUploadFileController>().MultiPartUploadFile(multiPartUploadFileInput);
            //await _next(context);
            var currentBody = context.Response.Body;
            using (var memoryStream = new MemoryStream())
            {
                context.Response.Body = memoryStream;
                await _next.Invoke(context);
                context.Response.Body = currentBody;
                memoryStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(memoryStream).ReadToEndAsync();
                IMultiPartUploadFileOutput output = null;

                if (GetContentType(context.Response.ContentType) == "application/json" && !string.IsNullOrEmpty(responseBody))
                {
                    try
                    {
                        output = JsonSerializer.Deserialize<MultiPartUploadFileDefaultOutput>(responseBody, jsonSerializerOptions);
                    }
                    catch (Exception e)
                    {
                        logger.LogError(e, "从分段上传文件接口读取返回结果序列化为MultiPartUploadFileDefaultOutput类型出错，返回数据为：{responseBody}", responseBody);
                    }
                }
                if (output is { IsMerged: true } && !string.IsNullOrEmpty(output.FileName))
                {
                    context.Request.Path = originPath;
                    context.SetEndpoint(null);
                    var newFormFileCollection = new FormFileCollection();
                    var fileFullPath = Path.Combine(environment.ContentRootPath, option.SavePath, output.FileName);
                    await using var fileStream = new FileStream(fileFullPath, FileMode.Open, FileAccess.Read);
                    var formFile = new FormFile(fileStream, 0, fileStream.Length, name, fileName);
                    newFormFileCollection.Add(formFile);
                    var newFormCollection = new Dictionary<string, StringValues>(formCollection);//formCollection.ToDictionary(null);
                    context.Request.Form = new FormCollection(newFormCollection, newFormFileCollection);
                    context.Response.ContentType = null;
                    context.Response.Headers.Clear();
                    await _next(context);
                    return;
                }
            }
            context.Request.Path = originPath;
        }

        private string GetContentType(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return "";
            }
            var contentType1 = contentType.ToLower();
            contentType1 = contentType1.Split(';', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            return contentType1 ?? "";
        }
    }
}
