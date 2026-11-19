using System;

namespace Mossmallow.Systems.Saving
{
    /// <summary>Base class for data stored by <see cref="VersionedJsonStore{T}"/>. Subclasses add [Serializable] fields.</summary>
    [Serializable]
    public abstract class SaveData
    {
        public int version;
    }
}
