### Description
Mimic creates type at runtime that implements target interface 
and handle all interface methods call with custom class

### Usage
To mock an interface
```c#
    public interface IClient
    {
        Task<int> Foo(string s, object o, int i);
        string Bar(string s);
    }
```
create handler class
```c#
    public class RealWorker : IMimicWorker
    {
        public async Task<T> Mock<T>(string mockMethodName, MockParameter[] args)
        {
            ...
        }
    }
```
and create mimic
```c#
    var mimic = new Mimic<IClient, RealWorker>();
```
now you can get mocking object
```c#
    var instance = mimic.NewInstance(new RealWorker());
    instance.Foo("pickle-pee", "pump-a-rum", 42);
```
generated type will be similar to
```c#
    public class IClientMimic : IClient
    {
        private readonly RealWorker _worker;

        public IClientMimic(RealWorker worker)
        {
            _worker = worker;
        }

        public Task<int> Foo(string s, object o, int i)
        {
            return _worker.Mock<int>(
                "Foo",
                new MockParameter[]
                {
                    new MockParameter("s", typeof(string), s),
                    new MockParameter("o", typeof(object), o),
                    new MockParameter("i", typeof(int), i)
                }
            );
        }

        public string Bar(string s)
        {
            return _worker.Mock<string>(
                "Bar",
                new MockParameter[]
                {
                    new MockParameter("s", typeof(string), s)
                }
            ).Result;
        }
    }
```
### ASP Example

See AspExample for full solution

For example we have class with interface injected through DI

```c#
    // IUsefulStuff.cs
    public interface IUsefulStuff
    {
        Task<string> GetSomeValue(string key);
        Task SetSomeValue(string key, string value);
    }
    
    // UsefulStuff.cs
    public class UsefulStuffService : IUsefulStuff
    {
        public async Task<string> GetSomeValue(string key)
        {
            ...
        }

        public async Task SetSomeValue(string key, string value)
        {
            ...
        }
    }
    
    // Starup.cs
    public void ConfigureServices(IServiceCollection services)
        {
            ...
            services.AddScoped<IUsefulService, UsefulService>();
            ...
        }
     
    // ImportantController.cs   
    [ApiController]
    [Route("[controller]")]
    public class ImportantController : ControllerBase
    {
        private readonly IUsefulStuff _usefulStuff;

        public ImportantController(IUsefulStuff usefulStuff)
        {
            _usefulStuff = usefulStuff;
        }
        
        ...
    }        
```

But at some point UsefulStuff migrated to separate remote service
We can "publish" it as jsonRPC (example use [EdjCase.JsonRpc.Router](https://github.com/edjCase/JsonRpc)), create JsonRPC "client" for mimic and add mimic to DI instead of UsefulStuffService

```c#
    // RemoteUsefulStuffServiceController.cs
    [RpcRoute("/api/v4/useful-jsonrpc")]
    public class RemoteUsefulStuffServiceController : RpcController, IUsefulStuff
    {
        public async Task<string> GetSomeValue(string key)
        {
            ...
        }

        public async Task SetSomeValue(string key, string value)
        {
            ...
        }
    }
    
    // JsonRpcRealWorker.cs
    public class JsonRpcRealWorker : IMimicWorker
    {
        ...

        public async Task<T> Mock<T>(string mockMethodName, MockParameter[] args)
        {
            var requestModel = new List<object>();

            foreach (var mockParameter in args)
            {
                requestModel.Add(mockParameter.Value);
            }

            var jsonRpcRequest = new
            {
                id = 1,
                jsonrpc = "2.0",
                method = mockMethodName,
                @params = requestModel
            };

            var jsonRequestString = JsonSerializer.Serialize(jsonRpcRequest);

            var responseMessage = await _httpClient.PostAsync($"api/v4/useful-jsonrpc",
                new StringContent(jsonRequestString, Encoding.UTF8, "application/json"));

            var responseString = await responseMessage.Content.ReadAsStringAsync();

            var response = JsonSerializer.Deserialize<JsonRpcResponse<T>>(responseString,
                new JsonSerializerOptions {PropertyNameCaseInsensitive = true});
            return response.Result;
        }
    }
    
    // Startup.cs
    services.AddHttpClient<JsonRpcRealWorker>(client =>
    {
        client.BaseAddress = new Uri("http://localhost:3000");
    });
    services.AddScopedMimic<IUsefulStuff, JsonRpcRealWorker>();
```