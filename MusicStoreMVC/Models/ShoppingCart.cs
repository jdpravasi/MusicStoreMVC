using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MusicStoreMVC.Models;
using MusicStoreMVC.Controllers;


namespace MusicStoreMVC.Models
{
    public partial class ShoppingCart
    {
        MusicStoreEntities storeDB = new MusicStoreEntities();
        string ShoppingCartId { get; set; }
        public const string CartSessionKey = "CartId";

        public static ShoppingCart GetCart(HttpContextBase context)
        {
            var cart = new ShoppingCart();
            cart.ShoppingCartId = cart.GetCartId(context);
            return cart;
        }
        // aa method simplify karse shooping cart na calls
        public static ShoppingCart GetCart(Controller controller)
        {
            return GetCart(controller.HttpContext);
        }

        public void AddToCart(Album album)
        {
            var cartItem = storeDB.Carts.SingleOrDefault(
                c => c.CartId == ShoppingCartId &&
                c.AlbumId == album.AlbumId );

            if(cartItem == null)
            {
                // navi item banse if kaij nai hoy cart ma
                cartItem = new Cart
                {
                    AlbumId = album.AlbumId,
                    CartId = ShoppingCartId,
                    Count = 1,
                    DateCreated = DateTime.Now,
                };
                storeDB.Carts.Add(cartItem);
            }
            else
            {
                cartItem.Count++;
            }
            storeDB.SaveChanges();
        }

        public int RemoveFromCart(int id)
        {
            // get cart
                var cartItem = storeDB.Carts.Single(
                cart => cart.CartId == ShoppingCartId && 
                cart.RecordId == id
                );
            int itemCount = 0;
            if(cartItem != null)
            {
                if(cartItem.Count > 1)
                {
                    cartItem.Count--;
                    itemCount = cartItem.Count;
                }
                else
                {
                    storeDB.Carts.Remove(cartItem);
                }
                storeDB.SaveChanges();
            }
            return itemCount;
        }

        public void EmptyCart()
        {
            var cartItems = storeDB.Carts.Where
                (
                    cart => cart.CartId == ShoppingCartId
                );
            foreach(var cartItem in cartItems)
            {
                storeDB.Carts.Remove(cartItem);
            }
        }

        public List<Cart> GetCartItems()
        {
            return storeDB.Carts.Where
                (
                cart => cart.CartId == ShoppingCartId
                ).ToList();
        }

        public int GetCount()
        {
            // count of each item in the cart and sum them up
            int? count = (from cartItems in storeDB.Carts
                          where cartItems.CartId == ShoppingCartId
                          select (int?) cartItems.Count).Sum();

            return count?? 0;
        }

        public decimal GetTotal()
        {
            decimal? total = (from cartItems in storeDB.Carts
                              where cartItems.CartId == ShoppingCartId
                              select (int?)cartItems.Count * cartItems.Album.Price).Sum();

            return total ?? decimal.Zero;
        }

        public int CreateOrder(Order order)
        {
            decimal orderTotal = 0;
            
            var cartItems = GetCartItems();
            // add order detail for each property by iterating over the items in cart

            foreach(var item in cartItems)
            {
                var orderDetail = new OrderDetail
                {
                  AlbumId = item.AlbumId,
                  OrderId = order.OrderId,
                  UnitPrice = item.Album.Price,
                  Quantity = item.Count
                };
                // setting order nu total to shopping cart
                orderTotal += (item.Count * item.Album.Price);
                storeDB.OrderDetails.Add(orderDetail);
            }
            // order na total ne orderToal ma save karvu
            order.Total = orderTotal;

            storeDB.SaveChanges();
            EmptyCart();
            return order.OrderId;
        }

        // aiya apde HttpContextBase no use karishu cookies ne allow karva mate
        public string GetCartId(HttpContextBase context)
        {
            if (context.Session[CartSessionKey] == null)
            {
                if (!string.IsNullOrWhiteSpace(context.User.Identity.Name))
                {
                    context.Session[CartSessionKey] = context.User.Identity.Name;
                }
                else
                {
                    Guid tempCartId = Guid.NewGuid();
                    context.Session[CartSessionKey] = tempCartId.ToString();
                }
            }
            return context.Session[CartSessionKey].ToString();
        }

        // when user has logged in,  we migrate their shopping cart to be associated with their username
        public void MigrateCart(string userName)
        {
            var shoppingCart = storeDB.Carts.Where(
                c => c.CartId == ShoppingCartId);

            foreach(Cart item in shoppingCart) 
            {
                item.CartId = userName;
            }
        }
    }
}