using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace XSX.MultiPartUploadFile.Dto
{
    /// <summary>
    /// 分段上传文件输入
    /// </summary>
    public interface IMultiPartUploadFileInput
    {
        /// <summary>
        /// 文件内容
        /// </summary>
        [Required]
        public IFormFile File { get; set; }
        /// <summary>
        /// 当前分段在全文件的开始字节位置
        /// </summary>
        [Required]
        public long StartByteIndex { get; set; }
        /// <summary>
        /// 文件总长度
        /// </summary>
        [Required]
        public long TotalLength { get; set; }
        /// <summary>
        /// 文件名称
        /// </summary>
        [Required]
        public string FileName { get; set; }
        /// <summary>
        /// 标识码，同一个文件多个分段应该一样，请使用随机值，避免与其他文件相同造成错误合并到其他文件的分段
        /// </summary>
        [Required]
        public string Id { get; set; }
    }
    /// <summary>
    /// 需要自己处理接口返回，如果定义的webapi服务参数继承了这个接口，中间件自动处理文件上传后仍然会调用原来的webapi服务，需要在服务中判断文件是否上传完，并处理，否则只会在文件上传完后调用原来的webapi服务
    /// </summary>
    public interface INeedHandleRequest { }
}
