using SpreadsheetUtilities;

namespace SS
{
    /// <summary>
    /// A new spreadsheet that can hold any number of cells.
    /// Each cell in the spreadsheet can hold a double/string/Formula.
    /// 
    /// Allows for easy use to set the contents of a cell, access the contents of a cell, and get the names of all non-empty cells.
    /// </summary>
    public class Spreadsheet : AbstractSpreadsheet
    {
        private readonly Dictionary<string, Cell> cells;
        private DependencyGraph graph;

        /// <summary>
        /// Creates a new Spreadsheet that can hold any number of cells
        /// </summary>
        public Spreadsheet()
        {
            cells = new Dictionary<string, Cell>();
            graph = new DependencyGraph();
        }

        public override object GetCellContents(string name)
        {
            return cells[name].Contents;
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            HashSet<string> names = new HashSet<string>();

            foreach (string cell in cells.Keys)
            {
                if (cells[cell].Contents.ToString() != "")
                {
                    // If the created cells aren't empty, add to list of names
                    names.Add(cell);
                }
            }

            return names;
        }

        public override IList<string> SetCellContents(string name, double number)
        {
            // Creates a new cell if necessary, and set the contents of the cell to the given number
            if (cells.ContainsKey(name))
            {
                cells[name] = new Cell(number.ToString(), 1);
            }
            else
            {
                cells.Add(name, new Cell(number.ToString(), 1));
            }

            foreach (string d in graph.GetDependees(name))
            {
                graph.RemoveDependency(d, name);
            }

            // Returns the cells that need to be recalculated (itself and all of the cell's dependents)
            return GetCellsToRecalculate(name).ToList();
        }

        public override IList<string> SetCellContents(string name, string text)
        {
            // Creates a new cell if necessary, and set the contents of the cell to the given string
            if (cells.ContainsKey(name))
            {
                cells[name] = new Cell(text, -1);
            }
            else
            {
                cells.Add(name, new Cell(text, -1));
            }

            foreach (string d in graph.GetDependees(name))
            {
                graph.RemoveDependency(d, name);
            }

            // Returns the cells that need to be recalculated (itself and all of the cell's dependents)
            return GetCellsToRecalculate(name).ToList();
        }

        public override IList<string> SetCellContents(string name, Formula formula)
        {
            // Creates a new cell if necessary, and set the contents of the cell to the given Formula
            if (cells.ContainsKey(name))
            {
                cells[name] = new Cell(formula.ToString(), 0);
            }
            else
            {
                cells.Add(name, new Cell(formula.ToString(), 0));
            }

            foreach (string d in graph.GetDependees(name))
            {
                graph.RemoveDependency(d, name);
            }

            Formula cell = (Formula)cells[name].Contents;
            foreach (string var in cell.GetVariables())
            {
                // Connects the cell to any cells it references within the formula
                graph.AddDependency(var, name);
            }

            // Returns the cells that need to be recalculated (itself and all of the cell's dependents)
            return GetCellsToRecalculate(name).ToList();
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return graph.GetDependents(name).ToList();
        }

        
        /// <summary>
        /// A cell that contains a double/string/Formula.
        /// 
        /// Keeps track of elements that are connected to any other cells
        /// </summary>
        private class Cell
        {
            public object Contents { get; private set; }

            /// <summary>
            /// Creates a new cell that can hold a double/string/Formula.
            /// </summary>
            public Cell(string content, int type)
            {
                switch (type)
                {
                    case 0:
                        // If creating a cell with a Formula
                        Contents = new Formula(content);
                        break;
                    case 1:
                        // If creating a cell with a double
                        Contents = double.Parse(content);
                        break;
                    default:
                        // If creating a cell with a string
                        Contents = content;
                        break;
                }
            }
        }
    }
}