namespace RegexReadData
{
    using System;

    /// <summary>
    /// Represents a user in the chat system.
    /// </summary>
    public sealed class User : IEquatable<User>
    {
        #region Immutables

        readonly string id;
        readonly string name;

        #endregion
        #region Properties

        /// <summary>
        /// Gets the unique identifier of the user.
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Gets the name of the user.
        /// </summary>
        public string Name { get { return name; } }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="User"/> class.
        /// </summary>
        /// <param name="id">
        /// The unique identifier for the user.
        /// </param>
        /// <param name="name">
        /// The name of the user.
        /// </param>
        public User(string id, string name)
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
        public bool Equals(User other)
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
            return this.Equals(obj as User);
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
