using System;
using System.Collections.Generic;
using System.Diagnostics;
using Craft.Net.Common;

namespace MineBot.Bot
{
    public enum ManState
    {
        Connect,
        Connecting,
        Idle,
        Sleep,
        Walk,
        Eat
    }

    public static class MinerStates
    {
        private const int HungerToEat = 100;
        private const int FatigueToSleep = 50;
        private static readonly Random StaticRandom = new Random();

        public static IEnumerable<IEnumState<Miner>> GetStates()
        {
            yield return new EnumState<Miner>(ManState.Connect, EnterState, Connect, ExitState);
            yield return new EnumState<Miner>(ManState.Connecting, EnterConnecting, Connecting, ExitState);
            yield return new EnumState<Miner>(ManState.Idle, EnterIdle, Idle, ExitState);
            yield return new EnumState<Miner>(ManState.Sleep, EnterSleep, Sleep, ExitSleep);
            yield return new EnumState<Miner>(ManState.Walk, EnterWalk, Walk, ExitState);
            yield return new EnumState<Miner>(ManState.Eat, EnterEat, Eat, ExitState);
        }

        private static void EnterState(IEnumStateContext<Miner> context, Miner miner)
        {
            Debug.WriteLine("Enter {0,-10} {1}", context.CurrentState, miner);
        }

        private static void ExitState(IEnumStateContext<Miner> context, Miner miner)
        {
            Debug.WriteLine("Exit  {0,-10} {1}", context.CurrentState, miner);
        }

        private static void Connect(IEnumStateContext<Miner> context, Miner miner)
        {
            miner.Connect();
            context.ChangeState(ManState.Connecting);
        }

        private static void EnterConnecting(IEnumStateContext<Miner> context, Miner miner)
        {
            Debug.WriteLine("Enter Connecting {0}", miner);
            miner.Idle = 10;
        }

        private static void Connecting(IEnumStateContext<Miner> context, Miner miner)
        {
            //Debug.WriteLine("Connecting {0} {1}", miner.World, miner.IsSpawned);
            if (miner.World == null) return;
            if (!miner.IsSpawned) return;
            if (--miner.Idle > 0) return;
            miner.StartPhysicsWorker();
            context.ChangeState(ManState.Idle);
        }

        private static void EnterIdle(IEnumStateContext<Miner> context, Miner miner)
        {
            Debug.WriteLine("Enter Idle       {0}", miner);
            miner.SendChat("Let me check my todo list");
            miner.Idle = StaticRandom.Next(3, 8);
        }

        private static void Idle(IEnumStateContext<Miner> context, Miner miner)
        {
            //Debug.WriteLine("idle " + miner.Name);
            if (miner.Fatigue > FatigueToSleep)
            {
                context.ChangeState(ManState.Sleep);
                return;
            }
            if (miner.Hunger > HungerToEat)
            {
                context.ChangeState(ManState.Eat);
                return;
            }
            if (--miner.Idle > 0) return;
            context.ChangeState(ManState.Walk);
        }

        private static void EnterWalk(IEnumStateContext<Miner> context, Miner miner)
        {
            Debug.WriteLine("Enter Walk       {0}", miner);
            //var dir = new Vector3(StaticRandom.Next(-5, 5), 0.01, StaticRandom.Next(-5, 5));
            var rotation = StaticRandom.NextDouble()*Math.PI*2.0;
            var dist = 1.0 + StaticRandom.NextDouble()*5.0;
            var dir = new Vector3(dist*Math.Sin(rotation), 0.01, dist*Math.Cos(rotation));
            var target = miner.Position + dir;
            //Debug.WriteLine("dir " + dir.ToString());
            miner.SendChatFormat("Going for a stroll to {0:F1} {1:F1} {2:F1}", target.X, target.Y, target.Z);
            miner.Target = target;
        }

        private static void Walk(IEnumStateContext<Miner> context, Miner miner)
        {
            //Debug.WriteLine("Walk " + miner.Name);
            miner.Fatigue++;
            miner.Hunger += 2;
            if (miner.Fatigue > FatigueToSleep)
            {
                context.ChangeState(ManState.Sleep);
                return;
            }
            if (miner.Hunger > HungerToEat)
            {
                context.ChangeState(ManState.Eat);
                return;
            }

            var dif = miner.Target - miner.Position;
            var dist = dif.Distance;
            if (dist > 0.5)
            {
                //_client.Move()
                var vel = dif;
                vel /= dist;
                //Debug.WriteLine("vel " + vel.ToString());
                miner.Velocity = vel;
            }
            else
            {
                context.ChangeState(ManState.Idle);
            }
        }

        private static void EnterEat(IEnumStateContext<Miner> context, Miner miner)
        {
            Debug.WriteLine("Enter Eat        {0}", miner);
            miner.SendChat("I'm so hungey time for a snack");
        }

        private static void Eat(IEnumStateContext<Miner> context, Miner miner)
        {
            //Debug.WriteLine("Eat " + miner.Name);
            miner.Hunger -= 5;
            if (miner.Hunger <= 0) context.ChangeState(ManState.Idle);
        }
        
        private static void EnterSleep(IEnumStateContext<Miner> context, Miner miner)
        {
            Debug.WriteLine("Enter Sleep      {0}", miner);
            miner.SendChat("ZZZzzZZzZZZzzzzz");
        }

        private static void Sleep(IEnumStateContext<Miner> context, Miner miner)
        {
            //Debug.WriteLine("Sleep " + miner.Name);
            miner.Hunger++;
            miner.Fatigue -= 2;
            if (miner.Fatigue <= 0) context.ChangeState(ManState.Idle);
            if (miner.Hunger > HungerToEat) context.ChangeState(ManState.Eat);
        }

        private static void ExitSleep(IEnumStateContext<Miner> context, Miner miner)
        {
            miner.SendChat("Goodmorning!");
        }
    }
}