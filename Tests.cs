using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Pandora
{
    /*  Run This somwhere
     *  var Report = Pandora.Tests.RunAll();
     *  Console.WriteLine(Report.ToString(true, false));
     *
     */

    public class TestResult
    {
        public string TestName;
        public bool Passed;
        public string Message;
        public Exception Error;
        public TimeSpan TestTime;
    }

    public class TestReport
    {
        public int TotalTests => TestResults.Length;
        public int Passed => TestResults.Count(t => t.Passed);
        public int Failed => TotalTests - Passed;

        public TestResult[] TestResults { get; }

        public TestReport(TestResult[] res)
        {
            TestResults = res;
        }

        public string ToString(bool Detailed, bool onlyFails)
        {
            StringBuilder buf = new StringBuilder();
            buf.AppendLine("Pandora Testing Suite");
            buf.AppendLine($"Total Tests: {TotalTests}");
            buf.AppendLine($"Passed Tests: {Passed}");
            buf.AppendLine($"Failed Tests: {Failed}");
            TimeSpan TotalTime = TimeSpan.Zero;
            for (int i = 0; i < TestResults.Length; i++)
            {
                TotalTime += TestResults[i].TestTime;
            }
            buf.AppendLine($"Total time: {TotalTime}");

            if (!Detailed) return buf.ToString();

            buf.Append("\nTest Description: \n");

            for (int i = 0; i < TestResults.Length; i++)
            {
                if(onlyFails && TestResults[i].Passed) continue;

                buf.AppendLine($"[{i+1} of {TestResults.Length}]: {TestResults[i].TestName} : {(TestResults[i].Passed ? "Passed" : "Failed")}");
                buf.AppendLine($"Time:{TestResults[i].TestTime}");
                buf.AppendLine($"Mesage:{TestResults[i].Message}\n\n");
            }
            
            return buf.ToString();
        }

        public override string ToString()
        {
            return ToString(false, false);
        }
    }

    public static class Tests
    {

        #region Tests

        private static void Test1(TestResult res)
        {
            res.TestName = "First test should pass";
        }

        private static void Test2(TestResult res)
        {
            res.TestName = "Second test that should fail";
            //Something bad 

            res.Passed = 1337 == 228;
            res.Message = "Fails as it should";
        }

        private static void Test3(TestResult res)
        {
            res.TestName = "Div by zero";

            int zero = 0;
            int a = 1 / zero;
        }
        
        #endregion



        #region RunHouseKeeping

        public static TestReport RunAll()
        {
            //Only functions that StartsWith "Test" will be runned
            var TestFuncs = typeof(Tests).GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(t => t.Name.StartsWith("Test")).ToArray();

            Action<TestResult>[] tests = new Action<TestResult>[TestFuncs.Length];
            for (int i = 0; i < TestFuncs.Length; i++)
            {
                tests[i] = (Action<TestResult>)TestFuncs[i].CreateDelegate(typeof(Action<TestResult>));
            }

            TestResult[] results = new TestResult[TestFuncs.Length];

            for (int i = 0; i < TestFuncs.Length; i++)
            {
                results[i] = RunTest(tests[i]);
            }

            return new TestReport(results);
        }
        
        private static TestResult RunTest(Action<TestResult> Test)
        {
            TestResult result = new TestResult();
            result.Passed = true;
            Stopwatch sw = Stopwatch.StartNew();

            //Test body
            try
            {
                Test(result);
                if (!result.Passed)
                {
                    result.TestTime = sw.Elapsed;
                    return result;
                }
            }
            catch (Exception e)
            {
                SetFailed(result, sw.Elapsed, e.Message, e);
                return result;
            }

            SetPassed(result, sw.Elapsed);
            return result;
        }

        private static void SetFailed(TestResult res, TimeSpan time, string message = "Test Failed", Exception error = null)
        {
            res.TestTime = time;
            res.Message = message;
            res.Error = error;
            res.Passed = false;
        }
        private static void SetPassed(TestResult res, TimeSpan time)
        {
            res.TestTime = time;
            res.Passed = true;
            res.Message = "Test Passed";
        }

        #endregion

    }
}
