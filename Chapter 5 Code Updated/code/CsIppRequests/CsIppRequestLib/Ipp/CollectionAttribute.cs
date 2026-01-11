using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CsIppRequestLib
{

    /// <summary>
    /// MemberAttribute
    /// 
    /// 
    /// In the Collection, each comma (',') seperates a member attribute. Each member attribute can be a single
    /// attribute or can itself host a collection of members.
    /// </summary>
    public class MemberAttribute : IEnumerable<Member>
    {
        public string Name { get; set; }
        public object Value { get; set; } = null;
        public byte ValueTag; //begin collection
        public List<Member> Members { get; set; } = new List<Member>();   
        public bool IsCollection { get; set; }

        public IEnumerator<Member> GetEnumerator()
        {
            return Members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Member
    /// 
    /// This class is a member (2 in total) of a member attribute collection.
    /// </summary>
    public class Member
    {
        public string Name { get; set; } = string.Empty;

        public object Value { get; set; } = null;
     
        public byte ValueTag { get; set; }   
    }


    /// <summary>
    /// CollectionAttribute
    /// 
    /// This is a set of attributes that are comma deliniated. They form an IPP Attribute but are themselves
    /// a collection of like-grouped items.
    /// </summary>
    public class CollectionAttribute : IEnumerable<MemberAttribute>
    {
        public string Name { get; set; }
        public List<MemberAttribute> MemberAttributes { get; set; } = new List<MemberAttribute>();

        public IEnumerator<MemberAttribute> GetEnumerator()
        {
            return MemberAttributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class CollectionItem
    {
        public string CollectionName { get; set; }
        public List<(string Key, string Value)> Items { get; set; } = new List<(string Key, string Value)>();
    }
}
