// iOS 14+ App Tracking Transparency 권한 요청 네이티브 브릿지.
// AdManager 에서 P/Invoke 로 호출하고 결과는 콜백으로 받는다.
#import <AppTrackingTransparency/AppTrackingTransparency.h>

typedef void (*ATTCallback)(int status);

extern "C" {
    void _RequestATTPermission(ATTCallback callback) {
        if (@available(iOS 14, *)) {
            [ATTrackingManager requestTrackingAuthorizationWithCompletionHandler:^(ATTrackingManagerAuthorizationStatus status) {
                // Unity 콜백은 반드시 메인 스레드에서 호출되어야 P/Invoke 안전
                dispatch_async(dispatch_get_main_queue(), ^{
                    if (callback) callback((int)status);
                });
            }];
        } else {
            // iOS 14 미만은 ATT 개념 자체가 없음 → 승인(3) 으로 처리해 후속 로직 통과
            if (callback) callback(3);
        }
    }
}
