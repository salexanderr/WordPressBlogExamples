namespace RegexReadData
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a conversation between one or more users.
    /// </summary>
    public sealed class UserConversation : Conversation
    {
        #region Immutables

        readonly IEnumerable<User> participants;

        #endregion
        #region Properties

        /// <summary>
        /// Gets the users that were part of the conversation.
        /// </summary>
        public IEnumerable<User> Participants { get { return participants; } }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="UserConversation"/> class.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the conversation.
        /// </param>
        /// <param name="messages">
        /// The messages sent in the conversation.
        /// </param>
        /// <param name="participants">
        /// The users that were part of the conversation.
        /// </param>
        internal UserConversation(string id, IEnumerable<Message> messages, IEnumerable<User> participants)
            : base(id, messages)
        {
            this.participants = participants;
        }        
    }
}
