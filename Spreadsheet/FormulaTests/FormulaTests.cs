using SpreadsheetUtilities;
using System.Security.Cryptography;

namespace FormulaTests
{
    [TestClass]
    public class FormulaTests
    {
        // ********************************************** Creation/Evaluation ***************************************************************
        // --------------------------------------- Tests catching exceptions/errors -----------------------------------------------------
        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestEmptyFormula()
        {
            Formula f = new Formula("");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestNumberAfterNumberNoOperator()
        {
            Formula f = new Formula("5 5");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestOpeningOperatorPlus()
        {
            Formula f = new Formula("+");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestOpeningOperatorMinus()
        {
            Formula f = new Formula("-");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestOpeningOperatorMult()
        {
            Formula f = new Formula("*");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestOpeningOperatorDiv()
        {
            Formula f = new Formula("/");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestOpeningOperatorCloseParen()
        {
            Formula f = new Formula(")");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestOperatorAfterOperator()
        {
            Formula f = new Formula("5 + + 5");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestNumberAfterCloseParen()
        {
            Formula f = new Formula("(5 + 5) 2");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestEndOperatorNotCloseParen()
        {
            Formula f = new Formula("21 +");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestOpenParenAfterCloseParen()
        {
            Formula f = new Formula("(5 + 5)(5 + 5)");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMoreCloseParenThanOpenParen()
        {
            Formula f = new Formula("5 + 5)");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestMoreOpenParenThanCloseParen()
        {
            Formula f = new Formula("(5 + 5");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestInvalidVariable()
        {
            Formula f = new Formula("5 + 2A");
        }

        [TestMethod()]
        [ExpectedException(typeof(FormulaFormatException))]
        public void TestInvalidToken()
        {
            Formula f = new Formula("5 + jka54_&");
        }

        [TestMethod()]
        public void TestDivideByZero()
        {
            Formula f = new Formula("5 / 0");
            Assert.IsTrue(f.Evaluate(s => 0).GetType() == typeof(FormulaError));

            f = new Formula("2 / (5 - 5)");
            Assert.IsTrue(f.Evaluate(s => 0).GetType() == typeof(FormulaError));

            f = new Formula("5 / ag36_");
            Assert.IsTrue(f.Evaluate(s => 0).GetType() == typeof(FormulaError));
        }

        [TestMethod()]
        public void TestUndefinedVariables()
        {
            Formula f = new Formula("5 / a1");
            Assert.IsTrue(f.Evaluate(null).GetType() == typeof(FormulaError));

            f = new Formula("5 * a1");
            Assert.IsTrue(f.Evaluate(null).GetType() == typeof(FormulaError));

            f = new Formula("5 + a1");
            Assert.IsTrue(f.Evaluate(null).GetType() == typeof(FormulaError));
        }

        // --------------------------------------- Tests not catching exceptions/errors -----------------------------------------------------

        [TestMethod()]
        public void TestValidVariable()
        {
            Formula f = new Formula("5 + a2");
        }

        [TestMethod()]
        public void TestSimpleOperations()
        {
            Formula f = new Formula("5 + 5");
            Assert.AreEqual(10.0, f.Evaluate(s => 0));

            f = new Formula("5 - 5");
            Assert.AreEqual(0.0, f.Evaluate(s => 0));

            f = new Formula("5 / 2");
            Assert.AreEqual(2.5, f.Evaluate(s => 0));

            f = new Formula("5 * 3");
            Assert.AreEqual(15.0, f.Evaluate(s => 0));
        }

        [TestMethod()]
        public void TestMoreComplicatedOperations()
        {
            Formula f = new Formula("6 + (5 - 3) + 5");
            Assert.AreEqual(13.0, f.Evaluate(s => 0));

            f = new Formula("3 * (8 - 4) / 6 + (3 * 2)");
            Assert.AreEqual(8.0, f.Evaluate(s => 0));

            f = new Formula("20 - (8 + 7 - 6) - 2");
            Assert.AreEqual(9.0, f.Evaluate(s => 0));
        }

        [TestMethod()]
        public void TestComplicatedOperations()
        {
            Formula f = new Formula("(21 - (4 * (6 / (1 + 3) * 3) - 4) / 2) + 6");
            Assert.AreEqual(20.0, f.Evaluate(s => 0));

            f = new Formula("5 - ((5*3) / 4) * 2");
            Assert.AreEqual(-2.5, f.Evaluate(s => 0));
        }

        [TestMethod()]
        public void TestEvaluateWithVariables()
        {
            Formula f = new Formula("5 + (a2 - h6) + (3 * _ab5k)");
            Assert.AreEqual(14.0, f.Evaluate(s => 3));
        }

        [TestMethod()]
        public void TestScientificNotation()
        {
            Formula f = new Formula("6.6e-3");
            Assert.AreEqual(0.0066, f.Evaluate(s => 0));

            f = new Formula("5 + 4.6e+2 - (10 * 3 - 3)");
            Assert.AreEqual(438.0, f.Evaluate(s => 0));
        }

        [TestMethod()]
        public void CreateFormulaFromToString()
        {
            Formula f = new Formula("5 + 5 / 3 * 2");
            Formula f1 = new Formula(f.ToString());

            Assert.IsTrue(f1.Equals(f));
        }

        [TestMethod()]
        public void TestNormalizer()
        {
            Formula f = new Formula("5 + (a1 - 3) * 4 - a2", s => s.ToUpper(), s => true);

            Assert.AreEqual(8.0, f.Evaluate(s => 5));
            Assert.IsTrue(f.GetVariables().ToArray().ElementAt(0).Equals("A1"));

             f = new Formula("5 + (A1 - 3) * 4 - a2", s => s.ToLower(), s => true);

            Assert.AreEqual(8.0, f.Evaluate(s => 5));
            Assert.IsTrue(f.GetVariables().ToArray().ElementAt(0).Equals("a1"));
        }

        // ********************************************** Access/Checking ***************************************************************

        [TestMethod()]
        public void TestGetVariables()
        {
            Formula f = new Formula("5 + 5");
            Assert.IsTrue(f.GetVariables().Any() == false);

            f = new Formula("5 + ((_adfk255 * afj3j) - 5) * b3");
            Assert.IsTrue(f.GetVariables().Count() == 3);

            f = new Formula("x+y*z", s => s.ToUpper(), s => true);
            foreach (string s in f.GetVariables())
            {
                Assert.IsTrue(s.Equals("X") || s.Equals("Y") || s.Equals("Z"));
            }

            f = new Formula("x+X*z", s => s.ToUpper(), s => true);
            Assert.IsTrue(f.GetVariables().Count() == 2);
            foreach (string s in f.GetVariables())
            {
                Assert.IsTrue(s.Equals("X") || s.Equals("Z"));
            }
        }

        [TestMethod()]
        public void TestToString()
        {
            Formula f = new Formula("5         + (6 /      3)");
            Assert.AreEqual("5+(6/3)", f.ToString());
        }

        [TestMethod()]
        public void TestEquals()
        {
            Formula a = new Formula("1.0000 *      (3.00000  -    1.000000)  +    5.000 /  3.00");
            Formula b = new Formula("1*(3-1)+5/3");
            Formula c = new Formula("(3-1)*1");

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);

            Assert.IsFalse(a.Equals(c));
            Assert.IsFalse(a == c);
            Assert.IsTrue(a != c);

            a = new Formula("a1 - 5");
            b = new Formula("a2 - 5");
            c = new Formula("a1 - 5");

            Assert.IsFalse(a.Equals(b));
            Assert.IsTrue(a.Equals(c));

            a = new Formula("5 + 5");
            b = new Formula("(5 + 5)");

            Assert.IsFalse(a.Equals(b));

            a = new Formula("6.1e-21");
            b = new Formula("6e-21");

            Assert.IsFalse(a.Equals(b));

            Formula? d = null;

            Assert.IsFalse(a.Equals(d));

            Assert.IsTrue(new Formula("x1+y2", s => s.ToUpper(), s => true).Equals(new Formula("X1  +  Y2")));
            Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("X1+Y2")));
            Assert.IsFalse(new Formula("x1+y2").Equals(new Formula("y2+x1")));
            Assert.IsTrue(new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")));
        }

        [TestMethod()]
        public void TestEqualsWithNonFormula()
        {
            Formula f = new Formula("5 + 5");

            Assert.IsFalse(f.Equals("10"));
            Assert.IsFalse(f.Equals(null));
        }

        [TestMethod()]
        public void TestHashCode()
        {
            Formula a = new Formula("1.0000 *      (3.00000  -    1.000000)");
            Formula b = new Formula("1*(3-1)");
            Formula c = new Formula("(3-1)*1");

            Assert.IsNotNull(a.GetHashCode());

            Assert.IsTrue(a.GetHashCode() == b.GetHashCode());
            Assert.IsFalse(a.GetHashCode() == c.GetHashCode());

            a = new Formula("53e-6");
            b = new Formula("53.000001e-6");

            Assert.IsFalse(a.GetHashCode() == b.GetHashCode());
        }
    }
}