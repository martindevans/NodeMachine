using System;
using System.Collections.Generic;

namespace NodeMachine.Model
{
    public class ProceduralNode
    {
        public Guid Guid { get; set; }

        public string TypeName { get; set; }

        public SubdivisionState State { get; set; }

        public BoundingBox Bounds { get; set; }

        public List<PropertyValue> HierarchicalProperties { get; set; }

        public List<ProceduralNode> Children { get; set; }

        public List<PropertyValue> Metadata { get; set; }
    }

    public enum SubdivisionState
    {   
        NotSubdivided,
        Subdivided,
        Faulted
    }
}
