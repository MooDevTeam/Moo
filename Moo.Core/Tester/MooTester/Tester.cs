using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Text.RegularExpressions;
using Moo.Core.Tester;
using Moo.Core.DB;
using Moo.Core.Utility;
namespace Moo.Core.Tester.MooTester
{
    public class Tester : ITester
    {
        delegate TestResult SocketToTestResult(Socket socket);

        public TestResult TestTranditional(string source, string language, IEnumerable<TranditionalTestCase> cases)
        {
            return WithSocket(socket => InnerTestTranditional(socket, source, language, cases));
        }

        TestResult InnerTestTranditional(Socket socket, string source, string language, IEnumerable<TranditionalTestCase> cases)
        {
            string execFile = Compile(socket, source, Command.GetCommand(language, "src2exe"));
            int score = 0;
            StringBuilder sb = new StringBuilder(Properties.Resources.MooTester_CompilerSuccess).AppendLine().AppendLine();
            foreach (TranditionalTestCase testCase in cases)
            {
                sb.AppendFormat(Properties.Resources.MooTester_TestCaseX, testCase.ID);

                socket.Send(new Message()
                {
                    Type = Message.MessageType.Test,
                    Content = new TestIn()
                    {
                        CmpPath = Properties.Resources.MooTester_TranditionalJudger,
                        ExecPath = Command.GetCommand(language,"execute").Replace("{Execute}",execFile),
                        Memory = testCase.MemoryLimit,
                        Time = testCase.TimeLimit,
                        Input = testCase.Input,
                        Output = testCase.Answer
                    }
                }.ToBytes());
                Out testResult = new Out(socket);

                int currentScore=0;
                switch (testResult.Type)
                {
                    case Out.ResultType.Success:
                        currentScore = testCase.Score;
                        sb.Append(Properties.Resources.MooTester_TestSuccess);
                        break;
                    case Out.ResultType.WrongAnswer:
                        sb.Append(Properties.Resources.MooTester_TestWA);
                        break;
                    case Out.ResultType.TimeLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestTLE);
                        break;
                    case Out.ResultType.RuntimeError:
                        sb.Append(Properties.Resources.MooTester_TestRE);
                        break;
                    case Out.ResultType.MemoryLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestMLE);
                        break;
                    case Out.ResultType.CompareError:
                        sb.Append(Properties.Resources.MooTester_TestCompareError);
                        break;
                    case Out.ResultType.OutputLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestOLE);
                        break;
                    default:
                        sb.Append(Properties.Resources.MooTester_TestUndefinedError);
                        break;
                }
                score+=currentScore;
                sb.AppendLine(string.Format(Properties.Resources.MooTester_TestInfo, currentScore, testResult.Time, testResult.Memory, testResult.Message.Replace('\r', ' ').Replace('\n', ' ')));
            }

            return new TestResult()
            {
                Score = score,
                Info = sb.ToString()
            };
        }

        public TestResult TestSpecialJudged(string source, string language, IEnumerable<SpecialJudgedTestCase> cases)
        {
            return WithSocket(socket => InnerTestSpecialJudged(socket, source, language, cases));
        }

        TestResult InnerTestSpecialJudged(Socket socket, string source, string language, IEnumerable<SpecialJudgedTestCase> cases)
        {
            string execFile = Compile(socket, source, Command.GetCommand(language, "src2exe"));
            int score = 0;
            StringBuilder sb = new StringBuilder(Properties.Resources.MooTester_CompilerSuccess).AppendLine().AppendLine();
            foreach (SpecialJudgedTestCase testCase in cases)
            {
                sb.AppendFormat(Properties.Resources.MooTester_TestCaseX, testCase.ID);

                socket.Send(new Message()
                {
                    Type = Message.MessageType.Test,
                    Content = new TestIn()
                    {
                        CmpPath = testCase.Judger.Path,
                        ExecPath = Command.GetCommand(language,"execute").Replace("{Execute}",execFile),
                        Memory = testCase.MemoryLimit,
                        Time = testCase.TimeLimit,
                        Input = testCase.Input,
                        Output = testCase.Answer
                    }
                }.ToBytes());
                Out testResult = new Out(socket);

                int currentScore=0;
                switch (testResult.Type)
                {
                    case Out.ResultType.Success:
                        currentScore = GetScore(ref testResult.Message);
                        sb.Append(Properties.Resources.MooTester_TestSuccess);
                        break;
                    case Out.ResultType.WrongAnswer:
                        currentScore = GetScore(ref testResult.Message);
                        sb.Append(Properties.Resources.MooTester_TestWA);
                        break;
                    case Out.ResultType.TimeLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestTLE);
                        break;
                    case Out.ResultType.RuntimeError:
                        sb.Append(Properties.Resources.MooTester_TestRE);
                        break;
                    case Out.ResultType.MemoryLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestMLE);
                        break;
                    case Out.ResultType.CompareError:
                        sb.Append(Properties.Resources.MooTester_TestCompareError);
                        break;
                    case Out.ResultType.OutputLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestOLE);
                        break;
                    default:
                        sb.Append(Properties.Resources.MooTester_TestUndefinedError);
                        break;
                }
                score += currentScore;
                sb.AppendLine(string.Format(Properties.Resources.MooTester_TestInfo, currentScore, testResult.Time, testResult.Memory, testResult.Message.Replace('\r', ' ').Replace('\n', ' ')));
            }

            return new TestResult()
            {
                Score = score,
                Info = sb.ToString()
            };
        }

        public TestResult TestInteractive(string source, string language, IEnumerable<InteractiveTestCase> cases)
        {
            return WithSocket(socket => InnerTestInteractive(socket, source, language, cases));
        }

        public TestResult InnerTestInteractive(Socket socket, string source, string language, IEnumerable<InteractiveTestCase> cases)
        {
            string objectFile = Compile(socket, source, Command.GetCommand(language, "src2obj"));
            int score = 0;
            StringBuilder sb = new StringBuilder(Properties.Resources.MooTester_CompilerSuccess).AppendLine().AppendLine();
            foreach (InteractiveTestCase testCase in cases)
            {
                sb.AppendFormat(Properties.Resources.MooTester_TestCaseX, testCase.ID);

                string cmd = Command.GetCommand(language, "obj2exe");
                string objects = objectFile + " \"" + testCase.Invoker.Path + "\"";
                cmd = cmd.Replace("{Object}", objects);

                string execFile = Compile(socket, "", cmd);
                socket.Send(new Message()
                {
                    Type = Message.MessageType.Test,
                    Content = new TestIn()
                    {
                        Time = testCase.TimeLimit,
                        Memory = testCase.MemoryLimit,
                        CmpPath = "",
                        ExecPath = Command.GetCommand(language,"execute").Replace("{Execute}",execFile),
                        Input = testCase.TestData,
                        Output = new byte[0]
                    }
                }.ToBytes());

                Out testResult = new Out(socket);
                int currentScore=0;
                switch (testResult.Type)
                {
                    case Out.ResultType.Success:
                        currentScore = GetScore(ref testResult.Message);
                        sb.Append(Properties.Resources.MooTester_TestSuccess);
                        break;
                    case Out.ResultType.WrongAnswer:
                        currentScore = GetScore(ref testResult.Message);
                        sb.Append(Properties.Resources.MooTester_TestWA);
                        break;
                    case Out.ResultType.TimeLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestTLE);
                        break;
                    case Out.ResultType.RuntimeError:
                        sb.Append(Properties.Resources.MooTester_TestRE);
                        break;
                    case Out.ResultType.MemoryLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestMLE);
                        break;
                    case Out.ResultType.OutputLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestOLE);
                        break;
                    default:
                        sb.Append(Properties.Resources.MooTester_TestUndefinedError);
                        break;
                }
                score += currentScore;
                sb.AppendLine(string.Format(Properties.Resources.MooTester_TestInfo, currentScore, testResult.Time, testResult.Memory, testResult.Message.Replace('\r', ' ').Replace('\n', ' ')));
            }

            return new TestResult()
            {
                Score = score,
                Info = sb.ToString()
            };
        }

        public TestResult TestAnswerOnly(IDictionary<int, string> answers, IEnumerable<AnswerOnlyTestCase> cases)
        {
            return WithSocket(socket => InnerTestAnswerOnly(socket, answers, cases));
        }

        TestResult InnerTestAnswerOnly(Socket socket, IDictionary<int, string> answers, IEnumerable<AnswerOnlyTestCase> cases)
        {
            int score = 0;
            StringBuilder sb = new StringBuilder();
            foreach (AnswerOnlyTestCase testCase in cases)
            {
                sb.AppendFormat(Properties.Resources.MooTester_TestCaseX, testCase.ID);

                socket.Send(new Message()
                {
                    Type = Message.MessageType.Test,
                    Content = new TestIn()
                    {
                        CmpPath = "",
                        ExecPath = testCase.Judger.Path,
                        Input = MergeAnswerAndTestData(answers.ContainsKey(testCase.ID) ? answers[testCase.ID] : "", testCase.TestData),
                        Memory = long.Parse(Properties.Resources.MooTester_TestAnswerOnlyMemory),
                        Time = long.Parse(Properties.Resources.MooTester_TestAnswerOnlyTime),
                        Output = new byte[0]
                    }
                }.ToBytes());

                Out testResult = new Out(socket);
                int currentScore=0;
                switch (testResult.Type)
                {
                    case Out.ResultType.Success:
                        currentScore = GetScore(ref testResult.Message);
                        sb.Append(Properties.Resources.MooTester_TestSuccess);
                        break;
                    case Out.ResultType.WrongAnswer:
                        currentScore = GetScore(ref testResult.Message);
                        sb.Append(Properties.Resources.MooTester_TestWA);
                        break;
                    case Out.ResultType.TimeLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestTLE);
                        break;
                    case Out.ResultType.RuntimeError:
                        sb.Append(Properties.Resources.MooTester_TestRE);
                        break;
                    case Out.ResultType.MemoryLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestMLE);
                        break;
                    case Out.ResultType.OutputLimitExceeded:
                        sb.Append(Properties.Resources.MooTester_TestOLE);
                        break;
                    default:
                        sb.Append(Properties.Resources.MooTester_TestUndefinedError);
                        break;
                }
                score += currentScore;
                sb.AppendLine(string.Format(Properties.Resources.MooTester_TestInfo, currentScore, testResult.Time, testResult.Memory, testResult.Message.Replace('\r', ' ').Replace('\n', ' ')));
            }
            return new TestResult()
            {
                Score = score,
                Info = sb.ToString()
            };
        }

        byte[] MergeAnswerAndTestData(string answer, byte[] testData)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(mem))
                {
                    byte[] answerBytes = Encoding.Default.GetBytes(answer);
                    writer.Write((uint)answerBytes.LongLength);
                    writer.Write(answerBytes);
                    writer.Write(testData);
                }
                return mem.ToArray();
            }
        }

        TestResult WithSocket(SocketToTestResult func)
        {
            int failureCount = 0;
            while (failureCount < 5)
            {
                try
                {
                    using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                    {
                        socket.Connect(Properties.Resources.MooTester_TesterIP, int.Parse(Properties.Resources.MooTester_TesterPort));
                        //TODO: FIXME ReceiveTimeout
                        socket.SendTimeout = int.Parse(Properties.Resources.MooTester_SocketSendTimeout);
                        using (MemoryStream mem = new MemoryStream())
                        {
                            using (BinaryWriter writer = new BinaryWriter(mem))
                            {
                                writer.Write(34659308463532339L);
                            }
                            socket.Send(mem.ToArray());
                        }
                        return func(socket);
                    }
                }
                catch (SocketException)
                {
                    failureCount++;
                    Thread.Sleep(1000);
                }
                catch (MooTesterException e)
                {
                    return e.Result;
                }
            }
            return new TestResult()
            {
                Score = 0,
                Info = Properties.Resources.MooTester_NetworkError
            };
        }

        int GetScore(ref string message)
        {
            int result = 0;
            foreach(Match match in Regex.Matches(message, @"{Score:(\d+)}")){
                result+=int.Parse(match.Groups[1].Value);
            }
            message = Regex.Replace(message, @"{Score:\d+}", "");
            return result;
        }

        string Compile(Socket socket, string source, string command)
        {
            socket.Send(new Message()
            {
                Type = Message.MessageType.Compile,
                Content = new CompileIn()
                {
                    Command = command,
                    Code = source,
                    Memory = long.Parse(Properties.Resources.MooTester_CompileMemory),
                    Time = long.Parse(Properties.Resources.MooTester_CompileTime)
                }
            }.ToBytes());

            Out compileResult = new Out(socket);
            switch (compileResult.Type)
            {
                case Out.ResultType.Success:
                    return compileResult.Message;
                case Out.ResultType.TimeLimitExceeded:
                    throw new MooTesterException()
                    {
                        Result = new TestResult()
                                {
                                    Score = 0,
                                    Info = Properties.Resources.MooTester_CompilerTLE
                                }
                    };
                case Out.ResultType.RuntimeError:
                    throw new MooTesterException()
                    {
                        Result = new TestResult()
                                {
                                    Score = 0,
                                    Info = string.Format(Properties.Resources.MooTester_CompilerRE, compileResult.Message)
                                }
                    };
                case Out.ResultType.MemoryLimitExceeded:
                    throw new MooTesterException()
                    {
                        Result = new TestResult()
                                {
                                    Score = 0,
                                    Info = Properties.Resources.MooTester_CompilerMLE
                                }
                    };
                case Out.ResultType.OutputLimitExceeded:
                    throw new MooTesterException()
                    {
                        Result = new TestResult()
                                {
                                    Score = 0,
                                    Info = Properties.Resources.MooTester_CompileOLE
                                }
                    };
                default:
                    throw new MooTesterException()
                    {
                        Result = new TestResult()
                                {
                                    Score = 0,
                                    Info = Properties.Resources.MooTester_CompilerUndefinedError
                                }
                    };
            }
        }
    }
}
