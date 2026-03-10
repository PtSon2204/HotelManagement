using System.Diagnostics;
using HotelManagement.Models;
using HotelManagement.Models.Entities;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //Fake data sau này phải sửa 
            var roomTypes = new List<RoomType>
    {
        new RoomType
        {
            Name = "Single Room",
            Image = "img/room1.jpg",
            Price = 500000
        },
        new RoomType
        {
            Name = "Double Room",
            Image = "img/room2.jpg",
            Price = 800000
        },
        new RoomType
        {
            Name = "VIP Room",
            Image = "img/room3.jpg",
            Price = 1500000
        }
    };

            return View(roomTypes);
        }
    }
 }

