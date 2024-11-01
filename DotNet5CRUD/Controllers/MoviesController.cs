using DotNetCore5CRUD.Models;
using DotNetCore5CRUD.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DotNetCore5CRUD.Controllers
{
    public class MoviesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IToastNotification _toastNotification; 
        public MoviesController (ApplicationDbContext context , IToastNotification toastNotification)
        {
            _context = context;
            _toastNotification = toastNotification;
        }
        public async Task<IActionResult> Index()
        {
            var movies = await _context.Movies.OrderByDescending(b=>b.Rate).ToListAsync(); 
            return View(movies);
        }
        public async Task<IActionResult> Create()
        {
            MovieFormViewModel viewModel = new MovieFormViewModel() { 
                 Genres = await _context.Genres.OrderBy(o=>o.Name).ToListAsync()
            };
            return View("MovieForm", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MovieFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Genres = _context.Genres.OrderBy(b => b.Name).ToList();
                return View("MovieForm", model);
            }
            var CurrentYear = DateTime.Now.Year;
            if (!(model.Year >= 1950 && model.Year <= CurrentYear))
            {
                ModelState.AddModelError("Year", $"Year mest bet between 1950 and {CurrentYear} ");
                model.Genres = _context.Genres.OrderBy(b => b.Name).ToList();
                return View("MovieForm", model);
            }
            var files = Request.Form.Files; 
            if (!files.Any()) 
            {
                ModelState.AddModelError("poster", "please select a poster");
                model.Genres = _context.Genres.OrderBy(b => b.Name).ToList();
                return View("MovieForm", model); 
            }
            var poster = files.FirstOrDefault();
            var allowedExtentions = new List<string>{".jpg" , ".png" }; 
            if (!allowedExtentions.Contains(Path.GetExtension(poster.FileName).ToLower()))
            {
                ModelState.AddModelError("poster", "Only PNG , JPG are allowed");
                model.Genres = _context.Genres.OrderBy(b => b.Name).ToList();
                return View("MovieForm", model);
            }
            // if the size of image > 1 MB
            if (poster.Length> 1048576)
            {
                ModelState.AddModelError("poster", "poster cannot be more than 1 MB!");
                model.Genres = _context.Genres.OrderBy(b => b.Name).ToList();
                return View("MovieForm", model);
            }
            using var dataStream =  new MemoryStream();
            await poster.CopyToAsync(dataStream);
            Movie movie = new Movie
            {
                Title = model.Title,
                Year = model.Year,
                Rate = model.Rate,
                GenreId = model.GenreId,
                StoryLine = model.StoryLine,
                Poster = dataStream.ToArray(),
            };
            _context.Movies.Add(movie);
            _context.SaveChanges();

            _toastNotification.AddSuccessToastMessage("Movie Created Successfully"); 
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();

            var movieVm = new MovieFormViewModel()
            {
                Genres = _context.Genres.OrderBy(b => b.Name).ToList(),
                Id = movie.Id,
                Title = movie.Title,
                Year = movie.Year,
                Rate = movie.Rate,
                GenreId = movie.GenreId,
                StoryLine = movie.StoryLine,
                Poster = movie.Poster,
            };

            return View("MovieForm", movieVm); 
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(MovieFormViewModel modelVm)
        {
            if (!ModelState.IsValid)
            {
                modelVm.Genres = await _context.Genres.OrderBy(b => b.Name).ToListAsync();
                return View("MovieForm" ,modelVm);
            }
            var movie = await _context.Movies.FindAsync(modelVm.Id); 
            if (movie == null)
            {
                return NotFound();
            }
            movie.Title = modelVm.Title;
            movie.Year = modelVm.Year;
            movie.Rate = modelVm.Rate;
            movie.GenreId = modelVm.GenreId;
            movie.StoryLine = modelVm.StoryLine;

            var files = Request.Form.Files;
            if (files.Any())
            {
                var poster = files.FirstOrDefault();
                using var dataStream = new MemoryStream();
                await poster.CopyToAsync(dataStream);
                modelVm.Poster = dataStream.ToArray(); 

                var allowedExtentions = new List<string> { ".jpg", ".png" };
                if (!allowedExtentions.Contains(Path.GetExtension(poster.FileName).ToLower()))
                {
                    ModelState.AddModelError("poster", "Only PNG , JPG are allowed");
                    modelVm.Genres = _context.Genres.OrderBy(b => b.Name).ToList();
                    return View("MovieForm", modelVm);
                }
                // if the size of image > 1 MB
                if (poster.Length > 1048576)
                {
                    ModelState.AddModelError("poster", "poster cannot be more than 1 MB!");
                    modelVm.Genres = _context.Genres.OrderBy(b => b.Name).ToList();
                    _toastNotification.AddErrorToastMessage("Movie Updated faild ");
                    return View("MovieForm", modelVm);
                }
                movie.Poster = modelVm.Poster;
            }

            _context.SaveChanges();
            _toastNotification.AddSuccessToastMessage("Movie Updated Successfully");
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return BadRequest();
            var movie = await _context.Movies.Include(i => i.Genre).SingleAsync(i=>i.Id ==id);
            if (movie ==null)
                return NotFound();
            return View(movie); 
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id==null)
                return BadRequest();

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();
             _context.Movies.Remove(movie);
             _context.SaveChanges();
            _toastNotification.AddSuccessToastMessage("Movie Deleted Successfully");
            return Ok();
        }

    }
}
