﻿using System.Collections.Generic;
using System.Threading.Tasks;

using JudgeSystem.Common;
using JudgeSystem.Data.Models.Enums;
using JudgeSystem.Services.Data;
using JudgeSystem.Web.Filters;
using JudgeSystem.Web.InputModels.Contest;
using JudgeSystem.Web.Utilites;
using JudgeSystem.Web.ViewModels.Contest;

using Microsoft.AspNetCore.Mvc;

namespace JudgeSystem.Web.Areas.Administration.Controllers
{
    public class ContestController : AdministrationBaseController
	{
		private const int DefaultPage = 1;

		private readonly IContestService contestService;
		private readonly ICourseService courseService;
		private readonly ILessonService lessonService;
        private readonly ContestReslutsHelper contestReslutsHelper;

        public ContestController(
            IContestService contestService, 
            ICourseService courseService, 
            ILessonService lessonService,
            ContestReslutsHelper contestReslutsHelper)
		{
			this.contestService = contestService;
			this.courseService = courseService;
			this.lessonService = lessonService;
            this.contestReslutsHelper = contestReslutsHelper;
        }

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
        [ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ContestCreateInputModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			await contestService.Create(model);

			return Redirect("/");
		}

        [EndpointExceptionFilter]
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

        [ValidateAntiForgeryToken]
		[HttpPost]
		public async Task<IActionResult> Edit(ContestEditInputModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			await contestService.UpdateContest(model);

			return RedirectToAction(nameof(ActiveContests));
		}

		public async Task<IActionResult> Delete(int id)
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

		public IActionResult All(int page = DefaultPage)
		{
			int numberOfPages = contestService.GetNumberOfPages();
			IEnumerable<ContestViewModel> contests = contestService.GetAllConests(page);
			ContestAllViewModel model = new ContestAllViewModel { Contests = contests, NumberOfPages = numberOfPages, CurrentPage = page };
			return View(model);
		}

		public IActionResult Results(int id, int? page)
		{
			ViewData["numberOfPages"] = contestService.GetContestResultsPagesCount(id);
			if (page.HasValue)
			{
				ViewData["currentPage"] = page;
				var contestResults = contestService.GetContestReults(id, page.Value);
				return PartialView(contestResults);
			}
			else
			{
				ViewData["currentPage"] = DefaultPage;
				var contestResults = contestService.GetContestReults(id, DefaultPage);
				return View(contestResults);
			}
		}

        [EndpointExceptionFilter]
		[HttpGet("/Contest/Results/{contestId}/PagesCount")]
		public int GetContestResultPagesCount(int contestId)
		{
			return contestService.GetContestResultsPagesCount(contestId);
		}

        public IActionResult Submissions(string userId, int contestId, int? problemId, int page = DefaultPage )
        {
            string baseUrl = $"/{GlobalConstants.AdministrationArea}/Contest/Submissions?contestId={contestId}&userId={userId}";

            var model = contestReslutsHelper.GetContestSubmissions(contestId, userId, problemId, page, baseUrl);

            return View(model);
        }
	}
}
