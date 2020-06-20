using UnityEngine;

/**
 * ALPS AdMob interface script which allows individual live wallpapers
 * to handle events fired by AdMob. All callbacks provide the ad unit id
 * for which the callback is being made, which applications may use to map
 * the event against individual ads units.
 */
public class ALPSDemoAdMobListener : ALPS.AdMobListener {
    /**
    * ALPS will fire this callback when an ad has finished loading.
    */
    public void onLoaded(string adUnitId) {
        ALPS.showToast("AdMob:onLoaded: " + adUnitId);
        Debug.LogWarning("AdMob:onLoaded: " + adUnitId);
    }

    /**
    * ALPS will fire this callback when an ad has failed to load.
    * The code as returned by AdMob also will be passed.
    */
    public void onFailedToLoad(string adUnitId, int code) {
        ALPS.showToast("AdMob:onFailedToLoad: " + adUnitId + " " + code);
        Debug.LogWarning("AdMob:onFailedToLoad: " + adUnitId + " " + code);
    }

    /**
    * ALPS will fire this callback when user clicked on an Ad.
    */
    public void onOpened(string adUnitId) {
        ALPS.showToast("AdMob:onOpened: " + adUnitId);
        Debug.LogWarning("AdMob:onOpened: " + adUnitId);
    }

    /**
    * ALPS will fire this callback when an Ad click proceeds and leaves the app.
    */
    public void onLeftApplication(string adUnitId) {
        ALPS.showToast("AdMob:onLeftApplication: " + adUnitId);
        Debug.LogWarning("AdMob:onLeftApplication: " + adUnitId);
    }

    /**
    * ALPS will fire this callback when an ad has stopped being displayed.
    */
    public void onClosed(string adUnitId) {
        ALPS.showToast("AdMob:onClosed: " + adUnitId);
        Debug.LogWarning("AdMob:onClosed: " + adUnitId);
    }

    /**
    * ALPS will fire this callback when a rewarded video starts playback.
    */
    public void onVideoStart(string adUnitId) {
        ALPS.showToast("AdMob:onVideoStart: " + adUnitId);
        Debug.LogWarning("AdMob:onVideoStart: " + adUnitId);
    }

    /**
     * ALPS will fire this callback when a rewarded video complets playback.
     */
    public void onVideoComplete(string adUnitId) {
        ALPS.showToast("AdMob:onVideoComplete: " + adUnitId);
        Debug.LogWarning("AdMob:onVideoComplete: " + adUnitId);
    }

    /**
     * ALPS will fire this callback when a reward has been granted from a rewardrd video.
     */
    public void onVideoRewarded(string adUnitId, string type, int amount) {
        ALPS.showToast("AdMob:onVideoRewarded: " + adUnitId + " " + type + " " + amount);
        Debug.LogWarning("AdMob:onVideoRewarded: " + adUnitId + " " + type + " " + amount);
    }

    /**
     * ALPS will fire this callback when an api call has been made to show an interstitial
     * or a rewarded video ad, but the ad is not ready.
     */
    public void onAdMobNotReady(string adUnitId) {
        ALPS.showToast("AdMob:onAdMobNotReady: " + adUnitId);
        Debug.LogWarning("AdMob:onAdMobNotReady: " + adUnitId);
    }
}
