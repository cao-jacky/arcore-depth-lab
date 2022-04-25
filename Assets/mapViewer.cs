using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mapViewer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var webView = gameObject.AddComponent<UniWebView>();
        webView.ReferenceRectTransform = GameObject.Find("MapBackground").GetComponent<RectTransform>();
        
        UniWebViewLogger.Instance.LogLevel = UniWebViewLogger.Level.Verbose;
        UniWebViewInterface.SetWebContentsDebuggingEnabled(true);

        webView.AddPermissionTrustDomain("5gwebxr.com");

        // Load a URL.
        webView.Load("https://5gwebxr.com/arsocialdistancing/map.html");

        // Show it.
        webView.Show();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
