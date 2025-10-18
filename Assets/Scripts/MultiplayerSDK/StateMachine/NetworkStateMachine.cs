using System;
using System.Collections.Generic;
using System.Linq;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using Newtonsoft.Json;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using VContainer;

namespace MultiplayerSDK.StateMachine
{
    public struct StateData
    {
        public int Type;
        public string JsonData;
    }

    public static class NetworkStateMachineExtensions
    {
        public static T GetType<T>(this StateData stateData) where T : Enum
        {
            return UnsafeUtility.As<int, T>(ref stateData.Type);
        }
        
        public static StateData WithType<T>(this StateData stateData, T value) where T : Enum
        {
            stateData.Type = UnsafeUtility.As<T, int>(ref value);
            return stateData;
        }
        
        public static StateData WithData(this StateData stateData, string value)
        {
            stateData.JsonData = value;
            return stateData;
        }
    }
    
    public abstract class NetworkStateMachine<TStateType> : NetworkBehaviour
        where TStateType : Enum
    {
        public TStateType CurrentState => _currentStateData.Value.GetType<TStateType>();
        
        [Inject] private readonly IReadOnlyList<INetworkState<TStateType>> _states;

        private readonly SyncVar<StateData> _currentStateData = new(settings: new SyncTypeSettings(writePermissions: WritePermission.ServerOnly));
        private readonly EqualityComparer<TStateType> _comparer = EqualityComparer<TStateType>.Default;

        private INetworkState<TStateType> _currentState;
        private Dictionary<TStateType, INetworkState<TStateType>> _typedStates;

        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            this.InjectToMe();

            _typedStates = _states.ToDictionary(x => x.Type);
            foreach (var state in _states)
            {
                Debug.Log($"[NetworkStateMachine] Initialize game state {state.Type}");
                state.SetStateMachine(this);
            }
            
            SetInitialState();
            
            if (!IsServerInitialized)
            {
                var currentStateType = _currentStateData.Value.GetType<TStateType>();
                Debug.Log($"[NetworkStateMachine] Initialize first state on client: {currentStateType}");
                if (!_typedStates.TryGetValue(currentStateType, out var initState))
                {
                    Debug.LogError($"[NetworkStateMachine] Error initializing first state on client: {currentStateType}, no state found");
                    return;
                }
                
                if (!string.IsNullOrWhiteSpace(_currentStateData.Value.JsonData))
                    initState.OnEnterWithData(currentStateType, _currentStateData.Value.JsonData);
                else
                    initState.OnEnter(currentStateType);
            }
            
            _currentStateData.OnChange += OnStateChanged;
        }

        public override void OnStopNetwork()
        {
            base.OnStopNetwork();
            foreach (var state in _states)
            {
                state.CleanupStateMachine();
            }
            
            _currentStateData.OnChange -= OnStateChanged;
        }

        protected virtual void Update()
        {
            _currentState?.Update();
        }

        protected virtual void SetInitialState() { }

        protected void SetInitialStateInternal(TStateType stateType)
        {
            if (!_typedStates.TryGetValue(stateType, out var nextState))
            {
                Debug.LogError($"[NetworkStateMachine] Trying to set not registered initial state {stateType}");
                return;
            }
            
            _currentStateData.SetInitialValues(new StateData().WithType(stateType));
            nextState.OnEnter(default);
            _currentState = nextState;
        }

        protected void SetInitialStateInternal<T>(TStateType stateType, T data)
        {
            Debug.Log($"[NetworkStateMachine] Set initial state server with data: {stateType}");
            if (!_typedStates.TryGetValue(stateType, out var nextState))
            {
                Debug.LogError($"[NetworkStateMachine] Trying to set not registered initial state {stateType}");
                return;
            }
            
            if (nextState is not NetworkState<TStateType, T>)
            {
                var nextStateType = nextState.GetType();
                var arguments = nextStateType.GenericTypeArguments;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (arguments.Length == 1)
                    Debug.LogError($"[NetworkStateMachine] Trying to set state {stateType} with data type {typeof(T).Name}, but state doesn't receive any data");
                else
                    Debug.LogError($"[NetworkStateMachine] Trying to set state {stateType} with wrong data type, provided: {typeof(T).Name}, required: {arguments[1].Name}");
                
                return;
            }
            
            var jsonData = JsonConvert.SerializeObject(data);
            _currentStateData.SetInitialValues(new StateData().WithType(stateType).WithData(jsonData));
            nextState.OnEnterWithData(default, jsonData);
            _currentState = nextState;
        }

        [Server]
        public void SetStateServer(TStateType stateType)
        {
            Debug.Log($"[NetworkStateMachine] Set state server: {stateType}");
            if (!_typedStates.TryGetValue(stateType, out var nextState))
            {
                Debug.LogError($"[NetworkStateMachine] Trying to set not registered state: {stateType}");
                return;
            }
            
            SetStateServerInternal(nextState, null);
        }

        [Server]
        public void SetStateServer<T>(TStateType stateType, T data)
        {
            Debug.Log($"[NetworkStateMachine] Set state server with data: {stateType}");
            if (!_typedStates.TryGetValue(stateType, out var nextState))
            {
                Debug.LogError($"[NetworkStateMachine] Trying to set not registered state: {stateType}");
                return;
            }

            if (nextState is not NetworkState<TStateType, T>)
            {
                var nextStateType = nextState.GetType();
                var arguments = nextStateType.GenericTypeArguments;
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (arguments.Length == 1)
                    Debug.LogError($"[NetworkStateMachine] Trying to set state {stateType} with data type {typeof(T).Name}, but state doesn't receive any data");
                else
                    Debug.LogError($"[NetworkStateMachine] Trying to set state {stateType} with wrong data type, provided: {typeof(T).Name}, required: {arguments[1].Name}");
                
                return;
            }

            var jsonData = JsonConvert.SerializeObject(data);
            SetStateServerInternal(nextState, jsonData);
        }

        private void SetStateServerInternal(INetworkState<TStateType> nextState, string jsonData)
        {
            if (_comparer.Equals(nextState.Type, _currentStateData.Value.GetType<TStateType>()))
            {
                Debug.LogError($"[NetworkStateMachine] Trying to set same state: {nextState.Type}");
                return;
            }
            
            _currentStateData.Value = new StateData().WithData(jsonData).WithType(nextState.Type);
        }

        private void OnStateChanged(StateData prev, StateData next, bool asServer)
        {
            var prevType = prev.GetType<TStateType>();
            var nextType = next.GetType<TStateType>();
            Debug.Log($"[NetworkStateMachine] On state changed from {prevType} to {nextType}");
            if (!_typedStates.TryGetValue(nextType, out var nextState))
            {
                Debug.LogError($"[NetworkStateMachine] Received state changed on not registered state: {next.Type}");
                return;
            }
            
            if (_typedStates.TryGetValue(prevType, out var prevState))
                prevState.OnExit(nextType);

            _currentState = null;
            try
            {
                if (!string.IsNullOrWhiteSpace(next.JsonData))
                    nextState.OnEnterWithData(prevType, next.JsonData);
                else
                    nextState.OnEnter(prevType);

                _currentState = nextState;
            }
            catch (Exception e)
            {
                Debug.LogError($"[NetworkStateMachine] Error on entering state: {next.Type}: {e}");
            }
        }
    }
}