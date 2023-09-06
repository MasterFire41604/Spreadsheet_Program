namespace SpreadsheetTester
{
    internal class Tester
    {
        static void Main(string[] args)
        {
            string expression = "(1*1)-2/2";

            var evaluate = FormulaEvaluator.Evaluator.Evaluate(expression, AnyVariables);
            Console.WriteLine(evaluate);
        }

        static int AnyVariables(String input)
        {
            switch (input)
            {
                case "number7":
                    return 5;
                default:
                    return 0;
            }
        }
    }
}