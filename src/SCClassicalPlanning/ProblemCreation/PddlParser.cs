using Antlr4.Runtime;
using SCClassicalPlanning.ProblemCreation.Antlr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCClassicalPlanning.ProblemCreation
{
    /// <summary>
    /// 
    /// </summary>
    public static class PddlParser
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="problemPddl"></param>
        /// <param name="domainPddl"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Problem ParseProblem(string problemPddl, string domainPddl)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="problemPddl"></param>
        /// <param name="domainPddl"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public static Problem ParseProblem(string problemPddl, Domain domain)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pddl"></param>
        /// <returns></returns>
        public static Domain ParseDomain(string pddl)
        {
            throw new NotImplementedException();
        }

        private static MinimalPDDLParser MakeParser(string input)
        {
            AntlrInputStream inputStream = new(input);

            // NB: ANTLR apparently adds a listener by default that writes to the console.
            // Which is crazy default behaviour if you ask me, but never mind.
            // Remove it so that consumers of this lib don't get random messages turning up on their console.
            MinimalPDDLLexer lexer = new(inputStream);
            lexer.RemoveErrorListeners();
            CommonTokenStream tokens = new(lexer);

            // NB: In the parser, we add our own error listener that throws an exception.
            // Otherwise errors would just be ignored and the method would just return null, which is obviously bad behaviour.
            MinimalPDDLParser parser = new(tokens);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(ThrowingErrorListener.Instance);

            return parser;
        }

        private class ThrowingErrorListener : BaseErrorListener
        {
            public static ThrowingErrorListener Instance = new ThrowingErrorListener();

            public override void SyntaxError(TextWriter output, IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg, RecognitionException e)
            {
                throw new ArgumentException("line " + line + ":" + charPositionInLine + " " + msg, "sentence");
            }
        }
    }
}
