using FishNet.Object.Prediction;
using FishNet.Transporting;
using FishNet.Utility.Template;
using UnityEngine;

namespace DefaultNamespace
{
    public class PredictedMovableObjectController : TickNetworkBehaviour
    {
        public struct Input : IReplicateData
        {
            private uint _tick;

            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
            public void Dispose() { }
        }
        
        public struct State : IReconcileData
        {
            private uint _tick;

            public double StartMoveTime;

            public uint GetTick() => _tick;
            public void SetTick(uint value) => _tick = value;
            public void Dispose() { }
        }

        [SerializeField] private bool _debug;
        [SerializeField] private float _oneSideMoveTime;
        [SerializeField] private AnimationCurve _oneSideMoveCurve;
        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _endPoint;
        [SerializeField] private Rigidbody2D _rigidbody;

        [SerializeField] private float _moveTime;
        [SerializeField] private float _passedTime;
        [SerializeField] private bool _left;
        [SerializeField] private uint _tick;
        [SerializeField] private bool _reconcile;

        private double _startMoveTime;
        
        public override void OnStartNetwork()
        {
            _startMoveTime = TimeManager.TicksToTime(TimeManager.Tick);
        }

        protected override void TimeManager_OnTick()
        {
            SimulateInputs(new Input());
        }
        
        [Replicate]
        private void SimulateInputs(Input input, ReplicateState state = ReplicateState.Invalid, Channel channel = Channel.Unreliable)
        {
            var passedTime = (float)(TimeManager.TicksToTime(input.GetTick()) - _startMoveTime);
            var completedTrips = Mathf.FloorToInt(passedTime / _oneSideMoveTime);
            
            var timeSinceLastRoundTrip = passedTime - completedTrips * _oneSideMoveTime;
            bool isGoingForward = completedTrips % 2 == 0;
            
            var startPoint = isGoingForward ? _startPoint : _endPoint;
            var endPoint = isGoingForward ? _endPoint : _startPoint;

            float t = timeSinceLastRoundTrip / _oneSideMoveTime;
            
            t = t > 1 ? t - 1 : t;
            t = _oneSideMoveCurve.Evaluate(t);

            _moveTime = t;
            _left = isGoingForward;
            _passedTime = passedTime;
            _tick = input.GetTick();

            _reconcile = state == ReplicateState.Replayed;
            
            var position = Vector2.Lerp(startPoint.position, endPoint.position, t);
            _rigidbody.position = position;
        }

        protected override void TimeManager_OnPostTick()
        {
            CreateReconcile();
        }

        public override void CreateReconcile()
        {
            var state = new State
            {
                StartMoveTime = _startMoveTime,
            };
            ReconcileState(state);
        }

        [Reconcile]
        private void ReconcileState(State state, Channel channel = Channel.Unreliable)
        {
            _startMoveTime = state.StartMoveTime;
        }
    }
}