using SpreadsheetUtilities;

namespace DevelopmentTests;

/// <summary>
///This is a test class for DependencyGraphTest and is intended
///to contain all DependencyGraphTest Unit Tests (once completed by the student)
///</summary>
[TestClass()]
public class DependencyGraphTests
{
    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void SimpleEmptyTest()
    {
        DependencyGraph t = new DependencyGraph();
        Assert.AreEqual(0, t.NumDependencies);
    }


    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void SimpleEmptyRemoveTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        Assert.AreEqual(1, t.NumDependencies);
        t.RemoveDependency("x", "y");
        Assert.AreEqual(0, t.NumDependencies);
    }

    /// <summary>
    /// Should treat empty strings as variables as any other variable
    /// </summary>
    [TestMethod()]
    public void AddEmptyStrings()
    {
        DependencyGraph t = new();
        t.AddDependency("", "");
        Assert.AreEqual(1, t.NumDependencies);
    }


    /// <summary>
    ///Empty graph should contain nothing
    ///</summary>
    [TestMethod()]
    public void EmptyEnumeratorTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        IEnumerator<string> e1 = t.GetDependees("y").GetEnumerator();
        Assert.IsTrue(e1.MoveNext());
        Assert.AreEqual("x", e1.Current);
        IEnumerator<string> e2 = t.GetDependents("x").GetEnumerator();
        Assert.IsTrue(e2.MoveNext());
        Assert.AreEqual("y", e2.Current);
        t.RemoveDependency("x", "y");
        Assert.IsFalse(t.GetDependees("y").GetEnumerator().MoveNext());
        Assert.IsFalse(t.GetDependents("x").GetEnumerator().MoveNext());
    }


    /// <summary>
    /// Simply checks if each letter correctly assigns the dependents/dependees of that letter
    /// </summary>
    [TestMethod()]
    public void SimpleCheck()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("c", "b");
        t.AddDependency("b", "d");

        Assert.IsTrue(t.HasDependents("a"));
        Assert.IsFalse(t.HasDependees("a"));

        Assert.IsTrue(t.HasDependents("b"));
        Assert.IsTrue(t.HasDependees("b"));

        Assert.IsTrue(t.HasDependents("c"));
        Assert.IsTrue(t.HasDependees("c"));

        Assert.IsFalse(t.HasDependents("d"));
        Assert.IsTrue(t.HasDependees("d"));
    }


    /// <summary>
    ///Replace on an empty DG shouldn't fail
    ///</summary>
    [TestMethod()]
    public void SimpleReplaceTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "y");
        Assert.AreEqual(1, t.NumDependencies);
        t.RemoveDependency("x", "y");
        t.ReplaceDependents("x", new HashSet<string>());
        t.ReplaceDependees("y", new HashSet<string>());
    }

    /// <summary>
    /// Replaces dependents of a variable with a different number of dependents than there was previously, check for number of dependencies
    /// </summary>
    [TestMethod()]
    public void ReplaceUpdatesDepenciesNum()
    {
        DependencyGraph t = new();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("a", "d");
        t.AddDependency("b", "d");

        t.ReplaceDependents("a", new List<string>() { "z", "y", "x", "w", "v"});
        Assert.AreEqual(6, t.NumDependencies);
    }


    ///<summary>
    ///It should be possibe to have more than one DG at a time.
    ///</summary>
    [TestMethod()]
    public void StaticTest()
    {
        DependencyGraph t1 = new DependencyGraph();
        DependencyGraph t2 = new DependencyGraph();
        t1.AddDependency("x", "y");
        Assert.AreEqual(1, t1.NumDependencies);
        Assert.AreEqual(0, t2.NumDependencies);
    }




    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void SizeTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("c", "b");
        t.AddDependency("b", "d");
        Assert.AreEqual(4, t.NumDependencies);
    }


    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void EnumeratorTest()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("c", "b");
        t.AddDependency("b", "d");

        IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
        Assert.IsFalse(e.MoveNext());

        // This is one of several ways of testing whether your IEnumerable
        // contains the right values. This does not require any particular
        // ordering of the elements returned.
        e = t.GetDependees("b").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

        e = t.GetDependees("c").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("a", e.Current);
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("d").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("b", e.Current);
        Assert.IsFalse(e.MoveNext());
    }


    /// <summary>
    ///Non-empty graph contains something
    ///</summary>
    [TestMethod()]
    public void ReplaceThenEnumerate()
    {
        DependencyGraph t = new DependencyGraph();
        t.AddDependency("x", "b");
        t.AddDependency("a", "z");
        t.ReplaceDependents("b", new HashSet<string>());
        t.AddDependency("y", "b");
        t.ReplaceDependents("a", new HashSet<string>() { "c" });
        t.AddDependency("w", "d");
        t.ReplaceDependees("b", new HashSet<string>() { "a", "c" });
        t.ReplaceDependees("d", new HashSet<string>() { "b" });
        
        IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("b").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));


        e = t.GetDependees("c").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("a", e.Current);
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("d").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("b", e.Current);
        Assert.IsFalse(e.MoveNext());
    }


    /// <summary>
    /// Removes from a an empty graph, then a non-empty graph
    /// </summary>
    [TestMethod()]
    public void RemoveTest()
    {
        DependencyGraph t = new DependencyGraph();

        t.RemoveDependency("a", "b");

        t.AddDependency("a", "b");
        t.AddDependency("a", "b");
        t.AddDependency("a", "c");
        t.AddDependency("c", "b");
        t.AddDependency("b", "d");

        t.RemoveDependency("a", "c");
        Assert.AreEqual(3, t.NumDependencies);

        IEnumerator<string> e = t.GetDependees("c").GetEnumerator();
        Assert.IsFalse(e.MoveNext());
        Assert.AreEqual(null, e.Current);

        e = t.GetDependents("c").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("b", e.Current);
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependents("a").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("b", e.Current);
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependents("b").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        Assert.AreEqual("d", e.Current);
        Assert.IsFalse(e.MoveNext());

        e = t.GetDependees("b").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        String s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        String s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

        t.AddDependency("a", "d");
        t.AddDependency("c", "a");
        t.AddDependency("d", "a");

        e = t.GetDependents("c").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "a") && (s2 == "b")) || ((s1 == "b") && (s2 == "a")));

        e = t.GetDependees("a").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "d") && (s2 == "c")) || ((s1 == "c") && (s2 == "d")));

        e = t.GetDependents("a").GetEnumerator();
        Assert.IsTrue(e.MoveNext());
        s1 = e.Current;
        Assert.IsTrue(e.MoveNext());
        s2 = e.Current;
        Assert.IsFalse(e.MoveNext());
        Assert.IsTrue(((s1 == "b") && (s2 == "d")) || ((s1 == "d") && (s2 == "b")));
    }


    /// <summary>
    ///Using lots of data
    ///</summary>
    [TestMethod()]
    public void StressTest()
    {
        // Dependency graph
        DependencyGraph t = new DependencyGraph();

        // A bunch of strings to use
        const int SIZE = 200;
        string[] letters = new string[SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            letters[i] = ("" + (char)('a' + i));
        }

        // The correct answers
        HashSet<string>[] dents = new HashSet<string>[SIZE];
        HashSet<string>[] dees = new HashSet<string>[SIZE];
        for (int i = 0; i < SIZE; i++)
        {
            dents[i] = new HashSet<string>();
            dees[i] = new HashSet<string>();
        }

        // Add a bunch of dependencies
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = i + 1; j < SIZE; j++)
            {
                t.AddDependency(letters[i], letters[j]);
                dents[i].Add(letters[j]);
                dees[j].Add(letters[i]);
            }
        }

        // Remove a bunch of dependencies
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = i + 4; j < SIZE; j += 4)
            {
                t.RemoveDependency(letters[i], letters[j]);
                dents[i].Remove(letters[j]);
                dees[j].Remove(letters[i]);
            }
        }

        // Add some back
        for (int i = 0; i < SIZE; i++)
        {
            for (int j = i + 1; j < SIZE; j += 2)
            {
                t.AddDependency(letters[i], letters[j]);
                dents[i].Add(letters[j]);
                dees[j].Add(letters[i]);
            }
        }

        // Remove some more
        for (int i = 0; i < SIZE; i += 2)
        {
            for (int j = i + 3; j < SIZE; j += 3)
            {
                t.RemoveDependency(letters[i], letters[j]);
                dents[i].Remove(letters[j]);
                dees[j].Remove(letters[i]);
            }
        }

        // Make sure everything is right
        for (int i = 0; i < SIZE; i++)
        {
            Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
            Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));

            Assert.AreEqual(dees[i].Count, t.NumDependees(letters[i]));
        }
    }
}