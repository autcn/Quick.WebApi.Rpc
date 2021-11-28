# Quick.WebApi.Rpc
The library provides a very simple RPC mechanism in .net webapi project. It is very easy to use. You only need three steps.

The nuget url is : https://www.nuget.org/packages/Quick.WebApi.Rpc

## Step1:  Protocol

First of all, create a dll project to define the RPC protocol. 

``` c#
public interface IOrderService
{
    LoginResult Login(LoginRequest request);
}
public class LoginRequest
{
    public string User { get; set; }
    public string Password { get; set; }
}

public class LoginResult
{
    public int UserId { get; set; }
    public bool IsSuccess { get; set; }
    public string Remark { get; set; }
    public string Token { get; set; }
}
```

## Step 2:  Server

Create a webapi project and reference the dll created in last step.  

Implements IOrderService interface:

``` c#
 public class OrderService : IOrderService
 {
     public LoginResult Login(LoginRequest request)
     {
         if (request == null)
         {
             throw new Exception("The request parameter is required.");
         }
         LoginResult result = new LoginResult();
         if (request.User == "admin" && request.Password == "password")
         {
             result.UserId = 2334143;
             result.IsSuccess = true;
             result.Token = "2938d828s8a8823";
         }
         else
         {
             result.Remark = "The user name or password is invalid.";
         }
         return result;
     }
 }
```

In startup.cs file,  add following codes:

``` c#
 public void ConfigureServices(IServiceCollection services)
 {
	 ...
     services.AddQuickRpc();
     services.AddSingleton<IOrderService, OrderService>();
 }

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    ...
    app.UseQuickRpc("/rpc");
    ...
}
```

## Step 3:  Client

Use HttpRpcClient class to make RPC calls.

``` c#
class Program
{
    static void Main(string[] args)
    {
        try
        {
            HttpRpcClient udpRpcClient = new HttpRpcClient("https://127.0.0.1:5001/rpc");

            udpRpcClient.RegisterClientServiceProxy<IOrderService>();
            IOrderService orderService = udpRpcClient.GetClientServiceProxy<IOrderService>();
            
            //Run the RPC test.
            LoginRequest request = new LoginRequest()
            {
                User = "admin",
                Password = "password"
            };
            LoginResult loginResult = orderService.Login(request);
            Console.WriteLine(JsonConvert.SerializeObject(loginResult, Formatting.Indented));

            Console.WriteLine("Press any key to exit!");
            Console.ReadLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.ReadLine();
        }
    }
}
```

