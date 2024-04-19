using ApiServiceMVC.DatabaseEnty;
using ApiServiceMVC.DatabaseEnty.Service;
using ApiServiceMVC.Models.Orders;
using ApiServiceMVC.Models.Orders.BaseClass;
using ApiServiceMVC.Models.Orders.OrderWithItems;
using Microsoft.AspNetCore.Authorization;
using ApiServiceMVC.Models.Orders.CustomerWithItems;
using Microsoft.AspNetCore.Mvc;

namespace ApiServiceMVC.Controllers {
    public class OrderController : Controller {
        private readonly ILogger<OrderController> _logger;
        private DatabaseService databaseService;
        public OrderController(ILogger<OrderController> logger, DatabaseService databaseService) {
            _logger = logger;
            this.databaseService = databaseService;
        }
        [HttpPost]
        [Route("/addorder")]
        [Authorize]
        public async  Task<IActionResult> AddOrder([FromBody] OrderWithItems orderInfo) {
            Console.WriteLine("Addorder++");
            if (orderInfo.isFull())
            {
                try{
                   bool result =  await databaseService.AddOrder(orderInfo);
                    _logger.LogTrace($"result={result}");
                    if (result){
                        return Json(true);
                    }
                }catch (Exception ex){}
            }
            return Json(false);
        }

        [HttpGet]
        [Route("/orders/{skip}/{limit}")]
        [Authorize]
        public async Task<IActionResult> GetOrders(int limit, int skip, string searchPhone) {
            Console.WriteLine($"NotSearch");
            List<CustomerWithItems> customerWithItems = await databaseService.GetOrders(skip, limit, searchPhone);
            return Json(customerWithItems);
        }

        [HttpGet]
        [Route("/orders/{skip}/{limit}/{search}")]
        [Authorize]
        public async Task<IActionResult> SearchOrders(int limit, int skip, string searchPhone) {
            Console.WriteLine($"Search");
            List<CustomerWithItems> customerWithItems = await databaseService.GetOrders(skip,limit,searchPhone);
            _logger.LogTrace(" / orders /{ skip}/{ limit}/{ search}");
            return Json(customerWithItems);
        }
    }
}
