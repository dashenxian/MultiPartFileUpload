using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSX.MultiPartUploadFile.Dto
{
    /// <summary>
    /// 文件分段上传返回结果
    /// </summary>
    public interface IMultiPartUploadFileOutput
    {
        /// <summary>
        /// 保存文件名称
        /// </summary>
        public string FileName { get; set; }
        /// <summary>
        /// 是否合并完成
        /// </summary>
        public bool IsMerged { get; set; }
    }
}
