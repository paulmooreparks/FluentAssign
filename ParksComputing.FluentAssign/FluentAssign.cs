using System;
using System.Collections.Generic;
using System.Linq;

namespace ParksComputing.FluentAssign;

/// <summary>
/// Provides a fluent interface for conditional value assignment and conversion.
/// </summary>
public static class Assign<TOut> {
    /// <summary>
    /// Creates a new <see cref="AssignmentBuilder{TOut}"/> instance.
    /// </summary>
    public static AssignmentBuilder<TOut> Create() {
        return new();
    }

    /// <summary>
    /// Begins a new assignment chain with the given candidate value.
    /// </summary>
    public static AssignmentBuilder<TOut> AssignIf<T>(T? candidate) {
        return new AssignmentBuilder<TOut>().AssignIf(candidate);
    }
}

/// <summary>
/// Builds a fluent assignment chain with conditional fallbacks and type conversions.
/// </summary>
public class AssignmentBuilder<TOut> {
    private readonly List<Func<object?>> _candidates = new();
    private readonly Dictionary<Type, Func<object, TOut>> _converters = new();
    private TOut? _defaultValue = default;
    private Func<TOut>? _defaultFactory = null;

    /// <summary>
    /// Adds a custom converter to transform values of type <typeparamref name="TIn"/> into the output type <typeparamref name="TOut"/>.
    /// </summary>
    public AssignmentBuilder<TOut> Convert<TIn>(Func<TIn, TOut> converter) {
        _converters[typeof(TIn)] = x => converter((TIn)x);
        return this;
    }

    /// <summary>
    /// Adds a candidate value for assignment.
    /// </summary>
    public AssignmentBuilder<TOut> AssignIf<T>(T? candidate) {
        return AssignIf(() => candidate);
    }

    /// <summary>
    /// Adds a candidate value with a condition that must pass for it to be used.
    /// </summary>
    public AssignmentBuilder<TOut> AssignIf<T>(T? candidate, Func<T, bool> predicate) {
        return AssignIf(() => candidate, predicate);
    }

    /// <summary>
    /// Adds a candidate with a predicate and a custom converter if the condition passes.
    /// </summary>
    public AssignmentBuilder<TOut> AssignIf<TIn>(TIn? candidate, Func<TIn, bool> predicate, Func<TIn, TOut> converter) {
        return AssignIf(() => candidate, predicate, converter);
    }

    /// <summary>
    /// Adds a deferred candidate-producing function.
    /// </summary>
    public AssignmentBuilder<TOut> AssignIf<T>(Func<T?> candidateFactory) {
        _candidates.Add(() => candidateFactory());
        return this;
    }

    /// <summary>
    /// Adds a deferred candidate-producing function with a predicate condition.
    /// </summary>
    public AssignmentBuilder<TOut> AssignIf<T>(Func<T?> candidateFactory, Func<T, bool> predicate) {
        _candidates.Add(() => {
            var val = candidateFactory();
            if (val != null && predicate(val)) {
                return val;
            }
            return null;
        });
        return this;
    }

    /// <summary>
    /// Adds a deferred candidate-producing function with a predicate and converter.
    /// </summary>
    public AssignmentBuilder<TOut> AssignIf<TIn>(Func<TIn?> candidateFactory, Func<TIn, bool> predicate, Func<TIn, TOut> converter) {
        _candidates.Add(() => {
            var val = candidateFactory();
            if (val != null && predicate(val)) {
                return converter(val);
            }
            return null;
        });
        return this;
    }

    /// <summary>
    /// Adds a fallback value to use if no candidate values match.
    /// </summary>
    public AssignmentBuilder<TOut> Else(object? fallback) {
        _candidates.Add(() => fallback);
        return this;
    }

    /// <summary>
    /// Adds a fallback value of type <typeparamref name="TOut"/> to use if no candidates succeed.
    /// </summary>
    public AssignmentBuilder<TOut> Default(TOut fallback) {
        _defaultValue = fallback;
        return this;
    }

    /// <summary>
    /// Adds a deferred fallback factory to use if no candidates succeed.
    /// </summary>
    public AssignmentBuilder<TOut> Default(Func<TOut> fallbackFactory) {
        _defaultFactory = fallbackFactory;
        return this;
    }

    /// <summary>
    /// Evaluates the assignment chain and returns the resolved value.
    /// Throws if no value could be resolved.
    /// </summary>
    public TOut To() {
        foreach (var candidateFunc in _candidates) {
            var candidate = candidateFunc();
            if (candidate == null) {
                continue;
            }

            if (candidate is TOut direct) {
                return direct;
            }

            var candidateType = candidate.GetType();
            if (_converters.TryGetValue(candidateType, out var exactConverter)) {
                return exactConverter(candidate);
            }

            var fallbackMatch = _converters.FirstOrDefault(pair => pair.Key.IsAssignableFrom(candidateType));
            if (fallbackMatch.Value != null) {
                return fallbackMatch.Value(candidate);
            }
        }

        if (_defaultValue is not null) {
            return _defaultValue;
        }

        if (_defaultFactory is not null) {
            return _defaultFactory();
        }

        throw new InvalidOperationException($"No suitable candidate could be converted to {typeof(TOut).Name}.");
    }

    /// <summary>
    /// Evaluates the assignment chain and returns a nullable result.
    /// Returns null if no candidate matched.
    /// </summary>
    public TOut? ToNullable() {
        foreach (var candidateFunc in _candidates) {
            var candidate = candidateFunc();
            if (candidate == null) {
                continue;
            }

            if (candidate is TOut direct) {
                return direct;
            }

            var candidateType = candidate.GetType();
            if (_converters.TryGetValue(candidateType, out var exactConverter)) {
                return exactConverter(candidate);
            }

            var fallbackMatch = _converters.FirstOrDefault(pair => pair.Key.IsAssignableFrom(candidateType));
            if (fallbackMatch.Value != null) {
                return fallbackMatch.Value(candidate);
            }
        }

        if (_defaultValue is TOut def) {
            return def;
        }

        if (_defaultFactory is not null) {
            return _defaultFactory();
        }

        return default(TOut);
    }
}
