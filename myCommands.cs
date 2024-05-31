// (C) Copyright 2024 by Home 
//
using App = Autodesk.AutoCAD.ApplicationServices ;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using System;
using System.Windows;
using ToolEx;
using Autodesk.AutoCAD.ApplicationServices;


// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ArxDotNet.CommondDemo))]


namespace ArxDotNet
{

    public class CommondDemo
    {
        /// <summary>
        /// 将实体添加到模型空间
        /// </summary>
        /// <param name="db"></param>
        /// <param name="ent"></param>
        /// <returns></returns>
        [CommandMethod("TaoNet", nameof(ChangeColor), CommandFlags.Modal)]
        public void ChangeColor() // This method can have any name
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = App.Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                ObjectId id = ed.GetEntity("\n 请选择需要更改颜色得对象").ObjectId;

                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    Entity ent = trans.GetObject(id, OpenMode.ForWrite) as Entity;

                    if (ent != null)
                    {
                        ent.ColorIndex = 6;
                    }

                    trans.Commit();

                }

            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                switch (ex.ErrorStatus)
                {
                    case ErrorStatus.InvalidIndex:
                        ed.WriteMessage("\n 输入颜色异常！");
                        break;
                    case ErrorStatus.InvalidObjectId:
                        ed.WriteMessage("\n 未选择对象！");
                        break;
                    default:
                        ed.WriteMessage(ex.ErrorStatus.ToString());
                        break;
                }
            }
        }

        // Modal Command with pickfirst selection
        [CommandMethod("MyGroup", "MyPickFirst", "MyPickFirstLocal", CommandFlags.Modal | CommandFlags.UsePickSet)]
        public void MyPickFirst() // This method can have any name
        {
            PromptSelectionResult result = App.Application.DocumentManager.MdiActiveDocument.Editor.GetSelection();
            if (result.Status == PromptStatus.OK)
            {
                Editor ed = App.Application.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage($"供选择 {0} 个实体", result.Value.Count);
            }
            else
            {
                Editor ed = App.Application.DocumentManager.MdiActiveDocument.Editor;
                ed.WriteMessage("未选择对象");
                MessageBox.Show("未选择对象!");
            }
        }

        [CommandMethod(nameof(AddLine))]
        public void AddLine()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Point3d ptStart = new Point3d(0, 0, 0);
            Point3d ptEnd = new Point3d(100, 100, 0);
            Line line = new Line(ptStart, ptEnd);
            ObjectId lineId = db.PostToModelSpace(line);

            db.DimAssoc = 2;
            AlignedDimension dimension = new AlignedDimension();
            dimension.XLine1Point = ptStart;
            dimension.XLine2Point = ptEnd;

            dimension.DimLinePoint = new Point3d(45, 55, 0);
            dimension.DimensionStyle = db.Dimstyle;
            dimension.OwnerId = lineId;
            db.PostToModelSpace(dimension);

        }

        [CommandMethod(nameof (SecondLine))]
        public void SecondLine()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Point3d ptStart = new Point3d(0, 100, 0);
            Point3d ptEnd = new Point3d(0, 200, 0);
            Line line = new Line(ptStart, ptEnd);
            db.PostToModelSpace(line);
        }

        [CommandMethod(nameof(Demo))]
        public void Demo()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = App.Application.DocumentManager.MdiActiveDocument.Editor;
            ObjectId id = ed.GetEntity("\n 请选择需要移动对象").ObjectId;

            if (id != null)
            {
                using (Transaction trans = db.TransactionManager.StartTransaction())
                {
                    id.Move(new Vector3d(100, 100, 0));
                    trans.Commit();
                }

            }
        }

        [CommandMethod(nameof(TransDemo))]
        public void TransDemo()
        {
            Document doc = App.Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Autodesk.AutoCAD.DatabaseServices.TransactionManager tm = db.TransactionManager;

            using (Transaction tran1 = tm.StartTransaction())
            {
                Point3d ptStart = Point3d.Origin;
                Point3d ptEnd = new Point3d(100, 0, 0);
                Line line = new Line(ptStart, ptEnd);
                ObjectId id = db.PostToModelSpace(line);

                using (Transaction tran2 = tm.StartTransaction())
                {
                    line.UpgradeOpen();
                    line.ColorIndex = 1;
                    ObjectId copyId = id.TransformCopy(Matrix3d.Rotation(Math.PI * 0.5, Vector3d.ZAxis, ptStart));

                    using (Transaction trans3 = tm.StartTransaction())
                    {
                        Line line2 = trans3.GetObject(copyId, OpenMode.ForWrite) as Line;

                        line2.ColorIndex = 3;

                        trans3.Abort();

                    }

                    tran2.Commit();
                }

                tran1.Commit();
            }

        }

    }

}
