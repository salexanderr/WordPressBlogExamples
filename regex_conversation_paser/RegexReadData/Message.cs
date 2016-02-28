namespace RegexReadData
{
    using System;

    /// <summary>
    /// Represents a message in the chat system.
    /// </summary>
    public sealed class Message : IEquatable<Message>
    {
        #region Immutables

        readonly User sender;
        readonly DateTimeOffset sent;
        readonly string content;

        #endregion
        #region Properties

        /// <summary>
        /// Gets the sender of the message.
        /// </summary>
        public User Sender { get { return sender; } }

        /// <summary>
        /// Gets the time at which the message was sent.
        /// </summary>
        public DateTimeOffset Sent { get { return sent; } }

        /// <summary>
        /// Gets the message content.
        /// </summary>
        public string Content { get { return content; } }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Message"/> class.
        /// </summary>
        /// <param name="sender">
        /// The sender of the message.
        /// </param>
        /// <param name="content">
        /// The message content.
        /// </param>
        /// <param name="sent">
        /// The time at which the message was sent.
        /// </param>
        public Message(User sender, string content, DateTimeOffset sent)
        {
            this.sender = sender;
            this.sent = sent;
            this.content = content;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the current object is equal to the <paramref name="other"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Message other)
        {
            return other != null && this.Sender.Equals(other.Sender) && this.Content.Equals(other.Content)
                   && this.Sent.Equals(other.Sent);
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
            return this.Equals(obj as Message);
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
            return 31 * (31 * this.Sent.GetHashCode() + this.Content.GetHashCode()) + this.Sender.GetHashCode();
        }
    }
}
