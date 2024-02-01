using XSX.MultiPartUploadFile.Controllers;

namespace XSX.MultiPartUploadFile.Options
{
    public class MultiPartUploadFileOptions
    {
        public const string MultiPartUploadFile = "MultiPartUploadFileOption";
        /// <summary>
        /// 上传文件存储目录
        /// </summary>
        public string SavePath { get; set; } = "UploadFile";
        /// <summary>
        /// 上传路由
        /// </summary>
        public string UploadApiPath { get; set; } = $"/{nameof(MultiPartUploadFileController).Replace("Controller","")}/{nameof(MultiPartUploadFileController.UploadFile)}";
    }
}
