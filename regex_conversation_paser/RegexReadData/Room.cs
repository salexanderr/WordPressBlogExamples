using System;

namespace RegexReadData
{
    /// <summary>
    /// Represents the definition of a room.
    /// </summary>
    public sealed class Room : IEquatable<Room>
    {
        #region Immutables

        readonly string id;
        readonly string name;

        #endregion
        #region Properties

        /// <summary>
        /// Gets the unique identifier of the room.
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Gets the name of the room.
        /// </summary>
        public string Name { get { return name; } }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Room"/> class.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the room.
        /// </param>
        /// <param name="name">
        /// The name of the room.
        /// </param>
        internal Room(string id, string name)
        {
            this.id = id;
            this.name = name;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Room other)
        {
            return other != null && other.Id == this.Id && other.Name == this.Name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj as Room);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
