namespace System {
#if WINDOWS_PHONE
    /// <summary>
    /// This is a placeholder sealed class used in .NETCF because serializable
    /// is not available and we don't want to wrap all [Serializable]
    /// in #if WINDOWS.
    /// </summary>
    public sealed class SerializableAttribute : Attribute
    {
    }

    public sealed class NonSerializedAttribute: Attribute
    {
    }
#endif
}
