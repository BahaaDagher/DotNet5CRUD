using DotNetCore5CRUD.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DotNetCore5CRUD.ViewModels
{
    public class MovieFormViewModel
    {
        public int Id { get; set; }
        [Required, StringLength(250)]
        public string Title { get; set; }
        [Range(1950, int.MaxValue, ErrorMessage = "Year must be from 1950 to Now")]
        public int Year { get; set; }
        [Range(1,10)]
        public double Rate { get; set; }
        [Required, StringLength(2500)]
        public string StoryLine { get; set; }
        [Display(Name = "Select Poster ....")]
        public byte[] Poster { get; set; }
        [Display(Name = "Genre")]
        [Range(1, 255, ErrorMessage = "Please select a valid genre.")]
        public byte GenreId { get; set; }
        public IEnumerable<Genre> Genres { get; set; }
    }
}
