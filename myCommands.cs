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
// This line is not mandatory, but improves loading performances
[assembly: CommandClass(typeof(ArxDotNet.CommondDemo))]


namespace ArxDotNet
{

    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!

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
            db.PostToModelSpace(line);
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

    }

}
