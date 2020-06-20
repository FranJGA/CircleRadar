using UnityEngine;

/**
 * The interface to listen for IAP related callbacks from ALPS system.
 * It includes callbacks for all events related to managed items. Subcriptions are not supported.
 */
public class ALPSDemoIAPListener : ALPS.IAPListener {
    /**
     * ALPS will fire this callback after connecting to IAP service and once purchase statuses are ready.
     */
     public void onConnected() {
        ALPS.showToast("IAP onConnected");
        Debug.Log("IAP onConnected");

        ALPS.SkuData skuData = ALPS.iapGetSkuData("gopro");
        Debug.Log("IAP onConnected orderId >"+ skuData.purchaseToken + "< ");
        if (!skuData.purchaseToken.Equals("null")) {
            //If the "gopro" sku is owned, we use a different layout file and preference xml.
            ALPS.setPreferenceName("alps_preferences_pro", new string[]{"hidden1", "hidden2"});
            ALPS.setPreferenceLayoutName("alps_preference_activity_layout_pro");

        }else {
            //Initialize AdMob if user is running a non-pro version of the app. For apps that don't use AdMob,
            //this call must not be invoked, as it will make AdMod classes and resource to load in the background
            //and occupy memory and other resources. AdUnitId for banner ads are stated in the layout file itself.
            //You can have multiple banner ads in any given screen. But there is only one interstitial/rewardedVideo ad available at a time.
            ALPS.adMobInitialize(new ALPSDemoAdMobListener(), "ca-app-pub-3940256099942544/1033173712", "ca-app-pub-3940256099942544/5224354917");
        }
    }

    /**
     * ALPS will fire this callback when the IAP service gets disconnected. Applications might want
     * to attempt re-connecting at this point.
     */
    public void onDisconnected() {
        ALPS.showToast("IAP onDisconnected");
        Debug.Log("IAP onDisconnected");

        ALPS.iapConnect(this, new string[] { "gopro" });
    }

    /**
     * ALPS will fire this callback when a purchase flow completes successfully.
     */
    public void onPurchaseSuccess(ALPS.SkuData skuData, int code) {
        ALPS.showToast("IAP onPurchaseSuccess " + skuData.sku+", " + code);
        Debug.Log("IAP onPurchaseSuccess " + skuData.sku + ", " + code);

        //If the "gopro" sku is owned, we use a different layout file and preference xml.
        if (skuData.sku.Equals("gopro") && !skuData.purchaseToken.Equals("null")) {
            Debug.Log("IAP onPurchaseSuccess isPurchased " + skuData.purchaseToken);
            ALPS.setPreferenceName("alps_preferences_pro", new string[]{"hidden1", "hidden2"});
            ALPS.setPreferenceLayoutName("alps_preference_activity_layout_pro");
        }
    }

    /**
     * ALPS will fire this callback when a purchase flow fails for a reason as indicated.
     */
    public void onPurchaseFailure(string sku, int reason) {
        ALPS.showToast("IAP onPurchaseFailure " + sku + ", " + reason);
        Debug.Log("IAP onPurchaseFailure " + sku + ", " + reason);
    }

    /**
     * ALPS will fire this callback when a consume request completes successfully.
     */
    public void onConsumeSuccess(string sku, int code) {
        ALPS.showToast("IAP onConsumeSuccess " + sku + ", " + code);
        Debug.Log("IAP onConsumeSuccess " + sku + ", " + code);

        //If the "gopro" sku is consumes, we revert to the default layout file and preference xml.
        if (sku.Equals("gopro")) {
            ALPS.setPreferenceName("alps_preferences", new string[]{"hidden1", "hidden2"});
            ALPS.setPreferenceLayoutName("alps_preference_activity_layout");

            //Initialize AdMob if user is running a non-pro version of the app. For apps that don't use AdMob,
            //this call must not be invoked, as it will make AdMod classes and resource to load in the background
            //and occupy memory and other resources. AdUnitId for banner ads are stated in the layout file itself.
            //You can have multiple banner ads in any given screen. But there is only one interstitial/rewardedVideo ad available at a time.
            ALPS.adMobInitialize(new ALPSDemoAdMobListener(), "ca-app-pub-3940256099942544/1033173712", "ca-app-pub-3940256099942544/5224354917");
        }
    }

    /**
     * ALPS will fire this callback when a consume request fails for a reason as indicated.
     */
    public void onConsumeFailure(string sku, int reason) {
        ALPS.showToast("IAP onConsumeFailure " + sku + ", " + reason);
        Debug.Log("IAP onConsumeFailure " + sku + ", " + reason);
    }

    /**
     * ALPS will fire this callback when an iap refresh process is complete.
     */
    public void onRefreshComplete(int code) {
        ALPS.showToast("IAP onRefreshComplete " + code);
        Debug.Log("IAP onRefreshComplete " + code);
    }
}
