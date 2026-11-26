# Firebase 인증 공통 계약

프로젝트 설정의 Firebase 프로젝트 ID는 `metastudyroom`이다. 사용자 식별자는
Firebase Authentication UID이며, 본인 프로필은 `users/{uid}`에 저장한다.
현재 계정 코드가 사용하는 필드는 문자열 `nickname`뿐이다. 비밀번호는
Authentication에서만 처리하고 Firestore나 로그에 저장하지 않는다.

## 공통 API

Unity 메인 스레드에서 `await FirebaseManager.InitializeAsync()`로 초기화를 기다린다.
실패하면 예외가 발생하며, 이후 호출로 재시도할 수 있다. 완료 후
`FirebaseManager.IsReady`, `FirebaseManager.IsSignedIn`,
`FirebaseManager.CurrentUserId`, `FirebaseManager.Firestore`를 사용할 수 있다.
로그인하지 않았을 때 UID는 `null`이다.

로그인 상태 변화를 관찰하려면 `FirebaseManager.AuthStateChanged`에
`Action<string>`을 등록하고 더 이상 필요하지 않을 때 해제한다.
이벤트의 인수는 현재 UID이며 로그아웃하면 `null`이다. 인증 이벤트는
프로필 저장 완료를 의미하지 않으므로 프로필이 필요할 때는 별도로 조회한다.
등록 후에도 초기화 완료를 기다린 뒤 현재 UID를 직접 확인한다.
씬과 계정이 바뀌면 이전 UID의 화면, 구독, 진행 중인 요청 결과를 정리한다.

```csharp
await FirebaseManager.InitializeAsync();
if (FirebaseManager.IsSignedIn)
{
    string uid = FirebaseManager.CurrentUserId;
    string nickname = await AuthManager.GetNicknameAsync(uid);
}
```

`AuthManager.CreateAccountAsync`, `SignInAsync`, `SaveNicknameAsync`,
`GetNicknameAsync`, `SignOut`은 계정 화면에서 사용한다. 프로필 쓰기는
`MergeAll`을 사용하므로 다른 필드를 통째로 덮어쓰지 않는다. 조회는 서버
데이터를 요구한다. 프로필이 없으면 `AuthManager.ProfileMissingException`이
발생하며, 계정 화면에서는 닉네임 입력 후 저장을 재시도할 수 있다.

## 계정 화면

`Assets/Scenes/01_Account/Test_AccountScene.unity`의 기존 로그인 및 가입
프리팹을 사용한다. 로그인 버튼은 `AccountManager`가 실행 중 연결한다.
로그인 후 본인 닉네임과 로그아웃 버튼이 표시된다. 가입 시 계정 생성 후
프로필 저장까지 성공해야 완료 메시지를 표시하며, 이후 로그인 화면으로
돌아간다. 프로필 저장 실패 시 계정을 삭제하지 않고 재시도할 수 있다.

현재 빌드 씬 목록과 실제 시작 씬은 팀에서 따로 확인해야 한다. 다른 씬에
있는 아바타, 방, 통계 기능은 이 인증 작업에서 Firebase에 연결하지 않았다.

## 보안 규칙 및 검증

**규칙은 아직 게시하지 않았다.** Firebase 콘솔에서 프로젝트와 Firestore
접근 및 이메일/비밀번호 인증이 사용 설정된 것을 확인했다. 현재 게시된
규칙은 `connectionTests/{userId}`만 허용한다. 팀장이 기존 규칙을 유지하면서
`connectionTests` 등 기존 경로를 유지하면서 아래 사용자 프로필 규칙을
검토·추가해야 한다. 이 예시는 `users/{uid}` 문서만 다루며 하위 컬렉션의
규칙은 각 담당자가 별도로 정의해야 한다.

```firestore
match /users/{uid} {
  function isOwner() {
    return request.auth != null && request.auth.uid == uid;
  }
  allow read: if isOwner();
  allow create: if isOwner()
    && request.resource.data.keys().hasOnly(['nickname'])
    && request.resource.data.nickname is string
    && request.resource.data.nickname.size() > 0;
  allow update: if isOwner()
    && request.resource.data.diff(resource.data).affectedKeys().hasOnly(['nickname'])
    && request.resource.data.nickname is string
    && request.resource.data.nickname.size() > 0;
}
```

실제 규칙 게시 및 두 계정으로 가입·로그인·프로필·로그아웃·계정 전환을
확인해야 한다.
테스트 계정 생성과 실제 Firebase 데이터 변경은 아직 수행하지 않았다.
