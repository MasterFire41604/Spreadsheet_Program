using System.Text.RegularExpressions;

namespace SpreadsheetUtilities;

/// <summary>
/// Represents formulas written in standard infix notation using standard precedence
/// rules.  The allowed symbols are non-negative numbers written using double-precision
/// floating-point syntax (without unary preceeding '-' or '+');
/// variables that consist of a letter or underscore followed by
/// zero or more letters, underscores, or digits; parentheses; and the four operator
/// symbols +, -, *, and /.
///
/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable;
/// and "x 23" consists of a variable "x" and a number "23".
///
/// Associated with every formula are two delegates: a normalizer and a validator.  The
/// normalizer is used to convert variables into a canonical form. The validator is used to
/// add extra restrictions on the validity of a variable, beyond the base condition that
/// variables must always be legal: they must consist of a letter or underscore followed
/// by zero or more letters, underscores, or digits.
/// Their use is described in detail in the constructor and method comments.
/// </summary>
public class Formula
{
    private readonly string _formula;
    private readonly List<string> _variables;

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically invalid,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer is the identity function, and the associated validator
    /// maps every string to true.
    /// </summary>
    public Formula(string formula) :
        this(formula, s => s, s => true)
    {
    }

    /// <summary>
    /// Creates a Formula from a string that consists of an infix expression written as
    /// described in the class comment.  If the expression is syntactically incorrect,
    /// throws a FormulaFormatException with an explanatory Message.
    ///
    /// The associated normalizer and validator are the second and third parameters,
    /// respectively.
    ///
    /// If the formula contains a variable v such that normalize(v) is not a legal variable,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// If the formula contains a variable v such that isValid(normalize(v)) is false,
    /// throws a FormulaFormatException with an explanatory message.
    ///
    /// Suppose that N is a method that converts all the letters in a string to upper case, and
    /// that V is a method that returns true only if a string consists of one letter followed
    /// by one digit.  Then:
    ///
    /// new Formula("x2+y3", N, V) should succeed
    /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
    /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
    /// </summary>
    public Formula(string formula, Func<string, string> normalize, Func<string, bool> isValid)
    {
        var tokens = GetTokens(formula).ToList();
        string newFormula = "";
        int tokenCount = tokens.Count, openPar = 0, closePar = 0, newFormulaPointer = 0;
        _variables = new List<string>();

        if (tokenCount == 0)
        {
            throw new FormulaFormatException("Cannot input an empty expression");
        }

        for (int i = 0; i < tokenCount; i++, newFormulaPointer++)
        {
            string operPattern = @"[+\-*\/()]{1}";
            string operFollowPattern = @"[+\-*\/]{1}";
            string legalVarNames = @"^[a-zA-Z_][a-zA-Z0-9_]*$";

            string newT = tokens.ElementAt(i).Trim();

            if (double.TryParse(newT, out _))
            {
                // Is a number
                if (newFormula.Length > 0 && (newFormula[newFormula.Length - 1].Equals(')') || char.IsLetter(newFormula[newFormula.Length - 1]) || char.IsNumber(newFormula[newFormula.Length - 1])))
                {
                    // Checking the last character of newFormula and it being a number/character ensures that it is either a number or a variable
                    throw new FormulaFormatException("Invalid expression given. Only an operator/closing parenthesis can follow a number/variable/closing parenthesis");
                }

                newFormula += double.Parse(newT);
            }
            else if (Regex.IsMatch(newT, operPattern))
            {
                // Is an operator
                if (i == 0 && !newT.Equals("("))
                {
                    // If the first character in the expression is an operator that is not an opening parenthesis
                    throw new FormulaFormatException("Invalid expression given. An operator/closing parenthesis cannot be at the beginning of the expression");
                }
                if (i == tokenCount - 1 && !newT.Equals(")"))
                {
                    // If the last character in the expression is an operator that is not an closing parenthesis
                    throw new FormulaFormatException("Invalid expression given. An operator/opening parenthsis cannot be at the end of the expression");
                }
                if (newFormula.Length > 0 && Regex.IsMatch(newFormula[newFormula.Length - 1].ToString(), operFollowPattern) && !newT.Equals("("))
                {
                    // If any operator is given after another operator where the given operator is not an opening parenthesis
                    throw new FormulaFormatException("Invalid expression given. The only operator allowed after an operator/opening parenthesis is an opening parenthesis");
                }
                if (newFormula.Length > 0 && newFormula[newFormula.Length - 1].Equals(')') && newT.Equals("("))
                {
                    // If an opening parenthesis is given right after a closing parenthesis
                    throw new FormulaFormatException("Invalid expression given. Only an operator/closing parenthesis can follow a closing parenthesis");
                }

                if (newT.Equals("("))
                {

                    if (i > 0 && double.TryParse(newFormula.ElementAt(newFormulaPointer - 1).ToString(), out _))
                    {
                        // There is a number right before an opening parenthesis, so throw exception
                        throw new FormulaFormatException("Invalid expression given. Cannot have an opening parenthesis after a number");
                    }
                    openPar += 1;
                }
                if (newT.Equals(")"))
                {
                    closePar += 1;

                    if (openPar < closePar)
                    {
                        // If a closing parenthesis is found where there is not a matching opening parenthesis
                        throw new FormulaFormatException("Invalid expression given. Too many closing parenthesis");
                    }
                }

                newFormula += newT;
            }
            else
            {
                if (isValid(normalize(newT)) == true && Regex.IsMatch(newT, legalVarNames))
                {
                    // Is a valid variable name
                    if (newFormula.Length > 0 && (newFormula[newFormula.Length - 1].Equals(')') || char.IsLetter(newFormula[newFormula.Length - 1]) || char.IsNumber(newFormula[newFormula.Length - 1])))
                    {
                        // Checking the last character of newFormula and it being a number/character ensures that it is either a number or a variable
                        throw new FormulaFormatException("Invalid expression given. Only an operator/closing parenthesis can follow a number/variable/closing parenthesis.");
                    }

                    newFormula += normalize(newT);
                    newFormulaPointer++;
                    
                    // Adds the variable to the list of variables only if it isn't already there
                    if (!_variables.Contains(normalize(newT)))
                        _variables.Add(normalize(newT));
                }
                else
                {
                    // Is an invalid variable name/invalid token
                    throw new FormulaFormatException("The given variable/token is not valid");
                }
            }
        }

        if (openPar == closePar)
        {
            // Only sets the formula in stone if there are the same number of closing parenthesis as there are opening parenthesis
            _formula = newFormula;
        }
        else
        {
            throw new FormulaFormatException("The number of closing parenthesis does not match the number of opening parenthesis");
        }
    }

    /// <summary>
    /// Evaluates this Formula, using the lookup delegate to determine the values of
    /// variables.  When a variable symbol v needs to be determined, it should be looked up
    /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to
    /// the constructor.)
    ///
    /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters
    /// in a string to upper case:
    ///
    /// new Formula("x+7", N, s => true).Evaluate(L) is 11
    /// new Formula("x+7").Evaluate(L) is 9
    ///
    /// Given a variable symbol as its parameter, lookup returns the variable's value
    /// (if it has one) or throws an ArgumentException (otherwise).
    ///
    /// If no undefined variables or divisions by zero are encountered when evaluating
    /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.
    /// The Reason property of the FormulaError should have a meaningful explanation.
    ///
    /// This method should never throw an exception.
    /// </summary>
    public object Evaluate(Func<string, double> lookup)
    {
        Stack<double> values = new Stack<double>();
        Stack<string> operators = new Stack<string>();

        string[] substrings = GetTokens(_formula).ToArray();

        foreach (string s in substrings)
        {
            string newS = s.Trim();

            if (double.TryParse(newS, out _))
            {
                // If the token is a number, calculate and push to value stack if neccessary, otherwise just push to value stack
                if (operators.Count > 0 && (operators.Peek() == "*" || operators.Peek() == "/"))
                {
                    if (operators.Peek() == "/" && double.Parse(newS) == 0)
                    {
                        return new FormulaError("Cannot divide by zero");
                    }

                    PushToValues(values, operators, double.Parse(newS));
                }
                else
                {
                    values.Push(double.Parse(newS));
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

                        operators.Pop();

                        if (operators.Count > 0 && (operators.Peek() == "*" || operators.Peek() == "/"))
                        {
                            if (operators.Peek() == "/" && values.Peek() == 0)
                            {
                                return new FormulaError("Cannot divide by zero");
                            }

                            PushToValues(values, operators);
                        }

                        break;
                    default:
                        // If the token is a variable, calculate and push to value stack if neccessary, otherwise just push variable's value to value stack
                        if (operators.Count > 0 && (operators.Peek() == "*" || operators.Peek() == "/"))
                        {
                            try
                            {
                                if (operators.Peek() == "/" && lookup(newS) == 0)
                                {
                                    return new FormulaError("Cannot divide by zero (one of your variables contains a zero)");
                                }
                            }
                            catch
                            {
                                return new FormulaError("You have undefined variables in your expression");
                            }

                            try
                            {
                                PushToValues(values, operators, lookup(newS));
                            }
                            catch
                            {
                                return new FormulaError("You have undefined variables in your expression");
                            }
                        }
                        else
                        {
                            try
                            {
                                values.Push(lookup(newS));
                            }
                            catch
                            {
                                return new FormulaError("You have undefined variables in your expression");
                            }
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
    /// Pushes the resulting value to the values stack (Used for evaluate method)
    /// </summary>
    /// <param name="vals"> The value stack </param>
    /// <param name="ops"> the operator stack </param>
    /// <param name="newRightSide"> If using the variable without pushing to stack first </param>
    private static void PushToValues(Stack<double> vals, Stack<string> ops, double? newRightSide = null)
    {
        double rightSide = vals.Pop();
        double leftSide;
        if (newRightSide is null)
        {
            leftSide = vals.Pop();
        }
        else
        {
            leftSide = rightSide;
            rightSide = (double)newRightSide;
        }
        vals.Push(GetResult(leftSide, ops.Pop(), rightSide));
    }

    /// <summary>
    /// Calculates a value using the given operator (Used for evaluate method)
    /// </summary>
    /// <param name="leftSide"> The left hand side of the equation </param>
    /// <param name="op"> The operator to ultimately use on the numbers </param>
    /// <param name="rightSide"> The right hand side of the equation </param>
    /// <returns> The resulting value after the calculation </returns>
    private static double GetResult(double leftSide, string op, double rightSide)
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
                return leftSide / rightSide;
        }

        // Will never get here from how this method is being used
        return 0;
    }


    /// <summary>
    /// Enumerates the normalized versions of all of the variables that occur in this
    /// formula.  No normalization may appear more than once in the enumeration, even
    /// if it appears more than once in this Formula.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
    /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
    /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
    /// </summary>
    public IEnumerable<string> GetVariables()
    {
        return _variables;
    }

    /// <summary>
    /// Returns a string containing no spaces which, if passed to the Formula
    /// constructor, will produce a Formula f such that this.Equals(f).  All of the
    /// variables in the string should be normalized.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
    /// new Formula("x + Y").ToString() should return "x+Y"
    /// </summary>
    public override string ToString()
    {
        return _formula;
    }

    /// <summary>
    /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
    /// whether or not this Formula and obj are equal.
    ///
    /// Two Formulae are considered equal if they consist of the same tokens in the
    /// same order.  To determine token equality, all tokens are compared as strings
    /// except for numeric tokens and variable tokens.
    /// Numeric tokens are considered equal if they are equal after being "normalized" by
    /// using C#'s standard conversion from string to double (and optionally back to a string).
    /// Variable tokens are considered equal if their normalized forms are equal, as
    /// defined by the provided normalizer.
    ///
    /// For example, if N is a method that converts all the letters in a string to upper case:
    ///
    /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
    /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
    /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
    /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
    /// </summary>
    public override bool Equals(object? obj)
    {
        Formula? other = obj as Formula;

        if (other is not null)
        {
            // The formula was constructed in such a way where it will always be normalized, with no whitespace, and calculated doubles if necessary (1.0 == 1 || 1.0e-1 == 0.1)
            // This means that no matter what different formulae you use, they will all either be the exact same or not the same no matter what.
            // This is why just checking the hashcode of the two formulae has the same result as checking each character individually.
            if (GetHashCode() != other.GetHashCode())
            {
                return false;
            }

            return true;
        }

        return false;
    }

    /// <summary>
    /// Reports whether f1 == f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator ==(Formula f1, Formula f2)
    {
        return f1.Equals(f2);
    }

    /// <summary>
    /// Reports whether f1 != f2, using the notion of equality from the Equals method.
    /// Note that f1 and f2 cannot be null, because their types are non-nullable
    /// </summary>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !f1.Equals(f2);
    }

    /// <summary>
    /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
    /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two
    /// randomly-generated unequal Formulae have the same hash code should be extremely small.
    /// </summary>
    public override int GetHashCode()
    {
        return _formula.GetHashCode();
    }

    /// <summary>
    /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
    /// right paren; one of the four operator symbols; a legal variable token;
    /// a double literal; and anything that doesn't match one of those patterns.
    /// There are no empty tokens, and no token contains white space.
    /// </summary>
    private static IEnumerable<string> GetTokens(string formula)
    {
        // Patterns for individual tokens
        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                yield return s;
            }
        }

    }
}

/// <summary>
/// Used to report syntactic errors in the argument to the Formula constructor.
/// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    /// Constructs a FormulaFormatException containing the explanatory message.
    /// </summary>
    public FormulaFormatException(string message) : base(message)
    {
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public struct FormulaError
{
    /// <summary>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </summary>
    /// <param name="reason"></param>
    public FormulaError(string reason) : this()
    {
        Reason = reason;
    }

    /// <summary>
    ///  The reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
}