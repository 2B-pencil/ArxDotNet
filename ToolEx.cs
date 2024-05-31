using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows.Data;
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
        public static ObjectId PostToModelSpace(this Database db, Entity ent)
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

        public static ObjectIdCollection PostToModelSpace(this Database db, params Entity[] ents)
        {
            if (ents == null || ents.Length == 0) return new ObjectIdCollection();
            ObjectIdCollection ids = new ObjectIdCollection();
            using (Transaction tran = db.TransactionManager.StartTransaction())
            {
                BlockTable bt = tran.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                BlockTableRecord btr = tran.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                foreach (Entity ent in ents)
                {
                    ObjectId id = btr.AppendEntity(ent);
                    tran.AddNewlyCreatedDBObject(ent, true);
                    ids.Add(id);
                }
                tran.Commit();
            }
            return ids;
        }

        public static bool Transform(this ObjectId id, Matrix3d xform)
        {
            if (id.IsNull) return false;
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

        public static ObjectId Mirror(this ObjectId id, Line3d line, bool eraseSource = false)
        { 
            if (eraseSource)
            {
                Transform(id, Matrix3d.Mirroring(line));
                return id;
            }
            else
            {
                return TransformCopy(id, Matrix3d.Mirroring(line));
            }
        }

        public static ObjectId Mirror(this ObjectId id, Point3d p1, Point3d p2, bool eraseSource = false) { return Mirror(id, new Line3d(p1, p2), eraseSource); }

        public static ObjectId Mirror(this ObjectId id, Point3d basePoint, bool eraseSource = false) 
        { 
            if (eraseSource)
            {
                Transform(id, Matrix3d.Mirroring(new Plane(basePoint, Vector3d.ZAxis)));
                return id;
            }
            else
            {
                return TransformCopy(id, Matrix3d.Mirroring(new Plane(basePoint, Vector3d.ZAxis)));
            }
        }

        public static ObjectId TransformCopy(this ObjectId id, Matrix3d xform)
        {
            if (id.IsNull) return ObjectId.Null;
            Entity ent = id.GetObject(OpenMode.ForRead) as Entity;
            if (ent != null)
            {
                Entity entCopy = ent.GetTransformedCopy(xform);
                return ent.Database.PostToModelSpace(entCopy);
            }
            return ObjectId.Null;
        }

        public static ObjectIdCollection Offset(this ObjectId id, double distance)
        {
            if (!id.IsNull) return new ObjectIdCollection();

            Curve curve = id.GetObject(OpenMode.ForRead) as Curve;
            if (curve == null) return new ObjectIdCollection();

            DBObjectCollection offsetCurves = curve.GetOffsetCurves(distance);
            if (offsetCurves == null || offsetCurves.Count == 0) return new ObjectIdCollection();

            Entity[] offsetEnts = new Entity[offsetCurves.Count];
            offsetCurves.CopyTo(offsetEnts, 0);

            return id.Database.PostToModelSpace(offsetEnts);
        }


    }
}
