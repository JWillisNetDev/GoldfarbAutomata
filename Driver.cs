using System;
using System.Collections.Generic;

using EasyAutomata;
using EasyAutomata.Util;

namespace Driver
{
    public class Program
    {
        public enum BinaryMatcherStates
        {
            START,
            EVEN,
            ODD,
            REJECT
        };

        public class BinaryMatcher : DFA<BinaryMatcherStates, Optional<char>>
        {
            protected override BinaryMatcherStates Transition(Optional<char> next)
            {
                switch(CurrentState)
                {
                    case BinaryMatcherStates.START:
                    case BinaryMatcherStates.EVEN:
                    case BinaryMatcherStates.ODD:
                        if(next.HasValue){
                            switch((char) next)
                            {
                                case '0': return BinaryMatcherStates.EVEN;
                                case '1': return BinaryMatcherStates.ODD;
                                default: return BinaryMatcherStates.REJECT;
                            }
                        } else if(CurrentState == BinaryMatcherStates.START)
                        {
                            return BinaryMatcherStates.REJECT;
                        } else
                        {
                            return CurrentState;
                        }
                    case BinaryMatcherStates.REJECT: return BinaryMatcherStates.REJECT;
                }

                return BinaryMatcherStates.REJECT;
            }

            private BinaryMatcher(bool MatchOdd) : base((MatchOdd ? BinaryMatcherStates.ODD : BinaryMatcherStates.EVEN), BinaryMatcherStates.START)
            {
            }

            public bool Matches(string str)
            {
                Reset();

                foreach(char c in str)
                {
                    Step(c);
                }

                Step(new Optional<char>());

                return Accepts;
            }

            public static BinaryMatcher EvenMatcher()
            {
                return new BinaryMatcher(false);
            }

            public static BinaryMatcher OddMatcher()
            {
                return new BinaryMatcher(true);
            }
        }


        public static void Main(string[] args)
        {
            var OddMatcher = BinaryMatcher.OddMatcher();
            var EvenMatcher = BinaryMatcher.EvenMatcher();

            string line;

            do {
                line = Console.ReadLine();

                if(line != null){
                    if(OddMatcher.Matches(line)) Console.WriteLine("odd");
                    else if(EvenMatcher.Matches(line)) Console.WriteLine("even");
                    else Console.WriteLine("neither odd nor even");
                }
            }
            while(line != null);
        }
    }
}
