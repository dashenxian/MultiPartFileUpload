using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using XSX.MultiPartUploadFile.Options;

namespace XSX.MultiPartUploadFile.Middlewares
{
    public static class AddControllerExtensions
    {
        /// <summary>
        /// 注册当前项目的controller
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IMvcBuilder AddMultiPartFileUploadController(this IMvcBuilder builder)
        {
            return builder.AddApplicationPart(typeof(AddControllerExtensions).Assembly);
        }
    }
}
