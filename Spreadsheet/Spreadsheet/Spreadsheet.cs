using SpreadsheetUtilities;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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
        public Dictionary<string, Cell> Cells { get; private set; } // A dictionary of all the cells within the spreadsheet
        private readonly DependencyGraph graph; // A dependency graph of containing information for what cells connect to what cells
        private readonly Func<string, bool> validifier; // The validator function for checking name validity
        private readonly Func<string, string> normalizer;   // The normalizer function for normalizing the variable names

        /// <summary>
        /// Creates a new Spreadsheet that can hold any number of cells
        /// </summary>
        public Spreadsheet() : base("default")
        {
            Cells = new Dictionary<string, Cell>();
            graph = new DependencyGraph();
            validifier = s => true;
            normalizer = s => s;
        }

        /// <summary>
        /// Creates a new spreadsheet with the given validity function, normalizer function, and the version
        /// </summary>
        public Spreadsheet(Func<string, bool> validify, Func<string, string> normalize, string version) : base(version)
        {
            Cells = new Dictionary<string, Cell>();
            graph = new DependencyGraph();
            validifier = validify;
            normalizer = normalize;
        }

        /// <summary>
        /// Reads a saved spreadsheet from the file and uses it to construct a new spreadsheet.
        /// 
        /// The new spreadsheet uses the provided validity delegate, normalization delegate, and version
        /// </summary>
        public Spreadsheet(string filePath, Func<string, bool> validify, Func<string, string> normalize, string version) : base(version)
        {
            Spreadsheet? s;
            try
            {
                // Deserializes a given JSON file
                s = JsonSerializer.Deserialize<Spreadsheet>(File.ReadAllText(filePath));

                if (s is not null && s.Version != version)
                {
                    throw new Exception();  // If the given version doesn't match the file version, go to catch
                }

                if (s is not null)
                {
                    Cells = s.Cells;
                    graph = s.graph;
                }
                else
                {
                    Cells = new Dictionary<string, Cell>();
                    graph = new DependencyGraph();
                }

                validifier = validify;
                normalizer = normalize;

                foreach (var cell in Cells)
                {
                    // Recreates the spreadsheet from the deserialized file
                    SetContentsOfCell(cell.Key, cell.Value.StringForm);
                }
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Check if the file location is valid. Check if the given version is the same as the file version. If neither of these, the file might not be in JSON format");
            }
        }

        /// <summary>
        /// Constructs a spreadsheet from a deserialized JSON file
        /// </summary>
        [JsonConstructor]
        public Spreadsheet(Dictionary<string, Cell> cells, string version) : base(version)
        {
            Cells = cells;
            graph = new DependencyGraph();

            validifier = s => true;
            normalizer = s => s;
        }

        public override object GetCellContents(string name)
        {
            name = normalizer(name);

            if (CheckNameValidity(name))
            {
                if (Cells.ContainsKey(name))
                {
                    return Cells[name].Contents;
                }

                return "";
            }

            throw new InvalidNameException();
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            HashSet<string> names = new HashSet<string>();

            foreach (string cell in Cells.Keys)
            {
                if (Cells[cell].Contents.ToString() != "")
                {
                    // If the created cells aren't empty, add to list of names
                    names.Add(cell);
                }
            }

            return names;
        }

        public override object GetCellValue(string name)
        {
            name = normalizer(name);

            if (CheckNameValidity(name))
            {
                if (Cells.ContainsKey(name))
                {
                    return Cells[name].Value;
                }

                return "";
            }

            throw new InvalidNameException();
        }

        public override IList<string> SetContentsOfCell(string name, string content)
        {
            if (CheckNameValidity(name))
            {
                List<string> list = new();
                Changed = true;

                if (double.TryParse(content, out var value))
                {
                    list = SetCellContents(normalizer(name), value).ToList();
                }
                else
                {
                    if (content.StartsWith("="))
                    {
                        list = SetCellContents(normalizer(name), new Formula(content.Substring(1), normalizer, validifier)).ToList();
                    }
                    else
                    {
                        list = SetCellContents(normalizer(name), content).ToList();
                    }
                }

                Cells[normalizer(name)].StringForm = content;

                foreach (string s in list)
                {
                    RecalculateValue(s);
                }

                return list;
            }

            throw new InvalidNameException();
        }

        protected override IList<string> SetCellContents(string name, double number)
        {
            // Creates a new cell if necessary, and set the contents of the cell to the given number
            if (Cells.ContainsKey(name))
            {
                Cells[name] = new Cell(number.ToString(), 1);
            }
            else
            {
                Cells.Add(name, new Cell(number.ToString(), 1));
            }

            // Removes any previous dependencies between the given cell and its dependees
            foreach (string d in graph.GetDependees(name))
            {
                graph.RemoveDependency(d, name);
            }

            // Returns the cells that need to be recalculated (itself and all of the cell's dependents)
            return GetCellsToRecalculate(name).ToList();
        }

        protected override IList<string> SetCellContents(string name, string text)
        {
            // Creates a new cell if necessary, and set the contents of the cell to the given string
            if (Cells.ContainsKey(name))
            {
                Cells[name] = new Cell(text, -1);
            }
            else
            {
                Cells.Add(name, new Cell(text, -1));
            }

            // Removes any previous dependencies between the given cell and its dependees
            foreach (string d in graph.GetDependees(name))
            {
                graph.RemoveDependency(d, name);
            }

            // Returns the cells that need to be recalculated (itself and all of the cell's dependents)
            return GetCellsToRecalculate(name).ToList();
        }

        protected override IList<string> SetCellContents(string name, Formula formula)
        {
            object prevContent = "";

            // Creates a new cell if necessary, and set the contents of the cell to the given Formula
            if (Cells.ContainsKey(name))
            {
                prevContent = Cells[name].Contents;
                Cells[name] = new Cell(formula.ToString(), 0);
            }
            else
            {
                Cells.Add(name, new Cell(formula.ToString(), 0));
            }

            // Removes any previous dependencies between the given cell and its dependees
            foreach (string d in graph.GetDependees(name))
            {
                graph.RemoveDependency(d, name);
            }

            Formula cell = (Formula)Cells[name].Contents;
            foreach (string var in cell.GetVariables())
            {
                // Connects the cell to any cells it references within the formula
                graph.AddDependency(var, name);
            }
            List<string> list = new List<string>();

            // If no circular exception thrown, continue. Otherwise clear the list and reverse the setting of cell's content
            try
            {
                list = GetCellsToRecalculate(name).ToList();
            }
            catch (CircularException e)
            {
                list.Clear();
                Cells[name].Contents = prevContent;
                throw e;
            }

            // Returns the cells that need to be recalculated (itself and all of the cell's dependents)
            return list;
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            return graph.GetDependents(name).ToList();
        }

        /// <summary>
        /// Checks if the passed in name is a valid name
        /// </summary>
        private bool CheckNameValidity(string name)
        {
            string legalVarNames = @"^[a-zA-Z_][a-zA-Z0-9_]*$";

            if (Regex.IsMatch(name, legalVarNames) && validifier(name))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Recalculates and re-stores the value of a cell back into the cell
        /// </summary>
        private void RecalculateValue(string name)
        {
            if (Cells[name].Contents is Formula content)
            {
                Cells[name].Value = content.Evaluate(s => (double)GetCellValue(s));
            }
            else
            {
                Cells[name].Value = Cells[name].Contents;
            }
        }

        public override void Save(string filename)
        {
            try
            {
                File.WriteAllText(filename, JsonSerializer.Serialize(this));
                Changed = false;
            }
            catch
            {
                throw new SpreadsheetReadWriteException("Invalid file location");
            }
        }


        /// <summary>
        /// A cell that contains a double/string/Formula.
        /// 
        /// Keeps track of elements that are connected to any other cells
        /// </summary>
        ///
        public class Cell
        {
            [JsonIgnore] public object Contents { get; set; }
            [JsonInclude] public string StringForm;
            [JsonIgnore] public object Value { get; set; }

            /// <summary>
            /// Creates a new cell that can hold a double/string/Formula.
            /// </summary>
            public Cell(string content, int type)
            {
                Value = "";
                StringForm = "";

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

            /// <summary>
            /// Recreates a cell when deserializing JSON file. (Doesn't need to set contents and value because it will set all cells when finishing deserializing)
            /// </summary>
            [JsonConstructor]
            public Cell(string stringForm)
            {
                this.StringForm = stringForm;
                Contents = "";
                Value = "";
            }
        }
    }
}