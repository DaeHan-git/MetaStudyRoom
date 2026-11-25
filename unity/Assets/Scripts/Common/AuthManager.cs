using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;

public static class AuthManager
{
    public static async Task<FirebaseUser> CreateAccountAsync(string email, string password)
    {
        await FirebaseManager.InitializeAsync();
        var result = await FirebaseManager.Auth.CreateUserWithEmailAndPasswordAsync(email, password);
        return result.User;
    }

    public static async Task<FirebaseUser> SignInAsync(string email, string password)
    {
        await FirebaseManager.InitializeAsync();
        var result = await FirebaseManager.Auth.SignInWithEmailAndPasswordAsync(email, password);
        return result.User;
    }

    public static async Task SaveNicknameAsync(string uid, string nickname)
    {
        if (string.IsNullOrWhiteSpace(nickname))
            throw new ArgumentException("Nickname must not be empty.", nameof(nickname));
        await FirebaseManager.InitializeAsync();
        RequireCurrentUser(uid);
        var data = new Dictionary<string, object> { { "nickname", nickname } };
        await FirebaseManager.Firestore.Collection("users").Document(uid).SetAsync(data, SetOptions.MergeAll);
        RequireCurrentUser(uid);
    }

    public static async Task<string> GetNicknameAsync(string uid)
    {
        await FirebaseManager.InitializeAsync();
        RequireCurrentUser(uid);
        var snapshot = await FirebaseManager.Firestore.Collection("users").Document(uid).GetSnapshotAsync(Source.Server);
        RequireCurrentUser(uid);
        if (!snapshot.Exists || !snapshot.TryGetValue("nickname", out string nickname) || string.IsNullOrWhiteSpace(nickname))
            throw new ProfileMissingException();
        return nickname;
    }

    public sealed class ProfileMissingException : Exception
    {
        public ProfileMissingException() : base("The user profile or nickname does not exist.") { }
    }

    public static void SignOut()
    {
        FirebaseManager.Auth.SignOut();
    }

    private static void RequireCurrentUser(string uid)
    {
        if (string.IsNullOrEmpty(uid) || FirebaseManager.CurrentUserId != uid)
            throw new InvalidOperationException("The authenticated user has changed.");
    }
}
