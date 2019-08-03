﻿namespace JudgeSystem.Web.Areas.Administration.Controllers
{
    using System.Threading.Tasks;

    using JudgeSystem.Common;
    using JudgeSystem.Data.Models;
    using JudgeSystem.Services.Data;
    using JudgeSystem.Services.Mapping;
    using JudgeSystem.Web.ViewModels.Problem;
    using JudgeSystem.Web.InputModels.Test;
    using JudgeSystem.Web.InputModels.Problem;

    using Microsoft.AspNetCore.Mvc;
    using System.Collections.Generic;

    public class ProblemController : AdministrationBaseController
	{
		private readonly IProblemService problemService;
		private readonly ITestService testService;
        private readonly IPracticeService practiceService;
        private readonly ILessonService lessonService;

        public ProblemController(
            IProblemService problemService, 
            ITestService testService, 
            IPracticeService practiceService,
            ILessonService lessonService)
		{
			this.problemService = problemService;
			this.testService = testService;
            this.practiceService = practiceService;
            this.lessonService = lessonService;
        }

		public IActionResult Create()
		{
			return View(new ProblemInputModel());
		}

		[HttpPost]
		public async Task<IActionResult> Create(ProblemInputModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			Problem problem = await problemService.Create(model);
			return RedirectToAction(nameof(AddTest), "Problem",
				new { area = GlobalConstants.AdministrationArea, problemId = problem.Id });
		}

		public IActionResult All(int lessonId)
		{
            IEnumerable<LessonProblemViewModel> problems = problemService.LessonProblems(lessonId);
            int practiceId = lessonService.GetPracticeId(lessonId);
            var model = new ProblemAllViewModel
            {
                Problems = problems,
                LesosnId = lessonId,
                PracticeId = practiceId
            };
			return View(model);
		}

		public async Task<IActionResult> Edit(int id)
		{
			Problem problem = await problemService.GetById(id);
			if (problem == null)
			{
				string errorMessage = string.Format(ErrorMessages.NotFoundEntityMessage, "problem");
				return this.ShowError(errorMessage, "All", "Problem", GlobalConstants.AdministrationArea);
			}

			var model = problem.To<Problem, ProblemEditInputModel>();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(ProblemEditInputModel model)
		{
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			Problem problem = await problemService.Update(model);
			return RedirectToAction(nameof(All), "Problem",
				new { area = GlobalConstants.AdministrationArea, problem.LessonId });
		}

		public async Task<IActionResult> Delete(int id)
		{
			Problem problem = await problemService.GetByIdWithTests(id);
			if (problem == null)
			{
				string errorMessage = string.Format(ErrorMessages.NotFoundEntityMessage, "problem");
				return this.ShowError(errorMessage, "All", "Problem", GlobalConstants.AdministrationArea);
			}

			var model = problem.To<Problem, ProblemViewModel>();
			return View(model);
		}

		[HttpPost]
		public async Task<IActionResult> Delete(ProblemViewModel model)
		{
			Problem problem = await problemService.GetById(model.Id);
			int lessonId = problem.LessonId;

			if (problem == null)
			{
				this.ThrowEntityNotFoundException(nameof(problem));
			}

			await problemService.Delete(problem);

            return RedirectToAction(nameof(All), "Problem",
                new { area = GlobalConstants.AdministrationArea, lessonId });
        }

		public async Task<IActionResult> AddTest(int problemId)
		{
			ViewData["lessonId"] = await problemService.GetLessonId(problemId);
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> AddTest(TestInputModel model)
		{
			if (!ModelState.IsValid)
			{
				ViewData["lessonId"] = await problemService.GetLessonId(model.ProblemId);
				return View(model);
			}

			await testService.Add(model);
			return RedirectToAction(nameof(AddTest), new { problemId = model.ProblemId });
		}
	}
}
