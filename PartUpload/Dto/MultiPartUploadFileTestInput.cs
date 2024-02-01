using XSX.MultiPartUploadFile.Dto;

namespace PartUpload.Dto
{
    public class MultiPartUploadFileTestInput:IMultiPartUploadFileInput
    {
        public IFormFile File { get; set; }
        public long StartByteIndex { get; set; }
        public long TotalLength { get; set; }
        public string FileName { get; set; }
        public string Id { get; set; }

        public string? Name { get; set; }
    }
}
