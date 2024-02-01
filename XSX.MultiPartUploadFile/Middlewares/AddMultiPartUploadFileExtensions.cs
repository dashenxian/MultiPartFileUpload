using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace XSX.MultiPartUploadFile.Middlewares
{
    public static class AddMultiPartUploadFileExtensions
    {
        public static IHostApplicationBuilder AddMultiPartUploadFile(this IHostApplicationBuilder builder,IMvcBuilder mvcBuilder)
        {
            mvcBuilder.AddMultiPartFileUploadController();
            builder.AddMultiPartUploadFileConfig();
            return builder;
        }
    }
}
