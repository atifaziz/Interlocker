namespace Interlocker
{
    using System;
    using System.Diagnostics;
    using System.Threading;

    // ReSharper disable PartialTypeWithSinglePart

    static partial class InterlockedOptimized
    {
        public static InterlockedOptimized<T> Create<T>(T value = default)
            where T : class =>
            new InterlockedOptimized<T>(value);
    }

    [DebuggerDisplay("Value")]
    partial class InterlockedOptimized<T> where T : class
    {
        T _value;

        public InterlockedOptimized() : this(default) { }
        public InterlockedOptimized(T value) => _value = value;

        public T Value => _value;

        public T Update(Func<T, T> updater)
        {
            if (updater == null) throw new ArgumentNullException(nameof(updater));
            return Update(s => (s = updater(s), s));
        }

        public T UpdateExcept(Func<T, T> updater)
        {
            if (updater == null) Thrower.ArgumentNull();
            return Update(s => (s = updater(s), s));
        }

        public TResult Update<TResult>(Func<T, (T State, TResult Result)> updater) =>
            Update(updater, t => t.State, t => t.Result);

        public TResult Update<TResult>(Func<T, int, (T State, TResult Result)> updater) =>
            Update(updater, t => t.State, t => t.Result);

        public TResult Update<TUpdate, TResult>(
            Func<T, TUpdate> updater,
            Func<TUpdate, T> stateSelector,
            Func<TUpdate, TResult> resultSelector)
        {
            return Update((curr, i, attempt) => updater(curr), stateSelector, resultSelector);
        }

        public TResult Update<TUpdate, TResult>(
            Func<T, int, TUpdate> updater,
            Func<TUpdate, T> stateSelector,
            Func<TUpdate, TResult> resultSelector)
        {
            return Update((curr, i, _) => updater(curr, i), stateSelector, resultSelector);
        }

        public T UpdateShort(
            Func<T, T> updater)
        {
            var attempt = default(T);
            var i = 0;
            for (var sw = new SpinWait(); ; sw.SpinOnce(), i++)
            {
                var current = _value;
                var update = updater(current);
                var replacement = update;
                if (replacement == null || current == System.Threading.Interlocked.CompareExchange(ref _value, replacement, current))
                    return update;
                attempt = update;
            }
        }

        public TResult Update<TUpdate, TResult>(
            Func<T, int, TUpdate, TUpdate> updater,
            Func<TUpdate, T> replacementSelector,
            Func<TUpdate, TResult> resultSelector)
        {
            var attempt = default(TUpdate);
            var i = 0;
            for (var sw = new SpinWait(); ; sw.SpinOnce(), i++)
            {
                var current = _value;
                var update = updater(current, i, attempt);
                var replacement = replacementSelector(update);
                if (replacement == null || current == System.Threading.Interlocked.CompareExchange(ref _value, replacement, current))
                    return resultSelector(update);
                attempt = update;
            }
        }
    }
}
