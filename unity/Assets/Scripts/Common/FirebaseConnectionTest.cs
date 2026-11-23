using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using TMPro;
using UnityEngine;

public class FirebaseConnectionTest : MonoBehaviour
{
    [SerializeField] private TMP_InputField emailInput;
    [SerializeField] private TMP_InputField passwordInput;

    private FirebaseApp app;
    private FirebaseAuth auth;
    private FirebaseFirestore db;

    private bool isReady;
    private bool isRunning;

    private async void Start()
    {
        try
        {
            var status =
                await FirebaseApp.CheckAndFixDependenciesAsync();

            if (this == null)
                return;

            if (status != DependencyStatus.Available)
            {
                Debug.LogError($"Firebase 초기화 실패: {status}");
                return;
            }

            app = FirebaseApp.DefaultInstance;
            auth = FirebaseAuth.DefaultInstance;
            db = FirebaseFirestore.DefaultInstance;

            isReady = true;
            Debug.Log("Firebase 준비 완료. 테스트 버튼을 눌러주세요.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Firebase 초기화 오류: {e.Message}");
        }
    }

    public async void LoginAndTest()
    {
        if (!isReady)
        {
            Debug.LogWarning("Firebase 초기화가 완료될 때까지 기다려주세요.");
            return;
        }

        if (isRunning)
            return;

        if (emailInput == null || passwordInput == null)
        {
            Debug.LogError("Inspector에서 입력창 2개를 연결해주세요.");
            return;
        }

        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("이메일과 비밀번호를 입력해주세요.");
            return;
        }

        isRunning = true;
        string stage = "로그인";

        try
        {
            var result =
                await auth.SignInWithEmailAndPasswordAsync(email, password);

            string uid = result.User.UserId;
            Debug.Log($"로그인 성공! UID: {uid}");

            if (passwordInput != null)
                passwordInput.text = "";

            stage = "Firestore 저장";

            DocumentReference document =
                db.Collection("connectionTests").Document(uid);

            var data = new Dictionary<string, object>
            {
                { "message", "MetaStudyRoom 연결 테스트" },
                { "updatedAt", FieldValue.ServerTimestamp }
            };

            await document.SetAsync(data);
            Debug.Log("Firestore 저장 성공!");

            stage = "Firestore 서버 조회";

            DocumentSnapshot snapshot =
                await document.GetSnapshotAsync(Source.Server);

            if (!snapshot.Exists)
                throw new Exception("저장한 문서를 찾지 못했습니다.");

            string message = snapshot.GetValue<string>("message");
            Debug.Log($"Firestore 서버 조회 성공! 내용: {message}");
        }
        catch (Exception e)
        {
            Debug.LogError($"{stage} 실패: {e.Message}");
        }
        finally
        {
            isRunning = false;
        }
    }
}