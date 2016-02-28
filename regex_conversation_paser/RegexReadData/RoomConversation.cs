namespace RegexReadData
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a conversation within a room.
    /// </summary>
    public sealed class RoomConversation : Conversation
    {
        #region Immutables

        readonly Room room;

        #endregion
        #region Properties

        /// <summary>
        /// Gets the room in which the conversation occured.
        /// </summary>
        public Room Room { get { return room; } }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RoomConversation"/> class.
        /// </summary>
        /// <param name="id">
        /// The unique identifier of the conversation.
        /// </param>
        /// <param name="messages">
        /// The messages sent in the conversation.
        /// </param>
        /// <param name="room">
        /// The room in which the conversation occured.
        /// </param>
        internal RoomConversation(string id, IEnumerable<Message> messages, Room room)
            : base(id, messages)
        {
            this.room = room;
        }
    }
}
