using HT.Framework;

using System;
using UnityEngine;

public class LitJsonRegiter
{
    public static void Register()
    {
        RegiterType();
        RegisterFloat();
        RegisterVector3();
        RegisterVector2();
        RegisterQuaternion();
        RegisterGameObject();
    }

    public static void RegiterType()
    {
        void Exporter(Type obj, JsonWriter writer)
        {
            writer.Write(obj.FullName);
        }

        JsonMapper.RegisterExporter((ExporterFunc<Type>)Exporter);

        Type Importer(string obj)
        {
            return Type.GetType(obj);
        }

        JsonMapper.RegisterImporter((ImporterFunc<string, Type>)Importer);
    }

    private static void RegisterFloat()
    {
        void Exporter(float obj, JsonWriter writer)
        {
            writer.Write(obj);
        }

        JsonMapper.RegisterExporter((ExporterFunc<float>)Exporter);

        float Importer(double obj)
        {
            return (float)obj;
        }

        JsonMapper.RegisterImporter((ImporterFunc<double, float>)Importer);
    }

    private static void RegisterVector3()
    {
        void Exporter(Vector3 obj, JsonWriter writer)
        {
            writer.WriteObjectStart();

            writer.WritePropertyName("x");//写入属性名
            writer.Write(obj.x);//写入值
            writer.WritePropertyName("y");
            writer.Write(obj.y);
            writer.WritePropertyName("z");
            writer.Write(obj.z);

            writer.WriteObjectEnd();
        }

        JsonMapper.RegisterExporter((ExporterFunc<Vector3>)Exporter);//序列化
    }

    private static void RegisterVector2()
    {
        void Exporter(Vector2 obj, JsonWriter writer)
        {
            writer.WriteObjectStart();

            writer.WritePropertyName("x");//写入属性名
            writer.Write(obj.x);//写入值
            writer.WritePropertyName("y");
            writer.Write(obj.y);

            writer.WriteObjectEnd();
        }

        JsonMapper.RegisterExporter((ExporterFunc<Vector2>)Exporter);//序列化
    }

    private static void RegisterQuaternion()
    {
        void Exporter(Quaternion obj, JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("x");//写入属性名
            writer.Write(obj.x);//写入值
            writer.WritePropertyName("y");
            writer.Write(obj.y);
            writer.WritePropertyName("z");
            writer.Write(obj.z);
            writer.WritePropertyName("w");
            writer.Write(obj.w);
            writer.WriteObjectEnd();
        }
        JsonMapper.RegisterExporter((ExporterFunc<Quaternion>)Exporter);//序列化
    }

    private static void RegisterGameObject()
    {
        void Exporter(GameObject obj, JsonWriter writer)
        {
            writer.WriteObjectStart();
            writer.WritePropertyName("name");//写入属性名
            if (obj != null)
                writer.Write(obj.name);//写入值
            else
                writer.Write("Prefab为Null");//写入值
            writer.WriteObjectEnd();
        }
        JsonMapper.RegisterExporter((ExporterFunc<GameObject>)Exporter);//序列化
    }
}
