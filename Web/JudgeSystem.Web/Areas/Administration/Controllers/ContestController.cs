﻿using JudgeSystem.Data.Models;
using JudgeSystem.Data.Models.Enums;
using JudgeSystem.Services.Data;
using JudgeSystem.Services.Mapping;
using JudgeSystem.Web.InputModels.Contest;
using JudgeSystem.Web.Utilites;
using JudgeSystem.Web.ViewModels.Contest;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JudgeSystem.Web.Areas.Administration.Controllers
{
	public class ContestController : AdministrationBaseController
	{
		private const int DefaultPage = 1;
		private readonly IContestService contestService;
		private readonly ICourseService courseService;
		private readonly ILessonService lessonService;

		public ContestController(IContestService contestService, ICourseService courseService, ILessonService lessonService)
		{
			this.contestService = contestService;
			this.courseService = courseService;
			this.lessonService = lessonService;
		}

		public IActionResult Create()
		{
			var courses = courseService.GetAllCourses().Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
			ViewData["courses"] = courses;
			ViewData["lessonTypes"] = Utility.GetSelectListItems<LessonType>();
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Create(ContestCreateInputModel model)
		{
			if(!ModelState.IsValid)
			{
				var courses = courseService.GetAllCourses().Select(c => new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
				ViewData["courses"] = courses;
				ViewData["lessonTypes"] = Utility.GetSelectListItems<LessonType>();
				return View(model);
			}

			Contest contest = model.To<ContestCreateInputModel, Contest>();
			await contestService.Create(contest);
			return Redirect("/");
		}

		public IActionResult GetLessons(int courseId, LessonType lessonType)
		{
			var lessons = lessonService.GetCourseLesosns(courseId, lessonType);
			return Json(lessons);
		}

		public IActionResult ActiveContests()
		{
			IEnumerable<ContestBreifInfoViewModel> activeContests = contestService.GetActiveAndFollowingContests();
			return View(activeContests);
		}

		public async Task<IActionResult> Details(int id)
		{
			ContestViewModel contest = await contestService.GetById<ContestViewModel>(id);
			return View(contest);
		}

		public async Task<IActionResult> Edit(int id)
		{
			ContestEditInputModel contest = await contestService.GetById<ContestEditInputModel>(id);
			return View(contest);
		}
																																																											
		[HttpPost]	
		public async Task<IActionResult> Edit(ContestEditInputModel model)
		{
			if(!ModelState.IsValid)
			{
				return View(model);
			}

			await contestService.UpdateContest(model);
			return RedirectToAction(nameof(ActiveContests));
		}

		public async Task<IActionResult> Delete (int id)
		{
			ContestViewModel contest = await contestService.GetById<ContestViewModel>(id);
			return View(contest);
		}

		[HttpPost]
		[ActionName(nameof(Delete))]
		public async Task<IActionResult> DeletePost(int id)
		{
			await contestService.DeleteContestById(id);
			return RedirectToAction(nameof(ActiveContests));
		}

		public IActionResult All(int? page)
		{
			int numberOfPages = contestService.GetNumberOfPages();
			if (!page.HasValue)
			{
				IEnumerable<ContestViewModel> contests = contestService.GetAllConests(DefaultPage);
				ContestAllViewModel model = new ContestAllViewModel { Contests = contests, NumberOfPages = numberOfPages, CurrentPage = DefaultPage };
				return View(model);
			}
			else
			{
				IEnumerable<ContestViewModel> contests = contestService.GetAllConests(page.Value);
				ContestAllViewModel model = new ContestAllViewModel { Contests = contests, NumberOfPages = numberOfPages, CurrentPage = page.Value };
				return PartialView(model);
			}
		}
	}
}
