using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using XSX.MultiPartUploadFile.Dto;
using XSX.MultiPartUploadFile.Options;

namespace XSX.MultiPartUploadFile.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class MultiPartUploadFileController : ControllerBase
    {
        private readonly IHostEnvironment _environment;
        private readonly MultiPartUploadFileOptions _multiPartUploadFileOption;

        public MultiPartUploadFileController(IOptions<MultiPartUploadFileOptions> options, IHostEnvironment environment)
        {
            _environment = environment;
            _multiPartUploadFileOption = options.Value;
        }

        /// <summary>
        /// 分段上传文件，每段存为临时文件,临时文件名为：{fileName}_start_length.temp
        /// </summary>
        /// <param name="input"></param>
        /// <exception cref="ArgumentException"></exception>

        [HttpPost()]
        public async Task<IMultiPartUploadFileOutput> UploadFile([FromForm]MultiPartUploadFileDefaultInput input)
        {
            var fileName = input.Id+Path.GetExtension(input.FileName);
            if (fileName.Contains('/') || fileName.Contains('\\'))
            {
                throw new ArgumentException($"文件名称不合法:{fileName}", nameof(input.Id));
            }
            if (input.File == null || input.File.Length == 0)
            {
                throw new ArgumentException("文件内容不能为空", nameof(input.File));
            }

            var fileDir = Path.Combine(_environment.ContentRootPath, _multiPartUploadFileOption.SavePath);
            if (!Directory.Exists(fileDir))
            {
                Directory.CreateDirectory(fileDir);
            }


            var fileTemp = $"{fileName}_{input.StartByteIndex}_{input.File.Length}.temp";
            var fileTempFullPath = Path.Combine(fileDir, fileTemp);
            await using var stream = new FileStream(fileTempFullPath, FileMode.Create);
            await input.File.CopyToAsync(stream);
            stream.Close();
            //System.IO.File.WriteAllBytes(fileTempFullPath, input.File);

            var tempFiles = Directory.EnumerateFiles(fileDir, "*.temp").Where(d => d.Contains(fileName)).ToList();
            var allTempFiles = tempFiles.Select(d =>
            {
                var reg = Regex.Match(d, @"^.+_(\d+)_(\d+)\.temp$");
                return reg.Success
                    ? new
                    {
                        FileFullPath = d,
                        Start = long.Parse(reg.Groups[1].Value),
                        Length = long.Parse(reg.Groups[2].Value)
                    }
                    : null;
            }).Where(d => d != null).OrderBy(d => d.Start).ToList();
            var currentTotalLength = allTempFiles.DefaultIfEmpty().Sum(d => d.Length);
            if (currentTotalLength == input.TotalLength)
            {
                var currentLength = 0L;
                foreach (var tempFile in allTempFiles)
                {
                    if (currentLength!=tempFile.Start)
                    {
                        foreach (var tempFile1 in allTempFiles)
                        {
                            System.IO.File.Delete(tempFile1.FileFullPath);
                        }
                        throw new ArgumentException($"文件{input.Id}合并失败：长度不连续，文件已删除，请重新上传文件");
                    }
                    currentLength += tempFile.Length;
                }
                var mergeFileName = $"{input.FileName}";
                var mergeFileNameFullPath = Path.Combine(fileDir, mergeFileName);
                while (System.IO.File.Exists(mergeFileNameFullPath))
                {
                    mergeFileName = $"{Guid.NewGuid().ToString().Substring(0, 8)}_{input.FileName}";
                    mergeFileNameFullPath = Path.Combine(fileDir, mergeFileName);
                }

                await using var mergeFile = new FileStream(mergeFileNameFullPath,FileMode.CreateNew);

                foreach (var tempFile in allTempFiles)
                {
                    await using var sourceStream = new FileStream(tempFile.FileFullPath, FileMode.Open,FileAccess.Read);
                    var buffer = new byte[4096]; // 缓冲区大小

                    int bytesRead;
                    while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        mergeFile.Write(buffer, 0, bytesRead);
                    }
                    sourceStream.Close();
                    System.IO.File.Delete(tempFile.FileFullPath);
                }

                return new MultiPartUploadFileDefaultOutput()
                {
                    FileName = mergeFileName,
                    IsMerged = true
                };
            }

            return new MultiPartUploadFileDefaultOutput()
            {
                FileName = fileTemp,
                IsMerged = false
            };
        }

       
    }
}
