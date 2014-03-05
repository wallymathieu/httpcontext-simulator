using System;
using NUnit.Framework;
using Http.TestLibrary;
using System.Web;
namespace HttpSimulatorTests
{
	[TestFixture()]
	public class Test
	{
		/// <summary>
		/// Determines whether this instance [can get set session].
		/// </summary>
		[Test]
		public void CanCurrentIsNotNull()
		{
			using (new HttpSimulator("/", @"c:\inetpub\").SimulateRequest())
			{
				Assert.NotNull(HttpContext.Current);
			}
		}
	}
}

