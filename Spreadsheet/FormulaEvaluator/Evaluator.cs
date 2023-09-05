using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    /// <summary>
    /// Evaluates integer arithmetic expressions written using standard infix notation
    /// </summary>
    public static class Evaluator
    {
        public delegate int Lookup(String v);

        /// <summary>
        /// Calculates the given expression, using a given method to determine given variables if necessary
        /// </summary>
        /// <param name="exp"> The expression to calculate </param>
        /// <param name="variableEvaluator"> The method to use for determining given variables within the expression </param>
        /// <returns> The answer to the expression, given as a whole number </returns>
        /// <exception cref="ArgumentException"> If the expression is written incorrectly </exception>
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            Stack<int> values = new Stack<int>();
            Stack<string> operators = new Stack<string>();

            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            if (substrings.Length <= 1)
            {
                if (string.IsNullOrEmpty(substrings[0]))
                {
                    // If the expression is empty
                    throw new ArgumentException("Invalid expression given");
                }
            }

            foreach (string s in substrings)
            {
                string newS = s.Trim();

                if (string.IsNullOrWhiteSpace(newS))
                {
                    // If the token in the expression is whiteSpace
                    continue;
                }

                if (!IsValidToken(newS))
                {
                    // If the token is not supported for calculations
                    throw new ArgumentException("Unsupported equation");
                }

                if (int.TryParse(newS, out _))
                {
                    // If the token is a number, calculate and push to value stack if neccessary, otherwise just push to value stack
                    if (operators.Count > 0 && (operators.Peek() == "*" || operators.Peek() == "/"))
                    {
                        PushToValues(values, operators, int.Parse(newS));
                    }
                    else
                    {
                        values.Push(int.Parse(newS));
                    }
                }
                else
                {
                    switch (newS)
                    {
                        case "+":
                            // If the token is a +, check if there needs to be a calculation before adding to stack, then add to stack
                            if (operators.Count > 0 && (operators.Peek() == "+" || operators.Peek() == "-"))
                            {
                                PushToValues(values, operators);
                            }

                            operators.Push(newS);
                            break;
                        case "-":
                            // If the token is a -, check if there needs to be a calculation before adding to stack, then add to stack
                            if (operators.Count > 0 && (operators.Peek() == "+" || operators.Peek() == "-"))
                            {
                                PushToValues(values, operators);
                            }

                            operators.Push(newS);
                            break;
                        case "*":
                            // If the token is a *, add to stack
                            operators.Push("*");
                            break;
                        case "/":
                            // If the token is a /, add to stack
                            operators.Push("/");
                            break;
                        case "(":
                            // If the token is a (, add to stack
                            operators.Push("(");
                            break;
                        case ")":
                            // If the token is a ), calculate if necessary, get the other ( to close parenthesis, then make any additional necessary calculations
                            if (operators.Peek() == "+" || operators.Peek() == "-")
                            {
                                PushToValues(values, operators);
                            }
                            else
                            {
                                throw new ArgumentException("Invalid expression given");
                            }

                            if (operators.Count > 0)
                            {
                                string o = operators.Pop();
                            }
                            else
                            {
                                throw new ArgumentException("There isn't a closing parenthesis where expected");
                            }

                            if (operators.Count > 0 && (operators.Peek() == "*" || operators.Peek() == "/"))
                            {
                                PushToValues(values, operators);
                            }

                            break;
                        default:
                            // If the token is a variable, calculate and push to value stack if neccessary, otherwise just push variable's value to value stack
                            if (operators.Count > 0 && (operators.Peek() == "*" || operators.Peek() == "/"))
                            {
                                PushToValues(values, operators, variableEvaluator(newS));
                            }
                            else
                            {
                                values.Push(variableEvaluator(newS));
                            }
                            break;
                    }
                }
            }

            if (operators.Count > 0)
            {
                // If there is still an additional calculation after everything else is done, calculate and push to value stack
                PushToValues(values, operators);
            }

            return values.Pop();
        }

        /// <summary>
        /// Checks if the token in the expression is valid
        /// </summary>
        /// <param name="s"> The token to check for validity </param>
        /// <returns> True only if the token is valid, otherwise returns false </returns>
        private static bool IsValidToken(string s)
        {
            if (int.TryParse(s, out _))
            {
                return true;
            }
            else if (char.IsLetter(s.First()) && char.IsDigit(s.Last()))
            {
                return true;
            }
            else
            {
                switch (s)
                {
                    case "+":
                        return true;
                    case "-":
                        return true;
                    case "*":
                        return true;
                    case "/":
                        return true;
                    case "(":
                        return true;
                    case ")":
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Pushes the resulting value to the values stack
        /// </summary>
        /// <param name="vals"> The value stack </param>
        /// <param name="ops"> the operator stack </param>
        /// <param name="newRightSide"> If using the variable without pushing to stack first </param>
        private static void PushToValues(Stack<int> vals, Stack<string> ops, int newRightSide = -1)
        {
            if (vals.Count == 0)
            {
                throw new ArgumentException("Invalid expression given");
            }

            int rightSide = vals.Pop();
            int leftSide;
            if (newRightSide == -1)
            {
                if (vals.Count > 0)
                    leftSide = vals.Pop();
                else
                    throw new ArgumentException("Invalid expression given");
            }
            else
            {
                leftSide = rightSide;
                rightSide = newRightSide;
            }
            vals.Push(GetResult(leftSide, ops.Pop(), rightSide));
        }

        /// <summary>
        /// Calculates a value using the given operator
        /// </summary>
        /// <param name="leftSide"> The left hand side of the equation </param>
        /// <param name="op"> The operator to ultimately use on the numbers </param>
        /// <param name="rightSide"> The right hand side of the equation </param>
        /// <returns> The resulting value after the calculation </returns>
        private static int GetResult(int leftSide, string op, int rightSide)
        {
            switch (op)
            {
                case "+":
                    return leftSide + rightSide;
                case "-":
                    return leftSide - rightSide;
                case "*":
                    return leftSide * rightSide;
                case "/":
                    if (rightSide == 0)
                        throw new ArgumentException("Cannot divide by zero");

                    return leftSide / rightSide;
            }

            // Will never get here from how this method is being used
            return 0;
        }
    }
}