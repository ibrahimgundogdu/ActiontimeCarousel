using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ActionForce.Office
{
    public class ExcelExpenseDocument
    {
        public int MasrafMerkezi { get; set; }
        public short MasrafGrubu { get; set; }
        public short MasrafKalemi { get; set; }
        public float DagitimTutari { get; set; }
        public float KDVOrani { get; set; }
        public string DagitimGrubu { get; set; }
    }
}