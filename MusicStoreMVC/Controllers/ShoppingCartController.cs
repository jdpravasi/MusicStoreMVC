using MusicStoreMVC.Models;
using MusicStoreMVC.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MusicStoreMVC.Controllers
{
    public class ShoppingCartController : Controller
    {
        MusicStoreEntities storeDB = new MusicStoreEntities();
        // GET: /ShoppingCart
        public ActionResult Index()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);

            // Set up our ViewModel
            var viewModel = new ShoppingCartViewModel
            {
                CartItems = cart.GetCartItems(),
                CartTotal = cart.GetTotal()
            };
            return View(viewModel);
        }

        // Get/ShoppingCart/AddToCart/5
        public ActionResult AddToCart(int id)
        {
            // retrive album from DB
            var addedAlbum = storeDB.Albums.Single(
                              album => album.AlbumId == id);

            // add it to shopping cart
            var cart = ShoppingCart.GetCart(this.HttpContext);
            cart.AddToCart(addedAlbum);
            // back to main page for 
            return RedirectToAction("Index");
        }

        //AJAX: /ShoppingCart/RemoveFromCart/5
        [HttpPost]
        public ActionResult RemoveFromCart(int id)
        {
            // retriving item to delete
            var cart = ShoppingCart.GetCart(this.HttpContext);

            //getting name of album for confirmation
            string albumName = storeDB.Carts.Single(
                                item => item.RecordId == id).Album.Title;

            //Remove from cart
            int itemCount = cart.RemoveFromCart(id);

            // display confirmation message
            var result = new ShoppingCartRemoveViewModel
            {
                Message = Server.HtmlEncode(albumName) + "Has been removed from cart",
                CartTotal = cart.GetTotal(),
                CartCount = cart.GetCount(),
                ItemCount = itemCount,
                DeleteId = id
            };
            return Json(result);
        }

        // GET: /ShoppingCart/CartSummary
        [ChildActionOnly]
        public ActionResult CartSummary()
        {
            var cart = ShoppingCart.GetCart(this.HttpContext);
            ViewData["CartCount"] = cart.GetCount();    
            return PartialView(cart);
        }
    }
}