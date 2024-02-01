using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XSX.MultiPartUploadFile.Dto
{
    /// <summary>
    /// 文件分段上传返回结果默认实现
    /// </summary>
    public class MultiPartUploadFileDefaultOutput: IMultiPartUploadFileOutput
    {
        public string FileName { get; set; }
        public bool IsMerged { get; set; }
    }
}
