using System;

namespace App.Managers.Users
{
    public class User
    {
        public Guid Id { get; private set; }
        public string CognitoId { get; private set; }
        public string Username { get; private set; }
        public UserSettings Settings { get; private set; }
        public SupporterType.TypeCode ActiveSupporterType { get; private set; }
        public DateTime CreatedOnUtc { get; private set; }
        public DateTime? VoidedOnUtc { get; private set; }
        public bool IsActive => VoidedOnUtc == null;
        public int? Version { get; }

        private User(Guid id, string username, string cognitoId, UserSettings settings, SupporterType.TypeCode activeSupporterType, DateTime createdOnUtc, DateTime voidedOnUtc, int? version)
                :this(id, username, cognitoId, activeSupporterType, createdOnUtc)
        {
            Settings = settings;
            VoidedOnUtc = voidedOnUtc;
            Version = version;
        }

        private User(Guid id, string username, string cognitoId, SupporterType.TypeCode activeSupporterType, DateTime createdOnUtc)
        {
            Id = id;
            Username = username;
            CognitoId = cognitoId;
            ActiveSupporterType = activeSupporterType;
            CreatedOnUtc = createdOnUtc;
        }
        
        public static User Create(NewUser newUser, string cognitoId)
        {
            var user = new User(Guid.NewGuid(), newUser.Username, cognitoId, SupporterType.TypeCode.None, DateTime.UtcNow);

            return user;
        }

        public User Apply(UpdatedUser updatedUser)
        {
            if (updatedUser == null)
                return this;

            Username = updatedUser.Username;
            Settings = updatedUser.Settings;

            return this;
        }

        public User Void()
        {
            VoidedOnUtc = DateTime.UtcNow;
            return this;
        }

        public User Restore()
        {
            VoidedOnUtc = null;
            return this;
        }

        public User Load(Guid id, string username, string cognitoId, UserSettings settings, SupporterType.TypeCode activeSupporterType, DateTime createdOnUtc, DateTime voidedOnUtc, int? version)
        {
            return new User(
                id: id,
                username: username,
                cognitoId: cognitoId,
                settings: settings,
                activeSupporterType: activeSupporterType,
                createdOnUtc: createdOnUtc,
                voidedOnUtc: voidedOnUtc,
                version: version);
        }
    }
}
