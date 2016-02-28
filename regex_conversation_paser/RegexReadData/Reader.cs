namespace RegexReadData
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents an environment reader that can read a conversation from a file source.
    /// </summary>
    public sealed class Reader
    {
        /// <summary>
        /// Asynchronously reads the given <paramref name="conversationId" /> from the chat system file located at <paramref name="filePath" />.
        /// </summary>
        /// <param name="filePath">The path to the chat system file.</param>
        /// <param name="conversationId">The unique identifier of the conversation to be read.</param>
        /// <returns>
        /// A <see cref="Task{T}" /> representing the result of the asynchronous operation.
        /// <see cref="Task{T}.Result" /> will be resolved to the <see cref="Conversation" /> instance representing the conversation.
        /// <see cref="Task{T}.Result"/> will be resolved to <c>null</c> if <paramref name="conversationId"/> doesn't exist.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the given <paramref name="filePath"/> does not exist or cannot be read.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the conversation cannot be read in the current state.
        /// </exception>
        /// <exception cref="ReadConversationException">
        /// Thrown when there is a problem reading the conversation from the chat system.
        /// </exception>
        public Task<Conversation> ReadConversationAsync(string filePath, string conversationId)
        {
            return Task.Run<Conversation>(() =>
            {
                Conversation conversation = null;
                
                //  Define the regular expression that will be used to match the required conversation in the file.
                var regexConversation = new Regex(@"(?<conversation>conversation\s(?<id>" + conversationId + @")\r?\n)(?<all_messages>(?<last_message>[^\s]+\s\d+\s[^\r\n]+\r?\n?)*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var regexMessages = new Regex(@"(?<user>[^\s]+)\s(?<sent>\d+)\s(?<content>[^\r\n]+)\n?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                string content = null;  //  Will contain the contents of the input file.

                //  Read the contents of the given file to be matched using the regular expression.
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var streamReader = new StreamReader(fileStream);
                        content = streamReader.ReadToEnd();
                    }
                }
                catch (Exception ex)
                {
                    //  This will cause any errors reading the user file given to throw the ArgumentException. This is ensure that the given design
                    //  structure is adhered to, where any problems reading the file is supposed to do this.
                    throw new ArgumentException(string.Format("There was a problem reading the given file at {0}, does it exist at the location specified?", filePath), "filePath", ex);
                }

                //  Match the users in the file.
                var matchConversation = regexConversation.Match(content);

                if (matchConversation.Success)
                {
                    var usersDictionary = ReadUsersAllAsync(filePath).Result;           //  Get all users for lookup purposes.
                    var roomsDictionary = ReadRoomsAllAsync(filePath).Result;   //  Get all rooms for lookup purposes.

                    //  List for all the messages in the conversation.
                    var messages = new List<Message>();

                    //  Use hashset to store unique users from the conversation.
                    var usersHashSet = new HashSet<User>();

                    //  Get the messages contained.
                    foreach (Match matchMessage in regexMessages.Matches(matchConversation.Value))
                    {
                        var groupsMessage = matchMessage.Groups;

                        //  Get user from id.
                        var user = usersDictionary[groupsMessage["user"].Value];

                        //  If the user isn't present, store in the hashset.
                        usersHashSet.Add(user);

                        //  Create the message.
                        messages.Add(new Message(
                            user,
                            groupsMessage["content"].Value,
                            DateTimeOffset.FromUnixTimeSeconds(long.Parse(groupsMessage["sent"].Value))
                        ));
                    }

                    if (usersDictionary.ContainsKey(conversationId))
                    {
                        //  If the conversation id is present in the users dictionary, return a user conversation.
                        conversation = new UserConversation(conversationId, messages, usersHashSet.ToList());
                    }
                    else if (roomsDictionary.ContainsKey(conversationId))
                    {
                        //  If the conversation id corresponds to a room, return a room conversation.
                        conversation = new RoomConversation(conversationId, messages, roomsDictionary[conversationId]);
                    }
                    else throw new Exception(String.Format("No conversion found for specified conversation id = {0}", conversationId));
                }
                else throw new InvalidOperationException("Data cannot be read in its current state.", null);

                return conversation;
            });
        }
        
        /// <summary>
        /// Asynchronously reads the <see cref="Conversation"/> objects particapated by the given <paramref name="userId" /> 
        /// from the chat system file located at <paramref name="filePath" />.
        /// </summary>
        /// <param name="filePath">The path to the chat system file.</param>
        /// <param name="conversationId">The unique identifier of the user whose <see cref="Conversation"/> objects are to be retrieved.</param>
        /// <returns>
        /// A <see cref="Task{T}" /> representing the result of the asynchronous operation.
        /// <see cref="Task{T}.Result" /> will be resolved to the <see cref="Conversation" /> instance representing the conversation.
        /// <see cref="Task{T}.Result"/> will be resolved to <c>null</c> if <paramref name="userId"/> doesn't exist.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the given <paramref name="filePath"/> does not exist or cannot be read.
        /// </exception>
        /// <exception cref="ReadConversationException">
        /// Thrown when there is a problem reading the conversation from the chat system.
        /// </exception>
        public Task<IEnumerable<Conversation>> ReadConversationsAssociatedWithUserAsync(string filePath, string userId)
        {
            return Task.Run<IEnumerable<Conversation>>(() =>
            {
                var conversationsWhereUserParticipated = new List<Conversation>();

                //  Define the regular expression that will be used to match the required conversation in the file.
                var regexConversation = new Regex(@"(?<conversation>conversation\s(?<conversation_id>[^\s]+)\r?\n)(?<all_messages>(?<last_message>[^\s]+\s\d+\s[^\r\n]+\r?\n?)*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var regexMessages = new Regex(@"(?<user_id>[^\s]+)\s(?<sent>\d+)\s(?<content>[^\r\n]+)\n?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                string content = null;  //  Will contain the contents of the input file.

                //  Read the contents of the given file to be matched using the regular expression.
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var streamReader = new StreamReader(fileStream);
                        content = streamReader.ReadToEnd(); // Read the file to the string variable.
                    }
                }
                catch (Exception ex)
                {
                    //  This will cause any errors reading the user file given to throw the ArgumentException. This is ensure that the given design
                    //  structure is adhered to, where any problems reading the file is supposed to do this.
                    throw new ArgumentException(string.Format("There was a problem reading the given file at {0}", filePath), "filePath", ex);
                }
                //  Match the users in the file.
                var matchesConversation = regexConversation.Matches(content);

                var usersDictionary = ReadUsersAllAsync(filePath).Result;           //  Get all users for lookup purposes.
                var roomsDictionary = ReadRoomsAllAsync(filePath).Result;   //  Get all rooms for lookup purposes.

                foreach (Match matchConversation in matchesConversation)
                {
                    //  List for all the messages in the conversation.
                    var messages = new List<Message>();

                    //  Get the match for the user if it exists.
                    var groups = matchConversation.Groups;

                    //  Use hashset to store unique users from the conversation.
                    var usersHashSet = new HashSet<User>();

                    //  Flag to determine whether a message was sent by the user specified.
                    var userInConversation = false;

                    //  Get the messages contained.
                    foreach (Match matchMessage in regexMessages.Matches(matchConversation.Value))
                    {
                        var groupsMessage = matchMessage.Groups;

                        //  Get user from id.
                        var user = usersDictionary[groupsMessage["user_id"].Value];

                        //  Set the flag if user who sent message is the one specified.
                        if (user.Id == userId) userInConversation = true;

                        //  If the user isn't present, store in the hashset.
                        usersHashSet.Add(user);

                        //  Create the message.
                        messages.Add(new Message(
                            user,
                            groupsMessage["content"].Value,
                            DateTimeOffset.FromUnixTimeSeconds(long.Parse(groupsMessage["sent"].Value))
                        ));
                    }

                    if (userInConversation) //  If the flag has been set, it means that one/more of the messages in the conversation were sent by the specified user.
                    {
                        if (usersDictionary.ContainsKey(groups["conversation_id"].Value))
                        {
                            //  If the conversation id is present in the users dictionary, return a user conversation.
                            conversationsWhereUserParticipated.Add(new UserConversation(groups["conversation_id"].Value, messages, usersHashSet.ToList()));
                        }
                        else if (roomsDictionary.ContainsKey(groups["conversation_id"].Value))
                        {
                            //  If the conversation id corresponds to a room, return a room conversation.
                            conversationsWhereUserParticipated.Add(new RoomConversation(groups["conversation_id"].Value, messages, roomsDictionary[groups["conversation_id"].Value]));
                        }
                        else throw new Exception("No conversion found for specified conversation id, does it exist in the file?");
                    }
                }

                return conversationsWhereUserParticipated;
            });
        }

        /// <summary>
        /// Asynchronously reads the given <paramref name="userId"/> from the chat system file located at <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">The path to the chat system file.</param>
        /// <param name="userId">The unique identifier of the user to be read.</param>
        /// <returns>
        /// A <see cref="Task{T}"/> representing the result of the asynchronous operation.
        /// <see cref="Task{T}.Result"/> will be resolved to the <see cref="User"/> instance representing the user.
        /// <see cref="User"/> will be <c>null</c> if <paramref name="userId"/> doesn't exist.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the given <paramref name="filePath"/> does not exist or cannot be read.
        /// </exception>
        public Task<User> ReadUserAsync(string filePath, string userId)
        {
            return Task.Run<User>(() =>
            {
                //  Define the regular expression that will be used to match all of the users in the file.
                var regex = new Regex(@"user\s" + userId + @"\s(?<user_name>[^\r\n]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                
                string content = null;  //  Will contain the contents of the input file.

                //  Read the contents of the given file to be matched using the regular expression.
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var streamReader = new StreamReader(fileStream);
                        content = streamReader.ReadToEnd(); // Read the file to the string variable.
                    }
                }
                catch (Exception ex)
                {
                    //  This will cause any errors reading the user file given to throw the ArgumentException. This is ensure that the given design
                    //  structure is adhered to, where any problems reading the file is supposed to do this.
                    throw new ArgumentException(string.Format("There was a problem reading the given file at {0}, does it exist at the location specified?", filePath), "filePath", ex);
                }
                //  Match the users in the file.
                var match = regex.Match(content);                              

                //  The user object to be returned.
                User user = null;

                if (match.Success)
                {
                    //  Get the match for the user if it exists.
                    var groups = match.Groups;
                    user = new User(userId, groups["user_name"].Value);
                }

                return user;
            });
        }		
		
        /// <summary>
        /// Asynchronously reads all <see cref="User"/> objects from the chat system file located at <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">The path to the chat system file.</param>
        /// <returns>
        /// A <see cref="Task{T}"/> representing the result of the asynchronous operation.
        /// <see cref="Task{T}.Result"/> will be resolved to the <see cref="IDictionary<string, User>"/> instance representing the user dictionary.
        /// <see cref="Task{T}.Result"/> will be resolved to <c>null</c> if there are no <see cref="User"/> objects present in the file.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the given <paramref name="filePath"/> does not exist or cannot be read.
        /// </exception>
		public Task<IDictionary<string, User>> ReadUsersAllAsync(string filePath)
        {
            return Task.Run<IDictionary<string, User>>(() =>
            {
                //  Define the regular expression that will be used to match all of the users in the file.
                var regex = new Regex(@"user\s(?<user_id>[^\s]+)\s(?<user_name>[^\r\n]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                
                string content = null;  //  Will contain the contents of the input file.

                //  Read the contents of the given file to be matched using the regular expression.
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var streamReader = new StreamReader(fileStream);
                        content = streamReader.ReadToEnd(); // Read the file to the string variable.
                    }
                }
                catch (Exception ex)
                {
                    //  This will cause any errors reading the user file given to throw the ArgumentException. This is ensure that the given design
                    //  structure is adhered to, where any problems reading the file is supposed to do this.
                    throw new ArgumentException(string.Format("There was a problem reading the given file at {0}, does it exist at the location specified?", filePath), "filePath", ex);
                }
                //  Match the users in the file.
                var matches = regex.Matches(content);                              

                //  The user dictionary object to be returned.
                Dictionary<string, User> userDictionary = null;
				
                //  If matches have been found, instantiate an new dictionary and store them.
				if (matches.Count > 0)
				{
					userDictionary = new Dictionary<string, User> ();
					
					foreach (Match match in matches)
					{
						//  Get the match for the user if it exists.
						var groups = match.Groups;
						userDictionary.Add(groups["user_id"].Value, new User(groups["user_id"].Value, groups["user_name"].Value));
					}
				}

                return userDictionary;
            });
        }

        /// <summary>
        /// Asynchronously reads the given <paramref name="roomId"/> from the chat system file located at <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">The path to the chat system file.</param>
        /// <param name="roomId">The unique identifier of the room to be read.</param>
        /// <returns>
        /// A <see cref="Task{T}"/> representing the result of the asynchronous operation.
        /// <see cref="Task{T}.Result"/> will be resolved to the <see cref="Room"/> instance representing the room.
        /// <see cref="Task{T}.Result"/> will be resolved to <c>null</c> if <paramref name="roomId"/> doesn't exist.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the given <paramref name="filePath"/> does not exist or cannot be read.
        /// </exception>
        public Task<Room> ReadRoomAsync(string filePath, string roomId)
        {
            return Task.Run<Room>(() =>
            {
                //  Define the regular expression that will be used to match the room in the file.
                var regex = new Regex(@"room\s" + roomId + @"\s(?<room_name>[^\r\n]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                string content = null;  //  Will contain the contents of the input file.

                //  Read the contents of the given file to be matched using the regular expression.
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var streamReader = new StreamReader(fileStream);
                        content = streamReader.ReadToEnd(); // Read the file to the string variable.
                    }
                }
                catch (Exception ex)
                {
                    //  This will cause any errors reading the user file given to throw the ArgumentException. This is ensure that the given design
                    //  structure is adhered to, where any problems reading the file is supposed to do this.
                    throw new ArgumentException(string.Format("There was a problem reading the given file at {0}, does it exist at the location specified?", filePath), "filePath", ex);
                }
                //  Match the rooms in the file.
                var match = regex.Match(content);

                //  The room object to be returned.
                Room room = null;

                if (match.Success)
                {
                    //  Get the match for the room if it exists.
                    var groups = match.Groups;
                    room = new Room(roomId, groups["room_name"].Value);
                }

                return room;
            });
        }

        /// <summary>
        /// Asynchronously reads all <see cref="Room"/> objects from the chat system file located at <paramref name="filePath"/>.
        /// </summary>
        /// <param name="filePath">The path to the chat system file.</param>
        /// <returns>
        /// A <see cref="Task{T}"/> representing the result of the asynchronous operation.
        /// <see cref="Task{T}.Result"/> will be resolved to the <see cref="IDictionary<string, Room>"/> instance representing the room dictionary. 
        /// <see cref="Task{T}.Result"/> will be resolved to <c>null</c> if there are no <see cref="Room"/> objects present in the file.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when the given <paramref name="filePath"/> does not exist or cannot be read.
        /// </exception>
        public Task<IDictionary<string, Room>> ReadRoomsAllAsync(string filePath)
        {
            return Task.Run<IDictionary<string, Room>>(() =>
            {
                //  Define the regular expression that will be used to match all of the rooms in the file.
                var regex = new Regex(@"room\s(?<room_id>[^\s]+)\s(?<room_name>[^\r\n]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

                string content = null;  //  Will contain the contents of the input file.

                //  Read the contents of the given file to be matched using the regular expression.
                try
                {
                    using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        var streamReader = new StreamReader(fileStream);
                        content = streamReader.ReadToEnd(); // Read the file to the string variable.
                    }
                }
                catch (Exception ex)
                {
                    //  This will cause any errors reading the user file given to throw the ArgumentException. This is ensure that the given design
                    //  structure is adhered to, where any problems reading the file is supposed to do this.
                    throw new ArgumentException(string.Format("There was a problem reading the given file at {0}, does it exist at the location specified?", filePath), "filePath", ex);
                }
                //  Match the rooms in the file.
                var matches = regex.Matches(content);

                //  The room dictionary object to be returned.
                Dictionary<string, Room> roomDictionary = null;

                //  If matches have been found, instantiate an new dictionary and store them.
                if (matches.Count > 0)
                {
                    roomDictionary = new Dictionary<string, Room>();

                    foreach (Match match in matches)
                    {
                        //  Get the match for the room if it exists.
                        var groups = match.Groups;
                        roomDictionary.Add(groups["room_id"].Value, new Room(groups["room_id"].Value, groups["room_name"].Value));
                    }
                }

                return roomDictionary;
            });
        }
    }
}
