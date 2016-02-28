namespace CoreApiTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    using RegexReadData;

    using NUnit.Framework;

    /// <summary>
    /// Tests for the <see cref="Reader"/>.
    /// </summary>
    [TestFixture]
    sealed class ReaderTests
    {
        /// <summary>
        /// The environment reader under test.
        /// </summary>
        private Reader environmentReader;

        /// <summary>
        /// The test data.
        /// </summary>
        private const string FILE_PATH1 = "Example.txt";

        /// <summary>
        /// Sets up before any tests run.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            this.environmentReader = new Reader();
        }

        /// <summary>
        /// Tests that reading a conversation asynchronously given a room <paramref name="id"/> returns a <see cref="RoomConversation"/>.
        /// </summary>
        [Test]
        public async void ReadingConversationAsync_given_room_id_returns_room_conversation()
        {
            const string DummyRoomId = "TheRoom";
            const string DummyRoomName = "Best Movie Ever Made";

            var conversation = await this.environmentReader.ReadConversationAsync(FILE_PATH1, DummyRoomId);

            Assert.That(conversation, Is.Not.Null);
            Assert.That(conversation, Is.InstanceOf<RoomConversation>());

            var roomConversation = (RoomConversation)conversation;

            Assert.That(roomConversation.Id, Is.EqualTo(DummyRoomId));

            Assert.That(roomConversation.Room, Is.Not.Null);
            Assert.That(roomConversation.Room.Id, Is.EqualTo(DummyRoomId));
            Assert.That(roomConversation.Room.Name, Is.EqualTo(DummyRoomName));
            
            var dummyExpectedMessages = new[]
                                            {
                                                new Message(
                                                    new User("user1", "Jake Awesome"),
                                                    "Hey guys, next lyrics of the song, \"There used to be a graying tower alone on the sea\"",
                                                    new DateTimeOffset(2012, 3, 3, 20, 19, 5, TimeSpan.Zero)),
                                                new Message(
                                                    new User("user2", "Betty Incredible"),
                                                    "\"Uuuuuu became the light on the darkside of me\"",
                                                    new DateTimeOffset(2012, 3, 3, 20, 19, 15, TimeSpan.Zero)),
                                                new Message(
                                                    new User("user3", "David Amazing"),
                                                    "Keep rolling rolling rolling rolling uuuughhhh!!!",
                                                    new DateTimeOffset(2012, 3, 3, 20, 19, 27, TimeSpan.Zero)),
                                                new Message(
                                                    new User("user1", "Jake Awesome"),
                                                    "Seriously David! And you call yourself amazing!",
                                                    new DateTimeOffset(2012, 3, 3, 20, 19, 40, TimeSpan.Zero))
                                            };

            Assert.That(roomConversation.Messages, Is.EquivalentTo(dummyExpectedMessages));
        }

        /// <summary>
        /// Tests that reading a conversation asynchronously given a user room <paramref name="id"/> returns a <see cref="UserConversation"/>.
        /// </summary>
        [Test]
        public async void ReadingConversationAsync_given_user_id_returns_user_conversation()
        {
            const string DummyRoomId = "user2";

            var conversation = await this.environmentReader.ReadConversationAsync(FILE_PATH1, DummyRoomId);

            Assert.That(conversation, Is.Not.Null);
            Assert.That(conversation, Is.InstanceOf<UserConversation>());

            var userConversation = (UserConversation)conversation;
            Assert.That(userConversation.Id, Is.EqualTo(DummyRoomId));

            var dummyExpectedMessages = new[]
                                            {
                                                new Message(
                                                    new User("user1", "Jake Awesome"),
                                                    "Hey, what do you think about pickles?",
                                                    new DateTimeOffset(2012, 3, 1, 9, 23, 1, TimeSpan.Zero)),
                                                new Message(
                                                    new User("user2", "Betty Incredible"),
                                                    "Pickles are okay,",
                                                    new DateTimeOffset(2012, 3, 1, 9, 24, 41, TimeSpan.Zero)),
                                                new Message(
                                                    new User("user2", "Betty Incredible"),
                                                    "nothing special I think",
                                                    new DateTimeOffset(2012, 3, 1, 9, 24, 47, TimeSpan.Zero)),
                                                new Message(
                                                    new User("user1", "Jake Awesome"),
                                                    "Good for you... I hat them",
                                                    new DateTimeOffset(2012, 3, 1, 9, 24, 50, TimeSpan.Zero)),
                                                new Message(
                                                    new User("user1", "Jake Awesome"),
                                                    "Icky and all that jazz",
                                                    new DateTimeOffset(2012, 3, 1, 9, 24, 52, TimeSpan.Zero)),
                                                new Message(
                                                    new User("user1", "Jake Awesome"),
                                                    "*hate (what a moron)",
                                                    new DateTimeOffset(2012, 3, 1, 9, 24, 57, TimeSpan.Zero)),
                                                new Message(
                                                    new User("user2", "Betty Incredible"),
                                                    "Haha, Icky!!",
                                                    new DateTimeOffset(2012, 3, 1, 9, 25, 03, TimeSpan.Zero)),
                                            };

            Assert.That(userConversation.Messages, Is.EquivalentTo(dummyExpectedMessages));

            var dummyExpectedUsers = new[]
            {
                new User("user1", "Jake Awesome"),
                new User("user2", "Betty Incredible")
            };

            Assert.That(userConversation.Participants, Is.EquivalentTo(dummyExpectedUsers));
        }

        /// <summary>
        /// Tests that the conversations associated with the given <paramref name="userId"/> are returned with correct usage.
        /// </summary>
        [Test]
        public async void ReadConversationsAssociatedWithUserAsync_given_valid_user_id_returns_conversation_collection()
        {
            const string userId = "user1";

            const string conversationId1 = "TheRoom";
            const string conversationId2 = "user2";

            var conversations = await (this.environmentReader as Reader).ReadConversationsAssociatedWithUserAsync(FILE_PATH1, userId);
            Assert.That(conversations, !Is.Null);
            Assert.That(conversations, Is.InstanceOf<List<Conversation>>());

            var conversationsList = conversations as List<Conversation>;
            Assert.That(conversationsList.Count, Is.EqualTo(2), "user1 should be in 2 conversations, a UserConversation and a RoomConversation");

            //  Get the expected conversations.
            var conversationRoom = await (this.environmentReader as Reader).ReadConversationAsync(FILE_PATH1, conversationId1);
            var conversationUser = await (this.environmentReader as Reader).ReadConversationAsync(FILE_PATH1, conversationId2);

            Assert.That(conversationsList[0], Is.InstanceOf<UserConversation>(), "First element should be UserConversation");
            Assert.That(conversationsList[0].Messages, Is.InstanceOf<List<Message>>(), "Messages should be message list");
            Assert.That((conversationsList[0] as UserConversation).Participants, Is.InstanceOf<List<User>>(), "Participants should be user list");

            Assert.That(conversationsList[1], Is.InstanceOf<RoomConversation>(), "Second element should be UserConversation");
            Assert.That(conversationsList[1].Messages, Is.InstanceOf<List<Message>>(), "Messages should be message list");
            Assert.That((conversationsList[1] as RoomConversation).Room, Is.InstanceOf<Room>(), "Room should be of Room type");

            Assert.AreEqual(conversationUser.Id, conversationsList[0].Id);
            Assert.AreEqual(conversationRoom.Id, conversationsList[1].Id);

            var messagesUserConversationExpected = (conversationUser.Messages as List<Message>);
            var messagesRoomConversationExpected = (conversationRoom.Messages as List<Message>);

            var messagesUserConversationActual = (conversationsList[0].Messages as List<Message>);
            var messagesRoomConversationActual = (conversationsList[1].Messages as List<Message>);

            //  Test Messages values in list, unfortunately, Conversation doesn't derive from IEquatable, so manual loops are required.
            Assert.AreEqual(messagesUserConversationExpected.Count, messagesUserConversationActual.Count);
            Assert.AreEqual(messagesRoomConversationExpected.Count, messagesRoomConversationActual.Count);

            for (int i = 0; i < messagesUserConversationActual.Count; i++)
            {
                Assert.AreEqual(messagesUserConversationExpected[i], messagesUserConversationActual[i]);
            }

            for (int i = 0; i < (conversationsList[1].Messages as List<Message>).Count; i++)
            {
                Assert.AreEqual(messagesRoomConversationExpected[i], messagesRoomConversationActual[i]);
            }

            //  Test Paticipants values in UserConversation, unfortunately, Conversation doesn't derive from IEquatable, so manual loops are required.
            var participantsUserConversationExpected = (conversationUser as UserConversation).Participants as List<User>;
            var participantsUserConversationActual = (conversationsList[0] as UserConversation).Participants as List<User>;

            Assert.AreEqual(participantsUserConversationExpected.Count, participantsUserConversationActual.Count);

            for (int i = 0; i < participantsUserConversationActual.Count; i++)
            {
                Assert.AreEqual(participantsUserConversationExpected[i], participantsUserConversationActual[i]);
            }

            //  Test Room values in RoomConversation, unfortunately, Room doesn't derive from IEquatable, so manual checking required.
            var roomInRoomConversationExpected = (conversationRoom as RoomConversation).Room as Room;
            var roomInRoomConversationActual = (conversationsList[1] as RoomConversation).Room as Room;
            Assert.AreEqual(roomInRoomConversationExpected.Id, roomInRoomConversationActual.Id);
            Assert.AreEqual(roomInRoomConversationExpected.Name, roomInRoomConversationActual.Name);
        }

        /// <summary>
        /// Checks that an empty collection is returned when the <paramref name="userId"/> is non-existent.
        /// </summary>
        [Test]
        public async void ReadConversationsAssociatedWithUserAsync_non_existent_userId_returns_empty_collection()
        {
            var result = await (this.environmentReader as Reader).ReadConversationsAssociatedWithUserAsync(FILE_PATH1, "non_existent_id");

            Assert.That(result, Is.InstanceOf<List<Conversation>>());
            var list = result as List<Conversation>;
            Assert.That(list.Count, Is.EqualTo(0));
        }

        /// <summary>
        /// Tests that giving a valid user <paramref name="id"/> returns a <see cref="User"/> object with the correct values.
        /// </summary>
        [Test]
        public async void ReadUserAsync_correct_id_returns_user()
        {
            string user1Id = "user4";
            string user1Name = "Silvia Cool";

            string user2Id = "user6";
            string user2Name = "Katie Super";

            var user1 = await (this.environmentReader as Reader).ReadUserAsync(FILE_PATH1, user1Id);
            Assert.That(user1Id, Is.EqualTo(user1.Id));
            Assert.That(user1Name, Is.EqualTo(user1.Name));

            var user2 = await (this.environmentReader as Reader).ReadUserAsync(FILE_PATH1, user2Id);
            Assert.That(user2Id, Is.EqualTo(user2.Id));
            Assert.That(user2Name, Is.EqualTo(user2.Name));
        }

        /// <summary>
        /// Checks that correct usage returns all of the users.
        /// </summary>
        [Test]
        public async void ReadUsersAllAsync_correct_usage_returns_dictionary()
        {
            var userDictionary = await (this.environmentReader as Reader).ReadUsersAllAsync(FILE_PATH1);
            Assert.That(userDictionary, Is.TypeOf<Dictionary<string, User>>(), "Function has been used correctly thus Dictionary<string, User> should be returned");
            Assert.That(userDictionary.Count, Is.EqualTo(6), "The should be 6 user entries in the Dictionary");

            var user1 = "user1";
            var user2 = "user2";
            var user3 = "user3";
            var user4 = "user4";
            var user5 = "user5";
            var user6 = "user6";

            Assert.That(userDictionary[user1].Id, Is.EqualTo(user1));
            Assert.That(userDictionary[user2].Id, Is.EqualTo(user2));
            Assert.That(userDictionary[user3].Id, Is.EqualTo(user3));
            Assert.That(userDictionary[user4].Id, Is.EqualTo(user4));
            Assert.That(userDictionary[user5].Id, Is.EqualTo(user5));
            Assert.That(userDictionary[user6].Id, Is.EqualTo(user6));

            Assert.That(userDictionary[user1].Name, Is.EqualTo("Jake Awesome"));
            Assert.That(userDictionary[user2].Name, Is.EqualTo("Betty Incredible"));
            Assert.That(userDictionary[user3].Name, Is.EqualTo("David Amazing"));
            Assert.That(userDictionary[user4].Name, Is.EqualTo("Silvia Cool"));
            Assert.That(userDictionary[user5].Name, Is.EqualTo("Maria Fly"));
            Assert.That(userDictionary[user6].Name, Is.EqualTo("Katie Super"));
        }

        /// <summary>
        /// Tests that giving a valid room <paramref name="id"/> returns a <see cref="Room"/> object with the correct values.
        /// </summary>
        [Test]
        public async void ReadRoomAsync_correct_id_returns_room()
        {
            string room1Id = "TheRoom";
            string room1Name = "Best Movie Ever Made";

            string room2Id = "OhHiMark";
            string room2Name = "Hi Doggy";

            var room1 = await (this.environmentReader as Reader).ReadRoomAsync(FILE_PATH1, room1Id);
            Assert.That(room1Id, Is.EqualTo(room1.Id));
            Assert.That(room1Name, Is.EqualTo(room1.Name));

            var room2 = await (this.environmentReader as Reader).ReadRoomAsync(FILE_PATH1, room2Id);
            Assert.That(room2Id, Is.EqualTo(room2.Id));
            Assert.That(room2Name, Is.EqualTo(room2.Name));
        }

        /// <summary>
        /// Checks that correct usage returns all of the rooms.
        /// </summary>
        [Test]
        public async void ReadRoomsAllAsync_correct_usage_returns_dictionary()
        {
            var roomDictionary = await (this.environmentReader as Reader).ReadRoomsAllAsync(FILE_PATH1);
            Assert.That(roomDictionary, Is.TypeOf<Dictionary<string, Room>>(), "Function has been used correctly thus Dictionary<string, Room> should be returned");
            Assert.That(roomDictionary.Count, Is.EqualTo(2), "The should be 2 room entries in the Dictionary");

            var room1 = "TheRoom";
            var room2 = "OhHiMark";

            Assert.That(roomDictionary[room1].Id, Is.EqualTo(room1));
            Assert.That(roomDictionary[room2].Id, Is.EqualTo(room2));

            Assert.That(roomDictionary[room1].Name, Is.EqualTo("Best Movie Ever Made"));
            Assert.That(roomDictionary[room2].Name, Is.EqualTo("Hi Doggy"));
        }
    }
}
