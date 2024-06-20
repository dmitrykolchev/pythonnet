using System;

using static Python.Runtime.Runtime;

namespace Python.Runtime;

public class PyBytes : PyObject
{
    internal PyBytes(in StolenReference reference) : base(reference)
    {
        if (!IsBytesType(Reference))
        {
            throw new ArgumentException("object is not a bytes");
        }
    }

    internal PyBytes(BorrowedReference reference) : base(new NewReference(reference).Steal())
    {
    }

    public PyBytes(PyObject o) : base(FromObject(o))
    {
    }

    private static BorrowedReference FromObject(PyObject o)
    {
        if (o is null) throw new ArgumentNullException(nameof(o));
        if (!IsBytesType(o))
        {
            throw new ArgumentException("object is not an int");
        }
        return o.Reference;
    }

    internal static bool IsBytesType(BorrowedReference reference)
    {
        if (reference == null)
        {
            return false;
        }
        BorrowedReference type = Runtime.PyObject_TYPE(reference);
        return Runtime.PyType_IsSubtype(type, Runtime.PyBytesType);
    }

    public int Size
    {
        get
        {
            return (int)Runtime.PyBytes_Size(Reference);
        }
    }

    public byte[] ToArray()
    {
        ReadOnlySpan<byte> result = Runtime.PyBytes_AsStringAndSize(Reference);
        byte[] array = new byte[result.Length];
        result.CopyTo(array);
        return array;
    }

    public static unsafe PyBytes FromArray(byte[] array)
    {
        using NewReference result = Runtime.PyBytes_FromStringAndSize(array);
        return new PyBytes(result.StealOrThrow());
    }

    public unsafe int Read(Span<byte> bytes, int offset)
    {
        ReadOnlySpan<byte> result = Runtime.PyBytes_AsStringAndSize(Reference);
        if (offset >= result.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(offset));
        }
        int sourceRest = result.Length - offset;
        fixed (byte* dest = bytes)
        {
            int copySize = Math.Min(bytes.Length, sourceRest);
            result.Slice(offset, copySize).CopyTo(new Span<byte>(dest, copySize));
            return copySize;
        }
    }
}
