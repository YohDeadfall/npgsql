using Npgsql.BackendMessages;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Npgsql.TypeHandlers.Composite
{
    internal sealed class CompositeHandler<T> : NpgsqlTypeHandler<T>
    {
        protected override ValueTask<T> ReadValue(NpgsqlReadBuffer buffer, FieldDescription? fieldDescription, int length)
        {

        }

        protected override ValueTask WriteValue(T value, NpgsqlWriteBuffer buffer, NpgsqlParameter? parameter, NpgsqlLengthCache lengthCache) =>
            throw new NotImplementedException();

        protected override int ValidateAndGetLength(T value, NpgsqlParameter? parameter, NpgsqlLengthCache lengthCache)
        {
            throw new NotImplementedException();
        }
    }

    internal abstract class CompositeMembers
    {
        public abstract int Count { get; }

        public abstract TMember Get<TMember>(int index);
        public abstract void Set<TMember>(int index, TMember value);
    }

    internal abstract class CompositeMemberStateBuilder
    {
        public abstract CompositeMemberStateBuilder HasMember<TMember>();
        public abstract CompositeMembers CreateState();
    }

    internal sealed class CompositeInlinedMembersBuilder<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8> : CompositeMemberStateBuilder
    {
        public static readonly int Members = 0 +
            (typeof(TMember1) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember2) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember3) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember4) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember5) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember6) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember7) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember8) == typeof(Unused) ? 0 : 1);

        private readonly CompositeMemberStateBuilder? _previous;

        public CompositeInlinedMembersBuilder(CompositeMemberStateBuilder? previous) =>
            _previous = previous;

        public override CompositeMemberStateBuilder HasMember<TMember>() =>
            (Members + 1) switch
            {
                1 => new CompositeInlinedMembersBuilder<TMember, Unused, Unused, Unused, Unused, Unused, Unused, Unused>(_previous),
                2 => new CompositeInlinedMembersBuilder<TMember1, TMember, Unused, Unused, Unused, Unused, Unused, Unused>(_previous),
                3 => new CompositeInlinedMembersBuilder<TMember1, TMember2, TMember, Unused, Unused, Unused, Unused, Unused>(_previous),
                4 => new CompositeInlinedMembersBuilder<TMember1, TMember2, TMember3, TMember, Unused, Unused, Unused, Unused>(_previous),
                5 => new CompositeInlinedMembersBuilder<TMember1, TMember2, TMember3, TMember4, TMember, Unused, Unused, Unused>(_previous),
                6 => new CompositeInlinedMembersBuilder<TMember1, TMember2, TMember3, TMember4, TMember5, TMember, Unused, Unused>(_previous),
                7 => new CompositeInlinedMembersBuilder<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember, Unused>(_previous),
                8 => new CompositeInlinedMembersBuilder<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember>(_previous),
                _ => new CompositeInlinedMembersBuilder<TMember, Unused, Unused, Unused, Unused, Unused, Unused, Unused>(this)
            };

        public override CompositeMembers CreateState() =>
            new CompositeInlinedMembers<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8>(_previous?.CreateState());

    }
    internal sealed class CompositeInlinedMembers<TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8> : CompositeMembers
    {
        public static readonly int Members = 0 +
            (typeof(TMember1) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember2) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember3) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember4) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember5) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember6) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember7) == typeof(Unused) ? 0 : 1) +
            (typeof(TMember8) == typeof(Unused) ? 0 : 1);

        private readonly int _start;
        private readonly CompositeMembers? _rest;

        [AllowNull] private TMember1 _member1 = default;
        [AllowNull] private TMember2 _member2 = default;
        [AllowNull] private TMember3 _member3 = default;
        [AllowNull] private TMember4 _member4 = default;
        [AllowNull] private TMember5 _member5 = default;
        [AllowNull] private TMember6 _member6 = default;
        [AllowNull] private TMember7 _member7 = default;
        [AllowNull] private TMember8 _member8 = default;

        public CompositeInlinedMembers(CompositeMembers? rest)
        {
            if (rest is null)
                return;

            _start = rest.Count;
            _rest = rest;
        }

        public override int Count => _start + Members;

        [return: MaybeNull]
        public override TMember Get<TMember>(int index)
        {
            if (index < Members)
                switch (index)
                {
                    case 0: return UnsafeChecked.As<TMember1, TMember>(ref _member1);
                    case 1: return UnsafeChecked.As<TMember2, TMember>(ref _member2);
                    case 2: return UnsafeChecked.As<TMember3, TMember>(ref _member3);
                    case 3: return UnsafeChecked.As<TMember4, TMember>(ref _member4);
                    case 4: return UnsafeChecked.As<TMember5, TMember>(ref _member5);
                    case 5: return UnsafeChecked.As<TMember6, TMember>(ref _member6);
                    case 6: return UnsafeChecked.As<TMember7, TMember>(ref _member7);
                    case 7: return UnsafeChecked.As<TMember8, TMember>(ref _member8);
                }

            if (_rest is CompositeMembers rest)
                return rest.Get<TMember>(index);
            else
                throw new ArgumentOutOfRangeException(nameof(index));
        }

        public override void Set<TMember>(int index, TMember value)
        {
            if (index < Members)
                switch (index)
                {
                    case 0: _member1 = UnsafeChecked.As<TMember, TMember1>(ref value); return;
                    case 1: _member2 = UnsafeChecked.As<TMember, TMember2>(ref value); return;
                    case 2: _member3 = UnsafeChecked.As<TMember, TMember3>(ref value); return;
                    case 3: _member4 = UnsafeChecked.As<TMember, TMember4>(ref value); return;
                    case 4: _member5 = UnsafeChecked.As<TMember, TMember5>(ref value); return;
                    case 5: _member6 = UnsafeChecked.As<TMember, TMember6>(ref value); return;
                    case 6: _member7 = UnsafeChecked.As<TMember, TMember7>(ref value); return;
                    case 7: _member8 = UnsafeChecked.As<TMember, TMember8>(ref value); return;
                }

            if (_rest is CompositeMembers rest)
                rest.Set(index, value);
            else
                throw new ArgumentOutOfRangeException(nameof(index));
        }
    }

    internal sealed class Boxed<TMember>
    {
        [AllowNull]
        public TMember Value = default;
    }

    internal abstract class CompositeFactory<TComposite>
    {
        public abstract TComposite CreateComposite(CompositeMembers members);
    }

    internal sealed class CompositeFactory<TComposite, TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8> : CompositeFactory<TComposite>
    {
        private static readonly int Arguments = 0 +
            (typeof(TArg1) == typeof(Unused) ? 1 : 0) +
            (typeof(TArg2) == typeof(Unused) ? 1 : 0) +
            (typeof(TArg3) == typeof(Unused) ? 1 : 0) +
            (typeof(TArg4) == typeof(Unused) ? 1 : 0) +
            (typeof(TArg5) == typeof(Unused) ? 1 : 0) +
            (typeof(TArg6) == typeof(Unused) ? 1 : 0) +
            (typeof(TArg7) == typeof(Unused) ? 1 : 0) +
            (typeof(TArg8) == typeof(Unused) ? 1 : 0);

        private readonly Delegate _constructor;
        private readonly int _memberIndex1;
        private readonly int _memberIndex2;
        private readonly int _memberIndex3;
        private readonly int _memberIndex4;
        private readonly int _memberIndex5;
        private readonly int _memberIndex6;
        private readonly int _memberIndex7;
        private readonly int _memberIndex8;

        public CompositeFactory(
            Delegate constructor,
            int memberIndex1,
            int memberIndex2,
            int memberIndex3,
            int memberIndex4,
            int memberIndex5,
            int memberIndex6,
            int memberIndex7,
            int memberIndex8)
        {
            _constructor = constructor;
            _memberIndex1 = memberIndex1;
            _memberIndex2 = memberIndex2;
            _memberIndex3 = memberIndex3;
            _memberIndex4 = memberIndex4;
            _memberIndex5 = memberIndex5;
            _memberIndex6 = memberIndex6;
            _memberIndex7 = memberIndex7;
            _memberIndex8 = memberIndex8;
        }

        public override TComposite CreateComposite(CompositeMembers members) =>
            Arguments switch
            {
                0 => UnsafeChecked.As<Func<TComposite>>(_constructor).Invoke(),
                1 => UnsafeChecked.As<Func<TArg1, TComposite>>(_constructor).Invoke(
                    members.Get<TArg1>(_memberIndex1)),
                2 => UnsafeChecked.As<Func<TArg1, TArg2, TComposite>>(_constructor).Invoke(
                    members.Get<TArg1>(_memberIndex1),
                    members.Get<TArg2>(_memberIndex2)),
                3 => UnsafeChecked.As<Func<TArg1, TArg2, TArg3, TComposite>>(_constructor).Invoke(
                    members.Get<TArg1>(_memberIndex1),
                    members.Get<TArg2>(_memberIndex2),
                    members.Get<TArg3>(_memberIndex3)),
                4 => UnsafeChecked.As<Func<TArg1, TArg2, TArg3, TArg4, TComposite>>(_constructor).Invoke(
                    members.Get<TArg1>(_memberIndex1),
                    members.Get<TArg2>(_memberIndex2),
                    members.Get<TArg3>(_memberIndex3),
                    members.Get<TArg4>(_memberIndex4)),
                5 => UnsafeChecked.As<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TComposite>>(_constructor).Invoke(
                    members.Get<TArg1>(_memberIndex1),
                    members.Get<TArg2>(_memberIndex2),
                    members.Get<TArg3>(_memberIndex3),
                    members.Get<TArg4>(_memberIndex4),
                    members.Get<TArg5>(_memberIndex5)),
                6 => UnsafeChecked.As<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TComposite>>(_constructor).Invoke(
                    members.Get<TArg1>(_memberIndex1),
                    members.Get<TArg2>(_memberIndex2),
                    members.Get<TArg3>(_memberIndex3),
                    members.Get<TArg4>(_memberIndex4),
                    members.Get<TArg5>(_memberIndex5),
                    members.Get<TArg6>(_memberIndex6)),
                7 => UnsafeChecked.As<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TComposite>>(_constructor).Invoke(
                    members.Get<TArg1>(_memberIndex1),
                    members.Get<TArg2>(_memberIndex2),
                    members.Get<TArg3>(_memberIndex3),
                    members.Get<TArg4>(_memberIndex4),
                    members.Get<TArg5>(_memberIndex5),
                    members.Get<TArg6>(_memberIndex6),
                    members.Get<TArg7>(_memberIndex7)),
                8 => UnsafeChecked.As<Func<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6, TArg7, TArg8, TComposite>>(_constructor).Invoke(
                    members.Get<TArg1>(_memberIndex1),
                    members.Get<TArg2>(_memberIndex2),
                    members.Get<TArg3>(_memberIndex3),
                    members.Get<TArg4>(_memberIndex4),
                    members.Get<TArg5>(_memberIndex5),
                    members.Get<TArg6>(_memberIndex6),
                    members.Get<TArg7>(_memberIndex7),
                    members.Get<TArg8>(_memberIndex8)),
                _ => throw new NotSupportedException()
            };
    }

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    internal struct Unused
    {
    }

    internal static class UnsafeChecked
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T As<T>(object value)
            where T : class
        {
            Debug.Assert(value is T);
            return Unsafe.As<T>(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TTo As<TFrom, TTo>(ref TFrom value)
        {
            Debug.Assert(value is TTo);
            return Unsafe.As<TFrom, TTo>(ref value);
        }
    }

    internal static class IsValueType<T>
    {
        public static readonly bool Value = typeof(T).IsValueType;
    }

    internal abstract class CompositeOperation<TComposite>
    {
    }

    internal sealed class CompositeReadOperation<TComposite> : CompositeOperation<TComposite>
    {
        private readonly CompositeMembers _members;
        private readonly CompositeFactory<TComposite> _factory;
        private readonly CompositeMemberHandler<TComposite>[] _handlers;
    }

    internal sealed class CompositeWriteOperation<TComposite> : CompositeOperation<TComposite>
    {

    }

    internal abstract class CompositeMemberHandler<TComposite>
    {
        public abstract ValueTask ReadAsync(NpgsqlReadBuffer buffer, CompositeReadOperation<TComposite> state);
        public abstract ValueTask WriteAsync(NpgsqlWriteBuffer buffer, NpgsqlLengthCache lengthCache, CompositeWriteOperation<TComposite> state);
    }

    internal sealed class CompositeMemberHandler<TComposite, TMember> : CompositeMemberHandler<TComposite>
    {
        private readonly NpgsqlTypeHandler _handler;
        private readonly Delegate? _getter;
        private readonly Delegate? _setter;
        private readonly int _index;

        public CompositeMemberHandler(NpgsqlTypeHandler handler, Delegate? getter, Delegate? setter) =>
            (_handler, _getter, _setter) = (handler, getter, setter);

        public override ValueTask ReadAsync(NpgsqlReadBuffer buffer, CompositeReadOperation<TComposite> state)
        {
            var task = _handler.Read<TMember>(buffer);
            if (task.IsCompleted)
            {
                SetValue(state, task.Result);
                return default;
            }

            return ReadSlow(state, task);

            async ValueTask ReadSlow(CompositeReadOperation<TComposite> o, ValueTask<TMember> t) =>
                SetValue(o, await t);
        }

        public override ValueTask WriteAsync(NpgsqlWriteBuffer buffer, NpgsqlLengthCache lengthCache, State<TComposite> state) =>
            _handler.Write(GetValue(state), buffer, null, lengthCache);

        public TMember GetValue(State<TComposite> state)
        {
            if (_getter is null)
                throw new NotSupportedException();

            return IsValueType<TComposite>.Value
                ? UnsafeChecked.As<MemberGetterByRef<TComposite, TMember>>(_getter).Invoke(ref state.Composite)
                : UnsafeChecked.As<MemberGetter<TComposite, TMember>>(_getter).Invoke(state.Composite);
        }

        public void SetValue(CompositeReadOperation<TComposite> members, TMember value)
        {
            if (_setter is null)
                throw new NotSupportedException();

            if (members.CompositeConstructed)
            {
                if (IsValueType<TComposite>.Value)
                    UnsafeChecked.As<MemberSetterByRef<TComposite, TMember>>(_setter)
                        .Invoke(ref members.GetCompositeByRef(), value);
                else
                    UnsafeChecked.As<MemberSetter<TComposite, TMember>>(_setter)
                        .Invoke(members.GetComposite(), value);
            }
            else
            {
                members.SetMember<TMember>(_index, value);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TMember"></typeparam>
    /// <param name="composite"></param>
    /// <returns></returns>
    public delegate TMember MemberGetter<TType, TMember>(TType composite);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TMember"></typeparam>
    /// <param name="composite"></param>
    /// <param name="value"></param>
    public delegate void MemberSetter<TType, TMember>(TType composite, TMember value);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TMember"></typeparam>
    /// <param name="composite"></param>
    /// <returns></returns>
    public delegate TMember MemberGetterByRef<TType, TMember>(ref TType composite);

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TType"></typeparam>
    /// <typeparam name="TMember"></typeparam>
    /// <param name="composite"></param>
    /// <param name="value"></param>
    public delegate void MemberSetterByRef<TType, TMember>(ref TType composite, TMember value);

    internal sealed class CompositeHandlerFactory<T> : NpgsqlTypeHandlerFactory
    {

    }

    internal sealed class CompositeHandler
    {
        public static CompositeClassHandlerBuilder<TComposite> ForClass<TComposite>(Func<TComposite> constructor)
            where TComposite : class;

        public static CompositeStructHandlerBuilder<TComposite> ForStruct<TComposite>(Func<TComposite> constructor)
            where TComposite : struct;
    }

    internal ref struct CompositeClassHandlerBuilder<TComposite>
    {
        private CompositeHandlerBuilder<TComposite> _builder;

        public void HasMember<TMember>(
            string name,
            MemberGetter<TComposite, TMember>? getter,
            MemberSetter<TComposite, TMember>? setter)
        {
            _builder = _builder.HasMember<TMember>(name, getter, setter);
        }
    }

    internal ref struct CompositeStructHandlerBuilder<TComposite>
    {
        private CompositeHandlerBuilder<TComposite> _builder;

        public void HasMember<TMember>(
            string name,
            MemberGetterByRef<TComposite, TMember>? getter,
            MemberSetterByRef<TComposite, TMember>? setter)
        {
            _builder = _builder.HasMember<TMember>(name, getter, setter);
        }
    }

    internal sealed class CompositeConstructor
    {
        private readonly Delegate _constructor;
        private readonly Type[] _arguments;
    }

    internal abstract class CompositeHandlerBuilder<TComposite>
    {
        public abstract CompositeHandlerBuilder<TComposite> HasMember<TMember>(string name, Delegate? getter, Delegate? setter);

        public abstract CompositeReadOperation<TComposite> CreateReadOperation();
        public abstract CompositeWriteOperation<TComposite> CreateWriteOperation();
    }

    internal sealed class CompositeHandlerBuilder<TComposite, TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember8> : CompositeHandlerBuilder<TComposite>
    {
        private static readonly int Members = 0 +
            (typeof(TMember1) == typeof(Unused) ? 1 : 0) +
            (typeof(TMember2) == typeof(Unused) ? 1 : 0) +
            (typeof(TMember3) == typeof(Unused) ? 1 : 0) +
            (typeof(TMember4) == typeof(Unused) ? 1 : 0) +
            (typeof(TMember5) == typeof(Unused) ? 1 : 0) +
            (typeof(TMember6) == typeof(Unused) ? 1 : 0) +
            (typeof(TMember7) == typeof(Unused) ? 1 : 0) +
            (typeof(TMember8) == typeof(Unused) ? 1 : 0);

        private readonly CompositeConstructor _constructor;
        private readonly CompositeMemberBuilder<TComposite>? _member1;
        private readonly CompositeMemberBuilder<TComposite>? _member2;
        private readonly CompositeMemberBuilder<TComposite>? _member3;
        private readonly CompositeMemberBuilder<TComposite>? _member4;
        private readonly CompositeMemberBuilder<TComposite>? _member5;
        private readonly CompositeMemberBuilder<TComposite>? _member6;
        private readonly CompositeMemberBuilder<TComposite>? _member7;
        private readonly CompositeMemberBuilder<TComposite>? _member8;

        public CompositeHandlerBuilder(
            CompositeConstructor constructor,
            CompositeMemberBuilder<TComposite>? member1,
            CompositeMemberBuilder<TComposite>? member2,
            CompositeMemberBuilder<TComposite>? member3,
            CompositeMemberBuilder<TComposite>? member4,
            CompositeMemberBuilder<TComposite>? member5,
            CompositeMemberBuilder<TComposite>? member6,
            CompositeMemberBuilder<TComposite>? member7,
            CompositeMemberBuilder<TComposite>? member8)
        {
            _constructor = constructor;
            _member1 = member1;
            _member2 = member2;
            _member3 = member3;
            _member4 = member4;
            _member5 = member5;
            _member6 = member6;
            _member7 = member7;
            _member8 = member8;
        }

        public override CompositeHandlerBuilder<TComposite> HasMember<TMember>(string name, Delegate? getter, Delegate? setter)
        {
            var member = new CompositeMemberBuilder<TComposite, TMember>(name, getter, setter);
            return (Members + 1) switch
            {
                1 => new CompositeHandlerBuilder<TComposite, TMember, Unused, Unused, Unused, Unused, Unused, Unused, Unused>(
                    _constructor, member, default, default, default, default, default, default, default),
                2 => new CompositeHandlerBuilder<TComposite, TMember1, TMember, Unused, Unused, Unused, Unused, Unused, Unused>(
                    _constructor, _member1, member, default, default, default, default, default, default),
                3 => new CompositeHandlerBuilder<TComposite, TMember1, TMember2, TMember, Unused, Unused, Unused, Unused, Unused>(
                    _constructor, _member1, _member2, member, default, default, default, default, default),
                4 => new CompositeHandlerBuilder<TComposite, TMember1, TMember2, TMember3, TMember, Unused, Unused, Unused, Unused>(
                    _constructor, _member1, _member2, _member3, member, default, default, default, default),
                5 => new CompositeHandlerBuilder<TComposite, TMember1, TMember2, TMember3, TMember4, TMember, Unused, Unused, Unused>(
                    _constructor, _member1, _member2, _member3, _member4, member, default, default, default),
                6 => new CompositeHandlerBuilder<TComposite, TMember1, TMember2, TMember3, TMember4, TMember5, TMember, Unused, Unused>(
                    _constructor, _member1, _member2, _member3, _member4, _member5, member, default, default),
                7 => new CompositeHandlerBuilder<TComposite, TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember, Unused>(
                    _constructor, _member1, _member2, _member3, _member4, _member5, _member6, member, default),
                8 => new CompositeHandlerBuilder<TComposite, TMember1, TMember2, TMember3, TMember4, TMember5, TMember6, TMember7, TMember>(
                    _constructor, _member1, _member2, _member3, _member4, _member5, _member6, _member7, member),
                _ => new CompositeDynamicHandlerBuilder<TComposite>(
                    _constructor, _member1, _member2, _member3, _member4, _member5, _member6, _member7, _member8, member)
            };
        }

        public override CompositeReadOperation<TComposite> CreateReadOperation()
        {
            throw new NotImplementedException();
        }

        internal sealed class ReadOperation : CompositeReadOperation<TComposite>
        {
            private TMember1 _member1;
            private TMember2 _member2;
            private TMember3 _member3;
            private TMember4 _member4;
            private TMember5 _member5;
            private TMember6 _member6;
            private TMember7 _member7;
            private TMember8 _member8;

            public ReadOperation(
                CompositeFactory<TComposite> factory,
                CompositeMemberHandler<TComposite>[] members)
                : base(factory, members) { }

            public override TMember Get<TMember>(int index) =>
                index switch
                {
                    0 => UnsafeChecked.As<TMember1, TMember>(ref _member1),
                    1 => UnsafeChecked.As<TMember2, TMember>(ref _member2),
                    2 => UnsafeChecked.As<TMember3, TMember>(ref _member3),
                    3 => UnsafeChecked.As<TMember4, TMember>(ref _member4),
                    4 => UnsafeChecked.As<TMember5, TMember>(ref _member5),
                    5 => UnsafeChecked.As<TMember6, TMember>(ref _member6),
                    6 => UnsafeChecked.As<TMember7, TMember>(ref _member7),
                    7 => UnsafeChecked.As<TMember8, TMember>(ref _member8),
                    _ => throw new ArgumentOutOfRangeException(nameof(index))
                };

            public override void SetMember<TMember>(int index, TMember value)
            {
                switch (index)
                {
                    case 0: _member1 = UnsafeChecked.As<TMember, TMember1>(ref value); return;
                    case 1: _member2 = UnsafeChecked.As<TMember, TMember2>(ref value); return;
                    case 2: _member3 = UnsafeChecked.As<TMember, TMember3>(ref value); return;
                    case 3: _member4 = UnsafeChecked.As<TMember, TMember4>(ref value); return;
                    case 4: _member5 = UnsafeChecked.As<TMember, TMember5>(ref value); return;
                    case 5: _member6 = UnsafeChecked.As<TMember, TMember6>(ref value); return;
                    case 6: _member7 = UnsafeChecked.As<TMember, TMember7>(ref value); return;
                    case 7: _member8 = UnsafeChecked.As<TMember, TMember8>(ref value); return;
                    default: throw new ArgumentOutOfRangeException(nameof(index));
                }
            }
        }
    }

    internal sealed class CompositeDynamicHandlerBuilder<TComposite> : CompositeHandlerBuilder<TComposite>
    {
        private readonly CompositeConstructor _constructor;
        private readonly CompositeMemberBuilder<TComposite>?[] _members;
        private readonly int _memberCount;

        private CompositeDynamicHandlerBuilder(
            CompositeDynamicHandlerBuilder<TComposite> composite,
            CompositeMemberBuilder<TComposite> member)
        {
            _constructor = composite._constructor;
            _members = composite._members;
            _memberCount = composite._memberCount + 1;

            if (_members.Length > _memberCount)
                Array.Resize(ref _members, _members.Length * 2);

            _members[_memberCount] = member;
        }

        public CompositeDynamicHandlerBuilder(
            CompositeConstructor constructor,
            CompositeMemberBuilder<TComposite>? member1,
            CompositeMemberBuilder<TComposite>? member2,
            CompositeMemberBuilder<TComposite>? member3,
            CompositeMemberBuilder<TComposite>? member4,
            CompositeMemberBuilder<TComposite>? member5,
            CompositeMemberBuilder<TComposite>? member6,
            CompositeMemberBuilder<TComposite>? member7,
            CompositeMemberBuilder<TComposite>? member8,
            CompositeMemberBuilder<TComposite>? member9)
        {
            _constructor = constructor;
            _members = new CompositeMemberBuilder<TComposite>?[16];
            _members[_memberCount++] = member1;
            _members[_memberCount++] = member2;
            _members[_memberCount++] = member3;
            _members[_memberCount++] = member4;
            _members[_memberCount++] = member5;
            _members[_memberCount++] = member6;
            _members[_memberCount++] = member7;
            _members[_memberCount++] = member8;
            _members[_memberCount++] = member9;
        }

        public override CompositeHandlerBuilder<TComposite> HasMember<TMember>(string name, Delegate? getter, Delegate? setter) =>
            new CompositeDynamicHandlerBuilder<TComposite>(this, new CompositeMemberBuilder<TComposite, TMember>(name, getter, setter));

        internal sealed class CompositeFactory : CompositeFactory<TComposite>
        {
            private readonly Delegate _constructor;

            public override TComposite CreateComposite(CompositeReadOperation<TComposite> members) =>
                (TComposite)_constructor.DynamicInvoke();
        }

        internal sealed class ReadOperation : CompositeReadOperation<TComposite>
        {
            private readonly object[] _arguments;
            private readonly object[] _members;

            public ReadOperation(
                CompositeFactory<TComposite> factory,
                CompositeMemberHandler<TComposite>[] members)
                : base(factory, members)
            {
                _members = new object[members.Length];
            }

            public override TMember Get<TMember>(int index)
            {
                var member = _members[index];
                return IsValueType<TMember>.Value
                    ? UnsafeChecked.As<Boxed<TMember>>(member).Value
                    : (TMember)member;
            }

            public override void SetMember<TMember>(int index, TMember value)
            {
                if (IsValueType<TMember>.Value)
                    UnsafeChecked.As<Boxed<TMember>>(_members[index]).Value = value;
                else
                    _members[index] = value!;
            }
        }
    }

    internal abstract class CompositeMemberBuilder<TComposite>
    {
        public string Name { get; }

        public CompositeMemberBuilder(string name) =>
            Name = name;

        public abstract CompositeMemberHandler<TComposite> CreateHandler();
    }

    internal sealed class CompositeMemberBuilder<TComposite, TMember> : CompositeMemberBuilder<TComposite>
    {
        private readonly Delegate? _getter;
        private readonly Delegate? _setter;

        public CompositeMemberBuilder(string name, Delegate? getter, Delegate? setter)
            : base(name)
        {
            Debug.Assert(IsValueType<TComposite>.Value
                ? (getter is null || getter is MemberGetterByRef<TComposite, TMember>) && (setter is null || setter is MemberGetterByRef<TComposite, TMember>)
                : (getter is null || getter is MemberGetter<TComposite, TMember>) && (setter is null || setter is MemberGetter<TComposite, TMember>));

            _getter = getter;
            _setter = setter;
        }

        public override CompositeMemberHandler<TComposite> CreateHandler() =>
            new CompositeMemberHandler<TComposite, TMember>(default!, _getter, _setter);
    }
}
