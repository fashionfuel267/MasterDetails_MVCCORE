﻿using CodeFirstMasterDetails.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;
using System.Linq;

namespace CodeFirstMasterDetails.Controllers
{
	public class ApplicantController : Controller
	{
		private readonly ApplicantContext db;
		IWebHostEnvironment environment;
		public ApplicantController(ApplicantContext context, IWebHostEnvironment host)
		{
			db = context;
			environment = host;
		}
		[HttpGet]
		public IActionResult Index()
		{
			var data = db.Applicants.Include(m => m.Exprience).ToList();
			return View(data);
		}
		[HttpGet]
		public IActionResult Create()
		{
			Applicant NewApplicant = new Applicant();
			NewApplicant.Exprience.Add(new ApplicantExprience
			{
				Company = "",
				Designation = "",
				YearOfExp = 0
			});

			return View(NewApplicant);
		}
		[HttpPost]
		public IActionResult Create(Applicant applicant, string btn)
		{
			if (btn == "Add")
			{
				applicant.Exprience.Add(new ApplicantExprience());
			}

			if (btn == "Create")
			{

				if (applicant.Picture != null)
				{
					string ext = Path.GetExtension(applicant.Picture.FileName);
					if (ext == ".jpg" || ext == ".png")
					{
						applicant.TotalExp = applicant.Exprience.Sum(m => m.YearOfExp);

						if (applicant.Picture != null)
						{
							// var ext = Path.GetExtension(faculty.Picture.FileName);
							var rootPath = this.environment.ContentRootPath;
							var fileToSave = Path.Combine(rootPath, "wwwroot/Pictures", applicant.Picture.FileName);
							using (var fileStream = new FileStream(fileToSave, FileMode.Create))
							{
								applicant.Picture.CopyToAsync(fileStream).Wait();
							}
							applicant.PicPath = "~/Pictures/" + applicant.Picture.FileName;
							db.Applicants.Add(applicant);
							if (db.SaveChanges() > 0)
							{
								return RedirectToAction("Index");
							}
						}
						else
						{
							ModelState.AddModelError("", "Please Provide Profile Picture");
							return View(applicant);
						}
					} //if ext jpg

				} //if-pic	
			}
            return View(applicant);
        }
		[HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
			Applicant EditApplicant = db.Applicants.Include(m => m.Exprience).Where(m => m.Id.Equals(id)).FirstOrDefault();

            return View(EditApplicant);
        }

		public JsonResult AddRow(Applicant ViewModel)
		{
            ViewModel.Exprience.Add(new ApplicantExprience());

            return Json(ViewModel);
		}
		[HttpPost]
		public async Task<IActionResult> Edit(Applicant ViewModel, string btn)
		{

            //var applicant = await db.Applicants.FindAsync(ViewModel.Id);

			if (btn == "Add")
			{
				ViewModel.Exprience.Add(new ApplicantExprience());
			}

			if (btn == "Edit")
			{
                var applicant = db.Applicants.Include(a => a.Exprience).Where(e => e.Id.Equals(ViewModel.Id)).FirstOrDefault();
               
                
                
                if (applicant != null)
				{
                    var existingId = new HashSet<int>(applicant.Exprience.Select(e => e.Id));
                    var selectedId = new HashSet<int>(ViewModel.Exprience.Select(s => s.Id));
                    foreach (var e in db.Expriences)
                    {
                        if (selectedId.Contains(e.Id))
                        {
                            if (!existingId.Contains(e.Id))
                            {
                                applicant.Exprience.Add(ViewModel.Exprience.Where(e => e.Id.Equals(e.Id)).FirstOrDefault());
                            }
                        }
                        else
                        {
                            if (existingId.Contains(e.Id))
                            {
                                var exToRemove = db.Expriences.Where(i => i.Id == e.Id).FirstOrDefault();
                                db.Remove(exToRemove);
                            }
                        }
                    }
                    applicant.Name = ViewModel.Name;
					applicant.Birthday = ViewModel.Birthday;
					applicant.TotalExp = ViewModel.TotalExp;
					applicant.Picture = ViewModel.Picture;
					applicant.IsAvilable = ViewModel.IsAvilable;
					applicant.Exprience = ViewModel.Exprience;

					await db.SaveChangesAsync();
				}
				return RedirectToAction("index");
			}
			return View(ViewModel);
		}
    




        public IActionResult Delete(int id)
		{
			if (id != null)
			{
				var user = db.Applicants.Find(id);
				db.Applicants.Remove(user);
				db.SaveChanges();
			}
            return RedirectToAction("Index");
        }
    }
}

