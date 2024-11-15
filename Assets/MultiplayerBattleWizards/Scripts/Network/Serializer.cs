using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

/// <summary>
/// Converts an object to a byte array and vice versa.
/// Used when needing to send a custom class by a RPC.
/// </summary>
public class Serializer
{
    // Convert an object to a byte array.
    public static byte[] Serialize (object obj)
    {
        if(obj == null) return null;

        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();

        bf.Serialize(ms, obj);

        return ms.ToArray();
    }

    // Convert a byte array to an object.
    public static object Deserialize (byte[] byteArray)
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();

        ms.Write(byteArray, 0, byteArray.Length);
        ms.Seek(0, SeekOrigin.Begin);

        return (object)bf.Deserialize(ms);
    }
}
