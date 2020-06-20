using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using UnityEngine;

/*
        ╔═╗╦  ╔═╗╔═╗╔═╗╔═╗  ╔╦╗╔═╗  ╔╗╔╔═╗╔╦╗  ╔╦╗╔═╗╔╦╗╦╔═╗╦ ╦
        ╠═╝║  ║╣ ╠═╣╚═╗║╣    ║║║ ║  ║║║║ ║ ║   ║║║║ ║ ║║║╠╣ ╚╦╝
        ╩  ╩═╝╚═╝╩ ╩╚═╝╚═╝  ═╩╝╚═╝  ╝╚╝╚═╝ ╩   ╩ ╩╚═╝═╩╝╩╚   ╩ , UNLESS YOU KNOW WHAT YOU ARE DOING. 😉
*/

/* ALPS VERSION 6.7 */

/**
 * ALPS Core script. This class interfaces with the ALPS Java core. 
 * 
 * For use by purchased licencees only. Do not redistribute any part of ALPS.
 * © All rights reserved to AndrobeanStudio@gmail.com
 */
public class ALPS : MonoBehaviour {	
	//State variables.
	private static float mResolutionScale = 1;
	private static int mWidth, mHeight;
	private const int FRAME_TRIGGER = 1;

    //Utility objects
    private static StringBuilder builder = new StringBuilder();

	//Listener and ALPS link.
	private static AndroidJavaClass sAlpsInterface;
    private static AndroidJavaClass sAlpsAdmobInterface;
    private static AndroidJavaClass sAlpsBillingInterface;
	private static ALPSListener sALPSListener;
    private static AdMobListener sAdMobListener;
    private static IAPListener sIAPListener;

    private static object syncLock = new object();
	private static Queue<Action> queue = new Queue<Action>();

    public enum State {
        INVISIBLE, //The live wallpaper is not visible.
        ACTIVITY,  //The live wallpaper is being displayed in the main activity.
        PREVIEW,   //The live wallpaper is being displayed in the live wallpaper preview.
        WALLPAPER,  //The live wallpaper is being displayed on the home screen.
        PREFERENCES //The live wallpaper is active and is behind Preference activity.
    }

    /**
	 * The interface to listen for callbacks from ALPS system. It includes
	 * callbacks for resizing, preferences and scrolling.
	 */
    public interface ALPSListener{
		/**
		 * ALPS will fire this callback when window size changes, possible 
		 * because of a device rotation. It is fired once at startup also.
		 */
		void onSizeChanged (int width, int height);

        /**
         * ALPS will fire this callback when the display state changes.
         * State can be any one of the constants defined by ALPS.State enum.
         */
        void onStateChanged(State state);

        /**
        * ALPS will callback to this method with scrollPosition values in the range of [0, 1]. This method will get invoked any time
        * when the scrollPosition changes, be it by user drag, fling or even auto scrolling. A live wallapper app may use this to move
        * the camera based on scrollPosition.
        * 
        * THIS IS A SPECIAL CALLBACK WHICH IS MADE IN THE UNITY'S THREAD FOR PERFORMANCE REASONS. SO WE CAN DIRECTLY CHANGE THE SCENE.
        */
        void onScrollChanged(float scrollPosition);

		/**
		 * ALPS will fire this callback when user resets preferences by clicking
		 * on the reload icon in the actionbar in preference Activity.
		 */
		void onPlayerPrefsReset ();

		/**
		 * ALPS will fire this callback when user changes any preference from 
		 * preference Activity.
		 */
		void onPlayerPrefsChanged (string key);

        /**
         * ALPS will fire this callback when user does a long-press gesture.
         */
        void onLongPress(float x, float y);

        /**
         * ALPS will fire this callback when user has performed a double-tap gesture.
         */
        void onDoubleTap(float x, float y);

        /**
         * ALPS will fire this callback when a single tap has been confirmed. That is,
         * it is not part of a double-tap gesture.
         */
        void onSingleTapConfirmed(float x, float y);

        /**
         * ALPS will fire this callback whenever a single touch-up has occured. This
         * may be the first tap of a double-tap gesture. It is not fired on a long-press.
         */
        void onSingleTapUp(float x, float y);

        /**
         * ALPS will fire this callback when user clicks the settings button in live wallpaper
         * preview and enters the preference activity.
         */
        void onPreferencesOpened();

        /**
         *ALPS will fire this callback when user pressed back button to return from preference
         * activity to the live wallpaper preview.
         */
        void onPreferencesClosed();
    }

    /**
	 * The interface to listen for AdMob related callbacks from ALPS system.
     * It includes callbacks for all events reported from AdMob.
	 */
    public interface AdMobListener {
        /**
         * ALPS will fire this callback when an ad has finished loading.
         */
        void onLoaded(string adUnitId);

        /**
         * ALPS will fire this callback when an ad has failed to load.
         * The code as returned by AdMob also will be passed.
         */
        void onFailedToLoad(string adUnitId, int code);

        /**
         * ALPS will fire this callback when user clicked on an Ad.
         */
        void onOpened(string adUnitId);

        /**
         * ALPS will fire this callback when an Ad click proceeds and leaves the app.
         */
        void onLeftApplication(string adUnitId);

        /**
         * ALPS will fire this callback when an ad has stopped being displayed.
         */
        void onClosed(string adUnitId);

        /**
         * ALPS will fire this callback when a rewarded video starts playback.
         */
        void onVideoStart(string adUnitId);

        /**
         * ALPS will fire this callback when a rewarded video complets playback.
         */
        void onVideoComplete(string adUnitId);

        /**
         * ALPS will fire this callback when a reward has been granted from a rewardrd video.
         */
        void onVideoRewarded(string adUnitId, string type, int amount);

        /**
         * ALPS will fire this callback when an api call has been made to show an interstitial
         * or a rewarded video ad, but the ad is not ready.
         */
        void onAdMobNotReady(string adUnitId);
    }

    /**
     * The interface to listen for IAP related callbacks from ALPS system.
     * It includes callbacks for all events related to managed items. Subcriptions are not supported.
     */
    public interface IAPListener {
        /**
         * ALPS will fire this callback after connecting to IAP service and once purchase statuses are ready.
         */
        void onConnected();

        /**
         * ALPS will fire this callback when the IAP service gets disconnected. Applications might want
         * to attempt re-connecting at this point.
         */
        void onDisconnected();

        /**
         * ALPS will fire this callback when a purchase flow completes successfully.
         */
        void onPurchaseSuccess(SkuData skuData, int code);

        /**
         * ALPS will fire this callback when a purchase flow fails for a reason as indicated.
         */
        void onPurchaseFailure(string sku, int reason);

        /**
         * ALPS will fire this callback when a consume request completes successfully.
         */
        void onConsumeSuccess(string sku, int code);

        /**
         * ALPS will fire this callback when a consume request fails for a reason as indicated.
         */
        void onConsumeFailure(string sku, int reason);

        /**
         * ALPS will fire this callback when an iap refresh process is complete.
         */
        void onRefreshComplete(int code);
    }

    /**
     * This is a data class holding information about an sku. For items not owned, the purchaseToken
     * and purchaseTime fields will be "null" and 0 respectively.
     */
    public class SkuData {
        public string sku; //sku of the item as entered in PlayStore developer console.
        public string title; //Title as entered in PlayStore developer console.
        public string description; //Description as entered in PlayStore developer console.
        public string price; //Formatted price with currency symbol.

        public long purchaseTime; //Purchase time in milliseconds since the epoch (Jan 1, 1970). 0 if not owned.
        public string purchaseToken; //A unique token identifying the purchase. Will be "null" if not owned.
    }

    class AlpsCallback : AndroidJavaProxy {
        public AlpsCallback() : base("com.androbean.android.unityplugin.alps.AlpsInterface$AlpsCallback") { 

        }

        /**
        * This method is invoked by AlpsUnityInterface when user has performed a long-press gesture.
        */
        public void onLongPress(float x, float y){
            if (sALPSListener != null) {
                sALPSListener.onLongPress(x, y);
            }
        }

        /**
        * This method is invoked by AlpsUnityInterface when user has performed a double-tap gesture.
        */
        public void onDoubleTap(float x, float y){
            if (sALPSListener != null) {
                sALPSListener.onDoubleTap(x, y);
            }
        }

        /**
        * This method is invoked by AlpsUnityInterface when a single-tap is performed, that is not part 
        * of a double-tap gesture.
        */
        public void onSingleTapConfirmed(float x, float y){
            if (sALPSListener != null) {
                sALPSListener.onSingleTapConfirmed(x, y);
            }
        }

        /**
        * This method is invoked by AlpsUnityInterface whenever a single-tap is performed by the user.
        * It may be the first tap of a double-tap gesture.
        */
        public void onSingleTapUp(float x, float y){
            if (sALPSListener != null) {
                sALPSListener.onSingleTapUp(x, y);
            }
        }

        /**
        * ALPS will fire this callback when the display state changes.
        */
        public void onStateChanged(String state){
            if(sALPSListener != null){
                if("invisible".Equals(state)){
                    sALPSListener.onStateChanged (State.INVISIBLE);
                }else if("activity".Equals(state)){
                    sALPSListener.onStateChanged (State.ACTIVITY);
                }else if("preview".Equals(state)){
                    sALPSListener.onStateChanged (State.PREVIEW);
                }else if("wallpaper".Equals(state)){
                    sALPSListener.onStateChanged (State.WALLPAPER);
                }else if("preferences".Equals(state)){
                    sALPSListener.onStateChanged (State.PREFERENCES);
                }else{
                    throw new Exception("Unknown state: "+state);
                }
            }
        }

        /**
        * This method is invoked by AlpsUnityInterface when window size changes. It is fired once at startup also. 
        */
        public void onSizeChanged(int width, int height){
            //Save the size.
            mWidth = width;
            mHeight = height;

            //Apply the resolution settings.
            if(mWidth > 0 && mHeight > 0){
                int newWidth = (int)(mWidth * mResolutionScale);
                int newHeight = (int)(mHeight * mResolutionScale);
            
                if(Screen.currentResolution.width != newWidth || Screen.currentResolution.height != newHeight){
                    Screen.SetResolution(newWidth, newHeight, true);
                }
            } 

            //Check and fire listener.
            if(sALPSListener != null){
                sALPSListener.onSizeChanged (mWidth, mHeight);
            }
        }

        /**
        * This method is invoked by AlpsUnityInterface when user resets preferences by clicking on the reload icon
        * in the actionbar in preference Activity. It invokes respective method on ALPSListener if once is registered.
        */
        public void onPlayerPrefsReset(){
            if (sALPSListener != null) {
                sALPSListener.onPlayerPrefsReset ();   
            }
        }

        /**
        * This method is invoked by AlpsUnityInterface when user changes any preference from preference Activity.
        * It invokes respective method on ALPSListener if once is registered.
        */
        public void onPlayerPrefsChanged(String key){
            if(sALPSListener != null){
                sALPSListener.onPlayerPrefsChanged (key);
            }
        }

        /**
        * ALPS will fire this callback when user clicks the settings button in live wallpaper
        * preview and enters the preference activity.
        */
        public void onPreferencesOpened(){
            if (sALPSListener != null) {
                sALPSListener.onPreferencesOpened();
            }
        }

        /**
        * ALPS will fire this callback when user pressed back button to return from preference
        * activity to the live wallpaper preview.
        */
        public void onPreferencesClosed(){
            if (sALPSListener != null) {
                sALPSListener.onPreferencesClosed();
            }
        }
    }

    class AlpsAdMobCallback : AndroidJavaProxy {
        public AlpsAdMobCallback() : base("com.androbean.android.unityplugin.alps.admob.AlpsAdMobInterface$AlpsAdMobCallback") { 

        }

        /**
        * ALPS will fire this callback when an ad has finished loading.
        */
        public void onAdMobLoaded(string adUnitId) {
            if (sAdMobListener != null) {
                sAdMobListener.onLoaded(adUnitId);
            }
        }

        /**
        * ALPS will fire this callback when an ad has failed to load.
        * The code as returned by AdMob also will be passed.
        */
        public void onAdMobFailedToLoad(string adUnitId, int errorCode) {
            if (sAdMobListener != null) {
                sAdMobListener.onFailedToLoad(adUnitId, errorCode);
            }
        }

        /**
        * ALPS will fire this callback when user clicked on an Ad.
        */
        public void onAdMobOpened(string adUnitId) {
            if (sAdMobListener != null) {
                sAdMobListener.onOpened(adUnitId);
            }
        }

        /**
        * ALPS will fire this callback when an Ad click proceeds and leaves the app.
        */
        public void onAdMobLeftApplication(string adUnitId) {
            if (sAdMobListener != null) {
                sAdMobListener.onLeftApplication(adUnitId);
            }
        }

        /**
        * ALPS will fire this callback when an ad has stopped being displayed.
        */
        public void onAdMobClosed(string adUnitId) {
            if (sAdMobListener != null) {
                sAdMobListener.onClosed(adUnitId);
            }
        }

        /**
        * ALPS will fire this callback when a rewarded video starts playback.
        */
        public void onAdMobVideoStart(string adUnitId) {
            if (sAdMobListener != null) {
                sAdMobListener.onVideoStart(adUnitId);
            }
        }

        /**
        * ALPS will fire this callback when a rewarded video complets playback.
        */
        public void onAdMobVideoComplete(string adUnitId) {
            if (sAdMobListener != null) {
                sAdMobListener.onVideoComplete(adUnitId);
            }
        }

        /**
        * ALPS will fire this callback when a reward has been granted from a rewardrd video.
        */
        public void onAdMobVideoRewarded(string adUnitId, string type, int amount) {
            if (sAdMobListener != null) {
                sAdMobListener.onVideoRewarded(adUnitId, type, amount);
            }
        }

        /**
        * ALPS will fire this callback when an api call has been made to show an interstitial
        * or a rewarded video ad, but the ad is not ready.
        */
        public void onAdMobNotReady(string adUnitId) {
            if (sAdMobListener != null) {
                sAdMobListener.onAdMobNotReady(adUnitId);
            }
        }
    }

    class AlpsIAPCallback : AndroidJavaProxy{
        public AlpsIAPCallback() : base("com.androbean.android.unityplugin.alps.billing.AlpsBillingInterface$AlpsIAPCallback"){

        }

        /**
        * ALPS will fire this callback after connecting to IAP service and once purchase statuses are ready.
        */
        public void onIAPConnected() {
            if (sIAPListener != null) {
                sIAPListener.onConnected();
            }
        }
        
        /**
        * ALPS will fire this callback when an iap refresh process is complete.
        */
        public void onIAPRefreshComplete(int responseCode) {
            if (sIAPListener != null) {
                sIAPListener.onRefreshComplete(responseCode);
            }
        }

        /**
        * ALPS will fire this callback when the IAP service gets disconnected. Applications might want
        * to attempt re-connecting at this point.
        */
        public void onIAPDisconnected() {
            if (sIAPListener != null) {
                sIAPListener.onDisconnected();
            }
        }

        /**
        * ALPS will fire this callback when a purchase flow completes successfully.
        */
        public void onIAPPurchaseSuccess(string sku, string title, string description, string price, long purchaseTime, string purchaseToken, int responseCode) {
            if (sIAPListener != null) {
                SkuData skuData = new SkuData();

                skuData.sku = sku;
                skuData.title = title;
                skuData.description = description;
                skuData.price = price;
                skuData.purchaseTime = purchaseTime;
                skuData.purchaseToken = purchaseToken;

                sIAPListener.onPurchaseSuccess(skuData, responseCode);
            }
        }

        /**
        * ALPS will fire this callback when a purchase flow fails for a reason as indicated.
        */
        public void onIAPPurchaseFailure(string sku, int responseCode) {
            if (sIAPListener != null) {
                sIAPListener.onPurchaseFailure(sku, responseCode);
            }
        }

        /**
        * ALPS will fire this callback when a consume request completes successfully.
        */
        public void onIAPConsumeSuccess(string sku, int responseCode) {
            if (sIAPListener != null) {
                sIAPListener.onConsumeSuccess(sku, responseCode);
            }
        }

        /**
        * ALPS will fire this callback when a consume request fails for a reason as indicated.
        */
        public void onIAPConsumeFailure(string sku, int responseCode) {
            if (sIAPListener != null) {
                sIAPListener.onConsumeFailure(sku, responseCode);
            }
        }
    }

    /**
     * ALPS initialization. Please don't modify.
     */
    private static void init() {
        //Set default values
        Screen.orientation = ScreenOrientation.AutoRotation; //Setting this to some other values causes Screen.SetResolution to not work initially.
        setFPS(40);
        setResolutionScale(0.67f);

        //Check and load AlpsUnityInterface java class.
        if (Application.platform == RuntimePlatform.Android) {
            sAlpsInterface = new AndroidJavaClass("com.androbean.android.unityplugin.alps.AlpsInterface");
            sAlpsInterface.CallStatic("setAlpsCallback", new AlpsCallback());

            try{
                sAlpsAdmobInterface = new AndroidJavaClass("com.androbean.android.unityplugin.alps.admob.AlpsAdMobInterface");
                sAlpsAdmobInterface.CallStatic("setAlpsAdMobCallback", new AlpsAdMobCallback());
            }catch (Exception e){
                Debug.LogWarning(e);
            }

            try{
                sAlpsBillingInterface = new AndroidJavaClass("com.androbean.android.unityplugin.alps.billing.AlpsBillingInterface");
                sAlpsBillingInterface.CallStatic("setAlpsIAPCallback", new AlpsIAPCallback());
            }catch (Exception e){
                Debug.LogWarning(e);
            }

        } else {
            Debug.LogWarning("Not running on Android: ALPS will be disabled !");
        }
    }

    /**
    * ALPS will fire this callback whenever the scroll position changes. 
    * It may be triggered by a drag operation by user, an auto-scrolling
    * action or by the homescreen app based on the current scroll-mode.
    *
    * NOTE: THIS IS A SPECIAL CALLBACK WHICH IS MADE IN THE UNITY'S THREAD
    * FOR PERFORMANCE REASONS.
    */
    void onScrollChanged (string data){
        if(sALPSListener != null){
            sALPSListener.onScrollChanged(parseFloat(data));
        }
    }

	/**
	 * This method is invoked by Unity once at startup.
	 */
	void Awake() {
        //Initialize if not already done. It is possible that application would have called one of the ALPS methods below which already did the 'init' call.
        //This can happen for example, if applications called those methods from an 'Awake' method of their own script.
        if (sAlpsInterface == null) {
            init();
        }
    }

	/**
	 * This method is invoked by Unity before every frame.
	 */
	void Update() {
        if(mWidth > 0 && mHeight > 0){
            int width = (int)(mWidth * mResolutionScale);
            int height = (int)(mHeight * mResolutionScale);
            
            if(Screen.currentResolution.width != width || Screen.currentResolution.height != height){
                Screen.SetResolution(width, height, true);
            }
        } 

        lock(syncLock){
            foreach (Action action in queue) {
                action.Invoke();
            }
        }
	}

    public static string encode(params string[] array){
		if(array == null){
			return null;
		}
		
        if(array.Length == 1){
            return array[0];
        }

        for(int i=0; i<array.Length; i++){
            if(array[i] == null){
                array[i] = "null";
            }
        }

        builder.Clear();

        builder.Append(array.Length);
        builder.Append(':');

        foreach(string item in array){
            builder.Append(item.Length);
            builder.Append(':');
        }

        foreach(string item in array){
            builder.Append(item);
        }

        return builder.ToString();
    }

    public static string[] decode(string data){
		if(data == null){
			return null;
		}
		
        int markerIndex = data.IndexOf(':');
        if(markerIndex == -1){
            return new string[]{data};
        }        

        int count = parseInt(data.Substring(0, markerIndex));
        string[] result = new string[count];
        int[] lengths = new int[count];

        markerIndex++;
        for(int i=0; i<count; i++){
            int markerIndexNext = data.IndexOf(':', markerIndex);
            lengths[i] = parseInt(data.Substring(markerIndex, markerIndexNext-markerIndex));
            markerIndex = markerIndexNext + 1;
        }

        for(int i=0; i<count; i++){
            result[i] = data.Substring(markerIndex, lengths[i]);
            markerIndex += lengths[i];
        }

        return result;
    }

    private static int parseInt(string data){
        if("0".Equals(data)){
            return 0;
        }else{
            return Int32.Parse(data);
        }
    }

    private static float parseFloat(string data){
        if("0".Equals(data)){
            return 0;
        }else{
            return float.Parse(data, CultureInfo.InvariantCulture);;
        }
    }

    /////////////////////////////////////////////////////// API /////////////////////////////////////////////////////////////////////

    /**
	 * Applications has to use this api to set an ALPSListener in order to listen for the various callbacks that ALPS provides.
     * Until this method is called, ALPS is not 'ready' and user will see a 'Not ready' message if they clicked on the settings button.
	 */
    public static void setALPSListener(ALPSListener alpsListener) {
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sALPSListener = alpsListener;

        sAlpsInterface.CallStatic("setReady");
    }

    /**
	 * ALPS don't use Unity's user preference system. Applications has to use this API to save an Integer preference in ALPS.
	 */
    public static void setPrefInt(string key, int value){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("setPrefInt", key, value);
    }

	/**
	 * ALPS don't use Unity's user preference system. Applications has to use this API to get an Integer preference from ALPS.
	 */
	public static int getPrefInt(string key, int defaultValue=0){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        return sAlpsInterface.CallStatic<int>("getPrefInt", key, defaultValue);
    }

	/**
	 * ALPS don't use Unity's user preference system. Applications has to use this API to save a Float preference in ALPS.
	 */
	public static void setPrefFloat(string key, float value){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("setPrefFloat", key, value);
    }

	/**
	 * ALPS don't use Unity's user preference system. Applications has to use this API to get a Float preference from ALPS.
	 */
	public static float getPrefFloat(string key, float defaultValue=0){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        return sAlpsInterface.CallStatic<float>("getPrefFloat", key, defaultValue);
    }

	/**
	 * ALPS don't use Unity's user preference system. Applications has to use this API to save a String preference in ALPS.
	 */
	public static void setPrefString(string key, string value){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("setPrefString", key, value);
    }

	/**
	 * ALPS don't use Unity's user preference system. Applications has to use this API to get a String preference from ALPS.
	 */
	public static string getPrefString(string key, string defaultValue=""){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        return sAlpsInterface.CallStatic<string>("getPrefString", key, defaultValue);
    }

	/**
	 * ALPS don't use Unity's user preference system. ALPS supports Boolean preferences also.
	 * Applications has to use this API to save a Boolean preference in ALPS.
	 */
	public static void setPrefBool(string key, bool value){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("setPrefBoolean", key, value);
    }

	/**
	 * ALPS don't use Unity's user preference system.  ALPS supports Boolean preferences also.
	 * Applications has to use this API to get a Boolean preference from ALPS.
	 */
	public static bool getPrefBool(string key, bool defaultValue=false){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        return sAlpsInterface.CallStatic<bool>("getPrefBoolean", key, defaultValue);
    }

	/**
	 * Applications can use this API to enable/disable a set of preferences by specifying their keys.
	 */
	public static void setPreferenceKeysActivation(string enabledKeys, string disabledKeys){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("setPreferenceKeysActivation", enabledKeys, disabledKeys);
    }

	/**
	 * ALPS allows applications to set the desired frames-per-second using this API. Normally fps ranges from 0 to 60.
	 */
	public static void setFPS(int fps){
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = fps;
	}

	/**
	 * ALPS allows applications to scale the resolution down to speedup rendering as well as reduce battery drain. Scale is
	 * specified in [0, 1]. 0 being the lowest and 1 being the highest possible. Please note that the exact resolution is 
	 * automatically chosen by the device based on its hardware scaler's capability.
	 */
	public static void setResolutionScale(float resolutionScale){
		mResolutionScale = resolutionScale;        
	}

	/**
	 * Applications may call this api to set current scrolling mode as 'freeform'. This allows for a highly configurable
	 * scrolling behaviour including auto-scrolling. This scrolling is not linked to the homescreen scrolling of the launcher.
	 */
	public static void useFreeformScrolling(float speed, bool autoScroll, float autoScrollSpeed, bool isBidirectional){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("useFreeformScrolling", speed, autoScroll, autoScrollSpeed, isBidirectional);
    }

	/**
	 * Applications may call this api to set current scrolling mode as 'homescreen'.
     * This allows for a scrolling that happens in perfect synchrony to the homescreen scrollin by the launcher app.
	 */
	public static void useHomescreenScrolling(float positionStart, float positionEnd){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("useHomescreenScrolling", positionStart, positionEnd);
    }

	/**
	 * Applications may use this api to make ALPS send an Intent with specified action and uri.
     * This may be used to open link or similar actions that may be done using an Intent in Android.
	 */ 
	public static bool sendIntent(string action, string uri){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        return sAlpsInterface.CallStatic<bool>("sendIntent", action, uri);
    }

    /**
     * Applications may use this api to programatically launch the live wallpaper preview.
     * Note that live wallpaper preview is a system app and we cannot programatically close it.
     * You can however bring the main activity back on top using the launchMainActivity API.
     */
    public static void launchWallpaperPreview(){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("launchWallpaperPreview");
    }

    /**
     * Applications may use this api to programatically launch the preferences activity.
     * For example, when user has double tapped.
     */
    public static void launchPreferencesActivity(){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("launchPreferencesActivity");
    }

    /**
     * Applications may use this api to programatically terminate the preferences activity.
     * This could be useful for a livewallpaper that shows custom preferences screen using Unity.
     */
    public static void closePreferencesActivity() {
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("closePreferencesActivity");
    }

    /**
     * Applications may use this api to programatically launch the main activity.
     * For example, to go back to main activity from live wallpaper preview.
     */
    public static void launchMainActivity() {
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("launchMainActivity");
    }

    /**
     * Applications may use this api to programatically terminate the main activity.
     * This could be useful for a livewallpaper to explicitly terminate the main activity to free up resources.
     */
    public static void closeMainActivity() {
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("closeMainActivity");
    }

    /**
    * Applications may use this api to show a native 'Toast' with specified string for a short duration.
    */
    public static void showToast(string label){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("showToast", label);
    }

	/**
	 * Applications may use this helper api to create a Unity Color object out of argb color integer value.
	 */
	public static Color argbToColor(int argb){ 
		return new Color32((byte)((argb >> 16) & 0xFF), (byte)((argb >> 8) & 0xFF), (byte)((argb) & 0xFF), (byte)((argb >> 24) & 0xFF));
    }

    /**
     * Applications may use this api to enable/disable the reload button in the preferences activity.
     * It will take effect on the next launch of preference activity.
     */
    public static void setResetButtonEnabled(bool enabled){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("setResetButtonEnabled", enabled);
    }

    /**
     * Applications may use this api to change the preference xml programatically.
     * It will take effect immediately.
     */
    public static void setPreferenceName(string name, string[] excludeKeys) {
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("setPreferenceName", name, encode(excludeKeys));
    }

    /**
     * Applications may use this api to change the preference layout xml programatically.
     * It will take effect immediately. Please note that here we change the layout file
     * that hosts the preferences. To change the preferences, please use setPreferenceName method.
     */
    public static void setPreferenceLayoutName(string name) {
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        sAlpsInterface.CallStatic("setPreferenceLayoutName", name);
    }

    /**
	 * Applications has to use this api to set an ALPSListener in order to listen for the various callbacks that ALPS provides.
     * Make sure to pass interstitialAdUnitId and rewardedVideoAdUnitId only if you really use them. This is because, we want to avoid 
     * uncecessary data usage if we never actually use them. For fremium apps, make sure to not call this method once user hs paid, so that
     * AdMob is not loaded, and memory usage is reduced. Behaviour is undefined if this api gets called multiple times.
	 */
    public static void adMobInitialize(AdMobListener adMobListener, string interstitialAdUnitId, string rewardedVideoAdUnitId) {
        //Initialize if not already done.
        if (sAlpsAdmobInterface == null) {
            init();
        }

        sAdMobListener = adMobListener;

        sAlpsAdmobInterface.CallStatic("adMobInitialize");

        if (interstitialAdUnitId != null) {
            sAlpsAdmobInterface.CallStatic("adMobLoadInterstitialAd", interstitialAdUnitId);
        }
        if (rewardedVideoAdUnitId != null) {
            sAlpsAdmobInterface.CallStatic("adMobLoadRewardedVideoAd", rewardedVideoAdUnitId);
        }
    }

    /**
     * Applications use this api to show an AdMob interstitial ad.
     * If the ad is not ready, an asynchronous callback will be made
     * to the AdMobListener on the onAdMobNotReady method.
     */
    public static void adMobShowInterstitial() {
        //Initialize if not already done.
        if (sAlpsAdmobInterface == null) {
            init();
        }

        sAlpsAdmobInterface.CallStatic("adMobShowInterstitial");
    }

    /**
     * Applications use this api to show an AdMob rewarded video ad.
     * If the ad is not ready, an asynchronous callback will be made
     * to the AdMobListener on the onAdMobNotReady method.
     */
    public static void adMobShowRewardedVideo() {
        //Initialize if not already done.
        if (sAlpsAdmobInterface == null) {
            init();
        }

        sAlpsAdmobInterface.CallStatic("adMobShowRewardedVideo");
    }

    /**
     * Applications may use this method to initialize IAP. The array of skus contains list of skus 
     * from PlayStore developer console that the application wants to use. Note that only managed
     * skus are supported. Subscriptions skus and invalid skus will cause undefined behaviour.
     */
    public static void iapConnect(IAPListener listener, string[] skus) {
        sIAPListener = listener;

        //Initialize if not already done.
        if (sAlpsBillingInterface == null) {
            init();
        }

        sAlpsBillingInterface.CallStatic("iapConnect", encode(skus));
    }

    /**
     * Applications may use this api to shutdown IAP and cleanup underlying resources.
     */
    public static void iapDisconnect() {
        //Initialize if not already done.
        if (sAlpsBillingInterface == null) {
            init();
        }

        sAlpsBillingInterface.CallStatic("iapDisconnect");
    }

    /**
     * Applications may use this api to get information about a given sku. For owned skus,
     * the purchasetoken will be a string other than "null", and purchaceTime will be non zero.
     */
    public static SkuData iapGetSkuData(string sku) {
        //Initialize if not already done.
        if (sAlpsBillingInterface == null) {
            init();
        }

        string datas = sAlpsBillingInterface.CallStatic<string>("iapGetSkuData", sku);
        if (datas == null) {
            return null;
        }

        string[] data = decode(datas);
        SkuData skuData = new SkuData();
        skuData.sku = data[0];
        skuData.title = data[1];
        skuData.description = data[2];
        skuData.price = data[3];
        skuData.purchaseTime = long.Parse(data[4]);
        skuData.purchaseToken = data[5];

        return skuData;
    }

    /**
     * Applications may use this api to initiate a purchase flow. On successful completion, a callback
     * will be made to the IAPListener. Failure also is intimated though callback on the same listener.
     * This can be called only while preferences Activity is in the foreground.
     */
    public static void iapPurchase(string sku) {
        //Initialize if not already done.
        if (sAlpsBillingInterface == null) {
            init();
        }

        sAlpsBillingInterface.CallStatic("iapPurchase", sku);
    }

    /**
     * Applications may use this api to request consumption of an owned sku. On successful completion, a callback
     * will be made to the IAPListener. Failure also is intimated though callback on the same listener.
     */
    public static void iapConsume(string sku) {
        //Initialize if not already done.
        if (sAlpsBillingInterface == null) {
            init();
        }

        sAlpsBillingInterface.CallStatic("iapConsume", sku);
    }

    /**
     * Applications may use this api to refresh the iap cache. This api may be used to implement 'restore purchases'
     * feature. This will asynchronously sync with PlayStore and update local device cache. Once done, a callback
     * will be made to the IAPListener.
     */
    public static void iapRefresh() {
        //Initialize if not already done.
        if (sAlpsBillingInterface == null) {
            init();
        }

        sAlpsBillingInterface.CallStatic("iapRefresh");
    }

    /**
     * Applications may use this api to check the state the app is running in. Value retunred may be "wallpaper",
     * "preview", "activity" or "invisible". If there are multiple instances running, the most recently visible
     * instance's property will be reported.
     */
    public static string getState() {
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        return sAlpsInterface.CallStatic<string>("getState");
    }

    /**
     * Applications may use this api to gain reference to the current Activity/Service context object. This is 
     * intented for those apps which want to get access to the underlying Android Activity or Service and invoke
     * methods on it themselves. Typically this will not be necessary as all common functionalitied that live 
     * wallpapers will generally need are made available though ALPS.cs. ALPS will return AlpsActivity if the 
     * current state is 'activity'. 'AlpsPreferenceActivity' will be retuned if the current state is 'preference'.
     *  When state is 'preview' or 'wallpaper', reference to the AlpsWallpaperService will be returned. Note
     * that null will be returned if the state is 'invisible'.
     */
    public static AndroidJavaObject getContext(){
        //Initialize if not already done.
        if (sAlpsInterface == null) {
            init();
        }

        return sAlpsInterface.CallStatic<AndroidJavaObject>("getContext");
    }

    /**
    * Since ALPS 6.5, the callbacks(except onScrollChanged) from ALPS are made in non-Unity threads. This is so
    * because the UnityPlayer is not woken up just for callbacks. However, this means that any Scene updates you
    * want to do must not be done directly in the callback methods, rather they must be posted to run during next
    * Update() call. Apps can use this method to do this convenienty. Note that any action that the app need to
    * reflect immediately. For example changing some preference values, must be done right in the callback methods
    * for them to be reflected immediately. Only the Scene update might need to be deffered. Please have a look at
    * the demo app code ALPSDemoListener.applyAllPreferences() method for sample usage. 
    *
    * NOTE THAT TRYING TO ULDATE THE SCENE DIRECTLY IN CALLBACK METHODS MAY LEAD TO CRASHES !
    */
    public static void runInUpdate(Action action){
        lock(syncLock){
            queue.Enqueue(action);
        }
    }
}
