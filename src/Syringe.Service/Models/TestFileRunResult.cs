using System;
using System.Collections.Generic;
using Syringe.Core.Tasks;

namespace Syringe.Service.Models
{
	public class TestFileRunResult
	{
	    public Guid? ResultId { get; set; }
		public bool Completed { get; set; }
        public bool Failed { get; set; }
        public bool HasFailedTests { get; set; }
		public TimeSpan TimeTaken { get; set; }
		public string ErrorMessage { get; set; }
		public IEnumerable<LightweightResult> TestResults { get; set; }

	    public TestFileRunResult()
		{
			TestResults = new List<LightweightResult>();
		}
	}
}