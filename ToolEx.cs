using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ToolEx
{
    public static class Tools
    {
        static public ObjectId PostToModelSpace(this Database db, Entity ent)
        {
            if (ent == null) return ObjectId.Null;
            ObjectId entId = ObjectId.Null;
            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                entId = btr.AppendEntity(ent);
                trans.AddNewlyCreatedDBObject(ent, true);
                trans.Commit();
            }
            return entId;
        }

        public static bool Transform(this ObjectId id, Matrix3d xform)
        {
            Entity ent = id.GetObject(OpenMode.ForWrite) as Entity;
            if (ent != null)
            {
                ent.TransformBy(xform);
                ent.DowngradeOpen();
                return true;
            }
            return false;
        }

        public static bool Move(this ObjectId id, Vector3d vec) { return Transform(id, Matrix3d.Displacement(vec)); }

        public static bool Move(this ObjectId id, Point3d source, Point3d target) {  return Move(id, target - source); }

        public static bool Rotate(this ObjectId id, Point3d basePt, double angle) { return Transform(id, Matrix3d.Rotation(angle, Vector3d.ZAxis, basePt)); }

        public static bool Scale(this ObjectId id, Point3d basePt, double scale) {  return Transform(id, Matrix3d.Scaling(scale, basePt)); }

        public static bool Mirror(this ObjectId id, Line3d line) {  return Transform(id, Matrix3d.Mirroring(line));  }

        public static bool Mirror(this ObjectId id, Point3d p1, Point3d p2) { return Mirror(id, new Line3d(p1, p2)); }

        public static bool Mirror(this ObjectId id, Point3d basePoint) { return Transform(id, Matrix3d.Mirroring(basePoint));  }



    }
}
