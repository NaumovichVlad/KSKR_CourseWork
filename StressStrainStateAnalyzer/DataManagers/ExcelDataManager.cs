using IronXL;
using StressStrainStateAnalyzer.FiniteElements;
using StressStrainStateAnalyzer.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StressStrainStateAnalyzer.DataManagers
{
    public class ExcelDataManager : IDataManager
    {
        private WorkBook _wb;
        private WorkSheet _ws;

        public ExcelDataManager()
        {
            _wb = new WorkBook();
            _ws = _wb.DefaultWorkSheet;
        }

        public List<IFiniteElement> GetElements(string path, List<INode> nodes)
        {
            var elements = new List<IFiniteElement>();
            OpenFile(path);
            var i = 2;
            while (!_ws[$"A{i}"].IsEmpty)
            {
                var first = nodes.Find(n => n.Index == (_ws[$"B{i}"].IntValue - 1));
                var second = nodes.Find(n => n.Index == (_ws[$"C{i}"].IntValue - 1));
                var third = nodes.Find(n => n.Index == (_ws[$"D{i}"].IntValue - 1));
                if(first != null && second != null && third != null)
                    elements.Add(new TriangleFiniteElement(first, second, third));
                i++;
            }

            return elements;
        }

        public List<INode> GetNodes(string path)
        {
            var nodes = new List<INode>();
            OpenFile(path);
            var i = 2;
            while (!_ws[$"A{i}"].IsEmpty)
            {
                nodes.Add(new Node()
                {
                    Index = _ws[$"A{i}"].IntValue - 1,
                    X = _ws[$"B{i}"].DoubleValue,
                    Y = _ws[$"C{i}"].DoubleValue,
                });
                i++;
            }
            return nodes;
        }

        public List<INode> GetNodesWithStress(string path)
        {
            var nodes = new List<INode>();
            OpenFile(path);
            var i = 2;
            while (!_ws[$"A{i}"].IsEmpty)
            {
                nodes.Add(new Node()
                {
                    Index = _ws[$"A{i}"].IntValue - 1,
                    X = _ws[$"B{i}"].DoubleValue,
                    Y = _ws[$"C{i}"].DoubleValue,
                    Stress = _ws[$"E{i}"].DoubleValue
                });
                i++;
            }
            return nodes;
        }

        public void SaveReport(List<INode> nodes, List<INode> verifyNodes, string path)
        {
            _wb = WorkBook.Create(ExcelFileFormat.XLSX);
            _ws = _wb.CreateWorkSheet("Loss");
            _ws["A1"].Value = "Индекс";
            _ws["B1"].Value = "Координата X";
            _ws["C1"].Value = "Координата Y";
            _ws["D1"].Value = "Напряжение Ansys";
            _ws["E1"].Value = "Напряжение App";
            _ws["F1"].Value = "Loss";
            nodes = nodes.OrderBy(x => x.Index).ToList();
            verifyNodes = verifyNodes.OrderBy(x => x.Index).ToList();
            for (int i = 0; i < nodes.Count; i++)
            {
                _ws[$"A{i + 2}"].Value = nodes[i].Index;
                _ws[$"B{i + 2}"].Value = nodes[i].X;
                _ws[$"C{i + 2}"].Value = nodes[i].Y;
                _ws[$"D{i + 2}"].Value = verifyNodes[i].Stress;
                _ws[$"E{i + 2}"].Value = nodes[i].Stress;
                _ws[$"F{i + 2}"].Value = $"=ABS(D{i + 2} - E{i + 2}) / D{i + 2} * 100%";
            }
            _ws.SaveAs(path);
        }

        private void OpenFile(string path, int sheetNumber = 0)
        {
            _wb = WorkBook.Load(path);
            _ws = _wb.WorkSheets.First();
        } 
    }
}
