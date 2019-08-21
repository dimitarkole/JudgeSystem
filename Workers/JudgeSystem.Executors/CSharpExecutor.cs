﻿using System.Diagnostics;
using System.Threading.Tasks;

using JudgeSystem.Workers.Common;

namespace JudgeSystem.Executors
{
	public class CSharpExecutor
	{
		private const int ProcessMaxRunningTime = 1000;

		public async Task<ExecutionResult> ProcessExecutionResult(string dllFilePath, string input, int timeLimit, int memoryLimit)
		{
			string commandPromptArgument = @"/C dotnet " + dllFilePath;
			var executionResult = new ExecutionResult();

			using (var process = new Process())
			{
				process.StartInfo.FileName = "cmd.exe";
				process.StartInfo.Arguments = commandPromptArgument;
				process.StartInfo.RedirectStandardInput = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.UseShellExecute = false;

				process.Start();
				await process.StandardInput.WriteLineAsync(input);
				await process.StandardInput.FlushAsync();
				process.StandardInput.Close();
				executionResult.MemoryUsed = process.PrivateMemorySize64;
				bool exited = process.WaitForExit(ProcessMaxRunningTime);

				if (!exited)
				{
					if(!process.HasExited)
					{
						process.Kill();
					}
					executionResult.Type = ProcessExecutionResultType.TimeLimit;
				}

				string output = await process.StandardOutput.ReadToEndAsync();
				string error = await process.StandardError.ReadToEndAsync();

				executionResult.Error = error;
				executionResult.Output = output;
				executionResult.ExitCode = process.ExitCode;
				executionResult.TimeWorked = process.ExitTime - process.StartTime;
				executionResult.PrivilegedProcessorTime = process.PrivilegedProcessorTime;
				executionResult.UserProcessorTime = process.UserProcessorTime;

				if (executionResult.TotalProcessorTime.TotalMilliseconds > timeLimit)
				{
					executionResult.Type = ProcessExecutionResultType.TimeLimit;
				}

				if (!string.IsNullOrEmpty(executionResult.Error))
				{
					executionResult.Type = ProcessExecutionResultType.RunTimeError;
				}

				if (executionResult.MemoryUsed > memoryLimit)
				{
					executionResult.Type = ProcessExecutionResultType.MemoryLimit;
				}

				return executionResult;
			}
		}
	}
}
