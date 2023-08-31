namespace SpreadsheetTester
{
    internal class Tester
    {
        static void Main(string[] args)
        {
            string expression = "(5 + (8 / 2) * 4) / 2 - number7";
            expression = "5 * (2 + 1)";

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