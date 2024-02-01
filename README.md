# PartUpload

## 客户端调用示例linqpad
```c#
async Task<int> Main(string[] args)
{
	var tasks = new List<Task>();
	for (int i = 0; i < 10; i++)
	{
		tasks.Add(Task.Run(async () =>
		{
			await Upload();
		}));
	}

	await Task.WhenAll(tasks);

	return 0;
}
private static async Task Upload()
{
	var url = "http://localhost:5163/";
	//var gitApi = RestService.For<IGitHubApi>(url);
	var multiApi = ProxyRestService.For<IMultiPartUploadFileApi>(url);
	var file = new System.IO.FileStream(@"C:\Users\Administrator\Desktop\1.zip", FileMode.Open, FileAccess.Read);
	var totalLength = file.Length;
	var partCount = 10;
	var partLength = (int)(totalLength / partCount);
	using var httpclient = new HttpClient();
	var id = Guid.NewGuid().ToString();
	for (int i = 0; i < partCount; i++)
	{
		var currentPartLength = partLength;
		if (i == partCount - 1)
		{
			currentPartLength = (int)(totalLength - partLength * (partCount - 1));
		}
		var content = new byte[currentPartLength];
		file.Read(content, 0, currentPartLength);
		//using var ms = new MemoryStream(content);
		var input = new MultiPartUploadFileDefaultInput()
		{
			FileName = Path.GetFileName(file.Name),
			StartByteIndex = i * partLength,
			File = new ByteArrayPart(content, fileName: Path.GetFileName(file.Name)),
			TotalLength = totalLength,
			Id = id,
		};
		var result = await multiApi.MultiPartUploadFileTest(input.File, input.StartByteIndex, input.TotalLength, input.FileName, input.Id);
		//var result = await multiApi.MultiPartUploadFile(input);
		Console.WriteLine(result);
		//await Send(httpclient,input);
	}
}
public interface IMultiPartUploadFileApi
{
	[Multipart]
	[Post("/MultiPartUploadFile/UploadFile")]
	//Task<string> MultiPartUploadFile(MultiPartUploadFileDefaultInput input);
	Task<string> MultiPartUploadFile([AliasAs(nameof(MultiPartUploadFileDefaultInput.File))] MultipartItem File, long StartByteIndex, long TotalLength, string FileName, string id);
	[Multipart]
	[Post("/MultiPartUploadFileTest")]
	//Task<string> MultiPartUploadFile(MultiPartUploadFileDefaultInput input);
	Task<string> MultiPartUploadFileTest([AliasAs(nameof(MultiPartUploadFileDefaultInput.File))] MultipartItem File, long StartByteIndex, long TotalLength, string FileName, string id);
}

public class MultiPartUploadFileDefaultInput
{
	/// <summary>
	/// 文件内容
	/// </summary>
	[Required]
	public required MultipartItem File { get; set; }
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
	public required string FileName { get; set; }
	/// <summary>
	/// 标识码，同一个文件多个分段应该一样，请使用随机值，避免与其他文件相同造成错误合并到其他文件的分段
	/// </summary>
	[Required]
	public required string Id { get; set; }
}

public class ProxyRestService
{
	static readonly ProxyGenerator Generator = new ProxyGenerator();

	public static T For<T>(HttpClient client)
		where T : class
	{
		if (!typeof(T).IsInterface)
		{
			throw new InvalidOperationException("T must be an interface.");
		}

		var interceptor = new RestMethodInterceptor<T>(client);
		return Generator.CreateInterfaceProxyWithoutTarget<T>(interceptor);
	}

	public static T For<T>(string hostUrl)
		where T : class
	{
		var client = new HttpClient() { BaseAddress = new Uri(hostUrl) };
		return For<T>(client);
	}

	class RestMethodInterceptor<T> : IInterceptor
	{
		static readonly Dictionary<string, Func<HttpClient, object[], object>> methodImpls
			= typeof(T).GetMethods().Select(m => m.Name) //Refit 版本4+
			//RequestBuilder.ForType<T>().InterfaceHttpMethods //Refit 版本4-
				.ToDictionary(k => k, v => RequestBuilder.ForType<T>().BuildRestResultFuncForMethod(v));

		readonly HttpClient client;

		public RestMethodInterceptor(HttpClient client)
		{
			this.client = client;
		}

		public void Intercept(IInvocation invocation)
		{
			if (!methodImpls.ContainsKey(invocation.Method.Name))
			{
				throw new NotImplementedException();
			}
			invocation.ReturnValue = methodImpls[invocation.Method.Name](client, invocation.Arguments);
		}
	}
}
```