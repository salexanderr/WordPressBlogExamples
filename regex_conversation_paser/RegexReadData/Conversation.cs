namespace RegexReadData
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a converastion in the chat system.
    /// </summary>
    public abstract class Conversation
    {
        #region Immutables

        readonly string id;
        readonly IEnumerable<Message> messages;

        #endregion
        #region Properties

        /// <summary>
        /// Gets the unique identifier of the conversation.
        /// </summary>
        public string Id { get { return id; } }

        /// <summary>
        /// Gets the messages sent in the conversation.
        /// </summary>
        public IEnumerable<Message> Messages { get { return messages; } }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="Conversation"/> class.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the conversation.
        /// </param>
        /// <param name="messages">
        /// The messages sent in the conversation.
        /// </param>
        protected Conversation(string id, IEnumerable<Message> messages)
        {
            this.id = id;
            this.messages = messages;
        }
    }
}
