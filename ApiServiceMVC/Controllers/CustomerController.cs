using ApiServiceMVC.DatabaseEnty.Service;
using ApiServiceMVC.Models.Orders;
using ApiServiceMVC.Models.Orders.BaseClass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiServiceMVC.Controllers {
    public class CustomerController : Controller {
        private readonly ILogger<ItemController> _logger;
        private DatabaseService databaseService;
        public CustomerController(ILogger<ItemController> logger, DatabaseService databaseService) {
            _logger = logger;
            this.databaseService = databaseService;
        }
        [HttpGet]
        [Route("/customers/{skip}/{limit}")]
        [Authorize]
        public IActionResult GetCustomer(int limit,int skip, string searchPhone) {
            _logger.LogTrace("Not Search");
            List<Customer> customerList =  databaseService.GetCustomerList(limit,skip, searchPhone);
            if(customerList == null){
                _logger.LogError("customerlist null");
                return BadRequest();
            }
            List<BaseCustomer> baseCustomers = ConvertService.CustomerToBaseCustomerList(customerList);
            _logger.LogTrace("Log true");
            return Json(baseCustomers);
        }

        public async Task<IActionResult> NewCustomer([FromBody] BaseCustomer baseCustomer) {
           Customer customer = new Customer(baseCustomer.Name,baseCustomer.Surname,baseCustomer.Phone);
            if (await databaseService.NewCustomerAsync(customer)) {
                return Json((true));
            }
            return Json(false);
        }
        [HttpGet]
        [Route("/customers/{skip}/{limit}/{search}")]
        [Authorize]
        public IActionResult SearchCustomers(int limit, int skip, string search ) {
            _logger.LogTrace("Search");
            List<Customer> customerList = databaseService.GetCustomerList(limit, skip, search);
            if (customerList == null)
            {
                _logger.LogError("customerlist null");
                return BadRequest();
            }
            List<BaseCustomer> baseCustomers = ConvertService.CustomerToBaseCustomerList(customerList);
            _logger.LogTrace("Log true");
            return Json(baseCustomers);

        }
    }
}
