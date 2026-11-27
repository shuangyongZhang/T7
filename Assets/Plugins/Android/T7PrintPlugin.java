package com.lvbaizhineng.print;

import android.app.Activity;
import android.content.Context;
import android.print.PrintAttributes;
import android.print.PrintManager;
import android.view.ViewGroup;
import android.webkit.WebView;
import android.webkit.WebViewClient;

public final class T7PrintPlugin {
    private T7PrintPlugin() {
    }

    public static void printHtml(final Activity activity, final String html, final String jobName) {
        if (activity == null || html == null) return;
        activity.runOnUiThread(new Runnable() {
            @Override
            public void run() {
                final ViewGroup decorView = (ViewGroup) activity.getWindow().getDecorView();
                final WebView webView = new WebView(activity);
                webView.setLayoutParams(new ViewGroup.LayoutParams(1, 1));
                webView.getSettings().setJavaScriptEnabled(false);
                webView.getSettings().setAllowFileAccess(true);
                webView.setWebViewClient(new WebViewClient() {
                    @Override
                    public void onPageFinished(WebView view, String url) {
                        view.postDelayed(new Runnable() {
                            @Override
                            public void run() {
                                printDocument(activity, decorView, webView, jobName);
                            }
                        }, 250L);
                    }
                });
                decorView.addView(webView);
                webView.loadDataWithBaseURL(null, html, "text/html", "UTF-8", null);
            }
        });
    }

    private static void printDocument(Activity activity, ViewGroup decorView, WebView webView, String jobName) {
        PrintManager printManager = (PrintManager) activity.getSystemService(Context.PRINT_SERVICE);
        if (printManager == null) {
            decorView.removeView(webView);
            return;
        }

        String safeJobName = jobName == null || jobName.trim().length() == 0 ? "T7 Report" : jobName;
        PrintAttributes attributes = new PrintAttributes.Builder()
                .setMediaSize(PrintAttributes.MediaSize.ISO_A4)
                .setMinMargins(new PrintAttributes.Margins(300, 300, 300, 300))
                .build();
        printManager.print(safeJobName, webView.createPrintDocumentAdapter(safeJobName), attributes);
        decorView.postDelayed(new Runnable() {
            @Override
            public void run() {
                if (webView.getParent() instanceof ViewGroup) {
                    ((ViewGroup) webView.getParent()).removeView(webView);
                }
            }
        }, 30000L);
    }
}
