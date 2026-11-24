using MasterMind_Server.Exceptions;
using MasterMind_Server.Services.ErrorHelpers;
using NUnit.Framework;
using System;
using System.Data.Entity.Core;

namespace MasterMind_Server.Tests
{
    [TestFixture]
    public class DbFaultClassifierTests
    {
        [Test]
        public void TestClassifyEntityExceptionWithoutSqlErrorReturnsDataBaseError()
        {
            var exception = new EntityException("failure", new InvalidOperationException());

            ErrorMessages result = DbFaultClassifier.Classify(exception);

            Assert.That(result.ToString(), Is.EqualTo(ErrorMessages.DataBaseError.ToString()));
        }
    }
}
