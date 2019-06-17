﻿namespace JudgeSystem.Data.Models
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;

	using JudgeSystem.Data.Common.Models;

	public class Submission : BaseModel<int>
	{
		public Submission()
		{
			this.SubmisionDate = DateTime.Now;
			this.ExecutedTests = new HashSet<ExecutedTest>();
		}

		[Required]
		public byte[] Code { get; set; }

		public byte[] CompilationErrors { get; set; }

		public int ProblemId { get; set; }
		public Problem Problem { get; set; }

		public int? ContestId { get; set; }
		public Contest Contest { get; set; }

		public DateTime SubmisionDate { get; set; }

		public string UserId { get; set; }
		public ApplicationUser User { get; set; }

		public ICollection<ExecutedTest> ExecutedTests { get; set; }
	}
}
