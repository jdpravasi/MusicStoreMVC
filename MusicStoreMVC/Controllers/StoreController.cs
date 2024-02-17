using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using MusicStoreMVC.Models;

namespace MusicStoreMVC.Controllers
{
    public class StoreController : Controller
    {
        MusicStoreEntities storeDB = new MusicStoreEntities();

        // GET: Store
        public ActionResult Index()
        {
            var geners = storeDB.Genres.ToList();
            return View(geners);
        }

        //GET: /Store/Browse
        public ActionResult Browse(string genre)
        {
            var genreModel = storeDB.Genres.Include("Albums").Single(g => g.Name == genre);
            return View(genreModel);
            /*var genreModel = new Genre { Name = genre };*/
            /*var genreModel = new Genre { Name = genre };*/

        }

        // GET: /Store/Details
        public ActionResult Details(int id)
        {
            var album = storeDB.Albums.Find(id);
            return View(album);
        }
    }
}

