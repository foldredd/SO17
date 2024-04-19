using ApiServiceMVC.DatabaseEnty.Service;
using ApiServiceMVC.Models.Orders;
using ApiServiceMVC.Models.Orders.BaseClass;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiServiceMVC.Controllers {
    public class ItemController : Controller {
        private readonly ILogger<ItemController> _logger;
        private DatabaseService databaseService;
        public ItemController(ILogger<ItemController> logger,DatabaseService databaseService) {
            _logger = logger;
            this.databaseService = databaseService;
        }
        [HttpPost]
        [Authorize]
        public IActionResult CreateItem([FromBody] BaseItem baseItem) {
            Console.WriteLine($"{baseItem.Price}");
            if (baseItem.IsFull() == false){
                return BadRequest();
            }
            Item item = new Item(baseItem.Title, baseItem.SKU, baseItem.Quantity, baseItem.Price);
            if (databaseService.AddItem(item)){
                return Json(true);
            }
            return Json(false);
        }

    }
}
