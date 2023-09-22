using SpreadsheetUtilities;
using SS;

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
            Spreadsheet s = new Spreadsheet();
        }

        /// <summary>
        /// Tests setting a cell to a number
        /// </summary>
        [TestMethod()]
        public void TestSetCellContentsNum()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 5);
        }

        /// <summary>
        /// Tests setting a cell to a string
        /// </summary>
        [TestMethod()]
        public void TestSetCellContentsText()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", "hi there");
        }

        /// <summary>
        /// Tests setting a cell to a Formula
        /// </summary>
        [TestMethod()]
        public void TestSetCellContentsFormula()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", new Formula("2 + 2"));
        }

        /// <summary>
        /// Tests retrieving the contents of a cell
        /// </summary>
        [TestMethod()]
        public void TestGetCellContents()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 5);

            Assert.AreEqual(5.0, s.GetCellContents("A1"));

            s.SetCellContents("A2", "5");

            Assert.AreEqual("5", s.GetCellContents("A2"));
        }

        /// <summary>
        /// Tests retrieving a list of all non-empty cells
        /// </summary>
        [TestMethod()]
        public void TestGetNonemtpyCells()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 4);
            s.SetCellContents("B5", "b");
            s.SetCellContents("A3", new Formula("5 + 5"));
            s.SetCellContents("A2", "");

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
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", 4);
            s.SetCellContents("A2",new Formula("A1 + 2"));
            List<string> a3List = s.SetCellContents("A3", new Formula("A2 + 7")).ToList();
            List<string> a2List = s.SetCellContents("A2", new Formula("A1 + 2")).ToList();
            List<string> a1List = s.SetCellContents("A1", 4).ToList();
            
            // Make sure there are the correct number of dependents for each list
            Assert.IsTrue(a1List.Count == 3);
            Assert.IsTrue(a2List.Count == 2);
            Assert.IsTrue(a3List.Count == 1);

            // overwrites A1 and A2, so A1 does not have any dependents anymore because A2 does not depend on A1
            a2List = s.SetCellContents("A2", "5").ToList();
            a1List = s.SetCellContents("A1", 4).ToList();

            // Make sure the number of dependents for each list are all updated correctly
            Assert.IsTrue(a1List.Count == 1);
            Assert.IsTrue(a2List.Count == 2);
            Assert.IsTrue(a3List.Count == 1);

            // Overwrites A3, so A2 does not affect A3 anymore
            a3List = s.SetCellContents("A3", 1).ToList();
            a2List = s.SetCellContents("A2", "5").ToList();
            a1List = s.SetCellContents("A1", 4).ToList();

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
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A1", new Formula("A1 + 1"));
        }

        /// <summary>
        /// Tests for a circular exception to be thrown when cells reference each other
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void TestCircularException()
        {
            Spreadsheet s = new Spreadsheet();
            s.SetCellContents("A2", 3);
            s.SetCellContents("A1", new Formula("A2 + 2"));
            s.SetCellContents("A2", new Formula("A1 + 1"));
        }
    }
}