using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;

public static class FirebaseManager
{
    private static Task initialization;
    private static FirebaseAuth auth;
    private static FirebaseFirestore firestore;

    public static bool IsReady => auth != null && firestore != null;
    public static bool IsSignedIn => IsReady && auth.CurrentUser != null;
    public static string CurrentUserId => IsSignedIn ? auth.CurrentUser.UserId : null;
    public static event Action<string> AuthStateChanged;

    public static FirebaseAuth Auth => auth ?? throw new InvalidOperationException("Firebase is not initialized.");
    public static FirebaseFirestore Firestore => firestore ?? throw new InvalidOperationException("Firebase is not initialized.");

    public static Task InitializeAsync()
    {
        if (initialization == null || initialization.IsFaulted || initialization.IsCanceled)
            initialization = InitializeCoreAsync();
        return initialization;
    }

    private static async Task InitializeCoreAsync()
    {
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
            throw new InvalidOperationException($"Firebase dependencies are unavailable: {status}");

        firestore = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += OnAuthStateChanged;
    }

    private static void OnAuthStateChanged(object sender, EventArgs args)
    {
        AuthStateChanged?.Invoke(CurrentUserId);
    }
}
