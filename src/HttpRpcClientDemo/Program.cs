using Newtonsoft.Json;
using Quick.WebApi.Rpc;
using RpcProtocolDemo;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HttpRpcClientDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                HttpRpcClient udpRpcClient = new HttpRpcClient("https://127.0.0.1:5001/rpc");

                udpRpcClient.RegisterClientServiceProxy<IOrderService>();
                IOrderService orderService = udpRpcClient.GetClientServiceProxy<IOrderService>();

                DateTime serverTime = orderService.GetServerTime();
                Console.WriteLine($"The server time is {serverTime}");

                RunOtherTest(orderService);
                RunMultiThreadTest(orderService);

                Console.WriteLine("Press any key to exit!");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }

        public static void RunOtherTest(IOrderService orderService)
        {
            //1
            bool isUserExist = orderService.IsUserNameExist("admin");
            Console.WriteLine(isUserExist ? "The user \"admin\" is exists." : "The user \"admin\" is not exists.");

            //2
            LoginRequest request = new LoginRequest()
            {
                User = "admin",
                Password = "password"
            };
            LoginResult loginResult = orderService.Login(request);
            Console.WriteLine(JsonConvert.SerializeObject(loginResult, Formatting.Indented));

            //3
            try
            {
                List<Order> orderList = orderService.GetUserOrderList(loginResult.UserId, loginResult.Token);
                Console.WriteLine(JsonConvert.SerializeObject(orderList, Formatting.Indented));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            //4
            double amount = orderService.SubmitOrder(123456);
            Console.WriteLine($"Order submitted successfully. The total amount is {amount}");

            //5
            orderService.LockUser(123456, null);
            orderService.LockUser(123456, 4);

            //6
            int[] users = new int[2] { 1, 2 };
            foreach (var id in users)
            {
                int? days = orderService.GetUserLockDays(id);
                if (days == null)
                {
                    Console.WriteLine("The user is not locked.");
                }
                else
                {
                    Console.WriteLine($"The user locked for {days.Value} days.");
                }
            }

            //7
            orderService.Ping();
            Console.WriteLine("Ping server successfully.");
        }

        public static void RunMultiThreadTest(IOrderService orderService)
        {
            for (int i = 1; i <= 3; i++)
            {
                Task.Factory.StartNew(idx =>
                {
                    for (int j = 1; j <= 100; j++)
                    {
                        DateTime dateTime = orderService.GetServerTime();
                        Console.WriteLine($"{idx}_{j}: {dateTime}");
                    }
                }, i);
            }
        }
    }
}
