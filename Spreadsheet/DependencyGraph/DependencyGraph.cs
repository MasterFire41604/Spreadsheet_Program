namespace SpreadsheetUtilities;

/// <summary>
/// (s1,t1) is an ordered pair of strings
/// t1 depends on s1; s1 must be evaluated before t1
/// 
/// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
/// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
/// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
/// set, and the element is already in the set, the set remains unchanged.
/// 
/// Given a DependencyGraph DG:
/// 
///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
///        (The set of things that depend on s)    
///        
///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
///        (The set of things that s depends on) 
//
// For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
//     dependents("a") = {"b", "c"}
//     dependents("b") = {"d"}
//     dependents("c") = {}
//     dependents("d") = {"d"}
//     dependees("a") = {}
//     dependees("b") = {"a"}
//     dependees("c") = {"a"}
//     dependees("d") = {"b", "d"}
/// </summary>
public class DependencyGraph
{
    private Dictionary<string, List<string>> dependents;
    private Dictionary<string, List<string>> dependees;
    private int totalDependencies;

    /// <summary>
    /// Creates an empty DependencyGraph.
    /// </summary>
    public DependencyGraph()
    {
        dependents = new Dictionary<string, List<string>>();
        dependees = new Dictionary<string, List<string>>();
        totalDependencies = 0;
    }


    /// <summary>
    /// The number of ordered pairs in the DependencyGraph.
    /// This is an example of a property.
    /// </summary>
    public int NumDependencies
    {
        get { return totalDependencies; }
    }


    /// <summary>
    /// Returns the size of dependees(s),
    /// that is, the number of things that s depends on.
    /// </summary>
    public int NumDependees(string s)
    {
        if (dependees.ContainsKey(s))
            return dependees[s].Count;
        else
            return 0;
    }


    /// <summary>
    /// Reports whether dependents(s) is non-empty.
    /// </summary>
    public bool HasDependents(string s)
    {
        if (dependents.ContainsKey(s))
            return dependents[s].Count > 0;
        else
            return false;
    }


    /// <summary>
    /// Reports whether dependees(s) is non-empty.
    /// </summary>
    public bool HasDependees(string s)
    {
        if (dependees.ContainsKey(s))
            return dependees[s].Count > 0;
        else
            return false;
    }


    /// <summary>
    /// Enumerates dependents(s).
    /// </summary>
    public IEnumerable<string> GetDependents(string s)
    {
        var result = new List<string>();

        if (dependents.ContainsKey(s))
            return dependents[s];
        else
            return result;
    }


    /// <summary>
    /// Enumerates dependees(s).
    /// </summary>
    public IEnumerable<string> GetDependees(string s)
    {
        var result = new List<string>();

        if (dependees.ContainsKey(s))
            return dependees[s];
        else
            return result;
    }


    /// <summary>
    /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
    /// 
    /// <para>This should be thought of as:</para>   
    /// 
    ///   t depends on s
    ///
    /// </summary>
    /// <param name="s"> s must be evaluated first. T depends on S</param>
    /// <param name="t"> t cannot be evaluated until s is</param>
    public void AddDependency(string s, string t)
    {
        List<string> list;

        if (!dependents.ContainsKey(s))
        {
            totalDependencies++;
            list = new List<string>() { t };
            dependents.Add(s, list);
        }
        else
        {
            list = dependents[s];
            if (!list.Contains(t))
            {
                totalDependencies++;
                list.Add(t);
                ReplaceDependents(s, list);
            }
        }

        if (!dependees.ContainsKey(t))
        {
            list = new List<string>() { s };
            dependees.Add(t, list);
        }
        else
        {
            list = dependees[t];
            if (!list.Contains(s))
            {
                list.Add(s);
                ReplaceDependees(t, list);
            }
        }
    }


    /// <summary>
    /// Removes the ordered pair (s,t), if it exists
    /// </summary>
    /// <param name="s"></param>
    /// <param name="t"></param>
    public void RemoveDependency(string s, string t)
    {
        List<string> list;

        if (dependents.ContainsKey(s))
        {
            list = dependents[s];

            if (list.Contains(t))
            {
                totalDependencies--;
                list.Remove(t);
                ReplaceDependents(s, list);
            }
        }

        if (dependees.ContainsKey(t))
        {
            list = dependees[t];

            if (list.Contains(s))
            {
                list.Remove(s);
                ReplaceDependees(t, list);
            }
        }
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (s,r).  Then, for each
    /// t in newDependents, adds the ordered pair (s,t).
    /// </summary>
    public void ReplaceDependents(string s, IEnumerable<string> newDependents)
    {
        dependents.Remove(s);
        dependents.Add(s, newDependents.ToList());

        foreach (string d in newDependents)
        {
            if (dependees.ContainsKey(d))
            {
                var list = dependees[d];
                if (!list.Contains(s))
                {
                    list.Add(s);
                    dependees.Remove(d);
                    dependees.Add(d, list);
                }
            }
            else
            {
                var list = new List<string>() { s };
                dependees.Add(d, list);
            }
        }
    }


    /// <summary>
    /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
    /// t in newDependees, adds the ordered pair (t,s).
    /// </summary>
    public void ReplaceDependees(string s, IEnumerable<string> newDependees)
    {
        dependees.Remove(s);
        dependees.Add(s, newDependees.ToList());

        foreach (string d in newDependees)
        {
            if (dependents.ContainsKey(d))
            {
                var list = dependents[d];
                if (!list.Contains(s))
                {
                    list.Add(s);
                    dependents.Remove(d);
                    dependents.Add(d, list);
                }
            }
            else
            {
                var list = new List<string>() { s };
                dependents.Add(d, list);
            }
        }
    }
}
