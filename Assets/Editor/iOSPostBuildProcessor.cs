#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using System.IO;

// iOS 빌드 완료 후 Xcode 프로젝트의 Info.plist에 AdMob App ID를 자동 삽입한다.
// 없으면 런타임에 "GADApplicationIdentifier not set" 크래시가 발생한다.
public static class iOSPostBuildProcessor
{
    // iOS App ID (AdMob 콘솔에서 발급)
    private const string AdMobAppId = "ca-app-pub-3079888946602647~6304172559";

    [PostProcessBuild(100)]
    public static void OnPostProcessBuild(BuildTarget target, string buildPath)
    {
        if (target != BuildTarget.iOS) return;

        // Info.plist 경로
        string plistPath = Path.Combine(buildPath, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        // AdMob App ID 삽입
        plist.root.SetString("GADApplicationIdentifier", AdMobAppId);

        plist.WriteToFile(plistPath);
    }
}
#endif
