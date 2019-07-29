﻿using JudgeSystem.Common;
using JudgeSystem.Data.Common.Repositories;
using JudgeSystem.Data.Models;
using JudgeSystem.Data.Repositories;
using JudgeSystem.Web.Infrastructure.Exceptions;
using JudgeSystem.Web.InputModels.Problem;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace JudgeSystem.Services.Data.Tests
{
    public class ProblemServiceTests : TransientDbContextProvider
    {
        [Fact]
        public async Task Create_WithValidData_ShouldAddNewProblemToDatabase()
        {
            var service = await CreateProblemService(new List<Problem>());
            var model = new ProblemInputModel
            {
                IsExtraTask = false,
                LessonId = 5,
                MaxPoints = 100,
                Name = "test"
            };

            var actualProblem = await service.Create(model);

            Assert.True(this.context.Problems.Any(x => x.Name == "test"));
            Assert.Equal(5, actualProblem.LessonId);
            Assert.Equal(100, actualProblem.MaxPoints);
            Assert.False(actualProblem.IsExtraTask);
            Assert.Equal("test", actualProblem.Name);
            Assert.True(actualProblem.Id > 0);
        }

        [Fact]
        public async Task Delete_WithValidData_ShouldWorkCorrect()
        {
            var testData = GetTestData();
            var problemService = await CreateProblemService(testData);

            var problem = testData.First(x => x.Name == "test2");
            await problemService.Delete(problem);

            Assert.False(this.context.Lessons.Any(x => x.Name == problem.Name));
        }

        [Fact]
        public async Task Delete_WithNonExistingProblem_ShouldThrowError()
        {
            var problemService = await CreateProblemService(new List<Problem>()); 

            await Assert.ThrowsAsync<EntityNotFoundException>(() => problemService.Delete(new Problem { Id = 161651 }));
        }

        [Fact]
        public async Task GetById_WithValidId_ShouldReturnCorrectData()
        {
            var testData = GetTestData();
            var service = await CreateProblemService(testData);

            int id = 2;
            var actualData = await service.GetById(id);
            var expectedData = testData[id - 1];

            Assert.Equal(actualData.Name, expectedData.Name);
            Assert.Equal(actualData.MaxPoints, expectedData.MaxPoints);
            Assert.Equal(actualData.IsExtraTask, expectedData.IsExtraTask);
            Assert.Equal(actualData.LessonId, expectedData.LessonId);
        }

        [Fact]
        public async Task GetById_WithInValidId_ShouldReturnNull()
        {
            var testData = GetTestData();
            var service = await CreateProblemService(testData);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetById(165));
        }

        [Fact]
        public async Task GetByIdWithTests_WithValidId_ShouldReturnCorrectData()
        {
            var testData = GetTestData();
            var service = await CreateProblemService(testData);
            var problem = new Problem
            {
                Id = 999,
                Name = "include all tests",
                Tests = new List<Test>
                {
                    new Test { Id = 1 },
                    new Test { Id = 2 },
                    new Test { Id = 3 },
                    new Test { Id = 4 },
                    new Test { Id = 5 },
                }
            };
            await context.Problems.AddAsync(problem);
            await context.SaveChangesAsync();

            var actualData = await service.GetByIdWithTests(999);

            Assert.Equal(problem.Name, actualData.Name);
            Assert.Equal(problem.MaxPoints, actualData.MaxPoints);
            Assert.Equal(problem.Tests.Count, actualData.Tests.Count);
            Assert.Equal(problem.Tests.Select(t => t.Id), actualData.Tests.Select(t => t.Id));
        }

        [Fact]
        public async Task GetByIdWithTests_WithInValidId_ShouldReturnNull()
        {
            var testData = GetTestData();
            var service = await CreateProblemService(testData);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetByIdWithTests(165));
        }

        [Theory]
        [InlineData(2, "test2, test4")]
        [InlineData(1, "test1")]
        [InlineData(45, "")]
        public void LessonProblems_WithDifferentData_ShouldWorkCorrect(int lessonId, string expectedNames)
        {
            var service = CreateProblemServiceWithMockedRepository(GetTestData().AsQueryable());

            var actualData = service.LessonProblems(lessonId);

            Assert.Equal(expectedNames, string.Join(", ", actualData.Select(x => x.Name)));
        }

        [Theory]
        [InlineData("C#", "c# WEb API, c# web - asp.NET CoRe")]
        [InlineData("web", "c# WEb API, c# web - asp.NET CoRe")]
        [InlineData("test", "test1, test2, test3, test4")]
        [InlineData("webasp", "")]
        [InlineData("asp", "c# web - asp.NET CoRe")]
        [InlineData("es", "test1, test2, test3, test4")]
        [InlineData("csharp", "")]
        [InlineData("1", "test1")]
        public async Task SearchByName_WithDifferentInputs_ShouldWorkCorrect(string keyword, string expectedResult)
        {
            var lesson = new Lesson() { Id = 1, Name = "lesson1" };
            var data = GetTestData();
            foreach (var problem in data)
            {
                problem.Lesson = lesson;
            }
            var service = await CreateProblemService(data);
            await context.Problems.AddRangeAsync(new List<Problem>
            {
                new Problem { Id = 5, Name = "programming basics", Lesson = lesson },
                new Problem { Id = 6, Name = "c# WEb API", Lesson = lesson },
                new Problem { Id = 7, Name = "c# web - asp.NET CoRe", Lesson = lesson },
            });
            await context.SaveChangesAsync();

            var actualResult = service.SearchByName(keyword);

            Assert.Equal(expectedResult, string.Join(", ", actualResult.Select(x => x.Name)));
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task SearchByName_WithDifferentInvalidInput_ShouldThrowArgumentException(string keyword)
        {
            var service = await CreateProblemService(GetTestData());
            await context.SaveChangesAsync();

            var exception = Assert.Throws<ArgumentException>(() => service.SearchByName(keyword));
            Assert.Equal(exception.Message, ErrorMessages.InvalidSearchKeyword);
        }

        [Fact]
        public async Task Update_WithValidData_ShouldWorkCorrect()
        {
            var testData = GetTestData();
            var service = await CreateProblemService(testData);
            var inputModel = new ProblemEditInputModel
            {
                Id = 3,
                IsExtraTask = true,
                MaxPoints = 220,
                Name = "edited"
            };

            await service.Update(inputModel);
            var actualProblem = context.Problems.First(x => x.Id == inputModel.Id);

            Assert.Equal("edited", actualProblem.Name);
            Assert.True(actualProblem.IsExtraTask);
            Assert.Equal(220, actualProblem.MaxPoints);
            Assert.Equal("edited", actualProblem.Name);
        }

        [Fact]
        public async Task Update_WithNonExistingLesson_ShouldThrowEntityNotFoundException()
        {
            var inputModel = new ProblemEditInputModel
            {
                Id = 45,
                IsExtraTask = true,
                MaxPoints = 220,
                Name = "edited"
            };
            var service = await CreateProblemService(GetTestData());

            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.Update(inputModel));
        }

        [Fact]
        public async Task GetLessonId_WithValidProblemId_ShouldReturnCorrectData()
        {
            var service = await CreateProblemService(GetTestData());

            int actualResult = await service.GetLessonId(4);

            Assert.Equal(2, actualResult);
        }

        [Fact]
        public async Task GetLessonId_WithInValidId_ShouldReturnThrowEntityNotFoundException()
        {
            var testData = GetTestData();
            var service = await CreateProblemService(testData);

            await Assert.ThrowsAsync<EntityNotFoundException>(() => service.GetLessonId(161651));
        }

        [Fact]
        public async Task GetProblemMaxPoints_WithValidId_ShouldReturnThrowEntityNotFoundException()
        {
            var testData = GetTestData();
            var service = await CreateProblemService(testData);

            int actualResult = service.GetProblemMaxPoints(3);

            Assert.Equal(100, actualResult);
        }

        [Fact]
        public async Task GetProblemMaxPoints_WithInValidId_ShouldReturnThrowEntityNotFoundException()
        {
            var testData = GetTestData();
            var service = await CreateProblemService(testData);

            Assert.Throws<EntityNotFoundException>(() => service.GetProblemMaxPoints(161651));
        }

        private async Task<ProblemService> CreateProblemService(List<Problem> testData)
        {
            await this.context.Problems.AddRangeAsync(testData);
            await this.context.SaveChangesAsync();
            IDeletableEntityRepository<Problem> repository = new EfDeletableEntityRepository<Problem>(this.context);
            var service = new ProblemService(repository);
            return service;
        }

        private ProblemService CreateProblemServiceWithMockedRepository(IQueryable<Problem> testData)
        {
            var reposotiryMock = new Mock<IDeletableEntityRepository<Problem>>();
            reposotiryMock.Setup(x => x.All()).Returns(testData);
            return new ProblemService(reposotiryMock.Object);
        }

        private List<Problem> GetTestData()
        {
            return new List<Problem>
            {
                new Problem { Id = 1, Name = "test1", LessonId = 1, IsExtraTask = false, MaxPoints = 100 },
                new Problem { Id = 2, Name = "test2", LessonId = 2, IsExtraTask = false, MaxPoints = 50 },
                new Problem { Id = 3, Name = "test3", LessonId = 3, IsExtraTask = true, MaxPoints = 100 },
                new Problem { Id = 4, Name = "test4", LessonId = 2, IsExtraTask = false, MaxPoints = 200 },
            };
        }
    }
}