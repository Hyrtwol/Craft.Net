using System;
using System.Collections.Generic;

// http://www.ai-junkie.com/architecture/state_driven/tut_state1.html

namespace MineBot.Bot
{
    public interface IEnumStateContext<T>
    {
        IEnumState<T> CurrentState { get; }
        void ChangeState(Enum newState);
    }

    public interface IEnumState<T>
    {
        void Enter(IEnumStateContext<T> context, T entity);
        void Execute(IEnumStateContext<T> context, T entity);
        void Exit(IEnumStateContext<T> context, T entity);
        Enum EnumValue { get; }
    }

    public class EnumState<T> : IEnumState<T>
    {
        private readonly Enum _enumValue;
        public readonly Action<IEnumStateContext<T>, T> Enter;
        public readonly Action<IEnumStateContext<T>, T> Execute;
        public readonly Action<IEnumStateContext<T>, T> Exit;

        public EnumState()
        {
        }

        public EnumState(Enum enumValue, Action<IEnumStateContext<T>, T> enter, Action<IEnumStateContext<T>, T> execute, Action<IEnumStateContext<T>, T> exit)
        {
            _enumValue = enumValue;
            Enter = enter;
            Execute = execute;
            Exit = exit;
        }

        public override string ToString()
        {
            return _enumValue.ToString();
        }

        public Enum EnumValue
        {
            get { return _enumValue; }
        }

        void IEnumState<T>.Enter(IEnumStateContext<T> context, T entity)
        {
            Enter(context, entity);
        }

        void IEnumState<T>.Execute(IEnumStateContext<T> context, T entity)
        {
            Execute(context, entity);
        }

        void IEnumState<T>.Exit(IEnumStateContext<T> context, T entity)
        {
            Exit(context, entity);
        }
    }

    public class EnumStateMachine<T> : IEnumStateContext<T>
    {
        private readonly T _entity;
        private bool _inExecute;
        private IEnumState<T> _nextState;
        private readonly IEnumState<T>[] _states;

        /// <summary>
        /// a pointer to the entity that owns this instance
        /// </summary>
        public T Entity { get { return _entity; } }

        /// <summary>
        /// this state logic is called every time the FSM is updated
        /// </summary>
        public IEnumState<T> CurrentState { get; private set; }

        /// <summary>
        /// a record of the last state the agent was in
        /// </summary>
        public IEnumState<T> PreviousState { get; private set; }

        public IEnumState<T> GlobalState { get; private set; }

        /// <summary>
        /// State cache
        /// </summary>
        //public Dictionary<Type, IState<T>> States { get; private set; }

        // constructor
        public EnumStateMachine(KeyValuePair<Enum, IEnumState<T>>[] states, T entity)
        {
            _entity = entity;
            CurrentState = null;
            PreviousState = null;
            GlobalState = null;

            var cnt = states.Length;
            _states = new IEnumState<T>[cnt];
            foreach (var kv in states)
            {
                int idx = (int)(object)kv.Key;
                if (_states[idx] != null) throw new InvalidOperationException();
                _states[idx] = kv.Value;
            }
        }

        ////use these methods to initialize the state machine
        public void SetCurrentState(IEnumState<T> state)
        {
            CurrentState = state;
        }

        //public void SetCurrentState<TS>() where TS : IState<T>
        //{
        //    var stateType = typeof(TS);
        //    IState<T> state;
        //    if (States.TryGetValue(stateType, out state))
        //    {
        //        CurrentState = state;
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException(
        //            string.Format("State {0} not found.", stateType));
        //    }
        //}


        public void SetGlobalState(IEnumState<T> state)
        {
            GlobalState = state;
        }

        /// <summary>
        /// call this to update the state machine
        /// </summary>
        public void Update()
        {
            //if a global state exists, call its execute method
            if (GlobalState != null)
                GlobalState.Execute(this, _entity);

            //same for the current state
            if (CurrentState != null)
            {
                try
                {
                    _inExecute = true;
                    CurrentState.Execute(this, _entity);
                }
                finally
                {
                    _inExecute = false;
                }
                if (_nextState != null)
                {
                    ChangeState(_nextState);
                    _nextState = null;
                }
            }
        }

        /// <summary>
        /// change to a new state
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeState(IEnumState<T> newState)
        {
            if (_inExecute)
            {
                _nextState = newState;
                return;
            }
            //Console.WriteLine("ChangeState {0} -> {1}",
            //                  currentState != null ? currentState.GetType().Name : null,
            //                  newState != null ? newState.GetType().Name : null);

            //assert(pNewState && "<StateMachine::ChangeState>: trying to change to a null state");

            //keep a record of the previous state
            PreviousState = CurrentState;

            //call the exit method of the existing state
            if (CurrentState != null)
                CurrentState.Exit(this, Entity);

            //change state to the new state
            CurrentState = newState;

            //call the entry method of the new state
            if (CurrentState != null)
                CurrentState.Enter(this, Entity);
        }

        //public void ChangeState<TState>() where TState : IState<T>
        //{
        //    var stateType = typeof(TState);
        //    IState<T> state;
        //    if (States.TryGetValue(stateType, out state))
        //    {
        //        ChangeState(state);
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException(
        //            string.Format("State {0} not found.", stateType));
        //    }
        //}

        public void ChangeState(Enum newState)
        {
            int idx = (int)(object)newState;
            //Console.WriteLine(idx);
            var state = _states[idx];
            ChangeState(state);
        }

        /// <summary>
        /// change state back to the previous state
        /// </summary>
        public void RevertToPreviousState()
        {
            ChangeState(PreviousState);
        }

        /// <summary>
        /// returns true if the current states type is equal to the type of the class passed as a parameter.
        /// </summary>
        /// <param name="st"></param>
        /// <returns></returns>
        public bool IsInState(IEnumState<T> st)
        {
            return st.Equals(CurrentState);
        }

        //public void AddState(IState<T> state)
        //{
        //    States.Add(state.GetType(), state);
        //}

        //public void AddState<TS>() where TS : IState<T>, new()
        //{
        //    var state = new TS();
        //    States.Add(state.GetType(), state);
        //}
    }
}