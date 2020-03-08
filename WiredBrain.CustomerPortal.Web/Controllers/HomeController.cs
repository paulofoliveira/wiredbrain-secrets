using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using WiredBrain.CustomerPortal.Web.Models;
using WiredBrain.CustomerPortal.Web.Repositories;

namespace WiredBrain.CustomerPortal.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IConfiguration _configuration;

        public HomeController(ICustomerRepository customerRepository, IConfiguration configuration)
        {
            this._customerRepository = customerRepository;
            this._configuration = configuration;
        }

        public IActionResult Index()
        {
            ViewBag.Title = "Enter Loyalty Number";
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(int loyaltyNumber)
        {
            var customer = await _customerRepository.GetCustomerByLoyaltyNumber(loyaltyNumber);
            if (customer == null)
            {
                ModelState.AddModelError(string.Empty, "Unknown loyalty number");
                return View();
            }
            return RedirectToAction("LoyaltyOverview", new { loyaltyNumber });
        }

        public async Task<IActionResult> LoyaltyOverview(int loyaltyNumber)
        {
            ViewBag.Title = "Your Account";

            var customer = await _customerRepository.GetCustomerByLoyaltyNumber(loyaltyNumber);
            var pointsNeeded = int.Parse(_configuration["CustomerPortalSettings:PointsNeeded"]);

            var loyaltyModel = LoyaltyModel.FromCustomer(customer, pointsNeeded);
            return View(loyaltyModel);
        }

        public async Task<IActionResult> EditFavorite(int loyaltyNumber)
        {
            ViewBag.Title = "Edit Favorite";

            var customer = await _customerRepository.GetCustomerByLoyaltyNumber(loyaltyNumber);
            return View(new EditFavoriteModel
            {
                LoyaltyNumber = customer.LoyaltyNumber,
                Favorite = customer.FavoriteDrink
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditFavorite(EditFavoriteModel model)
        {
            await _customerRepository.SetFavorite(model);
            return RedirectToAction("LoyaltyOverview", new { loyaltyNumber = model.LoyaltyNumber });
        }

        public async Task<IActionResult> AddCredit(int loyaltyNumber)
        {
            ViewBag.Title = "Redirect To Payment Provider...";
            ViewBag.PaymentProviderKey = "XY-InCode-1234";
            
            return View();
        }
    }
}
