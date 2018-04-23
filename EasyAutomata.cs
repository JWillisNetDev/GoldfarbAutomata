using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace EasyAutomata {
    public abstract class StateMachine<States,TransitionType>
    {
        protected abstract States Transition(TransitionType next);

        protected readonly ReadOnlyCollection<States> StatesList;

        public void Step(TransitionType next)
        {
            CurrentState = Transition(next);
            if(! StatesList.Contains(CurrentState)) throw new InvalidOperationException($"Unknown state {CurrentState} returned by transition function");
        }

        public readonly States InitialState;

        private States _currentState;
        public States CurrentState
        {
            get
            {
                return _currentState;
            }

            private set 
            {
                _currentState = value;
            }

        }

        public StateMachine(States InitialState)
        {
            if(typeof(States).IsEnum){
                var states = new List<States>();
                foreach(States state in Enum.GetValues(typeof(States))){
                    states.Add(state);
                }
                StatesList = states.AsReadOnly();
            } else throw new ArgumentException("No States list passed to StateMachine constructor");

            this.InitialState = InitialState;

            Reset();
        }

        public StateMachine(List<States> StatesList, States InitialState)
        {
            if(StatesList == null || StatesList.Count == 0) throw new ArgumentException("null or empty states list passed to StateMachine constructor");
            this.StatesList = StatesList.AsReadOnly();
            this.InitialState = InitialState;

            Reset();
        }

        public void Reset()
        {
            this.CurrentState = InitialState; 
        }
    }

    public abstract class DFA<States, TransitionType> : StateMachine<States, TransitionType>
    {
        private readonly ReadOnlyCollection<States> AcceptStates;

        public bool Accepts
        {
            get
            {
                return AcceptStates.Contains(CurrentState);
            }
        }

        public DFA(List<States> AllStates, List<States> AcceptStates, States InitialState) : base(AllStates, InitialState)
        {
            if(AcceptStates == null) throw new ArgumentException($"Null AcceptStates list passed to DFA constructor");
            else if(AcceptStates.Count == 0) throw new ArgumentException($"Empty AcceptState liast passed to DFA constructor");

            this.AcceptStates = AcceptStates.AsReadOnly();
            Reset();

        }

        public DFA(List<States> AcceptStates, States InitialState) : base(InitialState)
        {
            if(AcceptStates == null) throw new ArgumentException($"Null AcceptStates list passed to DFA constructor");
            else if(AcceptStates.Count == 0) throw new ArgumentException($"Empty AcceptState liast passed to DFA constructor");

            this.AcceptStates = AcceptStates.AsReadOnly();
            Reset();
        }

        public DFA(States AcceptState, States InitialState) : this(new List<States>(){ AcceptState }, InitialState)
        {
        }

        public DFA(List<States> AllStates, States AcceptState, States InitialState) : this(AllStates, new List<States>(){ AcceptState }, InitialState)
        {
        }
    }

    namespace Util {
        public class Optional<T>
        {
            public readonly bool HasValue;

            private readonly T _value;
            public T Value
            {
                get
                {
                    if(HasValue) return _value;
                    else throw new InvalidOperationException();
                }
            }

            public Optional()
            {
                HasValue = false;
                _value = default(T);
            }

            public Optional(T Value)
            {
                HasValue = typeof(T).IsClass ? (Value != null) : true;
                _value = Value;
            }

            public static Optional<T> None()
            {
                return new Optional<T>();
            }

            public static explicit operator T(Optional<T> Optional)
            {
                return Optional.Value;
            }

            public static implicit operator Optional<T>(T Value)
            {
                return new Optional<T>(Value);
            }

            public override bool Equals(object rhs)
            {
                if(rhs == null || !(rhs is Optional<T>)) return false;
                return Equals((Optional<T>) rhs);
            }

            public bool Equals(Optional<T> rhs)
            {
                if(HasValue && rhs.HasValue) return Value.Equals(rhs.Value);
                else return HasValue == rhs.HasValue;
            }

            public override int GetHashCode()
            {
                if(! HasValue) return Tuple.Create(false, default(T)).GetHashCode();
                else return Tuple.Create(true, _value).GetHashCode();
            }
        }

        public class Interval<T> where T : IComparable<T>
        {
            public readonly T Minimum;
            public readonly T Maximum;

            public Interval(T Minimum, T Maximum)
            {
                if(Minimum.CompareTo(Maximum) > 0) throw new ArgumentException("Interval minimum must be less than maximum");

                this.Minimum = Minimum;
                this.Maximum = Maximum;
            }

            public Interval(T val) : this(val, val)
            {

            }

            public virtual bool Contains(T val)
            {
                return val.CompareTo(Minimum) >= 0 && val.CompareTo(Maximum) <= 0;
            }

            public bool Contains(Interval<T> range)
            {
                return Contains(range.Minimum) && Contains(range.Maximum);
            }

            public Interval<T> Intersect(Interval<T> other)
            {
                T min, max;

                if(Maximum.CompareTo(other.Minimum) < 0 || Minimum.CompareTo(other.Maximum) > 0) throw new ArgumentException("Intervals have no intersection");

                if(Minimum.CompareTo(other.Minimum) <= 0) min = other.Minimum;
                else min = Minimum;

                if(Maximum.CompareTo(other.Maximum) >= 0) max = other.Maximum;
                else max = Maximum;

                return new Interval<T>(min, max);
            }

            public override bool Equals(Object rhs)
            {
                if(rhs == null || ! (rhs is Interval<T>)) return false;
                else return Minimum.CompareTo((rhs as Interval<T>).Minimum) == 0 && Maximum.CompareTo((rhs as Interval<T>).Maximum) == 0;
            }

            public override string ToString()
            {
                return $"Interval<{typeof(T).ToString()}>[{Minimum},{Maximum}]";
            }

            public override int GetHashCode()
            {
                return 17 * Minimum.GetHashCode() + Maximum.GetHashCode();
            }

            public bool this[T val]
            {
                get {
                    return Contains(val);
                }
            }
        }   
    }
}
