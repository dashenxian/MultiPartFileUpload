using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XSX.MultiPartUploadFile.Options;

namespace XSX.MultiPartUploadFile.Middlewares
{
    public static class AddMultiPartUploadFileConfigExtensions
    {
        /// <summary>
        /// 从配置节点[MultiPartUploadFileOption]读取文件上传配置
        /// </summary>
        /// <param name="builder"></param>
        public static void AddMultiPartUploadFileConfig(this IHostApplicationBuilder builder)
        {
            builder.Services.Configure<MultiPartUploadFileOptions>(
                builder.Configuration.GetSection(MultiPartUploadFileOptions.MultiPartUploadFile));
        }
        /// <summary>
        /// 配置文件上传配置
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="action"></param>
        public static void AddMultiPartUploadFileConfig(this IHostApplicationBuilder builder,Action<MultiPartUploadFileOptions> action)
        {
            builder.Services.Configure<MultiPartUploadFileOptions>(Microsoft.Extensions.Options.Options.DefaultName, action);
        }
    }
}
