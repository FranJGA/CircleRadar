using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * ALPS interface script which allows individual live wallpapers to do their own processing. This script already
 * handles all basic preferences for FPS, Resolution and scrolling behaviour. A live wallapper app may add or remove
 * code according to their needs. It is a good idea to code all interactions with APLS in this class only.
 */
public class ALPSDemoListener : ALPS.ALPSListener {
    private const int TEXT_COLOR = unchecked((int)0xFF008BFF);

    //Define preference keys.
    private static string key_fps = "fps";
    private static string key_resolution = "resolution";
    private static string key_camera_scroll_mode = "camera_scroll_mode";
    private static string key_camera_scroll_speed = "camera_scroll_speed";
    private static string key_camera_scroll_auto = "camera_scroll_auto";
    private static string key_camera_scroll_auto_speed = "camera_scroll_auto_speed";
    private static string key_camera_scroll_auto_bidirectional = "camera_scroll_auto_bidirectional";
    private static string key_camera_scroll_position_center = "camera_scroll_position_center";
    private static string key_camera_scroll_position_range = "camera_scroll_position_range";
    private static string key_text_label = "text_label";
    private static string key_text_color = "text_color";
    private static string key_promotion = "promotion";
    private static string key_upgrade = "upgrade";
    private static string key_downgrade = "downgrade";
    private static string key_refresh = "refresh";

    //Variabes to hold the preference values.
    private int mFPS;
    private float mResolutionScale;
    private int mCameraScrollMode;
    private float mCameraScrollSpeed;
    private bool mCameraScrollAuto;
    private float mCameraScrollAutoSpeed;
    private bool mCameraScrollAutoBidirectional;
    private float mCameraScrollPositionCenter;
    private float mCameraScrollPositionRange;
    private string mTextLabel;
    private int mTextColor;

    private GameObject mCube;
    private TextMesh mText;

    /**
	 * ALPS will call this method once at startup. Keep all intitialization code here.
	 */
    public ALPSDemoListener() {
        //Configure preference activity.
        ALPS.setResetButtonEnabled(true); //Default is true
        ALPS.setPreferenceName("alps_preferences", new string[]{"hidden1", "hidden2"}); //Default is alps_preferences. You may choose to show a different preferences xml based on if user has purchased content or not.

        //Lookup once and save.
        mCube = GameObject.Find("Cube");
        mText = GameObject.Find("Text").GetComponent<TextMesh>();

        //Load current preference settings, or defaults if the keys don't exist(First launch).
        mFPS = 5 + ALPS.getPrefInt(key_fps, 100) * 55 / 100;
        mResolutionScale = 0.1f + 0.9f * ALPS.getPrefInt(key_resolution, 60) / 100f;
        mCameraScrollMode = ALPS.getPrefInt(key_camera_scroll_mode, 0);
        mCameraScrollSpeed = 0.05f + 0.95f * ALPS.getPrefInt(key_camera_scroll_speed, 50) / 100f;
        mCameraScrollAuto = ALPS.getPrefBool(key_camera_scroll_auto, true);
        mCameraScrollAutoSpeed = 0.05f + 0.95f * ALPS.getPrefInt(key_camera_scroll_auto_speed, 50) / 100f;
        mCameraScrollAutoBidirectional = ALPS.getPrefBool(key_camera_scroll_auto_bidirectional, true);
        mCameraScrollPositionCenter = ALPS.getPrefInt(key_camera_scroll_position_center, 50) / 100f;
        mCameraScrollPositionRange = ALPS.getPrefInt(key_camera_scroll_position_range, 100) / 100f;
        mTextLabel = ALPS.getPrefString(key_text_label, "ALPS Demo");
        mTextColor = ALPS.getPrefInt(key_text_color, TEXT_COLOR);
        applyAllPreferences();
    }


    /**
	 * ALPS will callback to this method whenever the surface size changes. ALPS takes care of all the resizing operations
	 * normally. However a live wallapper app may add additional code here in case they have addition stuff to do on resize.
     * This method also gets called whenever a new instance comes into visible state.
	 */
    public void onSizeChanged(int width, int height) {
        ALPS.showToast("onSizeChanged " + width +" , "+ height);
        Debug.Log("onSizeChanged " + width +" , "+ height);
    }

    /**
     * ALPS will callback to this method whenever its state changes. ALPS taked care of putting UnityPlayer 
     * into paused state when live wallpaper becomes invisible as well as resuming it again when it becomes visible. It also
     * handles the complex lifecycle management of the UnityPlayer as user navigates between activity, preview and wallpaper.
     * Live wallpapers generally won't need to do anything here.
     */
    public void onStateChanged(ALPS.State state){
        ALPS.showToast("onStateChanged " + state);
        Debug.Log("onStateChanged " + state);

        //Access the context object and make some method calls.
        if(state == ALPS.State.ACTIVITY){
            AndroidJavaObject activity = ALPS.getContext();
            AndroidJavaObject resources = activity.Call<AndroidJavaObject>("getResources");
            AndroidJavaObject packageName = activity.Call<AndroidJavaObject>("getPackageName");

            int viewId = resources.Call<int>("getIdentifier", "alps_id_activity_title", "id", packageName);
            AndroidJavaObject view = activity.Call<AndroidJavaObject>("findViewById", viewId);

            view.Call("setOnClickListener", new ClickListener());
        }
    }


    /**
	 * ALPS will callback to this method whenever the user resets the preferences from actionbar in the preferences activity. 
	 * A live wallpaper app may want to add code here to handle additional keys that they may use.
	 */
    public void onPlayerPrefsReset() {
        ALPS.setPrefInt(key_fps, 100);
        ALPS.setPrefInt(key_resolution, 60);
        ALPS.setPrefInt(key_camera_scroll_mode, 0);
        ALPS.setPrefInt(key_camera_scroll_speed, 50);
        ALPS.setPrefBool(key_camera_scroll_auto, true);
        ALPS.setPrefInt(key_camera_scroll_auto_speed, 50);
        ALPS.setPrefBool(key_camera_scroll_auto_bidirectional, true);
        ALPS.setPrefInt(key_camera_scroll_position_center, 50);
        ALPS.setPrefInt(key_camera_scroll_position_range, 100);
        ALPS.setPrefString(key_text_label, "ALPS Demo");
        ALPS.setPrefInt(key_text_color, TEXT_COLOR);
        applyAllPreferences();
    }


    /**
	 * ALPS will callback to this method whenever the user changes a single preference from the in the preferences activity. 
	 * A live wallpaper app may want to add code here to handle additional keys that they may use.
	 */
    public void onPlayerPrefsChanged(string key) {
        //Handle promotion click and return.
        if (key_promotion.Equals(key)) {
            bool isSucess = ALPS.sendIntent("android.intent.action.VIEW", "details?id=com.androbean.app.launcherpp.freemium");
            if (!isSucess) {
                isSucess = ALPS.sendIntent("android.intent.action.VIEW", "https://play.google.com/store/apps/details?id=com.androbean.app.launcherpp.freemium");
                if (!isSucess) {
                    ALPS.showToast("Action failed !");
                }
            }

            return;
        }

        //Handle upgrade/downgrade key
        if (key_upgrade.Equals(key)) {
            ALPS.iapPurchase("gopro");
        }else if (key_downgrade.Equals(key)) {
            ALPS.iapConsume("gopro");
        }else if (key_refresh.Equals(key)) {
            ALPS.iapRefresh();
        }

        //Handle actual preference changes.
        if (key_fps.Equals(key)) {
            mFPS = 5 + ALPS.getPrefInt(key) * 55 / 100;
        }
        if (key_resolution.Equals(key)) {
            mResolutionScale = 0.1f + 0.9f * ALPS.getPrefInt(key_resolution, 60) / 100f;
        }
        else if (key_camera_scroll_mode.Equals(key)) {
            mCameraScrollMode = ALPS.getPrefInt(key_camera_scroll_mode);
        }
        else if (key_camera_scroll_speed.Equals(key)) {
            mCameraScrollSpeed = 0.05f + 0.95f * ALPS.getPrefInt(key_camera_scroll_speed) / 100f;
        }
        else if (key_camera_scroll_auto.Equals(key)) {
            mCameraScrollAuto = ALPS.getPrefBool(key_camera_scroll_auto);
        }
        else if (key_camera_scroll_auto_speed.Equals(key)) {
            mCameraScrollAutoSpeed = 0.05f + 0.95f * ALPS.getPrefInt(key_camera_scroll_auto_speed) / 100f;
        }
        else if (key_camera_scroll_auto_bidirectional.Equals(key)) {
            mCameraScrollAutoBidirectional = ALPS.getPrefBool(key_camera_scroll_auto_bidirectional);
        }
        else if (key_camera_scroll_position_center.Equals(key)) {
            mCameraScrollPositionCenter = ALPS.getPrefInt(key_camera_scroll_position_center) / 100f;
        }
        else if (key_camera_scroll_position_range.Equals(key)) {
            mCameraScrollPositionRange = ALPS.getPrefInt(key_camera_scroll_position_range) / 100f;
        }
        else if (key_text_label.Equals(key)) {
            mTextLabel = ALPS.getPrefString(key_text_label);
        }
        else if (key_text_color.Equals(key)) {
            mTextColor = ALPS.getPrefInt(key_text_color);
        }

        applyAllPreferences();
    }


    /**
	 * ALPS will callback to this method with scrollPosition values in the range of [0, 1]. This method will get invoked any time
	 * when the scrollPosition changes, be it by user drag, fling or even auto scrolling. A live wallapper app may use this to move
	 * the camera based on scrollPosition.
     * 
     * THIS IS A SPECIAL CALLBACK WHICH IS MADE IN THE UNITY'S THREAD FOR PERFORMANCE REASONS. SO WE CAN DIRECTLY CHANGE THE SCENE.
	 */
    public void onScrollChanged(float scrollPosition) {
        if(Camera.current == null){
            return;
        }

        Camera.current.transform.position = new Vector3(2.89f * Mathf.Sin(scrollPosition * -2 * 3.14f), 1.363f, 2.89f * Mathf.Cos(scrollPosition * -2 * 3.14f));        
        Camera.current.transform.LookAt(mCube.transform.position); 
    }


    /**
	 * Utility method to set wallpaper behaviour based on current settings. It also makes necesary calls to ALPS to
	 * set the relavant properties. A live wallpaper app may want to add additional code here based on their requirements.
	 */
    private void applyAllPreferences() {
        //Apply performance settings.
        ALPS.setFPS(mFPS);
        ALPS.setResolutionScale(mResolutionScale);

        //Apply scrolling settings.
        switch (mCameraScrollMode) {
            case 0:
                ALPS.useFreeformScrolling(mCameraScrollSpeed * 5, mCameraScrollAuto, mCameraScrollAutoSpeed * 5, mCameraScrollAutoBidirectional);
                break;
            case 1:
                ALPS.useHomescreenScrolling(mCameraScrollPositionCenter, mCameraScrollPositionRange);
                break;
        }

        //Disable preferences that are not relevant for the current settings.
        string launcherScrollKeys = ALPS.encode(key_camera_scroll_position_center, key_camera_scroll_position_range);
        string freeScrollKeys = ALPS.encode(key_camera_scroll_speed, key_camera_scroll_auto, key_camera_scroll_auto_speed, key_camera_scroll_auto_bidirectional);
        if (mCameraScrollMode == 0) {
            ALPS.setPreferenceKeysActivation(freeScrollKeys, launcherScrollKeys);
        }else {
            ALPS.setPreferenceKeysActivation(launcherScrollKeys, freeScrollKeys);
        }  

        //Do scene update only in Unity thread. App may crash or behave wierdly otherwise. Only onScrollChanged callback can directly change scenes.
        ALPS.runInUpdate(() => {
            //Apply text settings
            mText.text = mTextLabel;
            mText.color = ALPS.argbToColor(mTextColor);
        });
    }

    /**
     * This method is invoked by ALPS when user has performed a long-press gesture.
     */
    public void onLongPress(float x, float y) {
        ALPS.showToast("onLongPress: " + x + ", " + y);
        Debug.LogWarning("onLongPress: " + x + ", " + y);

        ALPS.launchWallpaperPreview();
    }

    /**
     * This method is invoked by ALPS when user has performed a double-tap gesture.
     */
    public void onDoubleTap(float x, float y) {
        ALPS.showToast("onDoubleTap: " + x + ", " + y);
        Debug.LogWarning("onDoubleTap: " + x + ", " + y);

        ALPS.launchPreferencesActivity();
    }

    /**
     * This method is invoked by ALPS when a single-tap is performed, that is not part 
     * of a double-tap gesture.
     */
    public void onSingleTapConfirmed(float x, float y) {
        ALPS.showToast("onSingleTapConfirmed: " + x + ", " + y);
        Debug.LogWarning("onSingleTapConfirmed: " + x + ", " + y);
    }

    /**
     * This method is invoked by ALPS whenever a single-tap is performed by the user.
     * It may be the first tap of a double-tap gesture.
     */
    public void onSingleTapUp(float x, float y) {
        ALPS.showToast("onSingleTapUp: " + x + " , " + y);
        Debug.LogWarning("onSingleTapUp: " + x + " , " + y);
    }

    /**
     * ALPS will fire this callback when user clicks the settings button in live wallpaper
     * preview and enters the preference activity. An application may use this callback to 
     * trigger custom settings implemented with Unity, and immediately close the
     * PreferenceActivity using the api closePreferencesActivity.
     */
    public void onPreferencesOpened() {
        ALPS.showToast("onPreferencesOpened");
        Debug.LogWarning("onPreferencesOpened");

        if (ALPS.iapGetSkuData("gopro").purchaseToken.Equals("null")) {
            ALPS.adMobShowRewardedVideo();
        }
    }

    /**
     *ALPS will fire this callback when user pressed back button to return from preference
     * activity to the live wallpaper preview.
     */
    public void onPreferencesClosed() {
        ALPS.showToast("onPreferencesClosed");
        Debug.LogWarning("onPreferencesClosed");

        if (ALPS.iapGetSkuData("gopro").purchaseToken.Equals("null")) {
            ALPS.adMobShowInterstitial();
        }
    }
}

class ClickListener : AndroidJavaProxy {
    public ClickListener() : base("android.view.View$OnClickListener") { 
    }

    public void onClick(AndroidJavaObject view){
        ALPS.showToast("Clicked on title");
        Debug.Log("Clicked on title");
    }
}
