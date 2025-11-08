using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;
using MultiplayerSDK.WebBridge;
using Newtonsoft.Json;
using UnityEngine;

// ReSharper disable once CheckNamespace
public class WebBridge : MonoBehaviour
{
    public event Action<WebPayload> OnPayloadReceived;
    
    [UsedImplicitly]
    [DllImport("__Internal")]
    private static extern void GameLoaded();
    
    [UsedImplicitly]
    [DllImport("__Internal")]
    private static extern void ConnectedToServer();

    [SerializeField] private bool _sendMessages;

    public void TriggerLoaded()
    {
        if (!_sendMessages)
            return;
        
        Debug.Log("Trigger GameLoaded");
#if UNITY_WEBGL && !UNITY_EDITOR
        GameLoaded();
#endif
    }

    public void TriggerConnected()
    {
        if (!_sendMessages)
            return;

        Debug.Log("Trigger ConnectedToServer");
#if UNITY_WEBGL && !UNITY_EDITOR
        ConnectedToServer();
#endif
    }

    [UsedImplicitly]
    private void Ping()
    {
        Debug.Log("Ping!");
    }
    
    [UsedImplicitly]
    private void SetPlayerId(int playerId)
    {
        Debug.Log($"Player id: {playerId}");
    }

    [UsedImplicitly]
    private void SetPlayerState(string json)
    {
        Debug.Log($"Player state: {json}");
        try
        {
            var payload = JsonConvert.DeserializeObject<WebPayload>(json);
            OnPayloadReceived?.Invoke(payload);
        }
        catch (Exception e)
        {
            Debug.LogError($"[WebBridge] Error on parsing payload: {e}");
        }
    }
}