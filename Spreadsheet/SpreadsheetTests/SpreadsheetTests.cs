using SpreadsheetUtilities;
using SS;
using System.Text.Json;

namespace SpreadsheetTests
{
    [TestClass]
    public class SpreadsheetTests
    {
        /// <summary>
        /// Simply tests if creating a spreadsheet works
        /// </summary>
        [TestMethod()]
        public void TestCreateSpreadsheet()
        {
            AbstractSpreadsheet s = new Spreadsheet();
        }

        /// <summary>
        /// Tests setting a cell to a number
        /// </summary>
        [TestMethod()]
        public void TestSetCellContentsNum()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "5");
        }

        /// <summary>
        /// Tests setting a cell to a string
        /// </summary>
        [TestMethod()]
        public void TestSetCellContentsText()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "hi there");
        }

        /// <summary>
        /// Tests setting a cell to a Formula
        /// </summary>
        [TestMethod()]
        public void TestSetCellContentsFormula()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=2 + 2");
        }

        /// <summary>
        /// Tests retrieving the contents of a cell
        /// </summary>
        [TestMethod()]
        public void TestGetCellContents()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "5");

            Assert.AreEqual(5.0, s.GetCellContents("A1"));

            s.SetContentsOfCell("A2", "5+5");

            Assert.AreEqual("5+5", s.GetCellContents("A2"));

            s.SetContentsOfCell("A2", "=5+5");

            Assert.AreEqual(new Formula("5+5"), s.GetCellContents("A2"));

            Assert.AreEqual("", s.GetCellContents("B47"));
        }

        /// <summary>
        /// Tests retrieving a list of all non-empty cells
        /// </summary>
        [TestMethod()]
        public void TestGetNonemtpyCells()
        {
            AbstractSpreadsheet s = new Spreadsheet();

            Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Count() == 0);

            s.SetContentsOfCell("A1", "4");
            s.SetContentsOfCell("B5", "b");
            s.SetContentsOfCell("A3", "=5 + 5");
            s.SetContentsOfCell("A2", "");

            List<string> names = s.GetNamesOfAllNonemptyCells().ToList();

            Assert.IsTrue((names[0] == "A1") || (names[0] == "B5") || (names[0] == "A3"));
            Assert.IsTrue((names[1] == "A1") || (names[1] == "B5") || (names[1] == "A3"));
            Assert.IsTrue((names[2] == "A1") || (names[2] == "B5") || (names[2] == "A3"));
            Assert.AreEqual(3, names.Count());
        }

        /// <summary>
        /// Tests overwriting a cell with new contents
        /// </summary>
        [TestMethod()]
        public void TestSetOverwriting()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "4");
            s.SetContentsOfCell("A2", "=A1 + 2");
            List<string> a3List = s.SetContentsOfCell("A3", "=A2 + 7").ToList();
            List<string> a2List = s.SetContentsOfCell("A2", "=A1 + 2").ToList();
            List<string> a1List = s.SetContentsOfCell("A1", "4").ToList();

            // Make sure there are the correct number of dependents for each list
            Assert.IsTrue(a1List.Count == 3);
            Assert.IsTrue(a2List.Count == 2);
            Assert.IsTrue(a3List.Count == 1);

            // overwrites A1 and A2, so A1 does not have any dependents anymore because A2 does not depend on A1
            a2List = s.SetContentsOfCell("A2", "5").ToList();
            a1List = s.SetContentsOfCell("A1", "4").ToList();

            // Make sure the number of dependents for each list are all updated correctly
            Assert.IsTrue(a1List.Count == 1);
            Assert.IsTrue(a2List.Count == 2);
            Assert.IsTrue(a3List.Count == 1);

            // Overwrites A3, so A2 does not affect A3 anymore
            a3List = s.SetContentsOfCell("A3", "hello").ToList();
            a2List = s.SetContentsOfCell("A2", "5").ToList();
            a1List = s.SetContentsOfCell("A1", "4").ToList();

            // Makes sure A2 no longer affects A3
            Assert.IsTrue(a1List.Count == 1);
            Assert.IsTrue(a2List.Count == 1);
            Assert.IsTrue(a3List.Count == 1);
        }

        /// <summary>
        /// Tests for a circular exception to be thrown when a cell references itself
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void TestCircularExceptionOnItself()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A1 + 1");
        }

        /// <summary>
        /// Tests for a circular exception to be thrown when cells reference each other
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void TestCircularException()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A2", "3");
            s.SetContentsOfCell("A1", "=A2 + 2");

            try
            {
                s.SetContentsOfCell("A2", "=A1 + 1");
            }
            catch (CircularException e)
            {
                Assert.AreEqual(3.0, s.GetCellContents("A2"));
                throw e;
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetContentsOnInvalidName()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.GetCellContents("5B");    //Should throw exception
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetOnInvalidNameDouble()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("23AG", "4");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetOnInvalidNameString()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("12G", "hello");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestSetOnInvalidNameFormula()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("3hi", "=5+5");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestInvalidVersionLoading()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.Save("Test.txt");

            try
            {
                s = new Spreadsheet("Test.txt", s => true, s => s.ToUpper(), "1");
            }
            catch (SpreadsheetReadWriteException e)
            {
                File.Delete("Test.txt");
                throw e;
            }
        }

        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void TestOverwriteWithCircular()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "4");
            s.SetContentsOfCell("B1", "=A1+C1");
            s.SetContentsOfCell("C1", "hello");

            try
            {
                s.SetContentsOfCell("C1", "=B1 + 1");
            }
            catch (CircularException e)
            {
                Assert.AreEqual("hello", s.GetCellContents("C1"));
                throw e;
            }
        }

        [TestMethod()]
        public void TestGetValue()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "5");
            s.SetContentsOfCell("B1", "=A1 - 2");
            s.SetContentsOfCell("C1", "=A1 + B1");
            s.SetContentsOfCell("D1", "=C1 + (2 * B1)");

            Assert.AreEqual(5.0, s.GetCellValue("A1"));
            Assert.AreEqual(3.0, s.GetCellValue("B1"));
            Assert.AreEqual(8.0, s.GetCellValue("C1"));
            Assert.AreEqual(14.0, s.GetCellValue("D1"));
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void TestGetValueOnInvalid()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "5");
            s.SetContentsOfCell("B1", "=A1 - 2");
            s.SetContentsOfCell("C1", "=A1 + B1");
            s.SetContentsOfCell("D1", "=C1 + (2 * B1)");

            s.GetCellValue("1A");
        }

        [TestMethod()]
        public void TestSaving()
        {
            AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToUpper(), "1");
            s.SetContentsOfCell("A1", "5");
            s.SetContentsOfCell("b1", "=a1-1");
            s.SetContentsOfCell("C1", "hello");
            s.Save("Test.txt");

            File.Delete("Test.txt");
        }

        [TestMethod()]
        public void TestLoading()
        {
            AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToUpper(), "1");
            s.SetContentsOfCell("A1", "5");
            s.SetContentsOfCell("b1", "=a1-1");
            s.SetContentsOfCell("C1", "hello");
            s.Save("Test.txt");

            s = new Spreadsheet("Test.txt", s => true, s => s.ToUpper(), "1");

            Assert.AreEqual(5.0, s.GetCellContents("A1"));
            Assert.AreEqual(new Formula("A1-1"), s.GetCellContents("B1"));
            Assert.AreEqual("hello", s.GetCellContents("C1"));
            Assert.AreEqual("1", s.Version);

            Assert.AreEqual(5.0, s.GetCellValue("A1"));
            Assert.AreEqual(4.0, s.GetCellValue("B1"));
            Assert.AreEqual("hello", s.GetCellValue("C1"));

            File.Delete("Test");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestSavingOnInvalidLocation()
        {
            AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToUpper(), "1");
            s.SetContentsOfCell("A1", "5");
            s.SetContentsOfCell("b1", "=a1-1");
            s.SetContentsOfCell("C1", "hello");
            s.Save("/some/nonsense/path.txt");
        }

        [TestMethod()]
        [ExpectedException(typeof(SpreadsheetReadWriteException))]
        public void TestLoadingOnInvalidLocation()
        {
            AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToUpper(), "1");
            s.SetContentsOfCell("A1", "5");
            s.SetContentsOfCell("b1", "=a1-1");
            s.SetContentsOfCell("C1", "hello");
            s.Save("Test.txt");

            try
            {
                s = new Spreadsheet("/some/nonsense/path.txt", s => true, s => s.ToUpper(), "default");
            }
            catch (SpreadsheetReadWriteException e)
            {
                File.Delete("Test.txt");
                throw e;
            }
        }

        [TestMethod()]
        public void TestLoadOnNull()
        {
            AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToUpper(), "1");
            s.Save("Test.txt");

            File.WriteAllText("Test.txt", "null");

            s = new Spreadsheet("Test.txt", s => true, s => s.ToUpper(), "default");
            s.Save("Test.txt");
        }

        [TestMethod()]
        public void TestValuesUpdatingProperly()
        {
            AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToUpper(), "1");
            s.SetContentsOfCell("A1", "5");
            s.SetContentsOfCell("b1", "=a1-1");
            s.SetContentsOfCell("C1", "=b1+A1");

            Assert.AreEqual(5.0, s.GetCellValue("A1"));
            Assert.AreEqual(4.0, s.GetCellValue("B1"));
            Assert.AreEqual(9.0, s.GetCellValue("C1"));

            s.SetContentsOfCell("A1", "100");

            Assert.AreEqual(100.0, s.GetCellValue("A1"));
            Assert.AreEqual(99.0, s.GetCellValue("B1"));
            Assert.AreEqual(199.0, s.GetCellValue("C1"));
        }

        [TestMethod()]
        public void StressTest()
        {
            AbstractSpreadsheet s = new Spreadsheet(s => true, s => s.ToUpper(), "1");
            double n = 500;

            for (double i = 0; i < n; i++)
            {
                if (i > 0)
                {
                    s.SetContentsOfCell("A" + i, "=A" + (i - 1) + " + 5");
                }
                else
                {
                    s.SetContentsOfCell("A" + i, "5");
                }
            }

            for (double i = 0; i < n; i++)
            {
                if (i > 0)
                {
                    Assert.AreEqual(new Formula("A" + (i - 1) + "+5"), s.GetCellContents("A" + i));
                }
                else
                {
                    Assert.AreEqual(5.0, s.GetCellContents("A" + i));
                }
            }

            for (double i = 0; i < n; i++)
            {
                if (i > 0)
                {
                    Assert.AreEqual(10.0 + (5 * (i - 1)), s.GetCellValue("A" + i));
                }
                else
                {
                    Assert.AreEqual(5.0, s.GetCellValue("A" + i));
                }
            }

            s.Save("Stress.txt");

            s = new Spreadsheet("Stress.txt", s => true, s => s.ToUpper(), "1");

            for (double i = 0; i < n; i++)
            {
                if (i > 0)
                {
                    Assert.AreEqual(new Formula("A" + (i - 1) + "+5"), s.GetCellContents("A" + i));
                }
                else
                {
                    Assert.AreEqual(5.0, s.GetCellContents("A" + i));
                }
            }

            for (double i = 0; i < n; i++)
            {
                if (i > 0)
                {
                    Assert.AreEqual(10.0 + (5 * (i - 1)), s.GetCellValue("A" + i));
                }
                else
                {
                    Assert.AreEqual(5.0, s.GetCellValue("A" + i));
                }
            }

            File.Delete("Stress.txt");
        }
    }
}