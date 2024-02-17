using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MusicStoreMVC.Models
{
    //                           *************            BEFORE VALIDATION (PART 6)                  ***************
    /*public class Album
    {
        public int AlbumId { get; set; }
        public int GenreId { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string AlbumArtUrl { get; set; }
        public Genre Genre { get; set; }
        public Artist Artist { get; set; }
    }*/

    //                           *************            Modification for validaiton (PART 6)                  ***************

    public class Album
    {
        public int AlbumId { get; set; }
        
        [DisplayName("Genre")]
        public int GenreId { get; set; }

        
        [DisplayName("Artist")]
        public int ArtistId { get; set; }

        [Required(ErrorMessage ="Album Title Is Required")]
        [StringLength(160)]
        public string Title { get; set; }
        
        [Range(1.00,100.00,ErrorMessage ="Price Must be between 1.00 to 100.00")]
        public decimal Price { get; set; }
        [DisplayName("Album art URL")]
        
        [StringLength(1024)]
        public string AlbumArtUrl { get; set; }
        public virtual Genre Genre { get; set; }
        public virtual Artist Artist { get; set; }
    }
}