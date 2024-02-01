using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace XSX.MultiPartUploadFile.Dto
{
    public class MultiPartUploadFileDefaultInput : IMultiPartUploadFileInput
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
}
