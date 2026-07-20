#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

// iOS 빌드 완료 후 Xcode 프로젝트를 세팅한다.
// 1) Info.plist 에 AdMob App ID 삽입 — 없으면 런타임 "GADApplicationIdentifier not set" 크래시.
// 2) Info.plist 에 ATT 사용 설명 삽입 — iOS 14+ 광고 personalization 위해 필수.
// 3) AppTrackingTransparency.framework 링크 — ATTBridge.mm 이 참조하므로 없으면 빌드 시 undefined symbol.
public static class iOSPostBuildProcessor
{
    // iOS App ID (AdMob 콘솔에서 발급)
    private const string AdMobAppId = "ca-app-pub-3079888946602647~6304172559";

    // ATT 권한 요청 팝업에 표시되는 문구. Apple 심사 시 검토 대상.
    private const string ATTUsageDescription = "맞춤형 광고를 제공하기 위해 광고 식별자를 사용합니다.";

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS) return;

        string plistPath = Path.Combine(buildPath, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        plist.root.SetString("GADApplicationIdentifier", AdMobAppId);
        plist.root.SetString("NSUserTrackingUsageDescription", ATTUsageDescription);
        plist.WriteToFile(plistPath);

        string projPath = PBXProject.GetPBXProjectPath(buildPath);
        var proj = new PBXProject();
        proj.ReadFromFile(projPath);
        string targetGuid = proj.GetUnityFrameworkTargetGuid();
        proj.AddFrameworkToProject(targetGuid, "AppTrackingTransparency.framework", false);
        proj.WriteToFile(projPath);
    }
}
#endif
